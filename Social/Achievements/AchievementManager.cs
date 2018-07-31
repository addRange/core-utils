//----------------------------------------------------------------------------------------------
// author Leonid [Zanleo] Voitko
//----------------------------------------------------------------------------------------------

using System;

namespace Social.Achievements
{
	public class AchievementManager : BaseSocialElementManager<AchievementManager, Achievement>
	{
		protected override void Init()
		{
			base.Init();
			AchievementIds.InitInstance();
		}

		protected override void DeInit()
		{
			AchievementIds.FreeInstance();
			base.DeInit();
		}

		public void Show(Action<bool> callback = null)
		{
			Social.Log("Show achievmenets");
			Social.Instance.TryConnect(
				(res, error) =>
				{
					if (!res)
					{
						Social.Log("After connect res=" + res);
						callback.SafeInvoke(false);
						return;
					}
					
					Social.Log("Fact of ShowAchievementsUI");
					UnityEngine.Social.ShowAchievementsUI();
				});
		}
	}
}