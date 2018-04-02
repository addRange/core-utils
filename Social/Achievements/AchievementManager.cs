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
			Social.Instance.TryConnect(
				(res, error) =>
				{
					if (!res)
					{
						callback.SafeInvoke(false);
						return;
					}

					UnityEngine.Social.ShowAchievementsUI();
				});
		}
	}
}