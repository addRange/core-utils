//----------------------------------------------------------------------------------------------
// author Leonid [Zanleo] Voitko (2018.03.13)
//----------------------------------------------------------------------------------------------

using System.IO;

public static class PathExt
{
	public static string Combine(params string[] args)
	{
		string path = args[0];
		int i = 1;
		while (i < args.Length)
		{
			path = Path.Combine(path, args[i]);
			++i;
		}
		return path;
	}
}