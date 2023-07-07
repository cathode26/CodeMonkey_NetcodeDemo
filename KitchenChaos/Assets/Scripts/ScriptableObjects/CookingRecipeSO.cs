using UnityEngine;

[CreateAssetMenu()]
public class CookingRecipeSO : ScriptableObject
{
    public enum State
    {
        Cooking, Cooked, Burned
    }
    public State cookState;
    public KitchenObjectSO input;
    public KitchenObjectSO output;
    public float cookTime;       //time is in seconds
}
