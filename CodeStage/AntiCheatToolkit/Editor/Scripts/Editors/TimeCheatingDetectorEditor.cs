#if UNITY_EDITOR
using CodeStage.AntiCheat.Detectors;
using UnityEditor;
using UnityEngine;

namespace CodeStage.AntiCheat.EditorCode.Editors
{
	[CustomEditor(typeof (TimeCheatingDetector))]
	internal class TimeCheatingDetectorEditor : ActDetectorEditor
	{
		private SerializedProperty interval;
		private SerializedProperty threshold;

		protected override void FindUniqueDetectorProperties()
		{
			interval = serializedObject.FindProperty("interval");
			threshold = serializedObject.FindProperty("threshold");
		}

		protected override void DrawUniqueDetectorProperties()
		{
			EditorGUILayout.PropertyField(interval);
			EditorGUILayout.PropertyField(threshold);

			GUILayout.Label("<b>Needs Internet connection!</b>", ActEditorGUI.RichMiniLabel);
		}
	}
}
#endif