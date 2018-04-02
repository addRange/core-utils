//----------------------------------------------------------------------------------------------
// Created by Leonid [Zanleo] Voitko (2017.02.14)
//----------------------------------------------------------------------------------------------

// Later del it, because it resolve by AssemblyDefinitions
#if UNITY_ANDROID || UNITY_2017_3_OR_NEWER
using UnityEngine;

namespace Android
{
	public abstract class AndroidJavaObject<T> where T : AndroidJavaObject<T>
	{
		static AndroidJavaObject()
		{
		}

		public AndroidJavaObject(params object[] args)
		{
			m_object = new AndroidJavaObject(JavaClassPath, args);
		}

		protected AndroidJavaObject(AndroidJavaObject androidJavaObject)
		{
			m_object = androidJavaObject;
		}

		public AndroidJavaObject Object
		{
			get { return m_object; }
		}

		protected static AndroidJavaClass JavaClass
		{
			get { return s_javaClass ?? (s_javaClass = new AndroidJavaClass(JavaClassPath)); }
		}

		protected static string JavaClassPath
		{
			get { return s_javaClassPath; }
			set { s_javaClassPath = value; }
		}

		protected static string s_javaClassPath = null;
		private static AndroidJavaClass s_javaClass = null;

		private AndroidJavaObject m_object = null;
	}
}
#endif