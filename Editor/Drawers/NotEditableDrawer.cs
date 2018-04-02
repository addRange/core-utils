//----------------------------------------------------------------------------------------------
// Created by Pavel Totolin (2017.11.28)
//----------------------------------------------------------------------------------------------

using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(NotEditableAttribute), true)]
public class NotEditableDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		bool enabledLast = GUI.enabled;

		GUI.enabled = false;
		EditorGUI.PropertyField(position, property, label);
		GUI.enabled = enabledLast;
	}
}