//----------------------------------------------------------------------------------------------
// Created by Leonid [Zanleo] Voitko (2018.02.15)
//----------------------------------------------------------------------------------------------

#if UNITY_IPHONE
public static class PbxProjectExt
{
	public static bool HasFramework(this UnityEditor.iOS.Xcode.PBXProject _this, string framework)
	{
		return _this.ContainsFileByRealPath("System/Library/Frameworks/" + framework);
	}
}
#endif