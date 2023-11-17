using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using static KitchenObjectParentStates;

/**
 * Class: KitchenObjectParentManager
 * 
 * Purpose:
 * This class serves as a central manager for handling the states of kitchen objects 
 * in a networked multiplayer game. It maintains a mapping between kitchen objects 
 * and their states and offers methods to handle state changes. This class is pivotal
 * in ensuring that when multiple players interact with the same object, the first player
 * who grabbed the object retains it after client-side prediction is reconciled.
 * 
 * Usage:
 * The KitchenObjectParentManager is utilized by the game's networking layer to manage
 * and synchronize object states across different clients, ensuring consistent and fair gameplay.
 */
public class KitchenObjectParentManager : NetworkBehaviour
{
    // Singleton instance of the manager.
    public static KitchenObjectParentManager Instance { get; private set; }

    // Maps kitchen objects to their respective states.
    private Dictionary<KitchenObject, KitchenObjectParentStates> kitchenObjectToStates = new Dictionary<KitchenObject, KitchenObjectParentStates>();

    private void Awake()
    {
        // Ensure a single instance of the manager.
        if (Instance != null && Instance != this)
        {
            UnityEngine.Debug.LogWarning("Another instance of KitchenObjectParentManager detected! Destroying...");
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
    //When a kitchen object is added to the counter or removed 
    public void AddStateChange(ulong clientId, double iteractionTime, KitchenObject kitchenObject, IKitchenObjectParent parentAdded, IKitchenObjectParent parentRemoved)
    {
        KitchenObjectParentStates kitchenObjectParentStates = GetKitchenObjectParentStates(kitchenObject);
        kitchenObjectParentStates.AddKitchenObjectState(clientId, iteractionTime, kitchenObject, parentAdded, parentRemoved);
        kitchenObjectParentStates.ClearExpiredKitchenObjectParentStates();
        kitchenObjectToStates[kitchenObject] = kitchenObjectParentStates;
    }
    public bool MaybeAddStateChange(ulong clientId, double iteractionTime, KitchenObject kitchenObject, IKitchenObjectParent parentAdded, IKitchenObjectParent parentRemoved)
    {
        KitchenObjectParentStates kitchenObjectParentStates = GetKitchenObjectParentStates(kitchenObject);
        if (kitchenObjectParentStates.HasState(iteractionTime))
            return false;

        kitchenObjectToStates[kitchenObject] = kitchenObjectParentStates;
        SortedList<double, KitchenObjectParentState> statesUnconflicted = kitchenObjectParentStates.GetUnconflictedStates();
        KitchenObjectParentState curKitchenObjectParentState = null;
        if (statesUnconflicted != null)
            curKitchenObjectParentState = statesUnconflicted.Last().Value; 

        AddStateChange(clientId, iteractionTime, kitchenObject, parentAdded, parentRemoved);
        if (kitchenObjectParentStates.KitchenObjectParent == curKitchenObjectParentState)
            return false;
        else
            return true;
    }
    private KitchenObjectParentStates GetKitchenObjectParentStates(KitchenObject kitchenObject)
    {
        if (!kitchenObjectToStates.TryGetValue(kitchenObject, out KitchenObjectParentStates states))
            states = new KitchenObjectParentStates();
        return states;
    }
    public IKitchenObjectParent GetKitchenObjectParent(KitchenObject kitchenObject)
    {
        KitchenObjectParentStates kitchenObjectParentStates = GetKitchenObjectParentStates(kitchenObject);
        return kitchenObjectParentStates.KitchenObjectParent;
    }
}
