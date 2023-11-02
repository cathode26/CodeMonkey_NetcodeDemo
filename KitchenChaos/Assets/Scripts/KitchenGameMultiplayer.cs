using UnityEngine;
using Unity.Netcode;

/*
 * Manages the spawning and synchronization of kitchen objects across multiple clients. 
 * This file plays a crucial role in ensuring that all players see consistent states of kitchen objects 
 * in a multiplayer environment, especially when objects are picked up, placed, or interacted with. 
 * It leverages the KitchenObjectListSO for efficient ServerRpc communications, utilizing an index-based 
 * lookup system due to the limitations of sending ScriptableObjects directly via ServerRpc.
 */

public class KitchenGameMultiplayer : NetworkBehaviour
{
    public static KitchenGameMultiplayer Instance { get; private set; }
    public int InitialSize { get => intialSize; set => intialSize = value; }
    public int TimeoutMilliseconds { get => timeoutMilliseconds; set => timeoutMilliseconds = value; }

    [SerializeField] 
    private KitchenObjectListSO kitchenObjectListSO;
    private KitchenObjectPooler kitchenObjectPooler;
    private float totalSupportTime = 30;    //time to support 30 seconds of continuous item use without lag
    private float timeToUseOneItem = 3;     //Lets assume that a user can use an item every 3 seconds
    private int intialSize = -1;
    private int timeoutMilliseconds = -1;
    private bool networkSpawned = false;
    private int pingsSend = 0;
    private int pingsReceived = 0;
    private float elapsedTime = 0.0f;
    private float UPDATE_DELAY = 0.1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Another instance of KitchenGameMultiplayer detected! Destroying...");
            Destroy(this.gameObject);
            return;
        }
        kitchenObjectPooler = GetComponent<KitchenObjectPooler>();
        kitchenObjectPooler.InitKitchenObjectListSO(kitchenObjectListSO);
        Instance = this;
    }
    public override void OnDestroy()
    {
        Instance = null;
        base.OnDestroy();
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        networkSpawned = true;
    }
    private void Update()
    {
        if (networkSpawned)
        {
            if (pingsSend < LatencyAverage.Instance.MaxSize)
            {
                elapsedTime += Time.deltaTime;
                if (elapsedTime > UPDATE_DELAY)
                {
                    elapsedTime = 0.0f;
                    pingsSend++;
                    CalculateLatencyServerRpc(Time.time, NetworkManager.Singleton.LocalClient.ClientId);
                }
            }
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void CalculateLatencyServerRpc(float startTime, ulong clientId)
    {
        CalculateLatencyClientRpc(startTime, clientId, ClientRpcManager.Instance.GetClientRpcParams(clientId));
    }
    [ClientRpc]
    private void CalculateLatencyClientRpc(float startTime, ulong clientId, ClientRpcParams clientRpcParams)
    {
        LatencyAverage.Instance.AddRoundTripValue(Time.time - startTime); // Calculate the estimated latency
        if (++pingsReceived >= LatencyAverage.Instance.MaxSize)
            UpdatePoolsInitialSizeAndTimeout();
    }
    private void UpdatePoolsInitialSizeAndTimeout()
    {
        float roundTripTime = LatencyAverage.Instance.GetAverageRoundTripTime();
        intialSize = (int)Mathf.Ceil(((totalSupportTime / timeToUseOneItem) - (totalSupportTime / (roundTripTime * 2.0f))));
        intialSize = Mathf.Max(intialSize, 1);
        timeoutMilliseconds = (int)(LatencyAverage.Instance.GetAverageRoundTripTime() * 2000);
        Debug.Log("roundTripTime is " + roundTripTime + " intialSize " + intialSize + " timeoutMilliseconds " + timeoutMilliseconds);
    }
    public async void SpawnKitchenObject(KitchenObjectSO kitchenObjectSO, IKitchenObjectParent kitchenObjectParent)
    {
        int objId = GetKitchenObjectId(kitchenObjectSO);
        kitchenObjectParent.WaitForNetwork();
        KitchenObject kitchenObject = await kitchenObjectPooler.RequestKitchenObjectAsync(NetworkManager.Singleton.LocalClient.ClientId, objId);
        if (kitchenObject != null)
        {
            kitchenObject.SetVisibilityLocal(true);
            kitchenObject.SetKitchenObjectsParent(kitchenObjectParent);
        }
        kitchenObjectParent.NetworkComplete();
    }
    public int GetKitchenObjectId(KitchenObjectSO kitchenObjectSO)
    {
        return kitchenObjectListSO.kitchenObjectSOList.IndexOf(kitchenObjectSO);
    }
    public KitchenObjectSO GetKitchenObjectSO(int index)
    {
        if(index >= 0 && index < kitchenObjectListSO.kitchenObjectSOList.Count)
            return kitchenObjectListSO.kitchenObjectSOList[index];
        else 
            return null;
    }
    public void ReturnKitchenObject(KitchenObject kitchenObject)
    {
        IKitchenObjectParent iKitchenObjectParent = kitchenObject.GetKitchenObjectsParent();
        if (iKitchenObjectParent != null)
            kitchenObject.ClearKitchenObjectParentClientRpc();
        kitchenObjectPooler.ReturnKitchenObject(kitchenObject);
    }
}
