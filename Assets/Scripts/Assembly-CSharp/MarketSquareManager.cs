using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

public class MarketSquareManager : MonoBehaviour
{
    public static MarketSquareManager Instance;

    public Transform worldSpawn;
    public MarketSquarePlatformTrigger platformTrigger;
    public float countdownDuration = 5f;

    private float countdownTimer = -1f;
    private bool isCountingDown = false;
    private bool matchStarting = false;

    private void Awake()
    {
        Instance = this;
        if (worldSpawn == null)
        {
            GameObject spawnObj = GameObject.Find("MarketSquareWorldSpawn");
            if (spawnObj != null)
            {
                worldSpawn = spawnObj.transform;
            }
        }
    }

    private void Start()
    {
        if (NetworkController.Instance != null)
        {
            NetworkController.Instance.loading = false;
        }

        if (platformTrigger == null)
        {
            platformTrigger = MarketSquarePlatformTrigger.Instance;
        }

        if (SteamLobby.Instance != null && SteamLobby.Instance.currentLobby.Id.Value != 0)
        {
            SteamLobby.Instance.currentLobby.SetJoinable(true);
        }

        if (LocalClient.serverOwner)
        {
            StartCoroutine(SpawnInitialHostAndPlayersRoutine());
        }
    }

    private IEnumerator SpawnInitialHostAndPlayersRoutine()
    {
        yield return new WaitForSeconds(0.1f);

        Vector3 spawnPos = worldSpawn != null ? worldSpawn.position : Vector3.zero;

        List<Vector3> spawnList = new List<Vector3>();
        int total = Server.clients.Count;
        for (int i = 0; i < total + 5; i++)
        {
            Vector2 offset = Random.insideUnitCircle * 2f;
            spawnList.Add(spawnPos + new Vector3(offset.x, 0f, offset.y));
        }

        if (GameManager.instance != null)
        {
            GameManager.instance.SendPlayersIntoGame(spawnList);
        }
    }

    public void OnPlayerPlatformStateChanged()
    {
        if (!LocalClient.serverOwner || matchStarting)
        {
            return;
        }

        int totalInLobby = GameManager.instance != null ? GameManager.instance.GetPlayersInLobby() : 1;
        int readyCount = platformTrigger != null ? platformTrigger.playersOnPlatform.Count : 0;

        if (readyCount >= totalInLobby && totalInLobby > 0)
        {
            if (!isCountingDown)
            {
                isCountingDown = true;
                countdownTimer = countdownDuration;
            }
        }
        else
        {
            if (isCountingDown)
            {
                isCountingDown = false;
                countdownTimer = -1f;
                ServerSend.MarketSquareCountdown(-1f, readyCount, totalInLobby);
                if (MarketSquareUI.Instance != null)
                {
                    MarketSquareUI.Instance.HideCountdown();
                }
            }
        }
    }

    private void Update()
    {
        if (!LocalClient.serverOwner || matchStarting)
        {
            return;
        }

        if (isCountingDown)
        {
            countdownTimer -= Time.deltaTime;
            int totalInLobby = GameManager.instance != null ? GameManager.instance.GetPlayersInLobby() : 1;
            int readyCount = platformTrigger != null ? platformTrigger.playersOnPlatform.Count : 0;

            ServerSend.MarketSquareCountdown(countdownTimer, readyCount, totalInLobby);
            if (MarketSquareUI.Instance != null)
            {
                MarketSquareUI.Instance.UpdateCountdown(countdownTimer, readyCount, totalInLobby);
            }

            if (countdownTimer <= 0f)
            {
                matchStarting = true;
                isCountingDown = false;
                StartMatchFromHub();
            }
        }
    }

    private void StartMatchFromHub()
    {
        if (SteamLobby.Instance != null && SteamLobby.Instance.currentLobby.Id.Value != 0)
        {
            SteamLobby.Instance.currentLobby.SetJoinable(false);
        }

        GameSettings settings = GameManager.gameSettings ?? new GameSettings(Random.Range(int.MinValue, int.MaxValue), GameSettings.GameMode.Survival, GameSettings.FriendlyFire.Off, GameSettings.Difficulty.Normal, GameSettings.GameLength.Short, GameSettings.Multiplayer.On);

        foreach (Client client in Server.clients.Values)
        {
            if (client?.player != null)
            {
                client.player.ready = false;
                client.player.loading = false;
                client.player.dead = false;
                ServerSend.StartGame(client.player.id, settings, "GameAfterLobby");
            }
        }
    }
}
