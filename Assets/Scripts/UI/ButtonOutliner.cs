using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIButtonImageOutlineHover : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    public Image targetImage;

    public Color outlineColor = Color.white;
    public Vector2 outlineDistance = new Vector2(2f, 2f);

    Outline outline;

    void Awake()
    {
        if (targetImage == null)
            targetImage = GetComponent<Image>();

        outline = targetImage.GetComponent<Outline>();
        if (outline == null)
            outline = targetImage.gameObject.AddComponent<Outline>();

        outline.effectColor = outlineColor;
        outline.effectDistance = outlineDistance;
        outline.enabled = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        outline.enabled = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        outline.enabled = false;
    }
}