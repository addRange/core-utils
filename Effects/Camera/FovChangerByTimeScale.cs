//----------------------------------------------------------------------------------------------
// Created by Leonid [Zanleo] Voitko (2016.10.20)
//----------------------------------------------------------------------------------------------

namespace Effects.Camera
{
	public class FovChangerByTimeScale : ChangerByTimeScale
	{
		protected override void ApplyNewValue(float newVal) { GetComponent<UnityEngine.Camera>().fieldOfView = newVal; }
	}
}