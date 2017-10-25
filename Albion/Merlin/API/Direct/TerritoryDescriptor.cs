////////////////////////////////////////////////////////////////////////////////////
// Merlin API for Albion Online v1.0.336.100246-prod
////////////////////////////////////////////////////////////////////////////////////
//------------------------------------------------------------------------------
// <auto-generated>
// This code was generated by a tool.
//
// Changes to this file may cause incorrect behavior and will be lost if
// the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

using UnityEngine;

using Albion.Common.Time;

namespace Merlin.API.Direct
{
    /* Internal type: al1 */
    public partial class TerritoryDescriptor
    {
        private static List<MethodInfo> _methodReflectionPool = new List<MethodInfo>();
        private static List<PropertyInfo> _propertyReflectionPool = new List<PropertyInfo>();
        private static List<FieldInfo> _fieldReflectionPool = new List<FieldInfo>();
        
        private al1 _internal;
        
        #region Properties
        
        public al1 TerritoryDescriptor_Internal => _internal;
        
        #endregion
        
        #region Fields
        
        
        #endregion
        
        #region Methods
        
        public ClusterDescriptor GetCluster() => _internal.p();
        public string GetName() => _internal.o();
        public Point2 GetSize() => _internal.t();
        public TerritoryType GetTerritoryType() => _internal.v().ToWrapped();
        
        #endregion
        
        #region Constructor
        
        public TerritoryDescriptor(al1 instance)
        {
            _internal = instance;
        }
        
        static TerritoryDescriptor()
        {
            
        }
        
        #endregion
        
        #region Conversion
        
        public static implicit operator al1(TerritoryDescriptor instance)
        {
            return instance._internal;
        }
        
        public static implicit operator TerritoryDescriptor(al1 instance)
        {
            return new TerritoryDescriptor(instance);
        }
        
        public static implicit operator bool(TerritoryDescriptor instance)
        {
            return instance._internal != null;
        }
        #endregion
    }
}
