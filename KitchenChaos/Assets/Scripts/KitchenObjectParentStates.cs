using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

/**
 * Class: KitchenObjectParentStates
 * 
 * Purpose:
 * This class manages the state history of kitchen objects in a multiplayer game environment.
 * It is primarily used to resolve conflicts when multiple players interact with the same object.
 * Each state is associated with a timestamp and the player who initiated the interaction.
 * The class provides functionality to add new states, retrieve the latest unconflicted state,
 * check for the existence of certain states, and clear expired states.
 *
 * Usage:
 * Used in conjunction with KitchenObjectParentManager to ensure that the player who first
 * interacts with a kitchen object retains ownership after client-side prediction is resolved.
 */

public class KitchenObjectParentStates
{
    // Stores the states of the kitchen object parent sorted by a key value.
    public SortedList<double, KitchenObjectParentState> states = new SortedList<double, KitchenObjectParentState>();

    // Reference to the kitchen object parent.
    public IKitchenObjectParent KitchenObjectParent { get; set; }

 	// The time after which a state is considered expired.
    private const double StateExpiryTime = 5.0; // in seconds

	/// <summary>
    /// Adds a new state for a kitchen object.
    /// </summary>
    public void AddKitchenObjectState(ulong clientId, double iteractionTime, KitchenObject kitchenObject, IKitchenObjectParent parentAdded, IKitchenObjectParent parentRemoved)
    {
        KitchenObjectParentState kitchenObjectParentState = new KitchenObjectParentState(clientId, iteractionTime, kitchenObject, parentAdded, parentRemoved);
        states.Add(kitchenObjectParentState.InteractionTime, kitchenObjectParentState);
        SortedList<double, KitchenObjectParentState> statesUnconflicted = GetUnconflictedStates();
        KitchenObjectParent = statesUnconflicted.Last().Value.KitchenObjectParentAdded;
    }
    public SortedList<double, KitchenObjectParentState> GetUnconflictedStates()
    {
        if(states.Count == 0)
            return null;

        List<double> keysToRemove = new List<double>();

        //loop through the list and remove all states that do not fit
        KitchenObjectParentState curState = states.Values[0];

        for (int i = 1; i < states.Count; i++)
        {
            KitchenObjectParentState nextState = states.Values[i];
            if (curState.KitchenObjectParentAdded != nextState.KitchenObjectParentRemoved)   //If the objects are not equal, there is a conflict
            {
                keysToRemove.Add(states.Values[i].InteractionTime); //so remove the nextState by interaction Time
            }
            else
            {
                //if the next state is removing then the kitchen objects must match
                curState = states.Values[i];
            }
        }
        SortedList<double, KitchenObjectParentState> statesUnconflicted = new SortedList<double, KitchenObjectParentState>(states);
        //now remove the bad keys
        foreach (var key in keysToRemove)
            statesUnconflicted.Remove(key);

        return statesUnconflicted;
    }
    public bool HasState(double iteractionTime)
    {
        return states.ContainsKey(iteractionTime);
    }
    public void ClearExpiredKitchenObjectParentStates()
    {
        //always leave the last state even if it is expired because that state is used validate the current interaction
        if (states.Count <= 1)
            return;

        double timeThreshold = NetworkManager.Singleton.ServerTime.Time - StateExpiryTime;
        // Get all keys except the last one
        var allButLastKeys = states.Keys.Take(states.Count - 1);
        //if the CompareTo is negative it means that the key is earlier than timeThreshold
        List<double> keysToRemove = allButLastKeys.TakeWhile(key => key.CompareTo(timeThreshold) < 0).ToList();
        SortedList<double, KitchenObjectParentState> parentStateListCpy = new SortedList<double, KitchenObjectParentState>(states);
        foreach (var key in keysToRemove)
            states.Remove(key);
    }
    public class KitchenObjectParentState
    {
        public ulong ClientId;
        public double InteractionTime;
        public IKitchenObjectParent KitchenObjectParentRemoved;
        public IKitchenObjectParent KitchenObjectParentAdded;
        public KitchenObject KitchenObject;

        public KitchenObjectParentState(ulong clientId, double interactionTime, KitchenObject kitchenObject, IKitchenObjectParent kitchenObjectParentAdded, IKitchenObjectParent kitchenObjectParentRemoved)
        {
            ClientId = clientId;
            InteractionTime = interactionTime;
            KitchenObject = kitchenObject;
            KitchenObjectParentAdded = kitchenObjectParentAdded;
            KitchenObjectParentRemoved = kitchenObjectParentRemoved;
        }
    }
}