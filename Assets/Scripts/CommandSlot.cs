using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CommandSlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image slotImage;
    public Color highlightColor = Color.yellow;
    private Color normalColor;
    
    void Start()
    {
        if (slotImage == null)
            slotImage = GetComponent<Image>();
        
        normalColor = slotImage.color;
    }

    public void OnDrop(PointerEventData eventData)
    {
        // Debug.Log("Drop detected in slot: " + gameObject.name);

        if (transform.childCount != 0)
        {
            Debug.Log("Sudah ada isinya");
            Destroy(transform.GetChild(0).gameObject);
        }
        
        // Cari original draggable command dari object yang di-drag
            GameObject draggedObject = eventData.pointerDrag;
        if (draggedObject != null)
        {
            DraggableCommand draggable = draggedObject.GetComponentInParent<DraggableCommand>();
            if (draggable != null)
            {
                Debug.Log("command berhasil masuk");
                draggable.CreatePermanentClone(transform);
            }
        }
        
        slotImage.color = normalColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Highlight slot ketika drag masuk
        if (eventData.dragging)
        {
            slotImage.color = highlightColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Kembalikan warna normal
        slotImage.color = normalColor;
    }
}