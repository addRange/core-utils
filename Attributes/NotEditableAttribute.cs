//----------------------------------------------------------------------------------------------
// Created by Leonid [Zanleo] Voitko (1/24/2016)
//----------------------------------------------------------------------------------------------

using System.Diagnostics;
using UnityEngine;

[Conditional("UNITY_EDITOR")]
public class NotEditableAttribute : PropertyAttribute
{
	public NotEditableAttribute()
	{
	}
}