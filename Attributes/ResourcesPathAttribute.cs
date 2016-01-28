//----------------------------------------------------------------------------------------------
// Created by Leonid [Zanleo] Voitko (1/13/2016)
//----------------------------------------------------------------------------------------------

using System;
using UnityEngine;

public class ResourcesPathAttribute : PropertyAttribute
{
	public ResourcesPathAttribute()
		: this(typeof(UnityEngine.Object))
	{
	}

	public ResourcesPathAttribute(Type assetType)
	{
		AssetType = assetType;
	}

	public Type AssetType { get; private set; }
}