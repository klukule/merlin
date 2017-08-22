using Merlin.API;
using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using YinYang.CodeProject.Projects.SimplePathfinding.PathFinders.AStar;

namespace Merlin.Profiles.Gatherer
{
    public partial class Gatherer
    {
        #region Fields

        private static int _maximumFailedFindAttempts = 60;

        private SimulationObjectView _currentTarget;

        private int _failedFindAttempts;
        private PositionPathingRequest _changeGatheringPathRequest;

        #endregion Fields

        #region Methods

        public void Search()
        {
            if (_localPlayerCharacterView.IsUnderAttack(out FightingObjectView attacker))
            {
                Core.Log("[Attacked]");
                _state.Fire(Trigger.EncounteredAttacker);
                return;
            }

            if (new Cluster(alb.a().u()).Name != _selectedGatherCluster)
            {
                Core.Log("[Travel to target cluster]");
                _targetCluster = _world.GetCluster(_selectedGatherCluster);
                _state.Fire(Trigger.StartTravelling);
                return;
            }

            if (_allowSiegeCampTreasure && CanUseSiegeCampTreasure && _localPlayerCharacterView.GetLoadPercent() > CapacityForSiegeCampTreasure)
            {
                _state.Fire(Trigger.StartSiegeCampTreasure);
                return;
            }

            if (_localPlayerCharacterView.GetLoadPercent() > CapacityForBanking)
            {
                Core.Log("Overweight");
                _state.Fire(Trigger.Overweight);
                return;
            }

            if (Loot())
                return;

            if (_currentTarget != null)
            {
                Core.Log("[Blacklisting target]");

                Blacklist(_currentTarget, TimeSpan.FromMinutes(0.5f));

                _currentTarget = null;
                _harvestPathingRequest = null;

                return;
            }

            if (IdentifiedTarget(out SimulationObjectView target))
            {
                Core.Log("[Checking Target]");
                _currentTarget = target;
            }

            if (_currentTarget != null && ValidateTarget(_currentTarget))
            {
                _changeGatheringPathRequest = null;
                _failedFindAttempts = 0;
                Core.Log("[Identified]");
                _state.Fire(Trigger.DiscoveredResource);
                return;
            }
            else
            {
                if (_changeGatheringPathRequest != null)
                {
                    if (_changeGatheringPathRequest.IsRunning)
                    {
                        if (!HandleMounting(Vector3.zero))
                            return;

                        _changeGatheringPathRequest.Continue();
                    }
                    else
                    {
                        _changeGatheringPathRequest = null;
                    }

                    return;
                }

                _failedFindAttempts++;
                if (_failedFindAttempts > _maximumFailedFindAttempts)
                {
                    Core.Log($"[Looking for fallback in {_gatheredSpots.Count} objects]");

                    //Remove all fallback points older than 1 hour
                    var entriesToRemove = _gatheredSpots.Where(kvp => !kvp.Value.HarvestDate.HasValue || kvp.Value.HarvestDate.Value.AddHours(1) < DateTime.UtcNow).ToArray();
                    foreach (var entry in entriesToRemove)
                    {
                        Core.Log($"[Removing {entry.Key} from fallback objects. Too old]");
                        _gatheredSpots.Remove(entry.Key);
                    }

                    var validEntries = _gatheredSpots.Where(kvp =>
                    {
                        var info = new GatherInformation(kvp.Value.ResourceType, kvp.Value.Tier, kvp.Value.EnchantmentLevel);
                        return _gatherInformations[info];
                    }).ToArray();

                    Core.Log($"[Found {validEntries.Length} valid fallback objects]");
                    if (validEntries.Length == 0)
                        return;

                    //Select a random fallback point
                    var spotToUse = validEntries[UnityEngine.Random.Range(0, validEntries.Length)];
                    if (_localPlayerCharacterView.TryFindPath(new ClusterPathfinder(), spotToUse.Key, IsBlocked, out List<Vector3> pathing))
                    {
                        Core.Log($"Falling back to {spotToUse.Key} which should hold {spotToUse.Value.ToString()}");
                        _changeGatheringPathRequest = new PositionPathingRequest(_localPlayerCharacterView, spotToUse.Key, pathing);
                        _gatheredSpots.Remove(spotToUse.Key);
                    }

                    _failedFindAttempts = 0;
                }
            }
        }

        public bool Loot()
        {
            //var silver = _client.GetEntities<SilverObjectView>(s => !s.IsLootProtected()).FirstOrDefault();
            //if (silver != null)
            //{
            //    Core.Log($"[Silver {silver.name}]");
            //    _localPlayerCharacterView.Interact(silver);
            //    return true;
            //}

            var loot = _client.GetEntities<LootObjectView>(l => !l.IsLootProtected()).FirstOrDefault();
            if (loot != null)
            {
                var needsInteraction = !GameGui.Instance.LootGui.gameObject.activeSelf && loot.CanBeUsed;

                if (needsInteraction)
                {
                    Core.Log($"[Loot {loot.name}]");
                    _localPlayerCharacterView.Interact(loot);
                    return true;
                }
                else
                {
                    Core.Log($"[Moving Loot]");
                    var playerStorage = GameGui.Instance.CharacterInfoGui.InventoryItemStorage;
                    var lootStorage = GameGui.Instance.LootGui.YourInventoryStorage;

                    //Get all items
                    var hasItems = lootStorage.ItemsSlotsRegistered.Any(i => i != null && i.ObservedItemView != null);
                    if (hasItems)
                    {
                        foreach (var slot in lootStorage.ItemsSlotsRegistered)
                            if (slot != null && slot.ObservedItemView != null)
                            {
                                Core.Log($"[Looting {slot.name}]");
                                GameGui.Instance.MoveItemToItemContainer(slot, playerStorage.ItemContainerProxy);
                            }
                        return true;
                    }
                    else
                        Core.Log($"[Looting done]");
                }
            }

            return false;
        }

        public bool IdentifiedTarget(out SimulationObjectView target)
        {
            var resources = _client.GetEntities<HarvestableObjectView>(ValidateHarvestable);
            var hostiles = _client.GetEntities<MobView>(ValidateMob);

            var views = new List<SimulationObjectView>();

            if (_allowMobHunting)
            {
                foreach (var h in hostiles)
                    if (h.GetResourceType().HasValue)
                        views.Add(h);
            }

            foreach (var r in resources)
            {
                views.Add(r);
            }
            //foreach (var h in hostiles) views.Add(h);

            var filteredViews = views.Where(view =>
            {
                if (_skipUnrestrictedPvPZones && _landscape.IsInAnyUnrestrictedPvPZone(view.transform.position))
                    return false;

                if (_skipKeeperPacks && ContainKeepers(view.transform.position))
                    return false;

                if (view is HarvestableObjectView harvestable)
                {
                    //resourceType contains EnchantmentLevel, so we cut it off
                    var resourceTypeString = harvestable.GetResourceType();
                    if (resourceTypeString.Contains("_"))
                        resourceTypeString = resourceTypeString.Substring(0, resourceTypeString.IndexOf("_"));

                    var resourceType = (ResourceType)Enum.Parse(typeof(ResourceType), resourceTypeString, true);
                    var tier = (Tier)harvestable.GetTier();
                    var enchantmentLevel = (EnchantmentLevel)harvestable.GetRareState();

                    var info = new GatherInformation(resourceType, tier, enchantmentLevel);

                    return _gatherInformations[info];
                }
                else if (view is MobView mob)
                {
                    var resourceType = mob.GetResourceType().Value;
                    var tier = (Tier)mob.GetTier();
                    var enchantmentLevel = (EnchantmentLevel)mob.GetRareState();

                    var info = new GatherInformation(resourceType, tier, enchantmentLevel);

                    return _gatherInformations[info];
                }
                else
                    return false;
            });

            target = filteredViews.OrderBy((view) =>
            {
                var playerPosition = _localPlayerCharacterView.transform.position;
                var resourcePosition = view.transform.position;

                var score = (resourcePosition - playerPosition).sqrMagnitude;

                if (view is HarvestableObjectView harvestable)
                {
                    var rareState = harvestable.GetRareState();

                    if (harvestable.GetTier() >= 3) score /= (harvestable.GetTier() - 1);
                    if (harvestable.GetCurrentCharges() == harvestable.GetMaxCharges()) score /= 2;
                    if (rareState > 0) score /= ((rareState + 1) * (rareState + 1));
                }
                else if (view is MobView mob)
                {
                    var rareState = mob.GetRareState();

                    if (mob.GetTier() >= 3) score /= (mob.GetTier() - 1);
                    //if (mob.GetCurrentCharges() == mob.GetMaxCharges()) score /= 2;
                    if (rareState > 0) score /= ((rareState + 1) * (rareState + 1));
                }

                var yDelta = Math.Abs(_landscape.GetLandscapeHeight(playerPosition.c()) - _landscape.GetLandscapeHeight(resourcePosition.c()));

                score += (yDelta * 10f);

                return (int)score;
            }).FirstOrDefault();

            if (target != null)
                Core.Log($"Resource spotted: {target.name}");
            else
                Core.Log($"No Resource spotted. Waiting...");

            return (target != default(SimulationObjectView));
        }

        public bool ContainKeepers(Vector3 location)
        {
            if (_keepers.Any(k => Vector3.Distance(location, k.transform.position) <= _keeperSkipRange))
                return true;

            return false;
        }

        public bool IsBlocked(Vector2 location)
        {
            var vector = new Vector3(location.x, 0, location.y);

            if (_skipUnrestrictedPvPZones && _landscape.IsInAnyUnrestrictedPvPZone(vector))
                return true;

            if (_skipKeeperPacks && ContainKeepers(vector))
                return true;

            if (_currentTarget != null)
            {
                var resourcePosition = new Vector2(_currentTarget.transform.position.x,
                                                    _currentTarget.transform.position.z);
                var distance = (resourcePosition - location).magnitude;

                if (distance < (_currentTarget.GetColliderExtents() + _localPlayerCharacterView.GetColliderExtents()))
                    return false;
            }

            if (_localPlayerCharacterView != null)
            {
                var playerLocation = new Vector2(_localPlayerCharacterView.transform.position.x,
                                                    _localPlayerCharacterView.transform.position.z);
                var distance = (playerLocation - location).magnitude;

                if (distance < 2f)
                    return false;
            }

            return (_client.Collision.GetFlag(vector, 1.0f) > 0);
        }

        #endregion Methods
    }
}