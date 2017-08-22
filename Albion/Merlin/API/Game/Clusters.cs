﻿using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Albion.Common.GameData.WorldInfos;

namespace Merlin.API
{
	public class Cluster
	{
		#region Static

		#endregion

		#region Fields

		private ake _cluster;

		#endregion

		#region Properties and Events

		public ClusterTypes ClusterType => _cluster.ao().ClusterType;
		public Biome Biome => _cluster.ao().Biome;
		public int Tier => _cluster.ao().Tier;
		public Continents Continent => _cluster.ao().Continent;
		public ClusterQualities ClusterQuality => _cluster.ao().ClusterQuality;
		public Faction Faction => _cluster.ao().Faction;

		public string Name => _cluster.an();
		public string InternalName => _cluster.ak();

		public iz.PvpRules PvPRules => _cluster.aq().ap();
		public iz.UiPvpTypes UiPvPRules => _cluster.aq().a0();

		public ake Internal => _cluster;

		#endregion

		#region Constructors and Cleanup
		
		public Cluster(ake cluster)
		{
			_cluster = cluster;
		}

		#endregion

		#region Methods

		public List<ClusterExit> GetExits()
		{
			var list = new List<ClusterExit>();

			foreach (var exit in _cluster.a1())
				list.Add(new ClusterExit(exit));

			return list;
		}

		#endregion
	}

	public class ClusterExit
	{
		#region Static

		#endregion

		#region Fields

		private akf _clusterExit;

		#endregion

		#region Properties and Events

		public Cluster Origin => new Cluster(_clusterExit.l());
		public Cluster Destination => new Cluster(_clusterExit.o());

		public bool IsRestricted => _clusterExit.u();

		public akf.Kind Kind => _clusterExit.r();

		public akf Internal => _clusterExit;

		#endregion

		#region Constructors and Cleanup
		
		public ClusterExit(akf clusterExit)
		{
			_clusterExit = clusterExit;
		}

		#endregion

		#region Methods

		#endregion
	}
}