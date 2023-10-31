//Bugs:
//If you have two clients, and you have 1 client take a bunch of items, it seems like those items are not being returned properly on the second client
//When you finish making the food and send it to the delivery counter, the player is still holding the food on the client that delivered it
//the plate is gone on all the clients
//If you spawn an object when a player is not connected, and then have a 2nd player join the game, the player cannot grab the placed kitchen object
//I think that the table doesnt have the reference on a freshly joined client


using ServerSignalList;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Manages a pool of kitchen objects for each connected client, allowing for efficient object reuse.
/// </summary>
/// <remarks>
/// The KitchenObjectPooler class is responsible for managing a pool of kitchen objects, minimizing the overhead of instantiating and destroying objects frequently.
/// It provides functionality to request and return kitchen objects, as well as initializing and managing the object pool per client.
/// </remarks>
public class KitchenObjectPooler : NetworkBehaviour
{
    private int initialSize = 2;
    //Array of queues, these are the objects that are available
    //The order is the same as the KitchenObjectListSO
    private Dictionary<ulong, List<Queue<KitchenObject>>> clientIdToKitchenObjectPool = new Dictionary<ulong, List<Queue<KitchenObject>>>();
    private KitchenObjectListSO kitchenObjectListSO;
    private Queue<ulong> queuedPlayers = new Queue<ulong>();
    private const float UPDATE_DELAY = 1.0f;
    private float elapsedTime = 0.0f;
    private Dictionary<(ulong clientId, int objId), TaskCompletionSource<KitchenObject>> awaiterDict = new Dictionary<(ulong clientId, int objId), TaskCompletionSource<KitchenObject>>();
    private Queue<(ulong clientId, int objId)> queuedClientIdObjectId = new Queue<(ulong clientId, int objId)>();
    private void Awake()
    {
        Signals.Get<OnPlayerSpawnedSignal>().AddListener(OnPlayerSpawned);
        Signals.Get<OnPlayerDespawnedSignal>().AddListener(OnPlayerDespawned);
    }
    public override void OnDestroy()
    {
        Signals.Get<OnPlayerSpawnedSignal>().RemoveListener(OnPlayerSpawned);
        Signals.Get<OnPlayerDespawnedSignal>().RemoveListener(OnPlayerDespawned);
        base.OnDestroy();
    }
    private void OnPlayerDespawned(ulong clientId)
    {
        if (clientIdToKitchenObjectPool.TryGetValue(clientId, out List<Queue<KitchenObject>> kitchenObjectPoolList))
        {
            foreach (Queue<KitchenObject> kitchenObjectPool in kitchenObjectPoolList)
            {
                while (kitchenObjectPool.Count > 0)
                {
                    KitchenObject kitchenObject = kitchenObjectPool.Dequeue();
                    NetworkObject kitchenObjectNetworkObject = kitchenObject.GetComponent<NetworkObject>();
                    if (kitchenObjectNetworkObject && kitchenObjectNetworkObject.IsSpawned)
                    {
                        kitchenObjectNetworkObject.Despawn(true);
                    }
                    else
                    {
                        Destroy(kitchenObject.gameObject);
                    }
                }
            }
        }
    }
    private void OnPlayerSpawned(ulong clientId)
    {
        queuedPlayers.Enqueue(clientId);
    }
    /// <summary>
    /// Initializes the Kitchen Object List Scriptable Object.
    /// </summary>
    /// <param name="kitchenObjectListSO">The Kitchen Object List Scriptable Object to initialize.</param>
    public void InitKitchenObjectListSO(KitchenObjectListSO kitchenObjectListSO)
    {
        this.kitchenObjectListSO = kitchenObjectListSO;
    }
    private void Update()
    {
        if (queuedPlayers.Count > 0)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime > UPDATE_DELAY)
            {
                if (queuedPlayers.TryDequeue(out ulong clientId))
                    InitializePlayerObjectPoolServerRpc(clientId);
            }
        }

        if (queuedClientIdObjectId.Count > 0)
        {
            (ulong clientId, int objId) clientIdObjectId = queuedClientIdObjectId.Dequeue();
            AddNewKitchenObjectToPoolServerRpc(clientIdObjectId.clientId, clientIdObjectId.objId);
        }
    }
    /// <summary>
    /// Server RPC to initialize the object pool for a specific player.
    /// </summary>
    /// <param name="clientId">The client ID of the player.</param>
    [ServerRpc(RequireOwnership = false)]
    private void InitializePlayerObjectPoolServerRpc(ulong clientId)
    {
        UpdatePlayerWithCurrentPools(clientId);
        NetworkObjectReference[][] networkKitchenObjectIds = SpawnNewPlayersKitchenObjects(clientId);
        for (int i=0; i<networkKitchenObjectIds.Length; ++i)
            UpdateObjectPoolClientRpc(clientId, (NetworkObjectReference[])networkKitchenObjectIds.GetValue(i));
    }
    private void UpdatePlayerWithCurrentPools(ulong clientId)
    {
        ClientRpcParams clientRpcParams = ClientRpcManager.Instance.GetClientRpcParams(clientId);
        foreach (KeyValuePair<ulong, List<Queue<KitchenObject>>> clientIdToKitchenObjectPoolPair in clientIdToKitchenObjectPool)
        {
            List<Queue<KitchenObject>> clientIdToKitchenObjectPoolList = clientIdToKitchenObjectPoolPair.Value;
            ulong poolsClientId = clientIdToKitchenObjectPoolPair.Key;
            foreach (Queue<KitchenObject> kitchenObjectPoolQueue in clientIdToKitchenObjectPoolList)
            {
                List<NetworkObjectReference> networkKitchenObjectIds = new List<NetworkObjectReference>();
                foreach (KitchenObject kitchenObject in kitchenObjectPoolQueue)
                {
                    NetworkObject NetworkObject = kitchenObject.GetComponent<NetworkObject>();
                    if (NetworkObject)
                        networkKitchenObjectIds.Add(NetworkObject);
                }
                UpdateObjectPoolClientRpc(poolsClientId, networkKitchenObjectIds.ToArray(), clientRpcParams);
            }
        }
    }
    private NetworkObjectReference[][] SpawnNewPlayersKitchenObjects(ulong clientId)
    {
        NetworkObjectReference[][] networkKitchenObjectIds = new NetworkObjectReference[kitchenObjectListSO.kitchenObjectSOList.Count][];
        for (int objId = 0; objId < kitchenObjectListSO.kitchenObjectSOList.Count; ++objId)
        {
            KitchenObjectSO kitchenObjectSO = kitchenObjectListSO.kitchenObjectSOList[objId];
            networkKitchenObjectIds[objId] = new NetworkObjectReference[initialSize];
            
            for (int i = 0; i < initialSize; i++)
            {
                Transform kitchenObjTransform = Instantiate(kitchenObjectSO.prefab);
                NetworkObject kitchenObjectNetworkObject = kitchenObjTransform.GetComponent<NetworkObject>();
                kitchenObjectNetworkObject.Spawn(true);
                networkKitchenObjectIds[objId][i] = kitchenObjectNetworkObject;
                kitchenObjTransform.parent = transform;

                KitchenObject kitchenObject = kitchenObjTransform.GetComponent<KitchenObject>();
                if (kitchenObject)
                {
                    kitchenObject.objId.Value = objId;
                    kitchenObject.clientId.Value = clientId;
                    kitchenObject.SetVisibility(false);
                }
            }
        }
        return networkKitchenObjectIds;
    }
    /// <summary>
    /// Client RPC to update the object pool on the client side.
    /// </summary>
    /// <param name="clientId">The client ID of the player.</param>
    /// <param name="networkKitchenObjectReferences">Array of Network Object References to the kitchen objects.</param>
    [ClientRpc]
    private void UpdateObjectPoolClientRpc(ulong clientId, NetworkObjectReference[] networkKitchenObjectReferences, ClientRpcParams clientRpcParams = default)
    {
        Queue<KitchenObject> kitchenObjectQueue = new Queue<KitchenObject>(); // Initialize the queue

        for (int ele = 0; ele < networkKitchenObjectReferences.Length; ++ele)
        {
            NetworkObjectReference networkObjectReference = networkKitchenObjectReferences[ele];
            if (networkObjectReference.TryGet(out NetworkObject networkObject))
            {
                KitchenObject kitchenObject = networkObject.GetComponent<KitchenObject>();
                if (kitchenObject)
                    kitchenObjectQueue.Enqueue(kitchenObject);
            }
        }
        
        if(!clientIdToKitchenObjectPool.ContainsKey(clientId))
            clientIdToKitchenObjectPool[clientId] = new List<Queue<KitchenObject>> { };
        clientIdToKitchenObjectPool[clientId].Add(kitchenObjectQueue);
    }
    /// <summary>
    /// Asynchronously requests a kitchen object from the pool.
    /// </summary>
    /// <param name="clientId">The client ID of the player.</param>
    /// <param name="objId">The object ID of the kitchen object.</param>
    /// <returns>A Task representing the asynchronous operation, with a KitchenObject as the result.</returns>
    public async Task<KitchenObject> RequestKitchenObjectAsync(ulong clientId, int objId)
    {
        if (clientIdToKitchenObjectPool.TryGetValue(clientId, out var kitchenObjectPool))
        {
            if (objId < 0)
            {
                Debug.LogWarning("KitchenObjectPooler.RequestObject: ClientIds:" + clientId + " kitchen object pool with object id " + objId + " doesn't exist.");
                return null;
            }
            
            if (kitchenObjectPool[objId].Count == 0)
            {
                TaskCompletionSource<KitchenObject> tcs = new TaskCompletionSource<KitchenObject>();
                awaiterDict[(clientId, objId)] = tcs;
                SpawnAndGetKitchenObjectServerRpc(clientId, objId);
                queuedClientIdObjectId.Enqueue((clientId, objId));
                return await tcs.Task;
            }
            KitchenObject kitchenObject = kitchenObjectPool[objId].Dequeue();
            if(kitchenObjectPool[objId].Count == 0)
                queuedClientIdObjectId.Enqueue((clientId, objId));
            SetKitchenObjectVisibilityServerRpc(kitchenObject.GetComponent<NetworkObject>(), true);
            return kitchenObject;
        }

        Debug.LogWarning("ClientId:" + clientId + " couldn't be found");
        return null;
    }
    [ServerRpc(RequireOwnership = false)]
    private void SetKitchenObjectVisibilityServerRpc(NetworkObjectReference networkObjectReference, bool visibility)
    {
        if (networkObjectReference.TryGet(out NetworkObject networkObject))
        {
            KitchenObject kitchenObject = networkObject.GetComponent<KitchenObject>();
            if (kitchenObject)
                kitchenObject.SetVisibility(visibility);
        }
    }
    /// <summary>
    /// Returns a kitchen object to the pool.
    /// </summary>
    /// <param name="kitchenObject">The kitchen object to return to the pool.</param>
    public void ReturnKitchenObject(KitchenObject kitchenObject)
    {
        //You must call the ServerRpc from the client
        ReturnKitchenObjectServerRpc(kitchenObject.NetworkObjectId);
    }
    /// <summary>
    /// Server RPC to hide the kitchen object.
    /// </summary>
    /// <param name="networkKitchenObjectId">The network ID of the kitchen object.</param>
    [ServerRpc(RequireOwnership = false)]
    public void ReturnKitchenObjectServerRpc(ulong networkKitchenObjectId)
    {
        var networkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkKitchenObjectId];
        KitchenObject kitchenObject = networkObject.GetComponent<KitchenObject>();
        if (kitchenObject)
            kitchenObject.SetVisibility(false);
        //You must call the ClientRpc from the server
        ReturnKitchenObjectClientRpc(kitchenObject.NetworkObjectId);

    }
    /// <summary>
    /// Client RPC to tell all of the clients to return the kitchen object to the pool.
    /// </summary>
    /// <param name="networkKitchenObjectId">The network ID of the kitchen object.</param>
    [ClientRpc]
    public void ReturnKitchenObjectClientRpc(ulong networkKitchenObjectId, ClientRpcParams clientRpc = default)
    {
        var networkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkKitchenObjectId];
        KitchenObject kitchenObject = networkObject.GetComponent<KitchenObject>();
        if (kitchenObject)
            clientIdToKitchenObjectPool[kitchenObject.clientId.Value][kitchenObject.objId.Value].Enqueue(kitchenObject);
    }
    /// <summary>
    /// Server RPC to spawn a new kitchen object and add it to the pool.
    /// </summary>
    /// <param name="clientId">The client ID of the player.</param>
    /// <param name="objId">The object ID of the kitchen object.</param>
    [ServerRpc(RequireOwnership = false)]
    public void SpawnAndGetKitchenObjectServerRpc(ulong clientId, int objId)
    {
        KitchenObjectSO kitchenObjectSO = GetKitchenObjectSO(objId);
        if (kitchenObjectSO != null)
        {
            Transform kitchenObjectTransform = Instantiate(kitchenObjectSO.prefab);
            NetworkObject kitchenObjectNetworkObject = kitchenObjectTransform.GetComponent<NetworkObject>();
            kitchenObjectNetworkObject.Spawn(true);
            KitchenObject kitchenObject = kitchenObjectTransform.GetComponent<KitchenObject>();
            if (kitchenObject)
            {
                kitchenObject.objId.Value = objId;
                kitchenObject.clientId.Value = clientId;
                kitchenObject.transform.parent = transform;
                if (clientIdToKitchenObjectPool.TryGetValue(clientId, out List<Queue<KitchenObject>> kitchenObjectPool))
                {
                    kitchenObjectPool[objId].Enqueue(kitchenObject);
                    SpawnAndGetKitchenObjectClientRpc(kitchenObjectNetworkObject);
                }
            }
        }
    }
    /// <summary>
    /// Client RPC to handle the spawning of a kitchen object on the client side.
    /// </summary>
    /// <param name="networkObjectReference">The Network Object Reference to the spawned kitchen object.</param>
    [ClientRpc]
    private void SpawnAndGetKitchenObjectClientRpc(NetworkObjectReference networkObjectReference)
    {
        if (networkObjectReference.TryGet(out NetworkObject networkObject))
        {
            KitchenObject kitchenObject = networkObject.GetComponent<KitchenObject>();
            if (kitchenObject)
            {
                kitchenObject.SetVisibility(false);
                if (!NetworkManager.Singleton.IsHost)
                    clientIdToKitchenObjectPool[kitchenObject.clientId.Value][kitchenObject.objId.Value].Enqueue(kitchenObject);

                if (awaiterDict.TryGetValue((kitchenObject.clientId.Value, kitchenObject.objId.Value), out TaskCompletionSource<KitchenObject> tcs))
                {
                    //set the kitchenObject as a result for the awaiting async function
                    tcs.SetResult(kitchenObject);
                    awaiterDict.Remove((kitchenObject.clientId.Value, kitchenObject.objId.Value));
                }
            }
        }
    }
    /// <summary>
    /// Server RPC to add a new kitchen object to the pool.
    /// </summary>
    /// <param name="clientId">The client ID of the player.</param>
    /// <param name="objId">The object ID of the kitchen object.</param>
    [ServerRpc(RequireOwnership = false)]
    private void AddNewKitchenObjectToPoolServerRpc(ulong clientId, int objId)
    {
        KitchenObjectSO kitchenObjectSO = GetKitchenObjectSO(objId);
        if (kitchenObjectSO != null)
        {
            Transform kitchenObjectTransform = Instantiate(kitchenObjectSO.prefab);
            NetworkObject kitchenObjectNetworkObject = kitchenObjectTransform.GetComponent<NetworkObject>();
            kitchenObjectNetworkObject.Spawn(true);
            KitchenObject kitchenObject = kitchenObjectTransform.GetComponent<KitchenObject>();
            if (kitchenObject)
            {
                kitchenObject.objId.Value = objId;
                kitchenObject.clientId.Value = clientId;
                kitchenObject.transform.parent = transform;
                if (clientIdToKitchenObjectPool.TryGetValue(clientId, out List<Queue<KitchenObject>> kitchenObjectPool))
                {
                    kitchenObjectPool[objId].Enqueue(kitchenObject);
                    AddNewKitchenObjectToPoolClientRpc(kitchenObjectNetworkObject);
                }
            }
        }
    }
    /// <summary>
    /// Client RPC to handle adding a new kitchen object to the pool on the client side.
    /// </summary>
    /// <param name="networkObjectReference">The Network Object Reference to the new kitchen object.</param>
    [ClientRpc]
    private void AddNewKitchenObjectToPoolClientRpc(NetworkObjectReference networkObjectReference)
    {
        if (networkObjectReference.TryGet(out NetworkObject networkObject))
        {
            KitchenObject kitchenObject = networkObject.GetComponent<KitchenObject>();
            if (kitchenObject)
            {
                kitchenObject.SetVisibility(false);
                if (!NetworkManager.Singleton.IsHost)
                    clientIdToKitchenObjectPool[kitchenObject.clientId.Value][kitchenObject.objId.Value].Enqueue(kitchenObject);
            }
        }
    }
    /// <summary>
    /// Retrieves the Kitchen Object Scriptable Object based on the object ID.
    /// </summary>
    /// <param name="objId">The object ID of the kitchen object.</param>
    /// <returns>The Kitchen Object Scriptable Object if found, null otherwise.</returns>
    public KitchenObjectSO GetKitchenObjectSO(int objId)
    {
        if (objId >= 0 && objId < kitchenObjectListSO.kitchenObjectSOList.Count)
            return kitchenObjectListSO.kitchenObjectSOList[objId];
        else
            return null;
    }
}
