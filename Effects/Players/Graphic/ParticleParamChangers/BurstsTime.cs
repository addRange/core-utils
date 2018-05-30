//----------------------------------------------------------------------------------------------
// Created by Leonid [Zanleo] Voitko (2016.12.16)
//----------------------------------------------------------------------------------------------

using UnityEngine;

namespace Effects.Particle.ParamChangers
{
	public class BurstsTime : ParticleParamChanger
	{
		protected override void SetValue(float value)
		{
			var emission = ParticleSystem.emission;
			var bursts = new ParticleSystem.Burst[emission.burstCount];
			emission.GetBursts(bursts);
			for (int i = 0; i < bursts.Length; i++)
			{
				var burst = bursts[i];
				burst.time = value;
			}
			emission.SetBursts(bursts);
		}
	}
}