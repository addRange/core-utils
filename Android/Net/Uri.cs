//----------------------------------------------------------------------------------------------
// Created by Leonid [Zanleo] Voitko (2017.02.14)
//----------------------------------------------------------------------------------------------

// Later del it, because it resolve by AssemblyDefinitions
#if UNITY_ANDROID || UNITY_2017_3_OR_NEWER
using UnityEngine;

namespace Android.Net
{
	public class Uri : AndroidJavaObject<Uri>
	{
		static Uri()
		{
			JavaClassPath = "android.net.Uri";
		}

		public Uri(AndroidJavaObject androidJavaObject) :
			base(androidJavaObject)
		{
		}

		public static Uri Parse(string path)
		{
			var obj = JavaClass.CallStatic<AndroidJavaObject>("parse", path);
			var uri = new Uri(obj);
			return uri;
		}
	}
}
#endif