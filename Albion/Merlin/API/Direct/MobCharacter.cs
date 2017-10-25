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
    /* Internal type: au8 */
    public partial class MobCharacter : FightingObject
    {
        private static List<MethodInfo> _methodReflectionPool = new List<MethodInfo>();
        private static List<PropertyInfo> _propertyReflectionPool = new List<PropertyInfo>();
        private static List<FieldInfo> _fieldReflectionPool = new List<FieldInfo>();
        
        private au8 _internal;
        
        #region Properties
        
        public au8 MobCharacter_Internal => _internal;
        
        #endregion
        
        #region Fields
        
        
        #endregion
        
        #region Methods
        
        public MobCharacterDescriptor GetMobDescriptor() => _internal.s1();
        
        #endregion
        
        #region Constructor
        
        public MobCharacter(au8 instance) : base(instance)
        {
            _internal = instance;
        }
        
        static MobCharacter()
        {
            
        }
        
        #endregion
        
        #region Conversion
        
        public static implicit operator au8(MobCharacter instance)
        {
            return instance._internal;
        }
        
        public static implicit operator MobCharacter(au8 instance)
        {
            return new MobCharacter(instance);
        }
        
        public static implicit operator bool(MobCharacter instance)
        {
            return instance._internal != null;
        }
        #endregion
    }
}
