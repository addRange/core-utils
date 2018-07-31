//----------------------------------------------------------------------------------------------
// Created by Leonid [Zanleo] Voitko (2017.01.25)
//----------------------------------------------------------------------------------------------

using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class Build
{
	public const string BuildPath = "Build";

	[MenuItem("Assets/BuildProject")]
	public static void BuildProject()
	{
		string targetBuildPresetName = "Release";
		BuildTarget targetBuildPlatform = BuildTarget.Android;

		for (int i = 0; i < Environment.GetCommandLineArgs().Length; i++)
		{
			var commandLineArg = Environment.GetCommandLineArgs()[i];
			if (commandLineArg.StartsWith("-buildPreset"))
			{
				targetBuildPresetName = commandLineArg.Split('=')[1];
			}
			if (commandLineArg.StartsWith("-switchBuildTarget"))
			{
				string platformName = commandLineArg.Split('=')[1];
				targetBuildPlatform = (BuildTarget)Enum.Parse(typeof(BuildTarget), platformName);
			}
			//Debug.Log(i + ") " + commandLineArg);
		}

		Debug.Log("TargetBuildPreset='" + targetBuildPresetName + "' activeBuildTarget='" + targetBuildPlatform + "'");
		var targetBuildPreset =
			AssetDatabase.LoadAssetAtPath<BuildConfig>("Assets/BuildPresets/" + targetBuildPresetName + ".asset");
		if (System.IO.Directory.Exists(BuildPath))
		{
			System.IO.Directory.Delete(BuildPath, true);
		}
		System.IO.Directory.CreateDirectory(BuildPath);
		targetBuildPreset.Apply();
		var result = BuildPipeline.BuildPlayer(targetBuildPreset.GetBuildPlayerOptions(BuildPath, targetBuildPlatform));
		switch (result.summary.result)
		{
			case BuildResult.Failed:
				Debug.LogWarning("Build Failed!");
				break;
			case BuildResult.Succeeded:
			default:
				Debug.Log("Build Success!");
				break;
		}
	}
}