using System.Collections.Generic;

using UnityEngine;

namespace Merlin.API
{
    public class Landscape
	{
		#region Static

		public static Landscape Instance
		{
			get
			{
				var internalLandscape = a6o.s().z();

				if (internalLandscape != null)
					return new Landscape(internalLandscape);

				return default(Landscape);
			}
		} 

		#endregion

		#region Fields

		#endregion

		#region Properties and Events

		private a6l _landscape;

		#endregion

		#region Constructors and Cleanup

		protected Landscape(a6l landscape)
		{
			_landscape = landscape;
		}

		#endregion

		#region Methods

		public float GetLandscapeHeight(ajg position)
		{
			return _landscape.d(position);
		}

		public List<aea> GetUnrestrictedPvPZones()
		{
			return _landscape.f().e;
		}

        public bool IsInAnyUnrestrictedPvPZone(Vector3 pos)
        {
            foreach (var pvpZone in GetUnrestrictedPvPZones())
                if (Mathf.Pow(pos.x - pvpZone.k(), 2) + Mathf.Pow(pos.z - pvpZone.l(), 2) < Mathf.Pow(pvpZone.m(), 2))
                    return true;

            return false;
        }

		#endregion
	}
}