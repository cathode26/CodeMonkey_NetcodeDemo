using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DeliveryManager : MonoBehaviour
{
    [SerializeField]
    private RecipeBookSO recipeBookSO;  // The recipe associated with this plate
    public static event System.Action<PlatingRecipeSO> OnAddPlatingRecipeChanged;
    public static event System.Action<PlatingRecipeSO> OnRemovedPlatingRecipeChanged;
    public static event System.Action OnRecipeSuccessChanged;
    public static event System.Action OnRecipeFailedChanged;

    private List<PlatingRecipeSO> waitingOnPlatesSO = new List<PlatingRecipeSO>();
    private float spawnRecipeTimer = 0;
    private float spawnRecipeTimerMax = 4.0f;
    private int waitingRecipesMax = 4;

    public RecipeBookSO RecipeBookSO { get => recipeBookSO; private set => recipeBookSO = value; }

    private void Update()
    {
        spawnRecipeTimer += Time.deltaTime;
        if (spawnRecipeTimer >= spawnRecipeTimerMax && waitingOnPlatesSO.Count < waitingRecipesMax)
        {
            System.Guid guid = System.Guid.NewGuid();
            byte[] bytes = guid.ToByteArray();
            int seed = System.BitConverter.ToInt32(bytes, 0);
            Random.InitState(seed);
            spawnRecipeTimer = 0.0f;
            PlatingRecipeSO platingRecipeSO = recipeBookSO.recipes[Random.Range(0, recipeBookSO.recipes.Count)];
            waitingOnPlatesSO.Add(platingRecipeSO);
            OnAddPlatingRecipeChanged.Invoke(platingRecipeSO);
            Debug.Log("Make a " + platingRecipeSO.recipeName);
        }
    }

    public bool DeliverPlate(PlateKitchenObject plateKitchenObject, out string recipeName)
    {
        HashSet<KitchenObjectSO> platedFoods = plateKitchenObject.GetPlatedFoods();
        foreach (PlatingRecipeSO platingRecipeSO in waitingOnPlatesSO)
        {
            if (platingRecipeSO.input.Count == platedFoods.Count && platedFoods.All(food => platingRecipeSO.input.Contains(food)))
            {
                OnRemovedPlatingRecipeChanged.Invoke(platingRecipeSO);
                waitingOnPlatesSO.Remove(platingRecipeSO);
                recipeName = platingRecipeSO.recipeName;
                OnRecipeSuccessChanged();
                return true;
            }
        }
        OnRecipeFailedChanged();
        recipeName = "Wrong Recipe";
        return false;
    }
}
