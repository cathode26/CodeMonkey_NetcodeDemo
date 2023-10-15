using UnityEngine;
using UnityEngine.UI;

public class RecipeUISetter : MonoBehaviour
{
    [SerializeField]
    private TMPro.TextMeshProUGUI recipeTextMesh;
    [SerializeField]
    private Transform iconContainer;
    [SerializeField]
    private Transform iconTemplate;

    public void SetText(PlatingRecipeSO platingRecipe)
    {
        recipeTextMesh.text = platingRecipe.recipeName;
        foreach (KitchenObjectSO kitchenObjectSO in platingRecipe.input)
        {
            Transform icon = Instantiate(iconTemplate, iconContainer);
            icon.GetComponent<Image>().sprite = kitchenObjectSO.sprite;
            icon.gameObject.SetActive(true);
        }
    }
    public string GetText()
    {
        return recipeTextMesh.text;
    }
}
