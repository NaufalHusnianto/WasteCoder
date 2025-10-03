using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableCommand : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Image image;
    
    [HideInInspector] public Transform originalParent;
    private GameObject dragClone;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        image = GetComponent<Image>();
        canvas = GetComponentInParent<Canvas>();
        originalParent = transform.parent;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Buat clone untuk di-drag
        dragClone = Instantiate(gameObject, canvas.transform);
        dragClone.name = "DraggedCommand";
        
        // Setup clone
        RectTransform cloneRect = dragClone.GetComponent<RectTransform>();
        cloneRect.sizeDelta = rectTransform.sizeDelta;
        cloneRect.position = rectTransform.position;
        
        // Non-aktifkan draggable pada clone agar tidak recursive
        Destroy(dragClone.GetComponent<DraggableCommand>());
        
        // Setup canvas group pada clone
        CanvasGroup cloneCanvasGroup = dragClone.GetComponent<CanvasGroup>();
        if (cloneCanvasGroup == null) 
            cloneCanvasGroup = dragClone.AddComponent<CanvasGroup>();
        
        cloneCanvasGroup.alpha = 0.8f;
        cloneCanvasGroup.blocksRaycasts = false;
        
        // Non-aktifkan raycast pada original selama drag
        canvasGroup.blocksRaycasts = false;
        image.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragClone != null)
        {
            RectTransform cloneRect = dragClone.GetComponent<RectTransform>();
            cloneRect.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Hancurkan clone drag
        if (dragClone != null)
        {
            Destroy(dragClone);
        }
        
        // Kembalikan original ke state normal
        canvasGroup.blocksRaycasts = true;
        image.raycastTarget = true;
    }

    // Dipanggil oleh CommandSlot untuk membuat instance permanen
    public void CreatePermanentClone(Transform slotParent)
    {
        GameObject permanentClone = Instantiate(gameObject, slotParent);
        permanentClone.name = gameObject.name;
        
        // Setup permanent clone
        RectTransform cloneRect = permanentClone.GetComponent<RectTransform>();
        cloneRect.anchoredPosition = Vector2.zero;
        cloneRect.localScale = Vector3.one;
        
        // Aktifkan draggable pada permanent clone
        DraggableCommand draggable = permanentClone.GetComponent<DraggableCommand>();
        draggable.originalParent = slotParent;
    }
}