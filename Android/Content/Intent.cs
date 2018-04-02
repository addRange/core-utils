//----------------------------------------------------------------------------------------------
// Created by Leonid [Zanleo] Voitko (2017.02.14)
//----------------------------------------------------------------------------------------------

// Later del it, because it resolve by AssemblyDefinitions
#if UNITY_ANDROID || UNITY_2017_3_OR_NEWER
using UnityEngine;

namespace Android.Content
{
	public class Intent : AndroidJavaObject<Intent>
	{
		static Intent()
		{
			JavaClassPath = "android.content.Intent";
		}

		public Intent SetFlags(int flags)
		{
			// in fact returnedIntent must be == Object
			Object.Call<AndroidJavaObject>("setFlags", flags);
			return this;
		}

		public Intent SetAction(string action)
		{
			// in fact returnedIntent must be == Object
			Object.Call<AndroidJavaObject>("setAction", action);
			return this;
		}

		public Intent SetType(string type)
		{
			// in fact returnedIntent must be == Object
			Object.Call<AndroidJavaObject>("setType", type);
			return this;
		}

		public Intent PutExtra<T>(string action, AndroidJavaObject<T> bundle) where T : AndroidJavaObject<T>
		{
			// in fact returnedIntent must be == Object
			Object.Call<AndroidJavaObject>("putExtra", action, bundle.Object);
			return this;
		}

		public static int FLAG_GRANT_READ_URI_PERMISSION
		{
			get { return JavaClass.GetStatic<int>("FLAG_GRANT_READ_URI_PERMISSION"); }
		}

		public static string ACTION_SEND
		{
			get { return JavaClass.GetStatic<string>("ACTION_SEND"); }
		}
		public static string EXTRA_STREAM
		{
			get { return JavaClass.GetStatic<string>("EXTRA_STREAM"); }
		}
	}
}
#endif