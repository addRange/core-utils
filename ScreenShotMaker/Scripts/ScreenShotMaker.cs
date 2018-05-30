// Modifed by Zanleo 2017

using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
#endif

//only work in the Editor
public class ScreenShotMaker : MonoBehaviour
{
	[System.Serializable]
	private class ScreenShotSize
	{
		[SerializeField]
		public string Name = string.Empty;
		public bool Enable = true;
		public Vector2 Size = Vector2.zero;
		public bool UseSafeArea = false;
		public Rect SafeAreaKoefficient = new Rect(0, 0, 1, 1);
		[Tooltip("Оставить пустым для всех платформ")]
		public List<RuntimePlatform> EnabledPlatforms = new List<RuntimePlatform>();
	}
	
	[SerializeField]
	private bool DontDestroy = true;
	[SerializeField]
	private bool MakeScreenshotsForAllPlatforms = false;

	[SerializeField, Tooltip("the key that will trigger a screenshot")]
	private KeyCode ScreenShotKey = KeyCode.F1;

	[SerializeField, Tooltip("the key that will trigger a current resolution screenshot")]
	private KeyCode CurResScreenShotKey = KeyCode.F1;

	[SerializeField, Tooltip("the keys that will trigger a screenshot if pressed at the same time")]
	private KeyCode[] ScreenShotKeys = new KeyCode[2];

	[SerializeField, Tooltip("any key will trigger a screenshot")]
	private bool ScreenShotAnyKey = false;

	[SerializeField, Tooltip("a List of ScreenShotsSizes")]
	private List<ScreenShotSize> ScreenShotSizes = new List<ScreenShotSize>();

	[Tooltip("number of frames between resizing and taking the screenshot")]
	public int frames = 30;

	//private
	private string _ScreenShotPath;
#if UNITY_EDITOR
	private int m_defaultSizeIndex;
#endif

	private void Start()
	{
		if (DontDestroy &&
			(Application.platform == RuntimePlatform.WindowsEditor ||
				Application.platform == RuntimePlatform.OSXEditor ||
				Application.platform == RuntimePlatform.LinuxEditor))
		{
			DontDestroyOnLoad(gameObject); //no not destory this GameObject
		}

#if UNITY_EDITOR
		CreateScreenShotFolder(); //creates a folder to store the screenshots

		m_defaultSizeIndex = GameViewUtils.GetSize();
		m_i = 0;
		while (m_i < ScreenShotSizes.Count)
		{
			CheckForSizeWithAdd(ScreenShotSizes[m_i]);
			m_i++;
		}
#endif
	}

	//creates a folder to store the screenshots
	private void CreateScreenShotFolder()
	{
		// /Users/JustinGarza/UnityProjects/ScreenShotMaker/Assets/~/ScreenShots
		_ScreenShotPath = Application.dataPath + "/~/ScreenShots";

		print(_ScreenShotPath);

		if (!Directory.Exists(_ScreenShotPath))
		{
			Directory.CreateDirectory(_ScreenShotPath);
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(ScreenShotKey))
		{
			StartCoroutine(TakeScreenShots());
		}
		if (Input.GetKeyDown(CurResScreenShotKey))
		{
			StartCoroutine(CurResTakeScreenShots());
		}

		if (ScreenShotKeys.Length == 2)
		{
			if (
				(Input.GetKey(ScreenShotKeys[0]) && Input.GetKeyDown(ScreenShotKeys[1]))
				|| (Input.GetKeyDown(ScreenShotKeys[0]) && Input.GetKey(ScreenShotKeys[1]))
				|| (Input.GetKeyDown(ScreenShotKeys[0]) && Input.GetKeyDown(ScreenShotKeys[1]))
			)
			{
				StartCoroutine(TakeScreenShots());
			}
		}

		if (Input.anyKeyDown && ScreenShotAnyKey)
		{
			StartCoroutine(TakeScreenShots());
		}
	}

	//take screenshots
	private int m_i = 0;
	private int m_stangeInt = 0;

	private IEnumerator CurResTakeScreenShots()
	{
#if UNITY_EDITOR
		string timeTag = System.DateTime.Now.ToString().Replace("/", "").Replace(" ", "")
			.Replace(":", ""); //get DatetimeTag
		string newFileName = _ScreenShotPath + "/" + timeTag + ".png";
		ScreenCapture.CaptureScreenshot(newFileName); //Save the Image
		Debug.Log("Captured in " + newFileName);
#endif
		yield break;
	}

	private IEnumerator TakeScreenShots()
	{
		m_i = 0;
		m_stangeInt = 0;

		string timeTag = System.DateTime.Now.ToString().Replace("/", "").Replace(" ", "")
			.Replace(":", ""); //get DatetimeTag
		// HACK for almost immidiatly screenShot)
		Time.captureFramerate = int.MaxValue;
#if UNITY_EDITOR
		bool isSomeSafeAreaApplied = false;
		SafeAreaApplier[] applieres = null;
#endif
		while (m_i < ScreenShotSizes.Count)
		{
			var curItem = ScreenShotSizes[m_i];
			if (!NeedPassItem(curItem))
			{
				m_i++;
				continue;
			}
			CheckForSizeWithAdd(curItem);
			//Debug.Log(m_i + ". frame " + m_stangeInt);
			if (m_stangeInt <= frames || frames == 0)
			{
				if (m_stangeInt == 0)
				{
#if UNITY_EDITOR
					int targRes = GameViewUtils.FindSize(GetCorrectedName(curItem.Name));
					GameViewUtils.SetSize(targRes);
					GameViewUtils.UpdateView();
					yield return 0;
					// Need wait whie Update view is apply Screen size
					if (curItem.UseSafeArea || isSomeSafeAreaApplied)
					{
						isSomeSafeAreaApplied = true;
						if (applieres == null)
						{
							applieres = GameObject.FindObjectsOfType<SafeAreaApplier>();
						}
						var targetRect = new Rect(0, 0, curItem.Size.x, curItem.Size.y);
						if (curItem.UseSafeArea)
						{
							targetRect.x = curItem.SafeAreaKoefficient.x * curItem.Size.x;
							targetRect.y = curItem.SafeAreaKoefficient.y * curItem.Size.y;
							targetRect.width = curItem.SafeAreaKoefficient.width * curItem.Size.x;
							targetRect.height = curItem.SafeAreaKoefficient.height * curItem.Size.y;
						}

						foreach (var safeAreaApplier in applieres)
						{
							safeAreaApplier.ApplySafeAreaWithKoefficients(targetRect);
						}
					}
#endif
					yield return new WaitForEndOfFrame();
				}

				if (m_stangeInt == frames)
				{
					string newFileName = _ScreenShotPath + "/" + curItem.Name + "_" + curItem.Size.x + "x" +
						curItem.Size.y + "_" + timeTag + ".png";
					Debug.Log(newFileName);
					ScreenCapture.CaptureScreenshot(newFileName); //Save the Image
				}

				m_stangeInt++;
				if (frames != 0)
				{
					yield return null;
				}
				else
				{
					m_stangeInt = 0;
					m_i++;
					//yield return null;
				}
			}
			else
			{
				m_stangeInt = 0;
				m_i++;
			}
		}

		Time.captureFramerate = 0;

#if UNITY_EDITOR
		GameViewUtils.SetSize(m_defaultSizeIndex);
		GameViewUtils.UpdateView();
		yield return null;
		yield return new WaitForEndOfFrame();
		if (isSomeSafeAreaApplied)
		{
			var targetRect2 = new Rect(0, 0, Screen.width, Screen.height);
			foreach (var safeAreaApplier in applieres)
			{
				safeAreaApplier.ApplySafeAreaWithKoefficients(targetRect2);
			}
		}
#endif
	}

	private bool NeedPassItem(ScreenShotSize curItem)
	{
		if (!curItem.Enable)
		{
			return false;
		}
		if (!MakeScreenshotsForAllPlatforms && curItem.EnabledPlatforms.Count > 0 && !curItem.EnabledPlatforms.Contains(Application.platform))
		{
			return false;
		}

		return true;
	}

	private void CheckForSizeWithAdd(ScreenShotSize curItem)
	{
#if UNITY_EDITOR
		var cName = GetCorrectedName(curItem.Name);
		bool resExist = GameViewUtils.SizeExists(cName);
		if (!resExist)
		{
			GameViewUtils.AddCustomSize(
				GameViewUtils.GameViewSizeType.FixedResolution,
				(int)curItem.Size.x, (int)curItem.Size.y, cName);
		}
#endif
	}

	private string GetCorrectedName(string nameForCorrect)
	{
		return nameForCorrect.Substring(0, Mathf.Min(nameForCorrect.Length, MaxNameLength));
	}

	[ContextMenu("Add to iPhone")]
	private void AddEmptyPlatformsToIphone() { AddEmptyPlatformsToTarget(RuntimePlatform.IPhonePlayer); }

	private void AddEmptyPlatformsToTarget(RuntimePlatform target)
	{
		foreach (var scrSiz in ScreenShotSizes)
		{
			if (scrSiz.EnabledPlatforms.Count > 0)
			{
				continue;
			}
			scrSiz.EnabledPlatforms.Add(target);
			Debug.Log("Added to " + scrSiz.Name);
		}
	}

	private const int MaxNameLength = 39;
}

#if UNITY_EDITOR
public static class GameViewUtils
{
	static object gameViewSizesInstance;
	static MethodInfo getGroup;
	static PropertyInfo getCurGroup;
 
	static GameViewUtils()
	{
		// gameViewSizesInstance  = ScriptableSingleton<GameViewSizes>.instance;
		
		var sizesType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizes");
		var singleType = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
		var instanceProp = singleType.GetProperty("instance");
		getGroup = sizesType.GetMethod("GetGroup");
		getCurGroup = sizesType.GetProperty("currentGroupType");
		gameViewSizesInstance = instanceProp.GetValue(null, null);
	}
 
	public enum GameViewSizeType
	{
		AspectRatio, FixedResolution
	}

	public static void UpdateView()
	{
		UnityEditorInternal.InternalEditorUtility.RepaintAllViews();

		Assembly assembly = typeof(EditorWindow).Assembly;
		System.Type type = assembly.GetType("UnityEditor.GameView");
		EditorWindow gameview = EditorWindow.GetWindow(type);
		gameview.Repaint();
	}

	public static int GetSize()
	{
		var gvWndType = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
		var selectedSizeIndexProp = gvWndType.GetProperty("selectedSizeIndex",
			BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		var gvWnd = EditorWindow.GetWindow(gvWndType);
		return (int)selectedSizeIndexProp.GetValue(gvWnd, null);
	}

	public static void SetSize(int index)
	{
		var gvWndType = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
		var selectedSizeIndexProp = gvWndType.GetProperty("selectedSizeIndex",
			BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		var gvWnd = EditorWindow.GetWindow(gvWndType);
		selectedSizeIndexProp.SetValue(gvWnd, index, null);
	}

	public static void AddCustomSize(
		GameViewSizeType viewSizeType, int width, int height, string text)
	{
		GameViewSizeGroupType sizeGroupType = GetCurGroup();
		AddCustomSize(viewSizeType, sizeGroupType, width, height, text);
	}

	public static void AddCustomSize(
		GameViewSizeType viewSizeType, GameViewSizeGroupType sizeGroupType, int width, int height, string text)
	{
		// GameViewSizes group = gameViewSizesInstance.GetGroup(sizeGroupTyge);
		// group.AddCustomSize(new GameViewSize(viewSizeType, width, height, text);

		var group = GetGroup(sizeGroupType);

		var addCustomSize = getGroup.ReturnType.GetMethod("AddCustomSize"); // or group.GetType().
		var gvsType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSize");
		var ctor = gvsType.GetConstructor(new System.Type[] {typeof(int), typeof(int), typeof(int), typeof(string)});
		var newSize = ctor.Invoke(new object[] {(int)viewSizeType, width, height, text});
		addCustomSize.Invoke(group, new object[] {newSize});
	}

	public static bool SizeExists(string text)
	{
		var sizeGroupType = GetCurGroup();
		return FindSize(sizeGroupType, text) != -1;
	}
 
	public static bool SizeExists(GameViewSizeGroupType sizeGroupType, string text)
	{
		return FindSize(sizeGroupType, text) != -1;
	}

	public static int FindSize(string text)
	{
		GameViewSizeGroupType sizeGroupType = GetCurGroup();
		return FindSize(sizeGroupType, text);
	}

	public static int FindSize(GameViewSizeGroupType sizeGroupType, string text)
	{
		// GameViewSizes group = gameViewSizesInstance.GetGroup(sizeGroupType);
		// string[] texts = group.GetDisplayTexts();
		// for loop...
 
		var group = GetGroup(sizeGroupType);
		var getDisplayTexts = group.GetType().GetMethod("GetDisplayTexts");
		var displayTexts = getDisplayTexts.Invoke(group, null) as string[];
		for(int i = 0; i < displayTexts.Length; i++)
		{
			string display = displayTexts[i];
			// the text we get is "Name (W:H)" if the size has a name, or just "W:H" e.g. 16:9
			// so if we're querying a custom size text we substring to only get the name
			// You could see the outputs by just logging
			// Debug.Log(display);
			int pren = display.LastIndexOf('(');
			if (pren != -1)
				display = display.Substring(0, pren-1); // -1 to remove the space that's before the prens. This is very implementation-depdenent
			if (display == text)
				return i;
		}
		return -1;
	}

	public static void RemoveSome()
	{
		GameViewSizeGroupType sizeGroupType = GetCurGroup();
		var group = GetGroup(sizeGroupType);
		var getDisplayTexts = group.GetType().GetMethod("GetDisplayTexts");
		var removeSome = group.GetType().GetMethod("RemoveCustomSize");
		var displayTexts = getDisplayTexts.Invoke(group, null) as string[];
		for (int i = displayTexts.Length - 1; i >= 0; i--)
		{
			string display = displayTexts[i];
			if (display.Contains("Apple") || display.Contains("Inch"))
			{
				removeSome.Invoke(group, new object[] {i});
			}
		}
	}

	public static int FindSize(int width, int height)
	{
		GameViewSizeGroupType sizeGroupType = GetCurGroup();
		return FindSize(sizeGroupType, width, height);
	}

	public static int FindSize(GameViewSizeGroupType sizeGroupType, int width, int height)
	{
		// goal:
		// GameViewSizes group = gameViewSizesInstance.GetGroup(sizeGroupType);
		// int sizesCount = group.GetBuiltinCount() + group.GetCustomCount();
		// iterate through the sizes via group.GetGameViewSize(int index)
 
		var group = GetGroup(sizeGroupType);
		var groupType = group.GetType();
		var getBuiltinCount = groupType.GetMethod("GetBuiltinCount");
		var getCustomCount = groupType.GetMethod("GetCustomCount");
		int sizesCount = (int)getBuiltinCount.Invoke(group, null) + (int)getCustomCount.Invoke(group, null);
		var getGameViewSize = groupType.GetMethod("GetGameViewSize");
		var gvsType = getGameViewSize.ReturnType;
		var widthProp = gvsType.GetProperty("width");
		var heightProp = gvsType.GetProperty("height");
		var indexValue = new object[1];
		for(int i = 0; i < sizesCount; i++)
		{
			indexValue[0] = i;
			var size = getGameViewSize.Invoke(group, indexValue);
			int sizeWidth = (int)widthProp.GetValue(size, null);
			int sizeHeight = (int)heightProp.GetValue(size, null);
			if (sizeWidth == width && sizeHeight == height)
				return i;
		}
		return -1;
	}

	public static bool SizeExists(int width, int height)
	{
		GameViewSizeGroupType sizeGroupType = GetCurGroup();
		return SizeExists(sizeGroupType, width, height);
	}
	public static bool SizeExists(GameViewSizeGroupType sizeGroupType, int width, int height)
	{
		return FindSize(sizeGroupType, width, height) != -1;
	}

	static object GetGroup(GameViewSizeGroupType type)
	{
		return getGroup.Invoke(gameViewSizesInstance, new object[] { (int)type });
	}

	static GameViewSizeGroupType GetCurGroup()
	{
		return (GameViewSizeGroupType)getCurGroup.GetValue(gameViewSizesInstance, null);
	}
}
#endif