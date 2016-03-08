// Created by Leonid [Zanleo] Voitko (2014)

#if UNITY_EDITOR
#define ENABLE_ASSERTS_IN_EDITOR
#endif

using System;
using System.Diagnostics;

public class Assert
{
#if !ENABLE_ASSERTS_IN_EDITOR
	[Conditional("UNITY_ASSERTIONS"), DebuggerStepThrough]
#endif
	public static void Test(bool logic, string debugString = "Assert!", UnityEngine.Object targetObject = null)
	{
		if (logic)
		{
			return;
		}

		UnityEngine.Debug.LogWarning(debugString, targetObject);
	}

#if !ENABLE_ASSERTS_IN_EDITOR
	[Conditional("UNITY_ASSERTIONS"), DebuggerStepThrough]
#endif
	public static void Test(bool logic, Func<string> debugStringGetter, UnityEngine.Object targetObject = null)
	{
		if (logic)
		{
			return;
		}

		UnityEngine.Debug.LogWarning(debugStringGetter(), targetObject);
	}

#if !ENABLE_ASSERTS_IN_EDITOR
	[Conditional("UNITY_ASSERTIONS"), DebuggerStepThrough]
#endif
	public static void Test(bool logic, UnityEngine.Object targetObject)
	{
		Assert.Test(logic, "Assert!", targetObject);
	}

	[DebuggerStepThrough]
	public static void Throw(string debugString)
	{
		UnityEngine.Debug.LogWarning(debugString);
	}

	[DebuggerStepThrough]
	public static void Throw(string debugString, UnityEngine.Object targetObject)
	{
		UnityEngine.Debug.LogWarning(debugString, targetObject);
	}
}
