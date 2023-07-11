using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class RecipeBookSO : ScriptableObject
{
    public List<PlatingRecipeSO> recipes = new List<PlatingRecipeSO>();
}
