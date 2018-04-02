// http://wiki.unity3d.com/index.php/EnumFlagPropertyDrawer

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(EnumFlagAttribute))]
public class EnumFlagDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		property.intValue = EditorGUI.MaskField(position, label, property.intValue, property.enumNames);
	}
}