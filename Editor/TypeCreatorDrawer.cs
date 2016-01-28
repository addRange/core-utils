// Copyright (c) 2014-2015. All rights reserved.
//----------------------------------------------------------------------------------------------
// author Leonid [Zanleo] Voitko
//----------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class TypeCreatorDrawer<T> where T : class
{
	public void Init(Func<Type, bool> selector = null)
	{
		Type curType = typeof(T);
		Type[] types = curType.Assembly.GetTypes();
		foreach (var type in types)
		{
			if (type.IsAbstract)
			{
				continue;
			}
			if (!curType.IsAssignableFrom(type))
			{
				continue;
			}
			if (selector != null && !selector(type))
			{
				continue;
			}
			m_types.Add(type);
		}
		m_names = m_types.Select<Type, string>(t => t.Name).ToArray();
		m_currentSelectedTypeIndex = 0;
	}

	public void DeInit()
	{
		m_currentSelectedTypeIndex = -1;
		m_names = null;
		m_types.Clear();
	}

	public void Draw(Action<Type> callbackNeedCreate)
	{
		EditorGUILayout.BeginHorizontal();
		m_currentSelectedTypeIndex = EditorGUILayout.Popup(m_currentSelectedTypeIndex, m_names);
		/*opt*/
		if (!GUILayout.Button("+", GUILayout.MaxWidth(22)))
		{
			EditorGUILayout.EndHorizontal();
			return;
		}
		callbackNeedCreate.SafeInvoke(m_types[m_currentSelectedTypeIndex]);
		EditorGUILayout.EndHorizontal();
	}

	private List<Type> m_types = new List<Type>();
	private string[] m_names = null;
	private int m_currentSelectedTypeIndex = -1;
}
