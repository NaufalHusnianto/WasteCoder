using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ConditionalDraggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    
    [HideInInspector] public Transform originalParent;
    private GameObject dragClone;
    
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
        originalParent = transform.parent;
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        // Buat clone untuk drag
        dragClone = Instantiate(gameObject, canvas.transform);
        dragClone.name = "Dragging_" + gameObject.name;
        
        // Setup clone
        RectTransform cloneRect = dragClone.GetComponent<RectTransform>();
        cloneRect.sizeDelta = rectTransform.sizeDelta;
        cloneRect.position = rectTransform.position;
        
        // Non-aktifkan script ini di clone
        Destroy(dragClone.GetComponent<ConditionalDraggable>());
        
        // Setup canvas group
        CanvasGroup cloneCanvasGroup = dragClone.AddComponent<CanvasGroup>();
        cloneCanvasGroup.alpha = 0.7f;
        cloneCanvasGroup.blocksRaycasts = false;
        
        // Non-aktifkan raycast pada original
        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = false;
        
        GetComponent<Image>().raycastTarget = false;
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (dragClone != null)
        {
            dragClone.GetComponent<RectTransform>().anchoredPosition += eventData.delta / canvas.scaleFactor;
        }
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        // Hapus clone
        if (dragClone != null)
        {
            Destroy(dragClone);
        }
        
        // Kembalikan original
        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = true;
        
        GetComponent<Image>().raycastTarget = true;
    }
    
    // Dipanggil oleh CommandSlot
    public void CreatePermanentClone(Transform slotParent)
    {
        GameObject permanentClone = Instantiate(gameObject, slotParent);
        permanentClone.name = gameObject.name;
        
        permanentClone.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        permanentClone.GetComponent<RectTransform>().localScale = Vector3.one;
    }
}