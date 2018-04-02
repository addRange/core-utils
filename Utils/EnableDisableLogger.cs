//----------------------------------------------------------------------------------------------
// Created by Leonid [Zanleo] Voitko (2018.03.20)
//----------------------------------------------------------------------------------------------

using UnityEngine;

public class EnableDisableLogger : MonoBehaviour
{
	public void OnEnable()
	{
		Debug.Log("OnEnable " + this, this);
	}
	public void OnDisable()
	{
		Debug.Log("OnDisable " + this, this);
	}
}