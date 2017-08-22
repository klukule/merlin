using Stateless;
using System.Collections.Generic;
using UnityEngine;

namespace Merlin.Profiles
{
    public class ClusterPathingRequest
    {
        #region Fields

        private bool _useCollider;

        private LocalPlayerCharacterView _player;
        private SimulationObjectView _target;

        private List<Vector3> _path;

        private StateMachine<State, Trigger> _state;

        #endregion Fields

        #region Properties and Events

        public bool IsRunning => _state.State != State.Finish;

        #endregion Properties and Events

        #region Constructors and Cleanup

        public ClusterPathingRequest(LocalPlayerCharacterView player, SimulationObjectView target, List<Vector3> path, bool useCollider = true)
        {
            _player = player;
            _target = target;

            _player.CreateTextEffect("ClusterPathingRequest : " + target.name);

            _path = path;

            _useCollider = useCollider;

            _state = new StateMachine<State, Trigger>(State.Start);

            _state.Configure(State.Start)
                .Permit(Trigger.ApprachTarget, State.Running);

            _state.Configure(State.Running)
                .Permit(Trigger.ReachedTarget, State.Finish);
        }

        #endregion Constructors and Cleanup

        #region Methods

        #region StuckProtection
        /*** StuckProtection BEGIN ***/
        private static class previousPlayerInfo
        {
            public static float x = 0f;
            public static float z = 0f;
            public static int violationCount = 0;
            public static int violationTolerance = 50;
        }

        private Vector3 StuckProtection()
        {
            if (
                    !_player.IsHarvesting()
                    && !_player.IsAttacking()
                    && _player.IsMounted()
                    && Mathf.Abs(_player.GetPosition().x - previousPlayerInfo.x) < 0.25f
                    && Mathf.Abs(_player.GetPosition().z - previousPlayerInfo.z) < 0.25f
                    && _state.State == State.Running
                )
            {
                previousPlayerInfo.violationCount++;

                if (previousPlayerInfo.violationCount
                        >= previousPlayerInfo.violationTolerance)
                {
                    var unstuckCoordinates = _player.GetPosition();
                    var method = "variable";
                    switch (method)
                    {
                        case "absolute":
                            float[] arrayValues = { -15f, +15f };
                            unstuckCoordinates.x = _player.GetPosition().x + arrayValues[Random.Range(0, arrayValues.Length)];
                            unstuckCoordinates.z = _player.GetPosition().z + arrayValues[Random.Range(0, arrayValues.Length)];
                            break;
                        case "variable":
                            unstuckCoordinates.x = _player.GetPosition().x + Random.Range(-25f, +25f);
                            unstuckCoordinates.z = _player.GetPosition().z + Random.Range(-25f, +25f);
                            break;
                        default:
                            break;
                    }
                    _path[0] = unstuckCoordinates;
                    _state.Fire(Trigger.ReachedTarget);
                    _player.CreateTextEffect("[Stuck detected - Resolving]");
                    previousPlayerInfo.violationCount = 0;
                    Profile.UpdateDelay = System.TimeSpan.FromSeconds(1.0d);
                }
            }
            else
            {
                previousPlayerInfo.violationCount = 0;
            }
            previousPlayerInfo.x = _player.GetPosition().x;
            previousPlayerInfo.z = _player.GetPosition().z;
            return _path[0];
        }
        /*** StuckProtection END ***/
        #endregion StuckProtection

        public void Continue()
        {
            Profile.UpdateDelay = System.TimeSpan.FromSeconds(0.1d); /*** StuckProtection Line ***/

            switch (_state.State)
            {
                case State.Start:
                    {
                        if (_path.Count > 0)
                            _state.Fire(Trigger.ApprachTarget);
                        else
                            _state.Fire(Trigger.ReachedTarget);

                        break;
                    }

                case State.Running:
                    {
                        //If we leave the current map, both will become null.
                        if (_player == null)
                        {
                            _state.Fire(Trigger.ReachedTarget);
                            break;
                        }

                        var currentNode = _path[0];
                        var minimumDistance = 3f;

                        if (_path.Count < 2 && _useCollider)
                        {
                            minimumDistance = _target.GetColliderExtents() + _player.GetColliderExtents();

                            var directionToPlayer = (_player.transform.position - _target.transform.position).normalized;
                            var bufferDistance = directionToPlayer * minimumDistance;

                            currentNode = _target.transform.position + bufferDistance;
                        }

                        var distanceToNode = (_player.transform.position - currentNode).sqrMagnitude;

                        if (distanceToNode < minimumDistance)
                        {
                            _path.RemoveAt(0);
                        }
                        else
                        {
                            _player.RequestMove(StuckProtection()); /*** StuckProtection Line ***/
                        }

                        if (_path.Count > 0)
                            break;

                        _state.Fire(Trigger.ReachedTarget);
                        break;
                    }
            }
        }

        #endregion Methods

        private enum Trigger
        {
            ApprachTarget,
            ReachedTarget,
        }

        private enum State
        {
            Start,
            Running,
            Finish
        }
    }
}