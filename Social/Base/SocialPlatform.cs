//----------------------------------------------------------------------------------------------
// author Leonid [Zanleo] Voitko
//----------------------------------------------------------------------------------------------

using System;

namespace Social
{
	[Serializable]
	public enum SocialPlatform
	{
		Editor = 0,

		// iOS
		GameCenter = 1,

		// Android
		GooglePlay = 2,

		// Facebook
		GameRoom = 3,
	}
}