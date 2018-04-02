#if (UNITY_EDITOR || DEVELOPMENT_BUILD)
#define ACTK_DEBUG_ENABLED
#endif

#define UNITY_5_4_PLUS
#if UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3
#undef UNITY_5_4_PLUS
#endif

#if UNITY_5_4_PLUS
using UnityEngine.SceneManagement;
#endif

using System;
using System.Collections;
using CodeStage.AntiCheat.Common;
using UnityEngine;
using UnityEngine.Events;
using Debug = UnityEngine.Debug;

#if !ACTK_PREVENT_INTERNET_PERMISSION
using System.Net;
using System.Net.Sockets;
#endif

namespace CodeStage.AntiCheat.Detectors
{
	/// <summary>
	/// Allows to detect time cheating using time servers. Needs internet connection.
	/// </summary>
	/// Doesn't detects cheating if there is no internet connection or if it's too weak to gather time from time servers.<br/>
	/// Just add it to any GameObject as usual or through the "GameObject > Create Other > Code Stage > Anti-Cheat Toolkit"
	/// menu to get started.<br/>
	/// You can use detector completely from inspector without writing any code except the actual reaction on cheating.
	/// 
	/// Avoid using detectors from code at the Awake phase.
	[AddComponentMenu(MENU_PATH + COMPONENT_NAME)]
	public class TimeCheatingDetector : ActDetectorBase
	{
		internal const string COMPONENT_NAME = "Time Cheating Detector";
		private const string FINAL_LOG_PREFIX = Constants.LOG_PREFIX + COMPONENT_NAME + ": ";
		private const string TIME_SERVER = "pool.ntp.org";
		//internal const string TIME_DIFFERENCE_KEY = COMPONENT_NAME + "_timediff";

		private static int instancesInScene;

		private readonly DateTime date1900 = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		#region public fields

		/// <summary> 
		/// Time (in minutes) between detector checks.
		/// </summary>
		[Tooltip("Time (in minutes) between detector checks.")]
		[Range(1, 60)]
		public int interval = 1;

		/// <summary>
		/// Maximum allowed difference between online and offline time, in minutes.
		/// </summary>
		[Tooltip("Maximum allowed difference between online and offline time, in minutes.")]
		public int threshold = 65;

		#endregion

		#region private variables
		
		#endregion

		#region public static methods
		/// <summary>
		/// Starts detection.
		/// </summary>
		/// Make sure you have properly configured detector in scene with #autoStart disabled before using this method.
		public static void StartDetection()
		{
			if (Instance != null)
			{
				Instance.StartDetectionInternal(null, Instance.interval);
			}
			else
			{
				Debug.LogError(FINAL_LOG_PREFIX + "can't be started since it doesn't exists in scene or not yet initialized!");
			}
		}

		/// <summary>
		/// Starts detection with specified callback.
		/// </summary>
		/// If you have detector in scene make sure it has empty Detection Event.<br/>
		/// Creates a new detector instance if it doesn't exists in scene.
		/// <param name="callback">Method to call after detection.</param>
		public static void StartDetection(UnityAction callback)
		{
			StartDetection(callback, GetOrCreateInstance.interval);
		}

		/// <summary>
		/// Starts detection with specified callback using passed interval.<br/>
		/// </summary>
		/// If you have detector in scene make sure it has empty Detection Event.<br/>
		/// Creates a new detector instance if it doesn't exists in scene.
		/// <param name="callback">Method to call after detection.</param>
		/// <param name="interval">Time in minutes between checks. Overrides #interval property.</param>
		public static void StartDetection(UnityAction callback, int interval)
		{
			GetOrCreateInstance.StartDetectionInternal(callback, interval);
		}

		/// <summary>
		/// Stops detector. Detector's component remains in the scene. Use Dispose() to completely remove detector.
		/// </summary>
		public static void StopDetection()
		{
			if (Instance != null) Instance.StopDetectionInternal();
		}

		/// <summary>
		/// Stops and completely disposes detector component.
		/// </summary>
		/// On dispose Detector follows 2 rules:
		/// - if Game Object's name is "Anti-Cheat Toolkit Detectors": it will be automatically 
		/// destroyed if no other Detectors left attached regardless of any other components or children;<br/>
		/// - if Game Object's name is NOT "Anti-Cheat Toolkit Detectors": it will be automatically destroyed only
		/// if it has neither other components nor children attached;
		public static void Dispose()
		{
			if (Instance != null) Instance.DisposeInternal();
		}
		#endregion

		#region static instance
		/// <summary>
		/// Allows reaching public properties from code. Can be null.
		/// </summary>
		public static TimeCheatingDetector Instance { get; private set; }

		private static TimeCheatingDetector GetOrCreateInstance
		{
			get
			{
				if (Instance != null)
					return Instance;

				if (detectorsContainer == null)
				{
					detectorsContainer = new GameObject(CONTAINER_NAME);
				}
				Instance = detectorsContainer.AddComponent<TimeCheatingDetector>();
				return Instance;
			}
		}
		#endregion

		private TimeCheatingDetector() { } // prevents direct instantiation

		#region unity messages
		private void Awake()
		{
			instancesInScene++;
			if (Init(Instance, COMPONENT_NAME))
			{
				Instance = this;
			}

		#if UNITY_5_4_PLUS
			SceneManager.sceneLoaded += OnLevelWasLoadedNew;
		#endif
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			instancesInScene--;
		}
			   
		#if UNITY_5_4_PLUS
		private void OnLevelWasLoadedNew(Scene scene, LoadSceneMode mode)
		{
			OnLevelLoadedCallback();
		}
		#else
		private void OnLevelWasLoaded()
		{
			OnLevelLoadedCallback();
		}
		#endif

		private void OnLevelLoadedCallback()
		{
			if (instancesInScene < 2)
			{
				if (!keepAlive)
				{
					DisposeInternal();
				}
			}
			else
			{
				if (!keepAlive && Instance != this)
				{
					DisposeInternal();
				}
			}
		}

		/*private void OnApplicationPause(bool pause)
		{
			if (!pause)
			{
				ResetStartTicks();
			}
		}*/
		#endregion

		private void StartDetectionInternal(UnityAction callback, int checkInterval)
		{
			if (isRunning)
			{
				Debug.LogWarning(FINAL_LOG_PREFIX + "already running!", this);
				return;
			}

			if (!enabled)
			{
				Debug.LogWarning(FINAL_LOG_PREFIX + "disabled but StartDetection still called from somewhere (see stack trace for this message)!", this);
				return;
			}

			if (callback != null && detectionEventHasListener)
			{
				Debug.LogWarning(FINAL_LOG_PREFIX + "has properly configured Detection Event in the inspector, but still get started with Action callback. Both Action and Detection Event will be called on detection. Are you sure you wish to do this?", this);
			}

			if (callback == null && !detectionEventHasListener)
			{
				Debug.LogWarning(FINAL_LOG_PREFIX + "was started without any callbacks. Please configure Detection Event in the inspector, or pass the callback Action to the StartDetection method.", this);
				enabled = false;
				return;
			}

			detectionAction = callback;
			interval = checkInterval;

			InvokeRepeating("CheckForCheat", 0, interval * 60);

			started = true;
			isRunning = true;
		}

		protected override void StartDetectionAutomatically()
		{
			StartDetectionInternal(null, interval);
		}

		protected override void PauseDetector()
		{
			isRunning = false;
		}

		protected override void ResumeDetector()
		{
			if (detectionAction == null && !detectionEventHasListener) return;
			isRunning = true;
		}

		protected override void StopDetectionInternal()
		{
			if (!started)
				return;

			CancelInvoke("CheckForCheat");

			detectionAction = null;
			started = false;
			isRunning = false;
		}

		protected override void DisposeInternal()
		{
			base.DisposeInternal();
			if (Instance == this) Instance = null;
		}

		private void CheckForCheat()
		{
			if (!isRunning) return;
			StartCoroutine(CheckForCheatCoroutine());
		}

		private IEnumerator CheckForCheatCoroutine()
		{
			double onlineTime = GetOnlineTime(TIME_SERVER);
			if (onlineTime <= 0)
			{
				Debug.LogWarning(FINAL_LOG_PREFIX + "Can't retrieve time from time server!");
				yield break;
			}

			double offlineTime = GetLocalTime();

			var onlineTimeSpan = new TimeSpan((long)onlineTime * TimeSpan.TicksPerMillisecond);
			var offlineTimeSpan = new TimeSpan((long)offlineTime * TimeSpan.TicksPerMillisecond);

			/*Debug.Log("Server time: " + onlineTimeSpan.Hours + ':' + onlineTimeSpan.Minutes + ':' + onlineTimeSpan.Seconds + '.' + onlineTimeSpan.Milliseconds);
			Debug.Log("Local time: " + offlineTimeSpan.Hours + ':' + offlineTimeSpan.Minutes + ':' + offlineTimeSpan.Seconds + '.' + offlineTimeSpan.Milliseconds);

			Debug.Log("Server min: " + onlineTimeSpan.TotalMinutes);
			Debug.Log("Local min: " + offlineTimeSpan.TotalMinutes);*/

			double minutesDifference = onlineTimeSpan.TotalMinutes - offlineTimeSpan.TotalMinutes;
			if (Math.Abs(minutesDifference) > threshold)
			{
				OnCheatingDetected();
			}

			/*double minutesDifferenceSaved = 0;
			double minutesDifference = onlineTimeSpan.TotalMinutes - offlineTimeSpan.TotalMinutes;
			if (!ObscuredPrefs.HasKey(TIME_DIFFERENCE_KEY))
			{
				ObscuredPrefs.SetDouble(TIME_DIFFERENCE_KEY, minutesDifference);
				minutesDifferenceSaved = minutesDifference;
			}
			else
			{
				minutesDifferenceSaved = ObscuredPrefs.GetDouble(TIME_DIFFERENCE_KEY);
			}

			if (Math.Abs(minutesDifference + minutesDifferenceSaved) > threshold)
			{
				OnCheatingDetected();
			}*/

			yield return null;
		}

#if !ACTK_PREVENT_INTERNET_PERMISSION
		/// <summary>
		/// Retrieves NTP time from the specified time server, e.g. "pool.ntp.org".
		/// </summary>
		/// Blocks until gets data from time server, or until 3 sec timeout is met.
		/// <param name="server">NTP time server address, e.g. "pool.ntp.org"</param>
		/// <returns>NTP time in milliseconds or -1 if there was an error getting time.</returns>
		public static double GetOnlineTime(string server)
		{
			try
			{
				var ntpData = new byte[48];

				ntpData[0] = 0x1B;

				IPAddress[] addresses = Dns.GetHostEntry(server).AddressList;
				var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
				socket.Connect(new IPEndPoint(addresses[0], 123));
				socket.ReceiveTimeout = 3000;

				socket.Send(ntpData);
				socket.Receive(ntpData);
				socket.Close();

				ulong intc = (ulong)ntpData[40] << 24 | (ulong)ntpData[41] << 16 | (ulong)ntpData[42] << 8 | ntpData[43];
				ulong frac = (ulong)ntpData[44] << 24 | (ulong)ntpData[45] << 16 | (ulong)ntpData[46] << 8 | ntpData[47];

				return intc * 1000d + frac * 1000d / 0x100000000L;
			}
			catch (Exception exception)
			{
				Debug.Log(FINAL_LOG_PREFIX + "Could not get NTP time from " + server + " =/\n" + exception);
				return -1;
			}
		}
#else
		private double GetOnlineTime()
		{
			return -1;
		}
#endif

		private double GetLocalTime()
		{
			return DateTime.UtcNow.Subtract(date1900).TotalMilliseconds;
		}
	}
}