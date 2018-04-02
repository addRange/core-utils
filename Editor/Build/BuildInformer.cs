//----------------------------------------------------------------------------------------------
// Created by Leonid [Zanleo] Voitko (2017.08.10)
//----------------------------------------------------------------------------------------------

#if UNITY_5_6_OR_NEWER
using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;

public class BuildInformer : IPreprocessBuild, IPostprocessBuild
{
	int IOrderedCallback.callbackOrder { get { return int.MaxValue; } }

	public void OnPreprocessBuild(BuildTarget target, string path)
	{
		//Debug.Log("MyCustomBuildProcessor.OnPreprocessBuild for target " + target + " at path " + path);
		m_startBuildTime = DateTime.Now;
	}
	public void OnPostprocessBuild(BuildTarget target, string path)
	{
		//Debug.Log("MyCustomBuildProcessor.OnPostprocessBuild for target " + target + " at path " + path);
		Debug.Log("BuildTime = " + (DateTime.Now - m_startBuildTime));
	}

	private static DateTime m_startBuildTime = DateTime.Now;
}
#endif