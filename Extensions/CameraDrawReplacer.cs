//----------------------------------------------------------------------------------------------
// Created by Leonid [Zanleo] Voitko (2018.04.05)
//----------------------------------------------------------------------------------------------

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class CameraDrawReplacer : MonoBehaviour
{
	public void OnEnable()
	{
		GetComponent<Camera>().SetReplacementShader(m_shader, m_tag);
	}
	public void OnDisable()
	{
		GetComponent<Camera>().SetReplacementShader(null, m_tag);
	}
	
	[SerializeField]
	private Shader m_shader = null;
	[SerializeField]
	private string m_tag = string.Empty;
}