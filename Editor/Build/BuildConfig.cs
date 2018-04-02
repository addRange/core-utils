//----------------------------------------------------------------------------------------------
// Created by Leonid [Zanleo] Voitko (2017.01.25)
//----------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Core.Utils;
using UnityEditor;
using UnityEngine;

public class BuildConfig : ScriptableObject
{
	[Serializable]
	private enum FlagBuildOptions
	{
		None = 0,
		Development = 1,
		AutoRunPlayer = 4,
		ShowBuiltPlayer = 8,
		BuildAdditionalStreamedScenes = 16,
		AcceptExternalModificationsToPlayer = 32,
		InstallInBuildFolder = 64,
		ConnectWithProfiler = 256,
		AllowDebugging = 512,
		SymlinkLibraries = 1024,
		UncompressedAssetBundle = 2048,
		ConnectToHost = 4096,
		EnableHeadlessMode = 16384,
		BuildScriptsOnly = 32768,
		Il2CPP = 65536,
		ForceEnableAssertions = 131072,
		ForceOptimizeScriptCompilation = 524288,
		ComputeCRC = 1048576,
		StrictMode = 2097152,
	}

	[MenuItem("Assets/Create/BuildConfig")]
	public static void CreateAsset()
	{
		ScriptableObjectUtility.CreateAsset<BuildConfig>(withLog: true);
	}

	public void Apply()
	{
	}

	public BuildPlayerOptions GetBuildPlayerOptions(string buildPath, BuildTarget targetBuildPlatform)
	{
		return new BuildPlayerOptions()
				 {
					 locationPathName = GetFullPath(buildPath),
					 scenes = GetBuildScene(),
					 target = targetBuildPlatform,
					 options = GetBuildOptions()
				 };
	}

	public string GetFullPath(string buildPath)
	{
		// TODO: need change fileName for platforms other then Android
		// for example can get ext`s from http://wiki.unity3d.com/index.php/AutoBuilder
		string fileName = "bin.apk";

		string fullPath = System.IO.Path.Combine(buildPath, fileName);
		return fullPath;
	}

	private string[] GetBuildScene()
	{
		List<string> scenes = new List<string>();
		foreach (var editorBuildSettingsScene in EditorBuildSettings.scenes)
		{
			scenes.Add(editorBuildSettingsScene.path);
		}

		return scenes.ToArray();
	}

	public BuildOptions GetBuildOptions()
	{
		int total = 0;
		for (int i = 0; i < m_BuildOptions.Length; ++i)
		{
			total |= (int) m_BuildOptions[i];
		}

		return (BuildOptions) total;
	}

	[SerializeField]
	private FlagBuildOptions[] m_BuildOptions = new FlagBuildOptions[0];
}