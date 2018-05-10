//----------------------------------------------------------------------------------------------
// Created by Leonid [Zanleo] Voitko (2018.03.14)
//----------------------------------------------------------------------------------------------

using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeAreaApplier : MonoBehaviour
{
#if UNITY_2017_3_OR_NEWER
	private void OnEnable()
	{
		var sa = Screen.safeArea;
		// DEBUG in editor with fake safeArea
		//const float multiplier = 0.1f;
		//float deltaY = ScreenHeight * multiplier;
		//sa.height -= 2 * deltaY;
		//sa.position = new Vector2(sa.position.x, sa.position.y + deltaY);
		//Debug.Log(sa);

		ApplySafeAreaWithKoefficients(sa);
	}
#else
	private void OnEnable()
	{
		Debug.LogWarning("Not supported by Unity lower than 2017.3");
	}
	private void OnValidate()
	{
		Debug.LogWarning("Not supported by Unity lower than 2017.3");
	}
#endif

	public void ApplySafeAreaWithKoefficients(Rect safeArea)
	{
		float leftNotSafe = safeArea.position.x;
		float rightNotSafe = ScreenWidth - (safeArea.position.x + safeArea.width);
		float bottomNotSafe = safeArea.position.y;
		float topNotSafe = ScreenHeight - (safeArea.position.y + safeArea.height);
		
		float tLeft = leftNotSafe * m_leftNotSafeAreaMultiplier;
		float tBottom = bottomNotSafe * m_bottomNotSafeAreaMultiplier;
		float tRight = rightNotSafe * m_rightNotSafeAreaMultiplier;
		float tTop = topNotSafe * m_topNotSafeAreaMultiplier;
		
		float tWidth = ScreenWidth - (tLeft + tRight);
		float tHeight = ScreenHeight - (tBottom + tTop);

		safeArea.position = new Vector2(tLeft, tBottom);
		safeArea.width = tWidth;
		safeArea.height = tHeight;

		ApplySafeArea(safeArea);
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="area">Area in ScreenSpace (pixels)</param>
	public void ApplySafeArea(Rect area)
	{
		if (m_lastAppliedSafeArea == null)
		{
			m_lastAppliedSafeArea = new Rect(0, 0, ScreenWidth, ScreenHeight);
		}

		bool isApplied = Mathf.Approximately(m_lastAppliedSafeArea.Value.x, area.x);
		if (!Mathf.Approximately(m_lastAppliedSafeArea.Value.y, area.y))
		{
			isApplied = false;
		}
		if (!Mathf.Approximately(m_lastAppliedSafeArea.Value.width, area.width))
		{
			isApplied = false;
		}
		if (!Mathf.Approximately(m_lastAppliedSafeArea.Value.height, area.height))
		{
			isApplied = false;
		}

		if (isApplied)
		{
			return;
		}
		var anchorMin = area.position;
		var anchorMax = area.position + area.size;
		anchorMin.x /= ScreenWidth;
		anchorMin.y /= ScreenHeight;
		anchorMax.x /= ScreenWidth;
		anchorMax.y /= ScreenHeight;
		ApplySafeArea(anchorMin, anchorMax);
	}

	/// <summary>
	/// anchor in koefficient values (Screen - from bottomRight) for apply on RectTransform
	/// </summary>
	/// <param name="anchorMin"></param>
	/// <param name="anchorMax"></param>
	public void ApplySafeArea(Vector2 anchorMin, Vector2 anchorMax)
	{
		var rectTransform = GetComponent<RectTransform>();
		rectTransform.anchorMin = anchorMin;
		rectTransform.anchorMax = anchorMax;
	}

	public int ScreenWidth
	{
		get { return Screen.width; }
	}

	public int ScreenHeight
	{
		get { return Screen.height; }
	}
	
	[SerializeField, Range(0f, 1f)]
	private float m_leftNotSafeAreaMultiplier = 1f;
	[SerializeField, Range(0f, 1f)]
	private float m_rightNotSafeAreaMultiplier = 1f;
	[SerializeField, Range(0f, 1f)]
	private float m_topNotSafeAreaMultiplier = 1f;
	[SerializeField, Range(0f, 1f)]
	private float m_bottomNotSafeAreaMultiplier = 1f;
	
	private Rect? m_lastAppliedSafeArea = null;
}
