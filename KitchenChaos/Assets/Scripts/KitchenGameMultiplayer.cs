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
    [SerializeField] private KitchenObjectListSO kitchenObjectListSO;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Another instance of KitchenGameMultiplayer detected! Destroying...");
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
    }
    public override void OnDestroy()
    {
        Instance = null;
        base.OnDestroy();
    }

    public void SpawnKitchenObject(KitchenObjectSO kitchenObjectSO, IKitchenObjectParent kitchenObjectParent)
    {
        SpawnKitchenObjectServerRpc(GetKitchenObjectIndex(kitchenObjectSO), kitchenObjectParent.GetNetworkObject());
    }
    [ServerRpc(RequireOwnership = false)]
    public void SpawnKitchenObjectServerRpc(int index, NetworkObjectReference kitchenObjectParentNetObjRef)
    {
        //replace with chopped item
        KitchenObjectSO kitchenObjectSO = GetKitchenObjectSO(index);
        if (kitchenObjectSO != null)
        {
            Transform kitchenObjectTransform = Instantiate(kitchenObjectSO.prefab);
            NetworkObject kitchenObjectNetworkObject = kitchenObjectTransform.GetComponent<NetworkObject>();
            kitchenObjectNetworkObject.Spawn(true);
            KitchenObject kitchenObject = kitchenObjectTransform.GetComponent<KitchenObject>();
            if (kitchenObjectParentNetObjRef.TryGet(out NetworkObject kitchenObjectParentNetObj))
            {
                IKitchenObjectParent kitchenObjectParent = kitchenObjectParentNetObj.GetComponent<IKitchenObjectParent>();
                kitchenObject.SetKitchenObjectsParent(kitchenObjectParent);
            }
        }
    }
    public int GetKitchenObjectIndex(KitchenObjectSO kitchenObjectSO)
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
}
