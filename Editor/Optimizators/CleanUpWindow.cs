//----------------------------------------------------------------------------------------------
// Created by Leonid [Zanleo] Voitko (2016.12.20)
//----------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using Assert = Core.Utils.Assert;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public class CleanUpWindow : EditorWindow
{
	private class ObjectsGroup
	{
		public class ObjectDep
		{
			public ObjectDep(Object obj)
			{
				Object = obj;
			}

			public Object Object { get; private set; }

			public string ObjectPath
			{
				get
				{
					if (string.IsNullOrEmpty(m_objectPath))
					{
						m_objectPath = AssetDatabase.GetAssetPath(Object);
					}
					return m_objectPath;
				}
			}

			private string m_objectPath = null;
		}

		public ObjectsGroup()
		{
			Objects = new List<ObjectDep>();
			ScrollPos = Vector2.zero;
			Fooldout = false;
		}

		public List<ObjectDep> Objects { get; private set; }
		public Vector2 ScrollPos { get; set; }
		public bool Fooldout { get; set; }
	}

	private enum ClearDependReason
	{
		// No reason for del
		None = 0,

		// Reason for NO del
		NoneFbx = -10,
		NoneFbxMesh = -11,
		NoneFbxAnim = -12,
		NoneFbxAavatar = -13,
		Font = -20,
		Texture = -30,
		Sprite = -40,
		AudioClip = -50,

		// Reason for del
		Null = 1,
		NoAssetPath = 2,
		ForeignAsset = 3,
		NotMainAsset = 4,
	}

	private enum DependenciesDeep
	{
		// Not collect
		None,

		// only for BuildSettings scenes
		Build,

		// for all scenes and resources in project
		Full
	}

	[MenuItem("Window/Utils/CleanUpWindow")]
	static void Init()
	{
		// Get existing open window or if none, make a new one:  
		CleanUpWindow window = (CleanUpWindow) EditorWindow.GetWindow(typeof(CleanUpWindow));
		window.Show();
	}

	private void OnGUI()
	{
		DrawFullObjectsDeendencies();
		GUILayout.Space(20);
		DrawCheckObjectDependency();
		GUILayout.Space(20);
		DrawCheckSelectedObjectsDependency();
		GUILayout.Space(20);
		m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos);
		DrawObjectsDeendenciesByType(m_lastCheckedObjectsByType, "Checked dependencies", true, CheckDependency);
		GUILayout.Space(20);
		DrawObjectsDeendenciesByType(m_objectDependenciesByType, "Collected dependencies", false, ClearDependencies);
		EditorGUILayout.EndScrollView();
	}

	private void DrawFullObjectsDeendencies()
	{
		DependenciesDeep deep = DependenciesDeep.None;
		var largeButtonHeight = GUILayout.Height(GUI.skin.button.lineHeight * 2);
		GUILayout.BeginHorizontal(largeButtonHeight);
		GUILayout.Space(20);
		if (GUILayout.Button("Collect ALL dependencies", largeButtonHeight))
		{
			deep = DependenciesDeep.Full;
		}
		GUILayout.Space(20);
		if (GUILayout.Button("Collect only build dependencies", largeButtonHeight))
		{
			deep = DependenciesDeep.Build;
		}
		GUILayout.Space(20);
		GUILayout.EndHorizontal();
		if (deep == DependenciesDeep.None)
		{
			return;
		}

		m_rootObjects.Clear();
		m_objectDependencies.Clear();
		m_objectDependenciesByType.Clear();
		CollectFromScenes(deep);
		CollectFromPrefabs(deep);
		CollectFromScriptableObjects(deep);
		FillDependenciesByType(m_objectDependenciesByType, m_objectDependencies);
	}

	private void FillByObject(Dictionary<Type, ObjectsGroup> objectsDict, Object obj)
	{
	}

	private void FillDependenciesByType(Dictionary<Type, ObjectsGroup> objectsDict, List<Object> objectsDependencies)
	{
		var newList = new Dictionary<Type, ObjectsGroup>();
		foreach (var objectDependency in objectsDependencies)
		{
			if (objectDependency == null)
			{
				continue;
			}

			Type forType = GetObjectTypeGroup(objectDependency);
			if (!newList.ContainsKey(forType))
			{
				newList.Add(forType, new ObjectsGroup());
			}
			if (newList[forType].Objects.Find(o => o.Object == objectDependency) != null)
			{
				continue;
			}

			newList[forType].Objects.Add(new ObjectsGroup.ObjectDep(objectDependency));
		}
		// Sort all
		foreach (var objDepKeyValue in newList)
		{
			var objects = objDepKeyValue.Value.Objects;
			objects.Sort(ObjectsSorterByName);
		}

		var sortedKeys = newList.Keys.ToList();
		sortedKeys.Sort(TypeSorter);
		objectsDict.Clear();
		foreach (var sortedKey in sortedKeys)
		{
			objectsDict.Add(sortedKey, newList[sortedKey]);
		}

		newList.Clear();
	}

	private int TypeSorter(Type x, Type y)
	{
		return String.Compare(x.ToString(), y.ToString(), StringComparison.Ordinal);
	}

	private void CollectFromPrefabs(DependenciesDeep deep)
	{
		var prefabGuids = AssetDatabase.FindAssets("t:Prefab");
		List<Object> rootObjects = new List<Object>();
		ChangeProgressForResourcesPhase("Start collect objects", 0);
		for (int i = 0; i < prefabGuids.Length; i++)
		{
			ChangeProgressForResourcesPhase("Start collect objects", 1, (float) i / prefabGuids.Length);
			string assetPath = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
			Object prefab = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
			rootObjects.Add(prefab);
		}

		ChangeProgressForResourcesPhase("CollectDependencies", 2);
		var objectDependencies = EditorUtility.CollectDependencies(rootObjects.ToArray());
		m_rootObjects.AddRange(rootObjects);
		ChangeProgressForResourcesPhase("ClearDependencies", 3);
		m_objectDependencies.AddRange(ClearDependencies(objectDependencies));
		EditorUtility.ClearProgressBar();
	}

	private void CollectFromScriptableObjects(DependenciesDeep deep)
	{
		var prefabGuids = AssetDatabase.FindAssets("t:ScriptableObject");
		List<Object> rootObjects = new List<Object>();
		ChangeProgressForResourcesPhase("Start collect ScriptableObject", 0);
		for (int i = 0; i < prefabGuids.Length; i++)
		{
			ChangeProgressForResourcesPhase("Start collect ScriptableObject", 1, (float)i / prefabGuids.Length);
			string assetPath = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
			Object prefab = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
			rootObjects.Add(prefab);
		}

		ChangeProgressForResourcesPhase("CollectDependencies ScriptableObject", 2);
		var objectDependencies = EditorUtility.CollectDependencies(rootObjects.ToArray());
		m_rootObjects.AddRange(rootObjects);
		ChangeProgressForResourcesPhase("ClearDependencies ScriptableObject", 3);
		m_objectDependencies.AddRange(ClearDependencies(objectDependencies));
		EditorUtility.ClearProgressBar();
	}

	private void CollectFromScenes(DependenciesDeep deep)
	{
		var oldScenesConfig = EditorBuildSettings.scenes;
		ChangeProgressForScenePhase("Start load scene", 0);
		try
		{
			List<Object> rootObjects = new List<Object>();
			var sceneSettings = EditorBuildSettings.scenes;
			if (deep == DependenciesDeep.Full)
			{
				var emptyScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
				Assert.IsTrue(emptyScene.isLoaded, "EmptyScene must be loaded but something wrong!");
				var sceneGuids = AssetDatabase.FindAssets("t:Scene");
				sceneSettings = new EditorBuildSettingsScene[sceneGuids.Length];
				for (int i = 0; i < sceneGuids.Length; i++)
				{
					string assetPath = AssetDatabase.GUIDToAssetPath(sceneGuids[i]);
					sceneSettings[i] = new EditorBuildSettingsScene(assetPath, true);
				}

				EditorBuildSettings.scenes = sceneSettings;
			}

			for (int i = 0; i < sceneSettings.Length; ++i)
			{
				var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(sceneSettings[i].path);
				ChangeProgressForScenePhase("Load " + scene.name, 1, (float) i / sceneSettings.Length);
				EditorSceneManager.OpenScene(sceneSettings[i].path, OpenSceneMode.Additive);
			}

			List<GameObject> sceneRootGameObjects = new List<GameObject>();
			for (int i = 0; i < SceneManager.sceneCount; ++i)
			{
				var scene = SceneManager.GetSceneAt(i);
				ChangeProgressForScenePhase("Collect roots from " + scene.name, 2, (float) i / SceneManager.sceneCount);
				scene.GetRootGameObjects(sceneRootGameObjects);
				rootObjects.AddRange(sceneRootGameObjects.Cast<Object>());
			}

			var objectDependencies = EditorUtility.CollectDependencies(rootObjects.ToArray());
			m_rootObjects.AddRange(rootObjects);
			m_objectDependencies.AddRange(ClearDependencies(objectDependencies));
			ChangeProgressForScenePhase("End CollectFromScenes", 3);
			EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
		}
		catch (Exception ex)
		{
			Debug.Log(ex.Message);
		}
		finally
		{
			EditorBuildSettings.scenes = oldScenesConfig;
		}

		EditorUtility.ClearProgressBar();
	}

	private static Object FixObjectDep(Object obj)
	{
		var reason = ClearDependenciesReason(obj);
		if (reason == ClearDependReason.NoneFbxAavatar ||
			 reason == ClearDependReason.NoneFbxAnim ||
			 reason == ClearDependReason.NoneFbxMesh)
		{
			string pathOfObj = AssetDatabase.GetAssetPath(obj);
			obj = AssetDatabase.LoadMainAssetAtPath(pathOfObj);
		}
		return obj;
	}

	private Object[] ClearDependencies(Object[] objectDependencies)
	{
		var list = new List<Object>(objectDependencies.Length);
		for (int i = 0; i < objectDependencies.Length; i++)
		{
			var obj = objectDependencies[i];
			var reason = ClearDependenciesReason(obj);
			if (reason > 0)
			{
				continue;
			}

			obj = FixObjectDep(obj);
			if (list.Contains(obj))
			{
				continue;
			}

			list.Add(obj);
		}

		list.RemoveAll(ClearDependencies);
		return list.ToArray();
	}

	private bool ClearDependencies(Object objectDependency)
	{
		return ClearDependenciesReason(objectDependency) > 0;
	}

	private static ClearDependReason ClearDependenciesReason(Object objectDependency)
	{
		if (objectDependency == null)
		{
			return ClearDependReason.Null;
		}

		string assetPath = AssetDatabase.GetAssetPath(objectDependency);
		if (string.IsNullOrEmpty(assetPath))
		{
			return ClearDependReason.NoAssetPath;
		}
		// exceptions
		var objectType = GetObjectTypeGroup(objectDependency);
		if (assetPath.ToLower().EndsWith(".fbx"))
		{
			if (typeof(Mesh).IsAssignableFrom(objectType))
			{
				return ClearDependReason.NoneFbxMesh;
			}
			if (typeof(Animation).IsAssignableFrom(objectType))
			{
				return ClearDependReason.NoneFbxAnim;
			}
			if (typeof(Avatar).IsAssignableFrom(objectType))
			{
				return ClearDependReason.NoneFbxAavatar;
			}

			return ClearDependReason.NoneFbx;
		}

		if (typeof(Sprite).IsAssignableFrom(objectType))
		{
			return ClearDependReason.Sprite;
		}
		if (typeof(Texture).IsAssignableFrom(objectType))
		{
			return ClearDependReason.Texture;
		}
		if (typeof(Font).IsAssignableFrom(objectType))
		{
			return ClearDependReason.Font;
		}
		if (typeof(AudioClip).IsAssignableFrom(objectType))
		{
			return ClearDependReason.AudioClip;
		}
		// 
		if (AssetDatabase.IsForeignAsset(objectDependency))
		{
			return ClearDependReason.ForeignAsset;
		}
		if (!AssetDatabase.IsMainAsset(objectDependency))
		{
			return ClearDependReason.NotMainAsset;
		}

		return ClearDependReason.None;
	}

	private void ChangeProgressForScenePhase(string curInfo, int phase = 0, float localProgress = 0)
	{
		float prevRes = 0;
		for (int i = 0; i < phase; ++i)
		{
			prevRes += m_sceneProgresses[i];
		}

		EditorUtility.DisplayProgressBar("CollectFromScenes", curInfo, prevRes + m_sceneProgresses[phase] * localProgress);
	}

	private void ChangeProgressForResourcesPhase(string curInfo, int phase = 0, float localProgress = 0)
	{
		float prevRes = 0;
		for (int i = 0; i < phase; ++i)
		{
			prevRes += m_resourcesProgresses[i];
		}

		EditorUtility.DisplayProgressBar("CollectFromResources", curInfo,
													prevRes + m_resourcesProgresses[phase] * localProgress);
	}

	private void DrawCheckObjectDependency()
	{
		var newObj = EditorGUILayout.ObjectField("Find Dependency", null, typeof(Object), false,
															  GUILayout.Height(GUI.skin.textField.lineHeight * 1.2f));
		if (newObj == null)
		{
			return;
		}
		//Debug.Log(ClearDependenciesReason(newObj).ToString() + "; " + GetObjectTypeGroup(newObj), newObj);
		CheckObjectsDependencies(new Object[] {newObj});
	}

	private void DrawCheckSelectedObjectsDependency()
	{
		if (!GUILayout.Button("Check all selected objects", GUILayout.Height(GUI.skin.textField.lineHeight * 2)))
		{
			return;
		}

		if (Selection.objects == null || Selection.objects.Length == 0)
		{
			Debug.Log("No objects selected");
			return;
		}

		CheckObjectsDependencies(Selection.objects);
	}

	private void CheckObjectsDependencies(Object[] objects)
	{
		m_lastCheckedObjectsByType.Clear();
		List<Object> objectsForCheck = new List<Object>(objects);
		var defaultAssets = objectsForCheck.FindAll(o => o is DefaultAsset);
		foreach (var o in defaultAssets)
		{
			CollectDefaultAssetObjectsForCheck(o, objectsForCheck);
		}

		FillDependenciesByType(m_lastCheckedObjectsByType, objectsForCheck);
		foreach (var obj in objectsForCheck)
		{
			CheckDependency(obj, true);
		}
	}

	private void CollectDefaultAssetObjectsForCheck(Object objectForCheck, List<Object> objectsForCheck)
	{
		objectsForCheck.Remove(objectForCheck);
		var alsoObjectUids = AssetDatabase.FindAssets("", new[] {AssetDatabase.GetAssetPath(objectForCheck)}).ToList();
		foreach (var alsoObjectUid in alsoObjectUids)
		{
			string path = AssetDatabase.GUIDToAssetPath(alsoObjectUid);
			var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
			if (obj is DefaultAsset)
			{
				continue;
			}
			if (objectsForCheck.Contains(obj))
			{
				continue;
			}

			objectsForCheck.Add(obj);
		}
	}

	private bool CheckDependency(Object obj)
	{
		return CheckDependency(obj, false);
	}

	private bool CheckDependency(Object newObj, bool withLog)
	{
		if (newObj == null)
		{
			return false;
		}

		newObj = FixObjectDep(newObj);
		var groupType = GetObjectTypeGroup(newObj);
		if (!m_objectDependenciesByType.ContainsKey(groupType))
		{
			if (withLog)
			{
				Debug.LogWarning("Not found type in dependencies " + groupType + " (" + newObj.GetType() + ") for object " + newObj,
									  newObj);
			}
			return false;
		}

		var obj = m_objectDependenciesByType[groupType].Objects.Find(o => o.Object == newObj);
		if (obj == null)
		{
			if (withLog)
			{
				Debug.LogWarning("Not found dependencies for " + newObj, newObj);
			}
			return false;
		}

		if (withLog)
		{
			Debug.Log("Found in dependency " + newObj, newObj);
		}
		return true;
	}

	private void DrawObjectsDeendenciesByType(Dictionary<Type, ObjectsGroup> objectDependenciesByType, string title,
															bool drawDelButton, Func<Object, bool> flagFunction)
	{
		GUILayout.Label(title + " (" + objectDependenciesByType.Count + ")");
		foreach (var objectDependency in objectDependenciesByType)
		{
			var objGroup = objectDependency.Value;
			var objects = objectDependency.Value.Objects;
			objGroup.Fooldout = EditorGUILayout.Foldout(objGroup.Fooldout, objectDependency.Key + " (" + objects.Count + ")");
			if (!objGroup.Fooldout)
			{
				continue;
			}
			var targHeight = GUILayout.Height(Mathf.Clamp(objects.Count * 22f + GUI.skin.button.lineHeight, 10f, 600));
			if (objects.Count == 0)
			{
				continue;
			}

			objGroup.ScrollPos = EditorGUILayout.BeginScrollView(objGroup.ScrollPos,
																				  targHeight, GUILayout.MaxWidth(position.width-30f));
			if (GUILayout.Button("DeleteAll", GUILayout.MaxWidth(150f)))
			{
				for (int i = 0; i < objects.Count; i++)
				{
					EditorUtility.DisplayProgressBar("Delete all " + objectDependency.Key, objects[i].ObjectPath,
																(float) i / objects.Count);
					AssetDatabase.MoveAssetToTrash(objects[i].ObjectPath);
					AssetDatabase.Refresh();
				}

				EditorUtility.ClearProgressBar();
				objects.Clear();
			}

			for (int i = 0; i < objects.Count; i++)
			{
				var obj = objects[i];
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.ObjectField("", obj.Object, typeof(Object), false, GUILayout.MaxWidth(position.width - 250f));
				if (flagFunction != null)
				{
					EditorGUILayout.Toggle(string.Empty, flagFunction(obj.Object), GUILayout.MaxWidth(20f));
				}
				if (drawDelButton)
				{
					if (GUILayout.Button("Delete", GUILayout.MaxWidth(120f)))
					{
						AssetDatabase.MoveAssetToTrash(obj.ObjectPath);
						AssetDatabase.Refresh();
						objects.Remove(obj);
						--i;
					}
				}
				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.EndScrollView();
		}
	}

	private int ObjectsSorterByName(ObjectsGroup.ObjectDep x, ObjectsGroup.ObjectDep y)
	{
		return x.Object.name.CompareTo(y.Object.name);
	}

	private static Type GetObjectTypeGroup(Object objectDependency)
	{
		if (objectDependency == null)
		{
			return null;
		}
		if (objectDependency is MonoScript)
		{
			return typeof(MonoScript);
		}
		if (objectDependency is Component)
		{
			return typeof(Component);
		}
		if (objectDependency is ScriptableObject)
		{
			return typeof(ScriptableObject);
		}

		return objectDependency.GetType();
	}

	private List<Object> m_rootObjects = new List<Object>();
	private List<Object> m_objectDependencies = new List<Object>();
	private Dictionary<Type, ObjectsGroup> m_objectDependenciesByType = new Dictionary<Type, ObjectsGroup>();
	private Dictionary<Type, ObjectsGroup> m_lastCheckedObjectsByType = new Dictionary<Type, ObjectsGroup>();
	private Vector2 m_scrollPos = Vector2.zero;

	private readonly float[] m_sceneProgresses = new float[]
																{
																	// Start of all
																	0f,
																	// Load scene
																	0.2f,
																	// CollectRoots
																	0.1f,
																	// Collect root dependencies
																	0.7f,
																	// End of all
																	1f
																};

	private readonly float[] m_resourcesProgresses = new float[]
																	 {
																		 // Start of all
																		 0f,
																		 // Start collect objects
																		 0.6f,
																		 // CollectDependencies
																		 0.3f,
																		 // ClearDependencies
																		 0.1f,
																		 // End of all
																		 1f
																	 };
}