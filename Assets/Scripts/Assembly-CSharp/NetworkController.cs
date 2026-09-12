using System;
using Steamworks.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkController : MonoBehaviour
{
	public bool loading { get; set; }

	private void Awake()
	{
		if (NetworkController.Instance)
		{
			Destroy(base.gameObject);
			return;
		}
		NetworkController.Instance = this;
		DontDestroyOnLoad(base.gameObject);
	}

	public void LoadGame(string[] names, string sceneName = "GameAfterLobby")
	{
		this.loading = true;
		this.playerNames = names;
		this.targetScene = sceneName;
		LoadingScreen.Instance.Show(1f);
		StartLoadingScene();
	}

	private void StartLoadingScene()
	{
		string scene = string.IsNullOrEmpty(this.targetScene) ? "GameAfterLobby" : this.targetScene;
		SceneManager.LoadScene(scene);
	}

	public string targetScene = "GameAfterLobby";

	public NetworkController.NetworkType networkType;

	public GameObject steam;

	public GameObject classic;

	public Lobby lobby;

	public string[] playerNames;

	public static NetworkController Instance;

	public enum NetworkType
	{
		Steam,
		Classic
	}
}
