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
	[CustomPropertyDrawer(typeof(ObscuredShort))]
	internal class ObscuredShortDrawer : ObscuredPropertyDrawer
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

			short currentCryptoKey = (short)cryptoKey.intValue;
			short val = 0;

			if (!inited.boolValue)
			{
				if (currentCryptoKey == 0)
				{
					currentCryptoKey = ObscuredShort.cryptoKeyEditor;
					cryptoKey.intValue = ObscuredShort.cryptoKeyEditor;
				}
				hiddenValue.intValue = ObscuredShort.EncryptDecrypt(0, currentCryptoKey);
				inited.boolValue = true;
			}
			else
			{
				val = ObscuredShort.EncryptDecrypt((short)hiddenValue.intValue, currentCryptoKey);
			}

			EditorGUI.BeginChangeCheck();
			val = (short)EditorGUI.IntField(position, label, val);
			if (EditorGUI.EndChangeCheck())
			{
				hiddenValue.intValue = ObscuredShort.EncryptDecrypt(val, currentCryptoKey);
				fakeValue.intValue = val;
				fakeValueActive.boolValue = true;
			}

			
			ResetBoldFont();
#endif
		}
	}
}
#endif