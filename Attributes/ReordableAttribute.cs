//----------------------------------------------------------------------------------------------
// Created by Leonid [Zanleo] Voitko (2016.10.24)
//----------------------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using UnityEngine;

[AttributeUsage(AttributeTargets.All)]
[Conditional("UNITY_EDITOR")]
public class ReordableAttribute : PropertyAttribute
{
	public ReordableAttribute()
	{
	}
}