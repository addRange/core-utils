using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Profiling;
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
		Profiler.BeginSample("FPS.Update", gameObject);
		m_timeleft -= Time.deltaTime;
		m_accum += Time.timeScale / Time.deltaTime;
		++m_frames;

		// Interval ended - update GUI text and start new interval
		if (m_timeleft > 0.0)
		{
			Profiler.EndSample();
			return;
		}

		// display two fractional digits (f2 format)
		float fps = m_accum / m_frames;
		int intVal = (int)fps * 100;
		string res = null;
		if (!m_values.TryGetValue(intVal, out res))
		{
			res = System.String.Format("{0:F2} FPS", fps);
			m_values.Add(intVal, res);
		}
		
		string format = res;
		m_text.text = format;

		//	DebugConsole.Log(format,level);
		m_timeleft = m_updateInterval;
		m_accum = 0.0F;
		m_frames = 0;
		Profiler.EndSample();
	}

	[SerializeField]
	private float m_updateInterval = 0.5F;

	[SerializeField]
	private Text m_text = null;

	private float m_accum = 0; // FPS accumulated over the interval
	private int m_frames = 0; // Frames drawn over the interval
	private float m_timeleft; // Left time for current interval
	private Dictionary<int, string> m_values = new Dictionary<int,string>();
}