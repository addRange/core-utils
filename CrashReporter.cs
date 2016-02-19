//----------------------------------------------------------------------------------------------
// Created by Leonid [Zanleo] Voitko (02/10/2016)
//----------------------------------------------------------------------------------------------

using UnityEngine;

public class CrashReporter : Singleton<CrashReporter>
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	public static void InitOnLoad()
	{
		CrashReporter.InitInstance();
	}

	protected override void Init()
	{
		base.Init();
		Application.logMessageReceived += OnLogMessageReceived;
	}

	private void OnLogMessageReceived(string condition, string stacktrace, LogType type)
	{
		if (type == LogType.Exception)
		{
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#else
			// TODO: test it
			Application.Quit();
#endif
		}
	}
}