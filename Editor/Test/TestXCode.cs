//----------------------------------------------------------------------------------------------
// Created by Leonid [Zanleo] Voitko (2018.03.16)
//----------------------------------------------------------------------------------------------

//#define UNITY_IPHONE

using System.Collections.Generic;
using System.IO;
using ChillyRoom.UnityEditor.iOS.Xcode;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class TestXCode : MonoBehaviour
{
	const string PathPrefix = @"d:\XXX\ZPlayIntegration\EmptyProjects3\4\";
	const string FullFrameworkPath = PathPrefix + @"Frameworks\YumiMediationAdMob";
	const string FrameworkDirRelPath = @"Frameworks\YumiMediationAdMob";
	const string FrameworkDirName = @"YumiMediationAdMob";
	const string FrameworkRelPath = FrameworkDirRelPath + @"\" + FrameworkName;
	const string FrameworkName = @"YumiMediationAdMob.framework";
	const string ResFunllBundleName = FrameworkDirRelPath + @"\" + ResBundleName;
	const string ResBundleName = @"Resources\" + BundleName;
	const string BundleName = @"YumiMediationAdMob.bundle";

	[MenuItem("Assets/Create/App Store Settings2")]
	public static void CheckAddFramework()
	{
		string buildPath = PathPrefix;
		
		var projPath = PBXProject.GetPBXProjectPath(buildPath);
		PBXProject proj = new PBXProject();
		proj.ReadFromFile(projPath);
		string target = proj.TargetGuidByName("Unity-iPhone");
		
		List<string> relPathesForGroups = new List<string>();
		CollectRelResources(relPathesForGroups, PathPrefix, FullFrameworkPath);
		string relativeFrameworkPath = FullFrameworkPath.Substring(PathPrefix.Length);
		var fileGuids = proj.CreateGroups(relativeFrameworkPath, relPathesForGroups.ToArray());
		for (var i = 0; i < fileGuids.Length; i++)
		{
			proj.AddFileToBuild(target, fileGuids[i]);
			if (relPathesForGroups[i].EndsWith(".framework"))
			{
				proj.AddBuildProperty(target, "FRAMEWORK_SEARCH_PATHS", "$(PROJECT_DIR)/" + FixSlashes(relPathesForGroups[i]));
			}
		}
		proj.WriteToFile(projPath);

		Debug.Log("Done");
	}

	private static void CollectRelResources(List<string> resources, string origFolder, string destFolder)
	{
		string relativePath;
		//Debug.Log(">>>>>>>>>> CollectRelResources " + "\r\n" + origFolder + "\r\n" + destFolder);
		DirectoryInfo dir = new DirectoryInfo(destFolder);
		
		DirectoryInfo[] dirs = dir.GetDirectories();
		// copying subdirectories, copy them and their contents to new location.
		foreach (DirectoryInfo subdir in dirs)
		{
			string fullPath = subdir.FullName;
			if (fullPath.EndsWith(".framework"))
			{
				relativePath = subdir.FullName.Substring(origFolder.Length);
				resources.Add(relativePath);
				//Debug.Log("Add " + subdir.Name + "; " + Path.GetFileName(relativePath));
				continue;
			}
			if (fullPath.EndsWith(".bundle"))
			{
				relativePath = fullPath.Substring(origFolder.Length);
				resources.Add(relativePath);
				//Debug.Log("Add " + subdir.Name + "; " + Path.GetFileName(relativePath));
				continue;
			}

			CollectRelResources(resources, origFolder, fullPath);
		}
		//Debug.Log("<<<<<<<<<< End CollectRelResources");
	}

	//private PBXGroupData CreateGroupForProject(PBXProject proj, string sourceGroupName, string sourceGroup)
	//{
	//	sourceGroup = FixSlashes(sourceGroup);
	//	PBXGroupData gr = proj.GroupsGetByProjectPath(sourceGroup);
	//	if (gr != null)
	//	{
	//		Debug.Log("Already exist");
	//		return gr;
	//	}

	//	// the group does not exist -- create new
	//	gr = proj.GroupsGetMainGroup();
	//	PBXGroupData newGroup = PBXGroupData.Create(sourceGroupName, sourceGroup, PBXSourceTree.Group);
	//	gr.children.AddGUID(newGroup.guid);
	//	GroupsAdd(projectPath, gr, newGroup);
	//	gr = newGroup;
	//}

	private void CheckAddDirRecursivly(PBXProject proj, string target, string origFolder, string destFolder, string ident = "")
	{
	//	string bundleGuid = proj.AddFile(origFolder, destFolder, PBXSourceTree.Sdk);
	//	proj.AddFileToBuild(target, bundleGuid);
	//	//return;

		// 1) AddFile for ".bundle" and ".framework"
		//string fileGuid = proj.AddFile(destFolder, "Frameworks/" + dirName, PBXSourceTree.Sdk);
		// 2) Build pahse only ".bundle"
		//proj.AddFileToBuild(target, fileGuid);
		// 3) FRAMEWORK_SEARCH_PATHS only ".framework"
		//proj.AddBuildProperty(target, "FRAMEWORK_SEARCH_PATHS", "$(PROJECT_DIR)/Frameworks/" + dirName);
		string relativePath;
		//Debug.Log(ident + ">>>>>>>>>> CheckAddDirRecursivly " + "\r\n" + origFolder + "\r\n" + destFolder);
		DirectoryInfo dir = new DirectoryInfo(destFolder);
		FileInfo[] files = dir.GetFiles();
		foreach (FileInfo file in files)
		{
			//Debug.Log(ident + "file=" + file.FullName);
			if (!file.FullName.EndsWith(".bundle"))
			{
				// What to od with this files?
				continue;
			}
			
			relativePath = file.FullName.Substring(origFolder.Length);
			//Debug.Log(origFolder + "\r\n" + file.FullName + "\r\n" + relativePath);
			relativePath = FixSlashes(relativePath);
			if (relativePath.StartsWith("/"))
			{
				relativePath = relativePath.Substring(1);
			}
			var fullRelativePath = Path.Combine("Frameworks/", relativePath);
			//Debug.Log(fullRelativePath);
			fullRelativePath = FixSlashes(fullRelativePath);
			//Debug.Log(fullRelativePath);
			string bundleGuid = proj.AddFile(file.FullName, fullRelativePath, PBXSourceTree.Sdk);
			proj.AddFileToBuild(target, bundleGuid);
		}

		DirectoryInfo[] dirs = dir.GetDirectories();
		// copying subdirectories, copy them and their contents to new location.
		foreach (DirectoryInfo subdir in dirs)
		{
			//Debug.Log(ident + "dir=" + subdir.FullName);
			if (subdir.FullName.EndsWith(".framework"))
			{
				relativePath = subdir.FullName.Substring(origFolder.Length);
				//Debug.Log(origFolder + "\r\n" + subdir.FullName + "\r\n" + relativePath);
				relativePath = FixSlashes(relativePath);
				if (relativePath.StartsWith("/"))
				{
					relativePath = relativePath.Substring(1);
				}
				var fullRelativePath = Path.Combine("Frameworks/", relativePath);
				//Debug.Log(fullRelativePath);
				fullRelativePath = FixSlashes(fullRelativePath);
				//Debug.Log(fullRelativePath);
				//string frameworkGuid = 
				proj.AddFile(subdir.FullName, fullRelativePath, PBXSourceTree.Sdk);
				proj.AddBuildProperty(target, "FRAMEWORK_SEARCH_PATHS", "$(PROJECT_DIR)/" + fullRelativePath);
				continue;
			}

			CheckAddDirRecursivly(proj, target, origFolder, subdir.FullName, ident + "   ");
		}

		//Debug.Log(ident + "<<<<<<<<<< End CheckAddDirRecursivly");
	}

	public static string FixSlashes(string path)
	{
		if (path == null)
			return null;
		return path.Replace('\\', '/');
	}
}