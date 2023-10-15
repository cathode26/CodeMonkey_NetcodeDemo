using UnityEngine;
using UnityEngine.UI;

public class IconSetter : MonoBehaviour
{
    [SerializeField]
    private Image icon;

    public void SetSprite(Sprite sprite)
    {
        icon.sprite = sprite;
    }
}
