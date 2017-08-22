using System;
using System.Collections.Generic;
using UnityEngine;

using YinYang.CodeProject.Projects.SimplePathfinding.PathFinders.AStar;

namespace Merlin.Profiles.Gatherer
{
    public partial class Gatherer
    {
        #region Static

        private static double _minimumAttackableRange = 10;
        private static int _minimumHarvestableTier = 2;

        private ClusterPathingRequest _harvestPathingRequest;

        #endregion Static

        #region Methods

        public bool ValidateHarvestable(HarvestableObjectView resource)
        {
            if (!resource.CanLoot(_localPlayerCharacterView) || resource.GetCurrentCharges() <= 0 || resource.GetTier() < _minimumHarvestableTier)
                return false;

            var position = resource.transform.position;
            var validHeight = _landscape.GetLandscapeHeight(position.c());

            if (position.y < validHeight - 5)
                return false;

            if (_blacklist.ContainsKey(resource))
                return false;

            return true;
        }

        public bool ValidateMob(MobView mob)
        {
            if (mob.IsDead())
                return false;

            var mobAttackTarget = mob.GetAttackTarget();
            if (mobAttackTarget != null && mobAttackTarget != _localPlayerCharacterView)
                return false;

            return true;
        }

        public bool ValidateTarget(SimulationObjectView target)
        {
            if (target is HarvestableObjectView resource)
                return ValidateHarvestable(resource);

            if (target is MobView mob)
                return ValidateMob(mob);

            return false;
        }

        public bool HandleMounting(Vector3 target)
        {
            if (!_localPlayerCharacterView.IsMounted)
            {
                if (_localPlayerCharacterView.IsMounting())
                    return false;

                if (_localPlayerCharacterView.GetMount(out MountObjectView mount))
                {
                    if (target != Vector3.zero && mount.InRange(target))
                        return true;

                    if (mount.IsInUseRange(_localPlayerCharacterView.LocalPlayerCharacter))
                        _localPlayerCharacterView.Interact(mount);
                    else
                        _localPlayerCharacterView.MountOrDismount();
                }
                else
                {
                    _localPlayerCharacterView.MountOrDismount();
                }

                return false;
            }

            return true;
        }

        public void Harvest()
        {
            if (_localPlayerCharacterView.IsUnderAttack(out FightingObjectView attacker))
            {
                _localPlayerCharacterView.CreateTextEffect("[Attacked]");
                _state.Fire(Trigger.EncounteredAttacker);
                return;
            }

            if (!ValidateTarget(_currentTarget))
            {
                _state.Fire(Trigger.DepletedResource);
                return;
            }

            var mob = _currentTarget as MobView;
            var resource = _currentTarget as HarvestableObjectView;

            /* Begin moving closer the target. */
            var targetCenter = _currentTarget.transform.position;
            var playerCenter = _localPlayerCharacterView.transform.position;

            var centerDistance = (targetCenter - playerCenter).magnitude;
            var isNotInLoS = mob != null ? _localPlayerCharacterView.IsInLineOfSight(mob) : false;

            if (_harvestPathingRequest != null)
            {
                if (mob != null && centerDistance <= _minimumAttackableRange && !isNotInLoS)
                {
                    _harvestPathingRequest = null;
                    return;
                }

                if (_harvestPathingRequest.IsRunning)
                {
                    if (!HandleMounting(Vector3.zero))
                        return;

                    _harvestPathingRequest.Continue();
                }
                else
                {
                    _harvestPathingRequest = null;
                }

                return;
            }

            var minimumDistance = mob != null ? _minimumAttackableRange : _currentTarget.GetColliderExtents() + _localPlayerCharacterView.GetColliderExtents() + 1.5f;

            if (centerDistance > minimumDistance || isNotInLoS)
            {
                if (!HandleMounting(targetCenter))
                    return;

                if (_localPlayerCharacterView.TryFindPath(new ClusterPathfinder(), targetCenter, IsBlocked, out List<Vector3> pathing))
                    _harvestPathingRequest = new ClusterPathingRequest(_localPlayerCharacterView, _currentTarget, pathing);
                else
                    _state.Fire(Trigger.DepletedResource);

                return;
            }

            if (resource != null)
            {
                if (_localPlayerCharacterView.IsHarvesting())
                    return;

                if (resource.GetCurrentCharges() <= 0)
                {
                    _state.Fire(Trigger.DepletedResource);
                    return;
                }

                _localPlayerCharacterView.CreateTextEffect("[Harvesting]");
                _localPlayerCharacterView.Interact(resource);

                var resourceTypeString = resource.GetResourceType();
                if (resourceTypeString.Contains("_"))
                    resourceTypeString = resourceTypeString.Substring(0, resourceTypeString.IndexOf("_"));

                var resourceType = (ResourceType)Enum.Parse(typeof(ResourceType), resourceTypeString, true);
                var tier = (Tier)resource.GetTier();
                var enchantmentLevel = (EnchantmentLevel)resource.GetRareState();

                var position = resource.transform.position;
                var info = new GatherInformation(resourceType, tier, enchantmentLevel);
                info.HarvestDate = DateTime.UtcNow;

                if (_gatheredSpots.ContainsKey(position))
                    _gatheredSpots[position] = info;
                else
                    _gatheredSpots.Add(position, info);
            }
            else if (mob != null)
            {
                if (_localPlayerCharacterView.IsAttacking())
                    return;

                if (mob.IsDead() && mob.DeadAnimationFinished)
                {
                    _localPlayerCharacterView.CreateTextEffect("[Mob Dead]");
                    _state.Fire(Trigger.DepletedResource);
                    return;
                }

                _localPlayerCharacterView.CreateTextEffect("[Attacking]");
                if (_localPlayerCharacterView.IsMounted)
                    _localPlayerCharacterView.MountOrDismount();
                _localPlayerCharacterView.SetSelectedObject(mob);
                _localPlayerCharacterView.AttackSelectedObject();
            }
        }

        #endregion Methods
    }
}
