using System.Collections.Generic;
using UnityEngine;

public class MarketSquarePlatformTrigger : MonoBehaviour
{
    public static MarketSquarePlatformTrigger Instance;

    public readonly HashSet<int> playersOnPlatform = new HashSet<int>();
    public BoxCollider triggerCollider;

    private void Awake()
    {
        Instance = this;
        FindTriggerCollider();
    }

    private void FindTriggerCollider()
    {
        if (triggerCollider == null)
        {
            BoxCollider[] colliders = GetComponents<BoxCollider>();
            foreach (var col in colliders)
            {
                if (col.isTrigger)
                {
                    triggerCollider = col;
                    break;
                }
            }
        }
    }

    private void Update()
    {
        if (triggerCollider == null)
        {
            FindTriggerCollider();
            if (triggerCollider == null) return;
        }

        bool stateChanged = false;

        // Check local player position
        if (PlayerMovement.Instance != null && LocalClient.instance != null)
        {
            int myId = LocalClient.instance.myId;
            Vector3 checkPos = PlayerMovement.Instance.transform.position + Vector3.up * 0.5f;
            bool isInside = triggerCollider.bounds.Contains(checkPos);

            if (isInside && !playersOnPlatform.Contains(myId))
            {
                playersOnPlatform.Add(myId);
                ClientSend.MarketSquarePlatform(true);
                stateChanged = true;
            }
            else if (!isInside && playersOnPlatform.Contains(myId))
            {
                playersOnPlatform.Remove(myId);
                ClientSend.MarketSquarePlatform(false);
                stateChanged = true;
            }
        }

        if (LocalClient.serverOwner && stateChanged && MarketSquareManager.Instance != null)
        {
            MarketSquareManager.Instance.OnPlayerPlatformStateChanged();
        }
    }

    public void RemovePlayer(int id)
    {
        if (playersOnPlatform.Remove(id))
        {
            if (MarketSquareManager.Instance != null)
            {
                MarketSquareManager.Instance.OnPlayerPlatformStateChanged();
            }
        }
    }
}
