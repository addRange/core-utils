//----------------------------------------------------------------------------------------------
// author Leonid [Zanleo] Voitko
//----------------------------------------------------------------------------------------------

using System;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

namespace Social
{
	public class Social : SingletonGameObject<Social>
	{
		public SocialPlatform GetSocialPlatform()
		{
#if UNITY_EDITOR
			return m_defaultPlatform;
#elif UNITY_IOS
			return SocialPlatform.GameCenter;
#elif UNITY_ANDROID
// if add amazon/samsung - need other defines
			return SocialPlatform.GooglePlay;
#elif UNITY_FACEBOOK
			return SocialPlatform.GameRoom;
#else
#warning UNKNOWN SOCIAL PLATFORM
			return m_defaultPlatform;
#endif
		}

		public void TryConnect(Action<bool, string> callback)
		{
			if (UnityEngine.Social.Active == null)
			{
				callback.SafeInvoke(false, NoActivePlatform);
				return;
			}
			if (UnityEngine.Social.Active.localUser == null)
			{
				callback.SafeInvoke(false, NoLocalUser);
				return;
			}
			if (!UnityEngine.Social.Active.localUser.authenticated)
			{
				IsConnecting = true;
				UnityEngine.Social.Active.localUser.Authenticate((res, error) =>
				{
#if UNITY_EDITOR
					var localUser = UnityEngine.Social.Active.localUser as LocalUser;
					if (localUser != null)
					{
						localUser.SetAuthenticated(true);
					}
#endif
					IsConnecting = false;
					callback.SafeInvoke(res, error);
				});
				return;
			}
			callback.SafeInvoke(true, null);
		}

		public const string NoActivePlatform = "NoActivePlatform";
		public const string NoLocalUser = "NoLocalUser";

		public bool IsConnected
		{
			get
			{
				if (UnityEngine.Social.Active == null)
				{
					return false;
				}
				if (UnityEngine.Social.Active.localUser == null)
				{
					return false;
				}
				if (!UnityEngine.Social.Active.localUser.authenticated)
				{
					return false;
				}

				return true;
			}
		}

		public bool IsConnecting { get; private set; }

#pragma warning disable 0414
		[SerializeField]
		private SocialPlatform m_defaultPlatform = SocialPlatform.Editor;
#pragma warning restore 0414
	}
}