using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class PlatingRecipeSO : ScriptableObject
{
    public List<KitchenObjectSO> input;
    public KitchenObjectSO output;
}
