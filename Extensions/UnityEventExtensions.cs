//----------------------------------------------------------------------------------------------
// author Leonid [Zanleo] Voitko
//----------------------------------------------------------------------------------------------

using UnityEngine.Events;

public static class UnityEventExtensions
{
	public static void SafeInvoke(this UnityEvent _this)
	{
		if (_this != null)
		{
			_this.Invoke();
		}
	}

	public static void SafeInvoke<T1>(this UnityEvent<T1> _this, T1 param1)
	{
		if (_this != null)
		{
			_this.Invoke(param1);
		}
	}

	public static void SafeInvoke<T1, T2>(this UnityEvent<T1, T2> _this, T1 param1, T2 param2)
	{
		if (_this != null)
		{
			_this.Invoke(param1, param2);
		}
	}

	public static void SafeInvoke<T1, T2, T3>(this UnityEvent<T1, T2, T3> _this, T1 param1, T2 param2, T3 param3)
	{
		if (_this != null)
		{
			_this.Invoke(param1, param2, param3);
		}
	}

	public static void SafeInvoke(this UnityAction _this)
	{
		if (_this != null)
		{
			_this.Invoke();
		}
	}

	public static void SafeInvoke<T1>(this UnityAction<T1> _this, T1 param1)
	{
		if (_this != null)
		{
			_this.Invoke(param1);
		}
	}

	public static void SafeInvoke<T1, T2>(this UnityAction<T1, T2> _this, T1 param1, T2 param2)
	{
		if (_this != null)
		{
			_this.Invoke(param1, param2);
		}
	}
}