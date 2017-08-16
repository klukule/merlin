using Merlin.API.Direct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Merlin
{
    public static class SimulationObjectExtensions
    {
        public static SimulationObjectView GetView(this SimulationObject instance)
        {
            return GameManager.GetInstance().GetView(instance);
        }

        public static float GetColliderExtents(this SimulationObject instance)
        {
            var view = instance.GetView();

            if (view is HarvestableObjectView resource)
                return 2.0f;

            var collider = view.GetComponent<Collider>();

            if (collider is SphereCollider sphere)
                return sphere.radius;
            else if (collider is CapsuleCollider capsule)
                return capsule.radius;
            else if (collider is BoxCollider box)
                return box.size.sqrMagnitude;

            return 1.0f;
        }

        public static bool ColliderContains(this SimulationObject instance, Vector3 location)
        {
            var view = instance.GetView();

            var collider = view.GetComponent<Collider>();
            var bounds = collider.bounds;

            if (bounds.Contains(location))
                return true;

            return false;
        }
    }
}
