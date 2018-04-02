//----------------------------------------------------------------------------------------------
// Created by Anton [Umka] Ushkalov (2018.01.17)
//----------------------------------------------------------------------------------------------

namespace UnityEngine.UI
{
	[AddComponentMenu("UI/ButtonMultiTint", 31)]
	public class ButtonMultiTint : Button
	{
		protected override void DoStateTransition(SelectionState state, bool instant)
		{
			base.DoStateTransition(state, instant);
			Color color;
			switch (state)
			{
				case SelectionState.Normal:
					color = colors.normalColor;
					break;
				case SelectionState.Highlighted:
					color = colors.highlightedColor;
					break;
				case SelectionState.Pressed:
					color = colors.pressedColor;
					break;
				case SelectionState.Disabled:
					color = colors.disabledColor;
					break;
				default:
					color = Color.black;
					break;
			}

			if (!gameObject.activeInHierarchy)
			{
				return;
			}

			switch (transition)
			{
				case Transition.ColorTint:
					var elements = GetComponentsInChildren<Graphic>(true);
					foreach (var image1 in elements)
					{
						image1.CrossFadeColor(color, !instant ? colors.fadeDuration : 0.0f, true, true);
					}

					break;
			}
		}
	}
}