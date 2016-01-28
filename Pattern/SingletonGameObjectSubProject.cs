//Created by Leonid [Zanleo] Voitko (2016)

// in fact this is only example for SubRepo projects
public abstract class SingletonGameObjectSubProject<T> : SingletonGameObject<T> where T : SingletonGameObject<T>
{
	static SingletonGameObjectSubProject()
	{
		PathToPrefabs = "MiniGames/Words/Prefabs/Scripts/";
	}

	public new static T Instance
	{
		get { return SingletonGameObject<T>.Instance; }
		set { SingletonGameObject<T>.Instance = value; }
	}
}
