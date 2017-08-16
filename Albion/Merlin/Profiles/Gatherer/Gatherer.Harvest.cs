﻿using System.Collections.Generic;
using UnityEngine;
using Merlin.API.Direct;
using YinYang.CodeProject.Projects.SimplePathfinding.PathFinders.AStar;

namespace Merlin.Profiles.Gatherer
{
    public partial class Gatherer
    {
        #region Static

        private static int _minimumHarvestableTier = 2;

        private ClusterPathingRequest _harvestPathingRequest;

        #endregion Static

        #region Methods

        public bool ValidateHarvestable(HarvestableObjectView resource)
        {
            if (!resource.GetHarvestableObject().IsHarvestable() || 
                 resource.GetHarvestableObject().GetCharges() <= 0 || 
                 resource.GetHarvestableObject().GetResourceDescriptor().Tier < _minimumHarvestableTier)
                return false;

            var position = resource.transform.position;
            var validHeight = _landscape.GetTerrainHeight(position.c(), out RaycastHit A_1);

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
                // if (_localPlayerCharacterView.GetCharacter().())
                //     return false;
                if (_localPlayerCharacterView.GetCharacter().GetPlayerCharacter().GetIsMounted())
                    return false;

                MountObjectView mount = _localPlayerCharacterView.GetComponent<MountObjectView>();

                if (mount != null)
                {
                    if (target != Vector3.zero && mount.IsInUseRange(_localPlayerCharacterView.GetCharacter().Internal))
                        return true;

                    if (mount.IsInUseRange(_localPlayerCharacterView.GetCharacter().Internal))
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

            if (_harvestPathingRequest != null)
            {
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

            /* Begin moving closer the target. */
            var targetCenter = _currentTarget.transform.position;
            var playerCenter = _localPlayerCharacterView.transform.position;

            var centerDistance = (targetCenter - playerCenter).magnitude;
            var minimumDistance = _currentTarget.GetColliderExtents() + _localPlayerCharacterView.GetColliderExtents() + 1.5f;

            if (centerDistance >= minimumDistance)
            {
                if (!HandleMounting(targetCenter))
                    return;

               /* if (_localPlayerCharacterView.TryFindPath(new ClusterPathfinder(), targetCenter, IsBlocked, out List<Vector3> pathing))
                    _harvestPathingRequest = new ClusterPathingRequest(_localPlayerCharacterView, _currentTarget, pathing);
                else
                {
                    _state.Fire(Trigger.DepletedResource);
                }
                */

                return;
            }

            if (_currentTarget is HarvestableObjectView resource)
            {
                if (_localPlayerCharacterView.IsHarvesting())
                    return;

                if (resource.GetHarvestableObject().GetCharges() <= 0)
                {
                    _state.Fire(Trigger.DepletedResource);
                    return;
                }

                _localPlayerCharacterView.CreateTextEffect("[Harvesting]");
                _localPlayerCharacterView.Interact(resource);
            }
        }

        #endregion Methods
    }
}
