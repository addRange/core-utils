//----------------------------------------------------------------------------------------------
// Created by Leonid [Zanleo] Voitko (2017.02.14)
//----------------------------------------------------------------------------------------------

// Later del it, because it resolve by AssemblyDefinitions
#if UNITY_ANDROID || UNITY_2017_3_OR_NEWER
using Android.Net;
using UnityEngine;

namespace Android.Support.V4.Content
{
	public class FileProvider : AndroidJavaObject<FileProvider>
	{
		static FileProvider()
		{
			JavaClassPath = "android.support.v4.content.FileProvider";
		}

		public FileProvider(AndroidJavaObject androidJavaObject) :
			base(androidJavaObject)
		{
		}

		public static Uri GetUriForFile(AndroidJavaObject context, string authority, object file)
		{
			var obj = JavaClass.CallStatic<AndroidJavaObject>("getUriForFile", context, authority, file);
			var uri = new Uri(obj);
			return uri;
		}
	}
}
#endif