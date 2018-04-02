//----------------------------------------------------------------------------------------------
// Created by Leonid [Zanleo] Voitko (02/10/2016)
//----------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Core.Utils
{
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
				StringBuilder sb = new StringBuilder(condition.Length + stacktrace.Length);
				sb.AppendLine(type.ToString());
				sb.AppendLine(condition);
				sb.AppendLine(stacktrace);
				string fileName = "CrashReport" + DateTime.UtcNow.ToString("yyyy_MM_dd hh_mm_ss") + ".txt";
				string path = Application.persistentDataPath + "/" + fileName;
				using (var file = File.CreateText(path))
				{
					file.Write(sb);
				}
				//Debug.LogError("Some crashreporter info " + condition + "; " + stacktrace);
#if UNITY_EDITOR
				if (Environment.MachineName.ToLower().Contains("zanleo") ||
					 Environment.UserName.ToLower().Contains("zanleo"))
				{
					UnityEditor.EditorApplication.isPaused = true;
				}
				else
				{
					UnityEditor.EditorApplication.isPlaying = false;
				}
#else
// TODO: test it
				Application.Quit();
#endif
			}
		}
	}
}