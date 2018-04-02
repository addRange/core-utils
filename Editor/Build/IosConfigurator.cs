//----------------------------------------------------------------------------------------------
// Created by Leonid [Zanleo] Voitko (2017.12.21)
//----------------------------------------------------------------------------------------------

#if UNITY_IOS || UNITY_IPHONE
using UnityEngine;
using UnityEditor.Callbacks;
using UnityEditor;
using System;
using UnityEditor.iOS.Xcode;
using System.IO;

public class IosConfigurator : MonoBehaviour
{
	[PostProcessBuild]
	public static void ChangeXcodePlist(BuildTarget buildTarget, string pathToBuiltProject)
	{
		if (buildTarget != BuildTarget.iOS)
		{
			return;
		}

		// Get plist
		string plistPath = pathToBuiltProject + "/Info.plist";
		var plist = new PlistDocument();
		plist.ReadFromString(File.ReadAllText(plistPath));

		// Get root
		var rootDict = plist.root;

		var buildKey2 = "ITSAppUsesNonExemptEncryption";
		rootDict.SetString(buildKey2, "false");


		// Write to file
		File.WriteAllText(plistPath, plist.WriteToString());
	}

	[PostProcessBuild]
	public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
	{
		ChangeXcodePlist(target, pathToBuiltProject);
	}
}
#endif