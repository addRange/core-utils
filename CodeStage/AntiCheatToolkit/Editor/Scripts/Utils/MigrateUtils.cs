#if UNITY_EDITOR

#define UNITY_5_0_PLUS
#if UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9
#undef UNITY_5_0_PLUS
#endif

using System;
using System.Runtime.InteropServices;
using CodeStage.AntiCheat.Common;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CodeStage.AntiCheat.EditorCode
{
	/// <summary>
	/// Class with utility functions to help with ACTk migrations after updates.
	/// </summary>
	public class MigrateUtils
	{
		/// <summary>
		/// Checks all prefabs in project for old version of obscured types and tries to migrate values to the new version.
		/// </summary>
		[UnityEditor.MenuItem(ActEditorGlobalStuff.WINDOWS_MENU_PATH + "Migrate obscured types on prefabs...", false, 1100)]
		public static void MigrateObscuredTypesOnPrefabs()
		{
			if (!EditorUtility.DisplayDialog("ACTk Obscured types migration",
					"Are you sure you wish to scan all prefabs in your project and automatically migrate values to the new format?",
					"Yes", "No"))
			{
				Debug.Log(ActEditorGlobalStuff.LOG_PREFIX + "Obscured types migration was cancelled by user.");
				return;
			}

			AssetDatabase.SaveAssets();

			string[] assets = AssetDatabase.FindAssets("t:ScriptableObject t:Prefab");
			//string[] prefabs = AssetDatabase.FindAssets("TestPrefab");
			int touchedCount = 0;
			int count = assets.Length;
			for (int i = 0; i < count; i++)
			{
				if (EditorUtility.DisplayCancelableProgressBar("Looking through objects", "Object " + (i + 1) + " from " + count,
					i / (float)count))
				{
					Debug.Log(ActEditorGlobalStuff.LOG_PREFIX + "Obscured types migration was cancelled by user.");
					break;
				}

				string guid = assets[i];
				string path = AssetDatabase.GUIDToAssetPath(guid);

				Object[] objects = AssetDatabase.LoadAllAssetsAtPath(path);
				foreach (Object unityObject in objects)
				{
					if (unityObject == null) continue;
					if (unityObject.name == "Deprecated EditorExtensionImpl") continue;

					bool modified = false;
					var so = new SerializedObject(unityObject);
					SerializedProperty sp = so.GetIterator();

					if (sp == null) continue;

					while (sp.NextVisible(true))
					{
						if (sp.propertyType != SerializedPropertyType.Generic) continue;

						string type = sp.type;

						switch (type)
						{
							case "ObscuredBool":
							{
								SerializedProperty hiddenValue = sp.FindPropertyRelative("hiddenValue");
								SerializedProperty cryptoKey = sp.FindPropertyRelative("currentCryptoKey");
								SerializedProperty fakeValue = sp.FindPropertyRelative("fakeValue");
								SerializedProperty fakeValueChanged = sp.FindPropertyRelative("fakeValueChanged");
								SerializedProperty fakeValueActive = sp.FindPropertyRelative("fakeValueActive");
								SerializedProperty inited = sp.FindPropertyRelative("inited");

								if (inited != null && inited.boolValue)
								{
									int currentCryptoKey = cryptoKey.intValue;
									bool real = ObscuredBool.Decrypt(hiddenValue.intValue, (byte) currentCryptoKey);
									bool fake = fakeValue.boolValue;

									if (real != fake)
									{
										Debug.Log(ActEditorGlobalStuff.LOG_PREFIX + "Fixed property " + sp.displayName + ":" + type +
										          " at the object " + unityObject.name + "\n" + path);
										fakeValue.boolValue = real;
										if (fakeValueChanged != null) fakeValueChanged.boolValue = true;
										if (fakeValueActive != null) fakeValueActive.boolValue = true;
										modified = true;
									}
								}
							}
							break;

							case "ObscuredDouble":
							{
								SerializedProperty hiddenValue = sp.FindPropertyRelative("hiddenValue");
								if (hiddenValue == null) continue;

								SerializedProperty hiddenValue1 = hiddenValue.FindPropertyRelative("b1");
								SerializedProperty hiddenValue2 = hiddenValue.FindPropertyRelative("b2");
								SerializedProperty hiddenValue3 = hiddenValue.FindPropertyRelative("b3");
								SerializedProperty hiddenValue4 = hiddenValue.FindPropertyRelative("b4");
								SerializedProperty hiddenValue5 = hiddenValue.FindPropertyRelative("b5");
								SerializedProperty hiddenValue6 = hiddenValue.FindPropertyRelative("b6");
								SerializedProperty hiddenValue7 = hiddenValue.FindPropertyRelative("b7");
								SerializedProperty hiddenValue8 = hiddenValue.FindPropertyRelative("b8");

								SerializedProperty hiddenValueOld = sp.FindPropertyRelative("hiddenValueOld");

								if (hiddenValueOld != null && hiddenValueOld.isArray && hiddenValueOld.arraySize == 8)
								{
									hiddenValue1.intValue = hiddenValueOld.GetArrayElementAtIndex(0).intValue;
									hiddenValue2.intValue = hiddenValueOld.GetArrayElementAtIndex(1).intValue;
									hiddenValue3.intValue = hiddenValueOld.GetArrayElementAtIndex(2).intValue;
									hiddenValue4.intValue = hiddenValueOld.GetArrayElementAtIndex(3).intValue;
									hiddenValue5.intValue = hiddenValueOld.GetArrayElementAtIndex(4).intValue;
									hiddenValue6.intValue = hiddenValueOld.GetArrayElementAtIndex(5).intValue;
									hiddenValue7.intValue = hiddenValueOld.GetArrayElementAtIndex(6).intValue;
									hiddenValue8.intValue = hiddenValueOld.GetArrayElementAtIndex(7).intValue;

									hiddenValueOld.arraySize = 0;

									Debug.Log(ActEditorGlobalStuff.LOG_PREFIX + "Migrated property " + sp.displayName + ":" + type +
									          " at the object " + unityObject.name + "\n" + path);
									modified = true;
								}

#if UNITY_5_0_PLUS

								SerializedProperty cryptoKey = sp.FindPropertyRelative("currentCryptoKey");
								SerializedProperty fakeValue = sp.FindPropertyRelative("fakeValue");
								SerializedProperty fakeValueActive = sp.FindPropertyRelative("fakeValueActive");
								SerializedProperty inited = sp.FindPropertyRelative("inited");

								if (inited != null && inited.boolValue)
								{
									var union = new LongBytesUnion();
									union.b8.b1 = (byte)hiddenValue1.intValue;
									union.b8.b2 = (byte)hiddenValue2.intValue;
									union.b8.b3 = (byte)hiddenValue3.intValue;
									union.b8.b4 = (byte)hiddenValue4.intValue;
									union.b8.b5 = (byte)hiddenValue5.intValue;
									union.b8.b6 = (byte)hiddenValue6.intValue;
									union.b8.b7 = (byte)hiddenValue7.intValue;
									union.b8.b8 = (byte)hiddenValue8.intValue;

									long currentCryptoKey = cryptoKey.longValue;
									double real = ObscuredDouble.Decrypt(union.l, currentCryptoKey);
									double fake = fakeValue.doubleValue;

									if (Math.Abs(real - fake) > 0.0000001d)
									{
										Debug.Log(ActEditorGlobalStuff.LOG_PREFIX + "Fixed property " + sp.displayName + ":" + type +
										          " at the object " + unityObject.name + "\n" + path);
											
										fakeValue.doubleValue = real;
										fakeValueActive.boolValue = true;
										modified = true;
									}
								}
#endif
							}
								break;

							case "ObscuredFloat":
							{
								SerializedProperty hiddenValue = sp.FindPropertyRelative("hiddenValue");
								if (hiddenValue == null) continue;

								SerializedProperty hiddenValue1 = hiddenValue.FindPropertyRelative("b1");
								SerializedProperty hiddenValue2 = hiddenValue.FindPropertyRelative("b2");
								SerializedProperty hiddenValue3 = hiddenValue.FindPropertyRelative("b3");
								SerializedProperty hiddenValue4 = hiddenValue.FindPropertyRelative("b4");

								SerializedProperty hiddenValueOld = sp.FindPropertyRelative("hiddenValueOld");

								if (hiddenValueOld != null && hiddenValueOld.isArray && hiddenValueOld.arraySize == 4)
								{
									hiddenValue1.intValue = hiddenValueOld.GetArrayElementAtIndex(0).intValue;
									hiddenValue2.intValue = hiddenValueOld.GetArrayElementAtIndex(1).intValue;
									hiddenValue3.intValue = hiddenValueOld.GetArrayElementAtIndex(2).intValue;
									hiddenValue4.intValue = hiddenValueOld.GetArrayElementAtIndex(3).intValue;

									hiddenValueOld.arraySize = 0;

									Debug.Log(ActEditorGlobalStuff.LOG_PREFIX + "Migrated property " + sp.displayName + ":" + type +
									          " at the object " + unityObject.name + "\n" + path);
									modified = true;
								}

								SerializedProperty cryptoKey = sp.FindPropertyRelative("currentCryptoKey");
								SerializedProperty fakeValue = sp.FindPropertyRelative("fakeValue");
								SerializedProperty fakeValueActive = sp.FindPropertyRelative("fakeValueActive");
								SerializedProperty inited = sp.FindPropertyRelative("inited");

								if (inited != null && inited.boolValue)
								{
									var union = new IntBytesUnion();
									union.b4.b1 = (byte)hiddenValue1.intValue;
									union.b4.b2 = (byte)hiddenValue2.intValue;
									union.b4.b3 = (byte)hiddenValue3.intValue;
									union.b4.b4 = (byte)hiddenValue4.intValue;

									int currentCryptoKey = cryptoKey.intValue;
									float real = ObscuredFloat.Decrypt(union.i, currentCryptoKey);
									float fake = fakeValue.floatValue;
									if (Math.Abs(real - fake) > 0.0000001f)
									{
										Debug.Log(ActEditorGlobalStuff.LOG_PREFIX + "Fixed property " + sp.displayName + ":" + type +
										          " at the object " + unityObject.name + "\n" + path);

										fakeValue.floatValue = real;
										fakeValueActive.boolValue = true;
										modified = true;
									}
								}
							}
								break;

							case "ObscuredInt":
							{
								SerializedProperty hiddenValue = sp.FindPropertyRelative("hiddenValue");
								SerializedProperty cryptoKey = sp.FindPropertyRelative("currentCryptoKey");
								SerializedProperty fakeValue = sp.FindPropertyRelative("fakeValue");
								SerializedProperty fakeValueActive = sp.FindPropertyRelative("fakeValueActive");
								SerializedProperty inited = sp.FindPropertyRelative("inited");

								if (inited != null && inited.boolValue)
								{
									int currentCryptoKey = cryptoKey.intValue;
									int real = ObscuredInt.Decrypt(hiddenValue.intValue, currentCryptoKey);
									int fake = fakeValue.intValue;

									if (real != fake)
									{
										Debug.Log(ActEditorGlobalStuff.LOG_PREFIX + "Fixed property " + sp.displayName + ":" + type +
										          " at the object " + unityObject.name + "\n" + path);
										fakeValue.intValue = real;
										fakeValueActive.boolValue = true;
										modified = true;
									}
								}
							}
								break;
#if UNITY_5_0_PLUS
							case "ObscuredLong":
							{
								SerializedProperty hiddenValue = sp.FindPropertyRelative("hiddenValue");
								SerializedProperty cryptoKey = sp.FindPropertyRelative("currentCryptoKey");
								SerializedProperty fakeValue = sp.FindPropertyRelative("fakeValue");
								SerializedProperty fakeValueActive = sp.FindPropertyRelative("fakeValueActive");
								SerializedProperty inited = sp.FindPropertyRelative("inited");

								if (inited != null && inited.boolValue)
								{
									long currentCryptoKey = cryptoKey.longValue;
									long real = ObscuredLong.Decrypt(hiddenValue.longValue, currentCryptoKey);
									long fake = fakeValue.longValue;

									if (real != fake)
									{
										Debug.Log(ActEditorGlobalStuff.LOG_PREFIX + "Fixed property " + sp.displayName + ":" + type +
										          " at the object " + unityObject.name + "\n" + path);
										fakeValue.longValue = real;
										fakeValueActive.boolValue = true;
										modified = true;
									}
								}
							}
								break;

							case "ObscuredShort":
							{
								SerializedProperty hiddenValue = sp.FindPropertyRelative("hiddenValue");
								SerializedProperty cryptoKey = sp.FindPropertyRelative("currentCryptoKey");
								SerializedProperty fakeValue = sp.FindPropertyRelative("fakeValue");
								SerializedProperty fakeValueActive = sp.FindPropertyRelative("fakeValueActive");
								SerializedProperty inited = sp.FindPropertyRelative("inited");

								if (inited != null && inited.boolValue)
								{
									short currentCryptoKey = (short)cryptoKey.intValue;
									short real = ObscuredShort.EncryptDecrypt((short)hiddenValue.intValue, currentCryptoKey);
									short fake = (short)fakeValue.intValue;

									if (real != fake)
									{
										Debug.Log(ActEditorGlobalStuff.LOG_PREFIX + "Fixed property " + sp.displayName + ":" + type +
										          " at the object " + unityObject.name + "\n" + path);
										fakeValue.intValue = real;
										fakeValueActive.boolValue = true;
										modified = true;
									}
								}
							}
								break;
#endif
							case "ObscuredString":
							{
								SerializedProperty hiddenValue = sp.FindPropertyRelative("hiddenValue");
								SerializedProperty cryptoKey = sp.FindPropertyRelative("currentCryptoKey");
								SerializedProperty fakeValue = sp.FindPropertyRelative("fakeValue");
								SerializedProperty fakeValueActive = sp.FindPropertyRelative("fakeValueActive");
								SerializedProperty inited = sp.FindPropertyRelative("inited");

								if (inited != null && inited.boolValue)
								{
									string currentCryptoKey = cryptoKey.stringValue;
									byte[] bytes = new byte[hiddenValue.arraySize];
									for (int j = 0; j < hiddenValue.arraySize; j++)
									{
										bytes[j] = (byte)hiddenValue.GetArrayElementAtIndex(j).intValue;
									}

									string real = ObscuredString.EncryptDecrypt(GetString(bytes), currentCryptoKey);
									string fake = fakeValue.stringValue;

									if (real != fake)
									{
										Debug.Log(ActEditorGlobalStuff.LOG_PREFIX + "Fixed property " + sp.displayName + ":" + type +
										          " at the object " + unityObject.name + "\n" + path);
										fakeValue.stringValue = real;
										fakeValueActive.boolValue = true;
										modified = true;
									}
								}
							}
								break;
#if UNITY_5_0_PLUS
							case "ObscuredUInt":
							{
								SerializedProperty hiddenValue = sp.FindPropertyRelative("hiddenValue");
								SerializedProperty cryptoKey = sp.FindPropertyRelative("currentCryptoKey");
								SerializedProperty fakeValue = sp.FindPropertyRelative("fakeValue");
								SerializedProperty fakeValueActive = sp.FindPropertyRelative("fakeValueActive");
								SerializedProperty inited = sp.FindPropertyRelative("inited");

								if (inited != null && inited.boolValue)
								{
									uint currentCryptoKey = (uint)cryptoKey.intValue;
									uint real = ObscuredUInt.Decrypt((uint)hiddenValue.intValue, currentCryptoKey);
									uint fake = (uint)fakeValue.intValue;

									if (real != fake)
									{
										Debug.Log(ActEditorGlobalStuff.LOG_PREFIX + "Fixed property " + sp.displayName + ":" + type +
										          " at the object " + unityObject.name + "\n" + path);
										fakeValue.intValue = (int)real;
										fakeValueActive.boolValue = true;
										modified = true;
									}
								}
							}
								break;

							case "ObscuredULong":
							{
								SerializedProperty hiddenValue = sp.FindPropertyRelative("hiddenValue");
								SerializedProperty cryptoKey = sp.FindPropertyRelative("currentCryptoKey");
								SerializedProperty fakeValue = sp.FindPropertyRelative("fakeValue");
								SerializedProperty fakeValueActive = sp.FindPropertyRelative("fakeValueActive");
								SerializedProperty inited = sp.FindPropertyRelative("inited");

								if (inited != null && inited.boolValue)
								{
									ulong currentCryptoKey = (ulong)cryptoKey.longValue;
									ulong real = ObscuredULong.Decrypt((ulong)hiddenValue.longValue, currentCryptoKey);
									ulong fake = (ulong)fakeValue.longValue;

									if (real != fake)
									{
										Debug.Log(ActEditorGlobalStuff.LOG_PREFIX + "Fixed property " + sp.displayName + ":" + type +
										          " at the object " + unityObject.name + "\n" + path);
										fakeValue.longValue = (long)real;
										fakeValueActive.boolValue = true;
										modified = true;
									}
								}
							}
								break;
#endif
							case "ObscuredVector2":
							{
								SerializedProperty hiddenValue = sp.FindPropertyRelative("hiddenValue");
								if (hiddenValue == null) continue;

								SerializedProperty hiddenValueX = hiddenValue.FindPropertyRelative("x");
								SerializedProperty hiddenValueY = hiddenValue.FindPropertyRelative("y");
									
								SerializedProperty cryptoKey = sp.FindPropertyRelative("currentCryptoKey");
								SerializedProperty fakeValue = sp.FindPropertyRelative("fakeValue");
								SerializedProperty fakeValueActive = sp.FindPropertyRelative("fakeValueActive");
								SerializedProperty inited = sp.FindPropertyRelative("inited");

								if (inited != null && inited.boolValue)
								{
									ObscuredVector2.RawEncryptedVector2 ev = new ObscuredVector2.RawEncryptedVector2();
									ev.x = hiddenValueX.intValue;
									ev.y = hiddenValueY.intValue;

									int currentCryptoKey = cryptoKey.intValue;
									Vector2 real = ObscuredVector2.Decrypt(ev, currentCryptoKey);
									Vector2 fake = fakeValue.vector2Value;

									if (real != fake)
									{
										Debug.Log(ActEditorGlobalStuff.LOG_PREFIX + "Fixed property " + sp.displayName + ":" + type +
										          " at the object " + unityObject.name + "\n" + path);
										fakeValue.vector2Value = real;
										fakeValueActive.boolValue = true;
										modified = true;
									}
								}
							}
								break;

							case "ObscuredVector3":
							{
								SerializedProperty hiddenValue = sp.FindPropertyRelative("hiddenValue");
								if (hiddenValue == null) continue;

								SerializedProperty hiddenValueX = hiddenValue.FindPropertyRelative("x");
								SerializedProperty hiddenValueY = hiddenValue.FindPropertyRelative("y");
								SerializedProperty hiddenValueZ = hiddenValue.FindPropertyRelative("z");

								SerializedProperty cryptoKey = sp.FindPropertyRelative("currentCryptoKey");
								SerializedProperty fakeValue = sp.FindPropertyRelative("fakeValue");
								SerializedProperty fakeValueActive = sp.FindPropertyRelative("fakeValueActive");
								SerializedProperty inited = sp.FindPropertyRelative("inited");

								if (inited != null && inited.boolValue)
								{
									var ev = new ObscuredVector3.RawEncryptedVector3();
									ev.x = hiddenValueX.intValue;
									ev.y = hiddenValueY.intValue;
									ev.z = hiddenValueZ.intValue;

									int currentCryptoKey = cryptoKey.intValue;
									Vector3 real = ObscuredVector3.Decrypt(ev, currentCryptoKey);
									Vector3 fake = fakeValue.vector3Value;

									if (real != fake)
									{
										Debug.Log(ActEditorGlobalStuff.LOG_PREFIX + "Fixed property " + sp.displayName + ":" + type +
										          " at the object " + unityObject.name + "\n" + path);
										fakeValue.vector3Value = real;
										fakeValueActive.boolValue = true;
										modified = true;
									}
								}
							}
								break;
						}
					}

					if (modified)
					{
						touchedCount++;
						so.ApplyModifiedProperties();
						EditorUtility.SetDirty(unityObject);
					}
				}
			}

			AssetDatabase.SaveAssets();

			EditorUtility.ClearProgressBar();

			if (touchedCount > 0)
				Debug.Log(ActEditorGlobalStuff.LOG_PREFIX + "Migrated obscured types on " + touchedCount + " objects.");
			else
				Debug.Log(ActEditorGlobalStuff.LOG_PREFIX + "No objects were found for obscured types migration.");
		}

		private static void EncryptAndSetBytes(string val, SerializedProperty prop, string key)
		{
			string encrypted = ObscuredString.EncryptDecrypt(val, key);
			byte[] encryptedBytes = GetBytes(encrypted);

			prop.ClearArray();
			prop.arraySize = encryptedBytes.Length;

			for (int i = 0; i < encryptedBytes.Length; i++)
			{
				prop.GetArrayElementAtIndex(i).intValue = encryptedBytes[i];
			}
		}

		private static byte[] GetBytes(string str)
		{
			byte[] bytes = new byte[str.Length * sizeof(char)];
			System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
			return bytes;
		}

		private static string GetString(byte[] bytes)
		{
			char[] chars = new char[bytes.Length / sizeof(char)];
			System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
			return new string(chars);
		}

		[StructLayout(LayoutKind.Explicit)]
		private struct LongBytesUnion
		{
			[FieldOffset(0)]
			public long l;

			[FieldOffset(0)]
			public ACTkByte8 b8;
		}

		[StructLayout(LayoutKind.Explicit)]
		private struct IntBytesUnion
		{
			[FieldOffset(0)]
			public int i;

			[FieldOffset(0)]
			public ACTkByte4 b4;
		}
	}
}
#endif