// Copyright (c) 2014-2014. All rights reserved.
//----------------------------------------------------------------------------------------------
// author Leonid [Zanleo] Voitko
//----------------------------------------------------------------------------------------------

using System;
using UnityEngine.Events;

public static class ActionExtensions
{
	public static void SafeInvoke(this Action _this)
	{
		if (_this != null)
		{
			_this.Invoke();
		}
	}

	public static void SafeInvoke<T1>(this Action<T1> _this, T1 param1)
	{
		if (_this != null)
		{
			_this.Invoke(param1);
		}
	}

	public static void SafeInvoke<T1, T2>(this Action<T1, T2> _this, T1 param1, T2 param2)
	{
		if (_this != null)
		{
			_this.Invoke(param1, param2);
		}
	}

	public static void SafeInvoke<T1, T2, T3>(this Action<T1, T2, T3> _this, T1 param1, T2 param2, T3 param3)
	{
		if (_this != null)
		{
			_this.Invoke(param1, param2, param3);
		}
	}
}