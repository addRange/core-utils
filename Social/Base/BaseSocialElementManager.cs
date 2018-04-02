//----------------------------------------------------------------------------------------------
// author Leonid [Zanleo] Voitko
//----------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Social
{
	public abstract class BaseSocialElementManager<T, T2> : SingletonGameObject<T> where T : BaseSocialElementManager<T, T2> where T2 : BaseElement
	{
		protected override void Init()
		{
			base.Init();
			foreach (var element in m_elements)
			{
				element.Init();
			}
		}

		protected override void DeInit()
		{
			foreach (var element in m_elements)
			{
				element.DeInit();
			}
			base.DeInit();
		}

		public List<T2> Elements
		{
			get { return m_elements; }
		}

		[UnityEngine.SerializeField]
		protected List<T2> m_elements = new List<T2>();
	}
}