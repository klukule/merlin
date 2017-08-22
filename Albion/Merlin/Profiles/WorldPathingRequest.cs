using Merlin.API;
using Stateless;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WorldMap;
using YinYang.CodeProject.Projects.SimplePathfinding.Helpers;
using YinYang.CodeProject.Projects.SimplePathfinding.PathFinders.AStar;

namespace Merlin.Profiles
{
    public class WorldPathingRequest
    {
        #region Fields

        private Client _client;
        private World _world;

        private WorldmapCluster _origin;
        private WorldmapCluster _destination;

        private List<WorldmapCluster> _path;

        private StateMachine<State, Trigger> _state;

        private PositionPathingRequest _exitPathingRequest;

        #endregion Fields

        #region Properties and Events

        public bool IsRunning => _state.State != State.Finish;

        #endregion Properties and Events

        #region Constructors and Cleanup

        public WorldPathingRequest(WorldmapCluster start, WorldmapCluster end, List<WorldmapCluster> path)
        {
            _client = Client.Instance;
            _world = World.Instance;

            _origin = start;
            _destination = end;

            _path = path;

            _state = new StateMachine<State, Trigger>(State.Start);

            _state.Configure(State.Start)
                .Permit(Trigger.ApproachDestination, State.Running);

            _state.Configure(State.Running)
                .Permit(Trigger.ReachedDestination, State.Finish);
        }

        #endregion Constructors and Cleanup

        #region Methods

        public void Continue()
        {
            switch (_state.State)
            {
                case State.Start:
                    {
                        if (_path.Count > 0)
                            _state.Fire(Trigger.ApproachDestination);
                        else
                            _state.Fire(Trigger.ReachedDestination);

                        break;
                    }

                case State.Running:
                    {
                        var nextCluster = _path[0];

                        if (_world.CurrentCluster != nextCluster)
                        {
                            if (_exitPathingRequest != null)
                            {
                                if (_exitPathingRequest.IsRunning)
                                {
                                    _exitPathingRequest.Continue();
                                }
                                else
                                {
                                    _exitPathingRequest = null;
                                }

                                break;
                            }

                            var player = _client.LocalPlayerCharacter;
                            var exits = _client.CurrentCluster.GetExits();

                            var exit = exits.FirstOrDefault(e => e.Destination.Internal == nextCluster.Info);
                            var exitLocation = exit.Internal.v();

                            var destination = new Vector3(exitLocation.g(), 0, exitLocation.h());

                            var isCity = Enum.GetNames(typeof(TownClusterName)).Select(n => n.Replace("_", " ")).ToArray().Contains(new Cluster(_world.CurrentCluster.Info).Name);
                            if (player.TryFindPath(new ClusterPathfinder(), destination, isCity ? (StopFunction<Vector2>)IsBlockedCity : IsBlocked, out List<Vector3> pathing))
                                _exitPathingRequest = new PositionPathingRequest(_client.LocalPlayerCharacter, destination, pathing, false);
                        }
                        else
                        {
                            _path.RemoveAt(0);
                            _exitPathingRequest = null;
                        }

                        if (_path.Count > 0)
                            break;

                        _state.Fire(Trigger.ReachedDestination);
                        break;
                    }
            }
        }

        public bool IsBlocked(Vector2 location)
        {
            return IsBlocked(new Vector3(location.x, 0, location.y));
        }

        public bool IsBlocked(Vector3 location)
        {
            var flag = _client.Collision.GetFlag(location, 1.2f);
            return flag > 0;
        }

        public bool IsBlockedCity(Vector2 location)
        {
            return IsBlockedCity(new Vector3(location.x, 0, location.y));
        }

        public bool IsBlockedCity(Vector3 location)
        {
            var flag = _client.Collision.GetFlag(location, 1.2f);
            return flag > 0 && flag < 255;
        }

        #endregion Methods

        private enum Trigger
        {
            ApproachDestination,
            ReachedDestination,
        }

        private enum State
        {
            Start,
            Running,
            Finish
        }
    }
}