//Created by Leonid [Zanleo] Voitko (2014)

using UnityEngine;

public abstract class SingletonGameObject<T> : MonoBehaviour where T : SingletonGameObject<T>
{
	/// Защищённый конструктор необходим для того, чтобы предотвратить создание 
	/// экземпляра класса Singleton. 
	/// Он будет вызван из закрытого конструктора наследственного класса.
	protected SingletonGameObject() { }

	public static T Instance
	{
		get
		{
			if (s_instance == null)
			{
#if UNITY_EDITOR
				if (!Application.isPlaying)
				{
					return null;
				}
#endif
				InitInstance();
			}
			return s_instance;
		}

		set
		{
			if (s_instance != null)
			{
				GameObject.Destroy(s_instance.gameObject);
			}
			s_instance = value;
		}
	}

	public static T TryInstance { get { return s_instance; } }

	private void Awake()
	{
		GameObject.DontDestroyOnLoad(this.gameObject);
		Assert.Test(s_instance == null, () => "Found not null instance for " + s_instance, this);
		if (s_instance == null)
		{
			s_instance = this as T;
			s_instance.Init();
		}
	}

	public static void InitInstance()
	{
		if (s_instance != null)
		{
			return;
		}
		UnityEngine.Object resource = Resources.Load(GetPathToPrefab());
		Assert.Test(resource != null, "Resource not found for '" + GetPathToPrefab() + "'");

		GameObject instanceObject = GameObject.Instantiate(resource) as GameObject;
		instanceObject.name = typeof(T).ToString();
		Assert.Test(instanceObject != null, "Failed create Object of the " + typeof(T).ToString());

		//s_instance = instanceObject.GetComponent<T>();
		Assert.Test(s_instance != null, "Failed create Instance object of the " + typeof(T).ToString());

		// Init called from Awake!
	}

	public static void FreeInstance()
	{
		if (s_instance == null)
		{
			return;
		}
		GameObject.DestroyImmediate(s_instance);
	}

	private void OnDestroy()
	{
		DeInit();
		if (s_instance == this)
		{
			s_instance = null;
		}
	}

	protected virtual void Init()
	{
		//Debug.Log("Init " + typeof(T).ToString());
	}

	protected virtual void DeInit()
	{
		//Debug.Log("DeInit " + typeof(T).ToString());
	}

	public static T GetComponentOnly()
	{
		var resource = Resources.Load<GameObject>(GetPathToPrefab());
		Assert.Test(resource != null, "Resource not found for '" + GetPathToPrefab() + "'");
		return resource.GetComponent<T>();
	}

	private static string GetPathToPrefab()
	{
		string pathToRes = typeof (T).ToString().Replace('.', '/');
		pathToRes = PathToPrefabs + pathToRes;
		return pathToRes;
	}

	//Используется Reflection для создания экземпляра класса без публичного конструктора
	private static T s_instance = null;

	protected static string PathToPrefabs = "Prefabs/Scripts/";
}
