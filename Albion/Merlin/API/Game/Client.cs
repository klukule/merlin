﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEngine;

namespace Merlin.API
{
	/* Internal Type: a6o */
	public class Client
	{
		#region Static

		public static Client Instance
		{
			get
			{
				var internalClient = a6o.s();

				if (internalClient != null)
					return new Client(internalClient);

				return default(Client);
			}
		} 

		#endregion

		#region Fields

		private readonly a6o _client;

		private readonly World _world;
		private readonly Collision _collision;

		#endregion

		#region Properties and Events

		public GameState State => (GameState)_client.w();

		public LocalPlayerCharacterView LocalPlayerCharacter => _client.v();

		public Collision Collision => _collision;

		public Cluster CurrentCluster => new Cluster(_world.CurrentCluster.Info);

	    public static float Zoom
        {
            get
            {
                return a6o.s().v() != null ? a6o.s().v().GetComponent<LocalActorCameraController>().Outside.Far.Distance : 0f;
            }
            set
            {
                if (a6o.s().v() != null)
                    a6o.s().v().GetComponent<LocalActorCameraController>().Outside.Far.Distance = value;
            }
        }

		public static bool GlobalFog
        {
            get
            {

                GlobalFog component = Camera.main.GetComponent<GlobalFog>();
                return !(component == null) && (bool)component.GetType().InvokeMember("a", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField, Type.DefaultBinder, component, null);
            }
            set
            {
                GlobalFog component = Camera.main.GetComponent<GlobalFog>();
                if (component == null)
                {
                    return;
                }
                component.GetType().InvokeMember("a", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetField, Type.DefaultBinder, component, new object[]
                {
                    value
                });
                component.GetType().InvokeMember("b", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetField, Type.DefaultBinder, component, new object[]
                {
                    value
                });
            }
        }

		#endregion

		#region Constructors and Cleanup

		protected Client(a6o client)
		{
			_client = client;

			_world = World.Instance;
			_collision = Collision.Instance;
		}

		#endregion

		#region Methods

		public SimulationObjectView GetEntity(ark entity) => _client.a(entity);

		public SimulationObjectView GetEntity(long id)
		{
			if (id > 0L)
				return _client.a(id);

			return default(SimulationObjectView);
		}

		/// <summary>
		/// Gets the collection of entities of the specified.
		/// </summary>
		public List<T> GetEntities<T>(Func<T, bool> selector) where T : SimulationObjectView
		{
			var list = new List<T>();

			foreach (var entity in _world.GetEntities().Values)
			{
				if (GetEntity(entity) is T t && selector(t))
					list.Add(t);
			}

			return list;
		}

		#endregion
	}

	public enum GameState
	{
		Unknown,
		LoggingIn,
		Loading,
		Playing
	}
}