// Copyright (c) 2014-2014. All rights reserved.
//----------------------------------------------------------------------------------------------
// author Leonid [Zanleo] Voitko
//----------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

public static class ListExtensions
{
	public static string JoinComma<T>(this IList<T> _this)
	{
		return String.Join(", ", _this.Select(i => i.ToString()).ToArray());
	}
	
	public static T Random<T>(this IList<T> _this)
	{
		return Random(_this, UnityEngine.Random.Range);
	}
	public static T Random<T>(this IList<T> _this, Func<int, int, int> randomFunc)
	{
		if (_this.Count == 0)
		{
			return default(T);
		}
		return _this[randomFunc(0, _this.Count)];
	}

	/// <summary>
	/// Finds the item in list in random order
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="listToSearch"></param>
	/// <param name="predicate"></param>
	/// <returns> Returns index of the found element. If none was found -1 is returned. </returns>
	public static int FindRandomItemIndex<T>(this List<T> listToSearch, Predicate<T> predicate)
	{
		List<int> indicesList = new List<int>();
		for (int i = 0; i < listToSearch.Count; ++i)
		{
			indicesList.Add(i);
		}

		int idx = UnityEngine.Random.Range(0, indicesList.Count);
		bool found = false;
		while (indicesList.Count != 0 && !found)
		{
			found = predicate.Invoke(listToSearch[indicesList[idx]]);
			if (!found)
			{
				indicesList.RemoveAt(idx);
				if (indicesList.Count > 0)
				{
					idx = UnityEngine.Random.Range(0, indicesList.Count);
				}
			}
		}

		return found ? indicesList[idx] : -1;
	}
}