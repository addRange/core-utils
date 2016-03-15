using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FpsCounter : MonoBehaviour
{
	private void Start()
	{
		if (m_text == null)
		{
			Debug.Log("m_text is null!");
			enabled = false;
			return;
		}
		m_timeleft = m_updateInterval;
	}

	private void Update()
	{
		m_timeleft -= Time.deltaTime;
		m_accum += Time.timeScale / Time.deltaTime;
		++m_frames;

		// Interval ended - update GUI text and start new interval
		if (m_timeleft > 0.0)
		{
			return;
		}

		// display two fractional digits (f2 format)
		float fps = m_accum / m_frames;
		string format = System.String.Format("{0:F2} FPS", fps);
		m_text.text = format;
		
		//	DebugConsole.Log(format,level);
		m_timeleft = m_updateInterval;
		m_accum = 0.0F;
		m_frames = 0;
	}

	[SerializeField]
	private float m_updateInterval = 0.5F;
	[SerializeField]
	private Text m_text = null;

	private float m_accum = 0; // FPS accumulated over the interval
	private int m_frames = 0; // Frames drawn over the interval
	private float m_timeleft; // Left time for current interval
}
