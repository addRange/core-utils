//----------------------------------------------------------------------------------------------
// author Leonid [Zanleo] Voitko
//----------------------------------------------------------------------------------------------

namespace Localization.Utils
{
	public static class TextManagerExtensions
	{
		public static string Translate(this string _this) { return TextManager.Instance.Translate(_this); }

		public static string Translate(this string _this, object arg0) { return string.Format(_this.Translate(), arg0); }

		public static string Translate(this string _this, object arg0, object arg1)
		{
			return string.Format(_this.Translate(), arg0, arg1);
		}

		public static string Translate(this string _this, object arg0, object arg1, object arg2)
		{
			return string.Format(_this.Translate(), arg0, arg1, arg2);
		}
	}
}