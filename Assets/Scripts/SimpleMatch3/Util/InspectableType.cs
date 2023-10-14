using System;
using UnityEngine;

namespace SimpleMatch3.Utils
{
    [Serializable]
    public class InspectableType<T> : ISerializationCallbackReceiver {

        [SerializeField]
        private string qualifiedName;

        private Type _storedType;

#if UNITY_EDITOR
        [SerializeField] string baseTypeName;
#endif
        
        public InspectableType(Type typeToStore) 
        {
            _storedType = typeToStore;
        }

        public override string ToString() 
        {
            if (_storedType == null) return string.Empty;
            return _storedType.Name;
        }

        public void OnBeforeSerialize()
        {
            if(_storedType == null)
                return;
            
            qualifiedName = _storedType.AssemblyQualifiedName;
            
#if UNITY_EDITOR
            baseTypeName = typeof(T).GetGenericTypeDefinition().AssemblyQualifiedName;
#endif
        }

        public void OnAfterDeserialize()
        {
            _storedType = string.IsNullOrEmpty(qualifiedName) ? null : Type.GetType(qualifiedName);
        }

        public static implicit operator Type(InspectableType<T> t) => t._storedType;
        public static implicit operator InspectableType<T>(Type t) => new(t);
    }
}