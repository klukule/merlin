using Merlin.API;
using Merlin.Pathing.Worldmap;
using System.Collections.Generic;
using UnityEngine;
using WorldMap;

namespace Merlin.Profiles.Gatherer
{
    public partial class Gatherer
    {
        #region Fields

        private WorldmapCluster _targetCluster;
        private WorldPathingRequest _worldPathingRequest;

        #endregion Fields

        #region Methods

        public void Travel()
        {
            if (!HandleMounting(Vector3.zero))
                return;

            if (_worldPathingRequest != null)
            {
                if (_worldPathingRequest.IsRunning)
                {
                    if (!HandleMounting(Vector3.zero))
                        return;

                    _worldPathingRequest.Continue();
                }
                else
                {
                    _worldPathingRequest = null;
                }
                return;
            }

            var currentCluster = new Cluster(_world.CurrentCluster.Info);
            var targetCluster = new Cluster(_targetCluster.Info);

            if (currentCluster.Name == targetCluster.Name)
            {
                Core.Log("[Traveling Done]");
                _state.Fire(Trigger.TravellingDone);
                return;
            }
            else
            {
                var path = new List<WorldmapCluster>();
                var pivotPoints = new List<WorldmapCluster>();

                var worldPathing = new WorldmapPathfinder();

                if (worldPathing.TryFindPath(_world.CurrentCluster, _targetCluster, StopClusterFunction, out path, out pivotPoints, true, false))
                {
                    Core.Log("[Traveling Path found]");
                    _worldPathingRequest = new WorldPathingRequest(_world.CurrentCluster, _targetCluster, path);
                }
                else
                    Core.Log("[No Traveling Path found]");
                return;
            }
        }

        public bool StopClusterFunction(WorldmapCluster cluster)
        {
            var clusterObj = new Cluster(cluster.Info);
            if (clusterObj.UiPvPRules == iz.UiPvpTypes.Full || clusterObj.UiPvPRules == iz.UiPvpTypes.Black)
                return true;

            return false;
        }

        #endregion Methods
    }
}