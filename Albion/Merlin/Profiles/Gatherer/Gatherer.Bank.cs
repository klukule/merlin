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
        #region Static

        public static string BankClusterFormat = "Marketplace";
        public static int CapacityForBanking = 99;

        #endregion Static

        #region Fields

        private PositionPathingRequest _bankEntracePathingRequest;
        private PositionPathingRequest _bankLeavePathingRequest;
        private ClusterPathingRequest _bankPathingRequest;
        private bool _isDepositing;

        #endregion Fields

        #region Methods

        public void Bank()
        {
            if (!HandleMounting(Vector3.zero))
                return;

            if (_bankLeavePathingRequest != null)
            {
                if (_bankLeavePathingRequest.IsRunning)
                {
                    if (!HandleMounting(Vector3.zero))
                        return;

                    _bankLeavePathingRequest.Continue();
                }
                else
                {
                    _bankLeavePathingRequest = null;
                    _isDepositing = false;
                    Core.Log("[Banking Done]");
                    _targetCluster = _world.GetCluster(_selectedGatherCluster);
                    _state.Fire(Trigger.BankDone);
                    return;
                }

                return;
            }

            if (_bankEntracePathingRequest != null)
            {
                if (_bankEntracePathingRequest.IsRunning)
                {
                    if (!HandleMounting(Vector3.zero))
                        return;

                    _bankEntracePathingRequest.Continue();
                }
                else
                {
                    _bankEntracePathingRequest = null;
                }

                return;
            }

            if (_bankPathingRequest != null)
            {
                if (_bankPathingRequest.IsRunning)
                {
                    if (!HandleMounting(Vector3.zero))
                        return;

                    _bankPathingRequest.Continue();
                }
                else
                {
                    _bankPathingRequest = null;
                }

                return;
            }

            if (!_isDepositing && _localPlayerCharacterView.GetLoadPercent() <= CapacityForBanking)
            {
                Core.Log("[Skipping. No Banking needed]");
                _localPlayerCharacterView.CreateTextEffect("[Restart]");
                _state.Fire(Trigger.Restart);
                return;
            }

            Vector3 playerCenter = _localPlayerCharacterView.transform.position;

            var currentCluster = new Cluster(alb.a().u());
            var townClusterObj = _world.GetCluster(SelectedTownCluster);
            var townCluster = new Cluster(townClusterObj.Info);
            var bankCluster = townCluster.GetExits().Find(e => e.Destination.Name.Contains(BankClusterFormat)).Destination;

            if (currentCluster.Name == townCluster.Name)
            {
                var bankExit = FindObjectsOfType<Exit>().First(e => e.name.ToLowerInvariant().Contains("bank"));

                ///* Begin moving closer the target. */
                var targetCenter = bankExit.transform.position;
                playerCenter = _localPlayerCharacterView.transform.position;

                var centerDistance = (targetCenter - playerCenter).magnitude;
                var minimumDistance = _localPlayerCharacterView.GetColliderExtents() + 1.5f;

                if (centerDistance >= minimumDistance)
                {
                    if (!HandleMounting(targetCenter))
                        return;

                    if (_localPlayerCharacterView.TryFindPath(new ClusterPathfinder(), targetCenter, (v) => false,
                        out List<Vector3> pathing))

                        _bankEntracePathingRequest =
                            new PositionPathingRequest(_localPlayerCharacterView, bankExit.transform.position, pathing);

                    return;
                }
            }
            else if (currentCluster.Name == bankCluster.Name)
            {
                var banks = _client.GetEntities<BankBuildingView>((x) => { return true; });

                if (banks.Count == 0)
                    return;

                _currentTarget = banks.First();

                /* Begin moving closer the target. */
                var targetCenter = _currentTarget.transform.position;
                playerCenter = _localPlayerCharacterView.transform.position;

                var centerDistance = (targetCenter - playerCenter).magnitude;
                var minimumDistance = _currentTarget.GetColliderExtents() +
                                      _localPlayerCharacterView.GetColliderExtents() + 1.5f;

                if (centerDistance >= minimumDistance)
                {
                    if (!HandleMounting(targetCenter))
                        return;

                    if (_localPlayerCharacterView.TryFindPath(new ClusterPathfinder(), targetCenter, (v) => false,
                        out List<Vector3> pathing))

                        _bankPathingRequest =
                            new ClusterPathingRequest(_localPlayerCharacterView, _currentTarget, pathing);

                    return;
                }
                //Fixes position, if is slightly invalid and opens UI

                if (_currentTarget is BankBuildingView resource)
                {
                    _localPlayerCharacterView.Interact(resource);
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
                }

                if (_isDepositing)
                    return;

                ///* Begin moving closer the target. */
                var exit = FindObjectsOfType<ExitObjectView>().ToList().Find(e => new Cluster(e.ExitObject.sc().o()).Name == townCluster.Name);

                targetCenter = exit.transform.position;
                playerCenter = _localPlayerCharacterView.transform.position;

                centerDistance = (targetCenter - playerCenter).magnitude;
                minimumDistance = _localPlayerCharacterView.GetColliderExtents() + 1.5f;

                if (centerDistance >= minimumDistance)
                {
                    if (_localPlayerCharacterView.TryFindPath(new ClusterPathfinder(), targetCenter, (v) => false,
                        out List<Vector3> pathing))

                        _bankLeavePathingRequest =
                            new PositionPathingRequest(_localPlayerCharacterView, targetCenter, pathing);

                    return;
                }
            }
            else
            {
                Core.Log("[Start Move To Bank Cluster]");
                _targetCluster = townClusterObj;
                _state.Fire(Trigger.StartTravelling);
                return;
            }
        }

        #endregion Methods
    }
}