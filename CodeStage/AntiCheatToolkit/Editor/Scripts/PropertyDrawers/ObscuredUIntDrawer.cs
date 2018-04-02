#if UNITY_EDITOR
#define UNITY_5_0_PLUS
#if UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9
#undef UNITY_5_0_PLUS
#endif

using CodeStage.AntiCheat.ObscuredTypes;
using UnityEditor;
using UnityEngine;

namespace CodeStage.AntiCheat.EditorCode.PropertyDrawers
{
	[CustomPropertyDrawer(typeof(ObscuredUInt))]
	internal class ObscuredUIntDrawer : ObscuredPropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
		{
#if !UNITY_5_0_PLUS
			EditorGUI.LabelField(position, label.text + " [works in Unity 5+]");
#else
			SerializedProperty hiddenValue = prop.FindPropertyRelative("hiddenValue");
			SetBoldIfValueOverridePrefab(prop, hiddenValue);

			SerializedProperty cryptoKey = prop.FindPropertyRelative("currentCryptoKey");
			SerializedProperty inited = prop.FindPropertyRelative("inited");
			SerializedProperty fakeValue = prop.FindPropertyRelative("fakeValue");
			SerializedProperty fakeValueActive = prop.FindPropertyRelative("fakeValueActive");

			uint currentCryptoKey = (uint)cryptoKey.intValue;
			uint val = 0;

			if (!inited.boolValue)
			{
				if (currentCryptoKey == 0)
				{
					currentCryptoKey = ObscuredUInt.cryptoKeyEditor;
					cryptoKey.intValue = (int)ObscuredUInt.cryptoKeyEditor;
				}
				hiddenValue.intValue = (int)ObscuredUInt.Encrypt(0, currentCryptoKey);
				inited.boolValue = true;
			}
			else
			{
				val = ObscuredUInt.Decrypt((uint)hiddenValue.intValue, currentCryptoKey);
			}

			EditorGUI.BeginChangeCheck();
			val = (uint)EditorGUI.IntField(position, label, (int)val);
			if (EditorGUI.EndChangeCheck())
			{
				hiddenValue.intValue = (int)ObscuredUInt.Encrypt(val, currentCryptoKey);
				fakeValue.intValue = (int)val;
				fakeValueActive.boolValue = true;
			}

			
			ResetBoldFont();
#endif
		}
	}
}
#endif