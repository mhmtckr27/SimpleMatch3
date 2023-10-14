using System;
using UnityEditor;
using UnityEngine;
using Util.Extensions;
using Type = System.Type;

namespace SimpleMatch3.Utils.Editor
{
    [CustomPropertyDrawer(typeof(InspectableType<>), true)]
    public class InspectableTypeDrawer : PropertyDrawer
    {
        private Type[] _derivedTypes;
        private GUIContent[] _optionLabels;
        private int _selectedIndex;
        private SerializedProperty _qualifiedNameProperty;
        private SerializedProperty _baseTypeProperty;
        private Type _baseType;
        private GUIContent _propLabel;
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            _qualifiedNameProperty = property.FindPropertyRelative("qualifiedName");

            if (_optionLabels == null)
            {
                Initialize(property);
            }

            _propLabel = EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();

            if (_optionLabels == null || _optionLabels.Length == 0)
                return;

            _selectedIndex = EditorGUI.Popup(position, _propLabel, _selectedIndex, _optionLabels);
            _selectedIndex = Math.Clamp(_selectedIndex, 0, _optionLabels?.Length - 1 ?? _selectedIndex);
            

            if(EditorGUI.EndChangeCheck())
            {
                _qualifiedNameProperty.stringValue = _selectedIndex < _derivedTypes.Length
                    ? _derivedTypes[_selectedIndex].AssemblyQualifiedName
                    : "";
            }

            EditorGUI.EndProperty();
        }

        // private string FormatTypeName(string typeName)
        // {
        //     var index = typeName.IndexOf("`1", StringComparison.Ordinal);
        //
        //     if (index != -1)
        //         typeName = typeName.Remove(index, 2);
        //
        //     index = typeName.IndexOf(FileHandlersPrefix, StringComparison.Ordinal);
        //
        //     if (index != -1)
        //         typeName = typeName.Remove(index, FileHandlersPrefix.Length);
        //
        //     return typeName;
        // }

        private void Initialize(SerializedProperty property)
        {
            _baseTypeProperty = property.FindPropertyRelative("baseTypeName");
            _baseType = Type.GetType(_baseTypeProperty.stringValue);
            Debug.LogError(_baseType);
            _derivedTypes = _baseType.GetDerivedTypes();

            if (_derivedTypes.Length == 0)
            {
                _optionLabels = new[] { new GUIContent($"No types derived from {_baseType?.Name} found.") };
                return;
            }

            _optionLabels = new GUIContent[_derivedTypes.Length];
            for (int i = 0; i < _derivedTypes.Length; i++)
            {
                _optionLabels[i] = new GUIContent((_derivedTypes[i].Name));
            }
        }
    }
}