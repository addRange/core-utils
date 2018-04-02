//----------------------------------------------------------------------------------------------
// author Leonid [Zanleo] Voitko
//----------------------------------------------------------------------------------------------

using System;
using UnityEngine.SocialPlatforms;

namespace Social.LeaderBoards
{
	public class LeaderBoardManager : BaseSocialElementManager<LeaderBoardManager, LeaderBoard>
	{
		protected override void Init()
		{
			base.Init();
			LeaderBoardIds.InitInstance();
		}

		protected override void DeInit()
		{
			LeaderBoardIds.FreeInstance();
			base.DeInit();
		}

		public void ReportScore(LeaderBoard leaderBoard, bool withConnect = true, Action<bool> callback = null)
		{
			if (withConnect)
			{
				Social.Instance.TryConnect(
					(res, error) =>
					{
						if (!res)
						{
							callback.SafeInvoke(false);
							return;
						}

						leaderBoard.ReportScore(callback);
					});
			}
			else
			{
				if (Social.Instance.IsConnected)
				{
					leaderBoard.ReportScore(callback);
				}
				else
				{
					callback.SafeInvoke(false);
				}
			}
		}

		public void LoadScores(LeaderBoard leaderBoard, bool withConnect = true, Action<IScore[]> callback = null)
		{
			if (withConnect)
			{
				Social.Instance.TryConnect(
					(res, error) =>
					{
						if (!res)
						{
							callback.SafeInvoke(null);
							return;
						}

						leaderBoard.LoadScore(callback);
					});
			}
			else
			{
				if (Social.Instance.IsConnected)
				{
					leaderBoard.LoadScore(callback);
				}
				else
				{
					callback.SafeInvoke(null);
				}
			}
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

					UnityEngine.Social.ShowLeaderboardUI();
				});
		}
	}
}