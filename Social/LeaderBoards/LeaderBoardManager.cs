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
			Social.Log("ReportScore withConnect=" + withConnect + "; " + leaderBoard.leaderboardID);
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
						
						Social.Log("Fact of ReportScore " + leaderBoard.leaderboardID);
						leaderBoard.ReportScore(callback);
					});
			}
			else
			{
				Social.Log("ReportScore Social.IsConnected=" + Social.Instance.IsConnected + "; " + leaderBoard.leaderboardID);
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
			Social.Log("LoadScores withConnect=" + withConnect + "; " + leaderBoard.leaderboardID);
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
						
						Social.Log("Fact of loadScores; " + leaderBoard.leaderboardID);
						leaderBoard.LoadScore(callback);
					});
			}
			else
			{
				Social.Log("Fact of loadScores; " + leaderBoard.leaderboardID);
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
			Social.Log("Show leaderboards");
			Social.Instance.TryConnect(
				(res, error) =>
				{
					if (!res)
					{
						callback.SafeInvoke(false);
						return;
					}
					
					Social.Log("Fact of ShowLeaderboardUI");
					UnityEngine.Social.ShowLeaderboardUI();
				});
		}
	}
}