//Created by Leonid [Zanleo] Voitko (2014)
using System.Diagnostics;

// TODO 
public class Assert
{
	[Conditional("UNITY_EDITOR")]
	public static void Test(bool logic, string debugString) {
		if (logic) return;

		UnityEngine.Debug.LogWarning(debugString);
	}

	[Conditional("UNITY_EDITOR")]
	public static void Test(bool logic) {
		if (logic) return;

		UnityEngine.Debug.LogWarning("Assert!");
	}

	[Conditional("UNITY_EDITOR")]
	public static void Test(bool logic, string debugString, UnityEngine.Object targetObject) {
		if (logic) return;

		UnityEngine.Debug.LogWarning(debugString, targetObject);
	}

	public static void Throw(string debugString) {
		UnityEngine.Debug.LogWarning(debugString);
	}

	public static void Throw(string debugString, UnityEngine.Object targetObject) {
		UnityEngine.Debug.LogWarning(debugString, targetObject);
	}
}
