//----------------------------------------------------------------------------------------------
// Created by Leonid [Zanleo] Voitko (2016.10.20)
//----------------------------------------------------------------------------------------------

namespace Effects.Camera
{
	public class PosZChangerByTimeScale : ChangerByTimeScale
	{
		protected override void ApplyNewValue(float newVal)
		{
			var curPos = transform.localPosition;
			curPos.z = newVal;
			transform.localPosition = curPos;
		}
	}
}