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
		if (s_instance == null)
		{
			// TODO! need destroy if on Awake instance not null!?
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
		UnityEngine.Object resource = Resources.Load(PathToPrefabs + typeof(T).ToString());
		Assert.Test(resource != null, "Resource not found for '" + PathToPrefabs + typeof(T).ToString() + "'");

		GameObject instanceObject = GameObject.Instantiate(resource) as GameObject;
		instanceObject.name = typeof(T).ToString();
		Assert.Test(instanceObject != null, "Failed create Object of the " + typeof(T).ToString());

		//s_instance = instanceObject.GetComponent<T>();
		Assert.Test(s_instance != null, "Failed create Instance object of the " + typeof(T).ToString());

		// Init called from Awake!
	}

	//TODO: add comment
	protected virtual void Init()
	{
		//Debug.Log("Init " + typeof(T).ToString());
	}

	//Используется Reflection для создания экземпляра класса без публичного конструктора
	private static T s_instance = null;

	protected static string PathToPrefabs = "Prefabs/Scripts/";
}
