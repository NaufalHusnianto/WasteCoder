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
        Debug.Log("Drop detected in slot: " + gameObject.name);

        if (transform.childCount != 0)
        {
            Debug.Log("Sudah ada isinya");
            Destroy(transform.GetChild(0).gameObject);
        }
        
        // Cari original draggable command dari object yang di-drag
        GameObject draggedObject = eventData.pointerDrag;
        if (draggedObject != null)
        {
            // Coba cari DraggableCommand (untuk Move, TurnLeft, TurnRight)
            DraggableCommand draggable = draggedObject.GetComponentInParent<DraggableCommand>();
            if (draggable != null)
            {
                Debug.Log("Command berhasil masuk (DraggableCommand)");
                draggable.CreatePermanentClone(transform);
            }
            else
            {
                // Coba cari CollectCommand (untuk CollectTrash)
                CollectCommand collectCommand = draggedObject.GetComponentInParent<CollectCommand>();
                if (collectCommand != null)
                {
                    Debug.Log("Command berhasil masuk (CollectCommand)");
                    collectCommand.CreatePermanentClone(transform);
                }
                else
                {
                    Debug.LogWarning("Tidak ada script draggable yang ditemukan pada object yang di-drag");
                }
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