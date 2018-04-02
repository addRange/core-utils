//----------------------------------------------------------------------------------------------
// author Leonid [Zanleo] Voitko
//----------------------------------------------------------------------------------------------

using System;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.Impl;

namespace Social.LeaderBoards
{
	[Serializable]
	public abstract class LeaderBoard : BaseElement, IScore
		// For localLeaderBoard always report
#if UNITY_EDITOR
		, ISerializationCallbackReceiver
#endif
	{
		protected override string GetPrefsPrefix()
		{
			return "LeaderBoard";
		}
		// For localLeaderBoard always report
#if UNITY_EDITOR
		public void OnBeforeSerialize() { }

		public void OnAfterDeserialize()
		{
			m_reported = false;
		}
#endif
		#region ILeaderboard

		public void ReportScore(Action<bool> callback)
		{
			IsLoadingOrReporting = true;
			bool isUnityImpl = UnityEngine.Social.Active.localUser is LocalUser;
			if (isUnityImpl || !UnityEngine.Social.Active.localUser.authenticated)
			{
				// TODO: mark that not reported
				return;
			}

			UnityEngine.Social.Active.ReportScore(value, leaderboardID, (reportRes) =>
			{
				Debug.Log("ReportScore res=" + reportRes);
				IsLoadingOrReporting = false;
				m_reported = reportRes;
				callback.SafeInvoke(m_reported);
			});
		}

		public void LoadScore(Action<IScore[]> callback)
		{
			IsLoadingOrReporting = true;
			UnityEngine.Social.Active.LoadScores(leaderboardID, (scores) =>
			{
				IsLoadingOrReporting = false;
				callback.SafeInvoke(scores);
			});
		}

		public string leaderboardID
		{
			get { return LeaderBoardIds.Instance.GetId(m_localId); }
			set { throw new NotImplementedException(); }
		}

		public long value
		{
			get { return m_value; }
			set
			{
				if (m_value >= value)
				{
					return;
				}

				m_value = value;
				m_reported = false;
				LeaderBoardManager.Instance.ReportScore(this, false);
			}
		}

		public DateTime date
		{
			get { throw new NotImplementedException(); }
		}

		public string formattedValue
		{
			// TODO
			get { return value.ToString(); }
		}

		public string userID
		{
			// TODO
			get { return UnityEngine.Social.Active.localUser.id; }
		}

		// TODO?
		public int rank
		{
			get { throw new NotImplementedException(); }
		}

		#endregion

		// TODO: replace to counter or add asseert in report/load
		public bool IsLoadingOrReporting { get; private set; }

		public bool IsReported
		{
			get
			{
				return m_reported;
			}
		}

		[SerializeField, HideInInspector]
		private long m_value = 0;

		[SerializeField, HideInInspector]
		private bool m_reported = true;
	}
}