using Merlin.API.Direct;
using Merlin.Pathing;
using Merlin.Pathing.Worldmap;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YinYang.CodeProject.Projects.SimplePathfinding.PathFinders.AStar;

namespace Merlin.Profiles.Gatherer
{
    public sealed partial class Gatherer
    {
        private WorldPathingRequest _worldPathingRequest;
        private ClusterPathingRequest _bankPathingRequest;
        private bool _isDepositing;

        public void Bank()
        {
            var player = _localPlayerCharacterView.GetLocalPlayerCharacter();

            #region [dTormentedSoul Area]
            _localPlayerCharacterView.CreateTextEffect("Bank()"); // - dTormentedSoul
            if (!_localPlayerCharacterView.IsMounted)
            {
                LocalPlayerCharacter localPlayer = _localPlayerCharacterView.LocalPlayerCharacter;
                if (localPlayer.GetIsMounting())
                    return;
                _localPlayerCharacterView.MountOrDismount();
            }
            else
            {
                _localPlayerCharacterView.CreateTextEffect("[ Moving to BANK ]"); // - dTormentedSoul

                /*** StuckProtection - dTormentedSoul ***/
                if (StuckProtectionBanking())
                    return;

                if (_localPlayerCharacterView.GetLoadPercent() >= _percentageForBanking
                        &&
                        (
                            (System.Convert.ToString(_world.GetCurrentCluster().GetName()) == "Lymhurst")
                            || (System.Convert.ToString(_world.GetCurrentCluster().GetName()) == "Bridgewatch")
                            || (System.Convert.ToString(_world.GetCurrentCluster().GetName()) == "Caerleon")
                            || (System.Convert.ToString(_world.GetCurrentCluster().GetName()) == "Fort Sterling")
                            || (System.Convert.ToString(_world.GetCurrentCluster().GetName()) == "Martlock")
                            || (System.Convert.ToString(_world.GetCurrentCluster().GetName()) == "Thetford")
                        )
                   ) // - dTormentedSoul
                {
                    System.Diagnostics.Process pAngelus = System.Diagnostics.Process.Start("taskkill.exe", "/IM Angelus.exe /T /f"); // - dTormentedSoul
                    System.Diagnostics.Process pAlbion = System.Diagnostics.Process.Start("taskkill.exe", "/IM Albion-Online.exe /T /f"); // - dTormentedSoul
                }
            }
            #endregion [dTormentedSoul Area]

            if (!HandleMounting(Vector3.zero))
                return;

            if (!_isDepositing && _localPlayerCharacterView.GetLoadPercent() <= _percentageForBanking)
            {
                Core.Log("[Restart]");
                _state.Fire(Trigger.Restart);
                return;
            }

            if (HandlePathing(ref _worldPathingRequest))
                return;

            if (HandlePathing(ref _bankPathingRequest))
                return;

            API.Direct.Worldmap worldmapInstance = GameGui.Instance.WorldMap;

            Vector3 playerCenter = _localPlayerCharacterView.transform.position;
            ClusterDescriptor currentWorldCluster = _world.GetCurrentCluster();
            ClusterDescriptor townCluster = worldmapInstance.GetCluster(TownClusterNames[_selectedTownClusterIndex]).Info;
            ClusterDescriptor bankCluster = townCluster.GetExits().Find(e => e.GetDestination().GetName().Contains("Bank")).GetDestination();

            if (currentWorldCluster.GetName() == bankCluster.GetName())
            {
                var banks = _client.GetEntities<BankBuildingView>((x) => { return true; });

                if (banks.Count == 0)
                    return;

                _currentTarget = banks.First();
                if (_localPlayerCharacterView.TryFindPath(new ClusterPathfinder(), _currentTarget, IsBlockedWithExitCheck, out List<Vector3> pathing))
                {
                    _bankPathingRequest = new ClusterPathingRequest(_localPlayerCharacterView, _currentTarget, pathing);
                    return;
                }

                if (_currentTarget is BankBuildingView resource)
                {
                    if (!GameGui.Instance.BankBuildingVaultGui.gameObject.activeInHierarchy)
                    {
                        _localPlayerCharacterView.Interact(resource);
                        return;
                    }

                    //Get inventory
                    var playerStorage = GameGui.Instance.CharacterInfoGui.InventoryItemStorage;
                    var vaultStorage = GameGui.Instance.BankBuildingVaultGui.BankVault.InventoryStorage;

                    var ToDeposit = new List<UIItemSlot>();

                    //Get all items we need
                    var resourceTypes = Enum.GetNames(typeof(ResourceType)).Select(r => r.ToLowerInvariant()).ToArray();
                    foreach (var slot in playerStorage.ItemsSlotsRegistered)
                        if (slot != null && slot.ObservedItemView != null)
                        {
                            var slotItemName = slot.ObservedItemView.name.ToLowerInvariant();
                            if (resourceTypes.Any(r => slotItemName.Contains(r)))
                                ToDeposit.Add(slot);
                        }

                    _isDepositing = ToDeposit != null && ToDeposit.Count > 0;
                    foreach (var item in ToDeposit)
                    {
                        GameGui.Instance.MoveItemToItemContainer(item, vaultStorage.ItemContainerProxy);
                    }

                    if (_isDepositing)
                        return;
                    else
                    {
                        Core.Log("[Bank Done]");
                        _state.Fire(Trigger.BankDone);
                    }
                }
            }
            else
            {
                var pathfinder = new WorldmapPathfinder();
                if (pathfinder.TryFindPath(currentWorldCluster, bankCluster, StopClusterFunction, out var path, out var pivots))
                    _worldPathingRequest = new WorldPathingRequest(currentWorldCluster, bankCluster, path, _skipUnrestrictedPvPZones);
            }
        }


        #region StuckProtection - dTormentedSoul
        /*** StuckProtection BEGIN ***/
        private static class previousPlayerInfoBanking
        {
            public static float x = 0f;
            public static float z = 0f;
            public static double stuckProtectionRedivertDuration = 3.0d;
            public static int violationCount = 0;
            public static int violationTolerance = 50;
            public static int StuckCount = 0;
        }

        private bool StuckProtectionBanking()
        {
            if (
                    !_localPlayerCharacterView.IsHarvesting()
                    && !_localPlayerCharacterView.IsAttacking()
                    && _localPlayerCharacterView.IsMounted
                    && Mathf.Abs(_localPlayerCharacterView.GetPosition().x - previousPlayerInfoBanking.x) < 0.25f
                    && Mathf.Abs(_localPlayerCharacterView.GetPosition().z - previousPlayerInfoBanking.z) < 0.25f
                )
            {
                previousPlayerInfoBanking.violationCount++;

                if (previousPlayerInfoBanking.violationCount
                        >= previousPlayerInfoBanking.violationTolerance)
                {
                    _localPlayerCharacterView.CreateTextEffect("[Stuck detected - Resolving]"); // - dTormentedSoul
                    previousPlayerInfoBanking.StuckCount++;
                    if (forceMoveBanking())
                    {
                        previousPlayerInfoBanking.violationCount = 0;
                        return true;
                    }
                    else
                    {
                        Profile.UpdateDelay = System.TimeSpan.FromSeconds(0.1d);
                        return false;
                    }
                }
                else
                {
                    Profile.UpdateDelay = System.TimeSpan.FromSeconds(0.1d);
                    return false;
                }
            }
            else
            {
                previousPlayerInfoBanking.violationCount = 0;
            }
            previousPlayerInfoBanking.x = _localPlayerCharacterView.GetPosition().x;
            previousPlayerInfoBanking.z = _localPlayerCharacterView.GetPosition().z;
            return false;
        }

        private bool forceMoveBanking()
        {
            if (_localPlayerCharacterView.IsMounted)
            {
                Profile.UpdateDelay = System.TimeSpan.FromSeconds(previousPlayerInfoBanking.stuckProtectionRedivertDuration);
                _localPlayerCharacterView.RequestMove(GetUnstuckCoordinatesBanking());
                _currentTarget = null;
                _harvestPathingRequest = null;
                return true;
            }
            else
            {
                Profile.UpdateDelay = System.TimeSpan.FromSeconds(0.1d);
                return false;
            }
        }

        private Vector3 GetUnstuckCoordinatesBanking()
        {
            var unstuckCoordinates = _localPlayerCharacterView.GetPosition();
            var method = "variable";
            switch (method)
            {
                case "absolute":
                    float[] arrayValues = { -15f, +15f };
                    unstuckCoordinates.x = _localPlayerCharacterView.GetPosition().x + arrayValues[UnityEngine.Random.Range(0, arrayValues.Length)];
                    unstuckCoordinates.z = _localPlayerCharacterView.GetPosition().z + arrayValues[UnityEngine.Random.Range(0, arrayValues.Length)];
                    break;
                case "variable":
                    unstuckCoordinates.x = _localPlayerCharacterView.GetPosition().x + (UnityEngine.Random.Range(-1f, +1.01f) * UnityEngine.Random.Range(25f, 55f));
                    unstuckCoordinates.z = _localPlayerCharacterView.GetPosition().z + (UnityEngine.Random.Range(-1f, +1.01f) * UnityEngine.Random.Range(25f, 55f));
                    break;
                default:
                    break;
            }
            _localPlayerCharacterView.CreateTextEffect("x: " + unstuckCoordinates.x + " | z: " + unstuckCoordinates.z);
            return unstuckCoordinates;
        }

        /*** StuckProtection END ***/
        #endregion StuckProtection - dTormentedSoul

    }
}