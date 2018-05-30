//----------------------------------------------------------------------------------------------
// Created by Leonid [Zanleo] Voitko (2018.05.30)
//----------------------------------------------------------------------------------------------

using System;
using Core.Utils;

/// <summary>
/// Path from Assets folder with it
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class FilePathAttribute : Attribute
{
	public FilePathAttribute(string relativePath)
	{
		Assert.IsFalse(string.IsNullOrEmpty(relativePath), "Invalid path");
		FilePath = relativePath;
	}

	public string FilePath { get; private set; }
}