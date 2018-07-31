//----------------------------------------------------------------------------------------------
// Created by Leonid [Zanleo] Voitko (2017.08.10)
//----------------------------------------------------------------------------------------------

#if UNITY_2018_1_OR_NEWER
using System;
using UnityEngine;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public class BuildInformer : IPostprocessBuildWithReport
{
	int IOrderedCallback.callbackOrder { get { return int.MaxValue; } }

	public void OnPostprocessBuild(BuildReport report)
	{
		Debug.Log("BuildTime = " + new DateTime(report.summary.totalTime.Ticks));
	}
}
#endif