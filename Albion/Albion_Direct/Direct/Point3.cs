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

using System.Collections.Generic;
using System.Reflection;

namespace Albion_Direct
{
    /* Internal type: afo */
    public partial struct Point3
    {
        private static List<MethodInfo> _methodReflectionPool = new List<MethodInfo>();
        private static List<PropertyInfo> _propertyReflectionPool = new List<PropertyInfo>();
        
        private afo _internal;
        
        #region Properties
        
        public afo Point3_Internal => _internal;
        
        #endregion
        
        #region Fields
        
        
        #endregion
        
        #region Methods
        
        
        #endregion
        
        #region Constructor
        
        public Point3(afo instance)
        {
            _internal = instance;
        }
        
        static Point3()
        {
            
        }
        
        #endregion
        
        #region Conversion
        
        public static implicit operator afo(Point3 instance)
        {
            return instance._internal;
        }
        
        public static implicit operator Point3(afo instance)
        {
            return new Point3(instance);
        }
        #endregion
    }
}