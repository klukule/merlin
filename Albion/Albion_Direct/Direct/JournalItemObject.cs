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
    /* Internal type: atx */
    public partial class JournalItemObject : DurableItemObject
    {
        private static List<MethodInfo> _methodReflectionPool = new List<MethodInfo>();
        private static List<PropertyInfo> _propertyReflectionPool = new List<PropertyInfo>();
        private static List<FieldInfo> _fieldReflectionPool = new List<FieldInfo>();
        
        private atx _internal;
        
        #region Properties
        
        public atx JournalItemObject_Internal => _internal;
        
        #endregion
        
        #region Fields
        
        
        #endregion
        
        #region Methods
        
        
        #endregion
        
        #region Constructor
        
        public JournalItemObject(atx instance) : base(instance)
        {
            _internal = instance;
        }
        
        static JournalItemObject()
        {
            
        }
        
        #endregion
        
        #region Conversion
        
        public static implicit operator atx(JournalItemObject instance)
        {
            return instance._internal;
        }
        
        public static implicit operator JournalItemObject(atx instance)
        {
            return new JournalItemObject(instance);
        }
        
        public static implicit operator bool(JournalItemObject instance)
        {
            return instance._internal != null;
        }
        #endregion
    }
}
