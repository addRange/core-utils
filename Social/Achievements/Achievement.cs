//----------------------------------------------------------------------------------------------
// author Leonid [Zanleo] Voitko
//----------------------------------------------------------------------------------------------

using System;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.Impl;

namespace Social.Achievements
{
	[Serializable]
	public abstract class Achievement : BaseElement, IAchievement
	{
		public override void Init()
		{
			base.Init();
			// TODO: think about correct report score if not reported
		}

		protected override string GetPrefsPrefix()
		{
			return "Achievement";
		}

		#region IAchievement

		public void ReportProgress(Action<bool> callback)
		{
			Profiler.BeginSample("ReportProgress");
            if (Application.platform == RuntimePlatform.OSXEditor ||
                Application.platform == RuntimePlatform.LinuxEditor ||
                Application.platform == RuntimePlatform.WindowsEditor ||
                !UnityEngine.Social.Active.localUser.authenticated)
			{
				callback.SafeInvoke(false);
				// TODO: mark that not reported
				Profiler.EndSample();
				return;
			}

			UnityEngine.Social.Active.ReportProgress(id, percentCompleted, (success) =>
			{
				// mark for reported?
				callback.SafeInvoke(success);
			});
			Profiler.EndSample();
		}

		public string id
		{
			get { return AchievementIds.Instance.GetId(m_localId); }
			set { throw new NotImplementedException(); }
		}

		/// <summary>
		/// Must be between 0.0 and 100.0
		/// </summary>
		public double percentCompleted
		{
			get { return m_percentCompleted; }
			set
			{
				double prevCompleted = m_percentCompleted;
				m_percentCompleted = value;
				if (prevCompleted < 100.0 && m_percentCompleted >= 100.0)
				{
					// TODO: think about mark of reported
					ReportProgress(null);
				}
			}
		}

		public bool completed
		{
			get { return percentCompleted >= 100.0; }
		}

		// TODO
		public bool hidden
		{
			get { return false; }
		}

		// TODO
		public DateTime lastReportedDate
		{
			get { return DateTime.Now; }
		}

		#endregion

		[SerializeField, HideInInspector]
		private double m_percentCompleted = 0;
	}
}