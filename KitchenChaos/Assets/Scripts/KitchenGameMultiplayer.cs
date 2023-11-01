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
    [SerializeField] 
    private KitchenObjectListSO kitchenObjectListSO;
    private KitchenObjectPooler kitchenObjectPooler;

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
