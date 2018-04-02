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
	[CustomPropertyDrawer(typeof(ObscuredULong))]
	internal class ObscuredULongDrawer : ObscuredPropertyDrawer
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

			ulong currentCryptoKey = (ulong)cryptoKey.longValue;
			ulong val = 0;

			if (!inited.boolValue)
			{
				if (currentCryptoKey == 0)
				{
					currentCryptoKey = ObscuredULong.cryptoKeyEditor;
					cryptoKey.longValue = (long)ObscuredULong.cryptoKeyEditor;
				}
				hiddenValue.longValue = (long)ObscuredULong.Encrypt(0, currentCryptoKey);
				inited.boolValue = true;
			}
			else
			{
				val = ObscuredULong.Decrypt((ulong)hiddenValue.longValue, currentCryptoKey);
			}

			EditorGUI.BeginChangeCheck();
			val = (ulong)EditorGUI.LongField(position, label, (long)val);
			if (EditorGUI.EndChangeCheck())
			{
				hiddenValue.longValue = (long)ObscuredULong.Encrypt(val, currentCryptoKey);
				fakeValue.longValue = (long)val;
				fakeValueActive.boolValue = true;
			}
			
			ResetBoldFont();
#endif
		}
	}
}
#endif