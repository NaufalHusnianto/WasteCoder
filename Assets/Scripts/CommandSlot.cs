using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CommandSlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("SLOT SETTINGS")]
    public bool staticSlot = false; // Jika true, tidak bisa dihapus saat reset
    public bool canReceiveCommands = true; // Jika false, tidak bisa menerima drop command
    
    [Header("VISUAL SETTINGS")]
    public Image slotImage;
    public Color highlightColor = Color.yellow;
    public Color staticSlotColor = Color.cyan;
    public Color disabledSlotColor = Color.gray;
    
    private Color normalColor;
    private Color currentColor;
    
    void Start()
    {
        // Inisialisasi komponen image
        if (slotImage == null)
            slotImage = GetComponent<Image>();
        
        normalColor = slotImage.color;
        UpdateSlotAppearance();
    }
    
    void UpdateSlotAppearance()
    {
        if (!canReceiveCommands)
        {
            slotImage.color = disabledSlotColor;
            currentColor = disabledSlotColor;
        }
        else if (staticSlot)
        {
            slotImage.color = staticSlotColor;
            currentColor = staticSlotColor;
        }
        else
        {
            slotImage.color = normalColor;
            currentColor = normalColor;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (!canReceiveCommands)
        {
            Debug.Log($"Slot {gameObject.name} tidak bisa menerima command");
            return;
        }
        
        Debug.Log("Drop detected in slot: " + gameObject.name);

        // Hapus child yang ada jika slot tidak static
        if (transform.childCount != 0 && !staticSlot)
        {
            Debug.Log($"Menghapus command lama dari slot {gameObject.name}");
            Destroy(transform.GetChild(0).gameObject);
        }
        else if (transform.childCount != 0 && staticSlot)
        {
            Debug.Log($"Slot {gameObject.name} adalah static, tidak bisa diubah");
            return;
        }
        
        // Cari original draggable command dari object yang di-drag
        GameObject draggedObject = eventData.pointerDrag;
        if (draggedObject != null)
        {
            // Coba cari DraggableCommand (untuk Move, TurnLeft, TurnRight)
            DraggableCommand draggable = draggedObject.GetComponentInParent<DraggableCommand>();
            if (draggable != null)
            {
                Debug.Log($"Command berhasil masuk ke slot {gameObject.name} (DraggableCommand)");
                draggable.CreatePermanentClone(transform);
            }
            else
            {
                // Coba cari CollectCommand (untuk CollectTrash)
                CollectCommand collectCommand = draggedObject.GetComponentInParent<CollectCommand>();
                if (collectCommand != null)
                {
                    Debug.Log($"Command berhasil masuk ke slot {gameObject.name} (CollectCommand)");
                    collectCommand.CreatePermanentClone(transform);
                }
                else
                {
                    Debug.LogWarning("Tidak ada script draggable yang ditemukan pada object yang di-drag");
                }
            }
        }
        
        slotImage.color = currentColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Highlight slot ketika drag masuk, hanya jika bisa menerima command
        if (eventData.dragging && canReceiveCommands)
        {
            slotImage.color = highlightColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Kembalikan warna yang sesuai
        slotImage.color = currentColor;
    }
    
    // Method untuk mengubah setting slot secara runtime
    public void SetStatic(bool isStatic)
    {
        staticSlot = isStatic;
        UpdateSlotAppearance();
    }
    
    public void SetCanReceiveCommands(bool canReceive)
    {
        canReceiveCommands = canReceive;
        UpdateSlotAppearance();
    }
    
    // Method untuk mereset isi slot (hanya jika bukan static slot)
    public bool TryClearSlot()
    {
        if (staticSlot)
        {
            Debug.Log($"Slot {gameObject.name} adalah static, tidak bisa direset");
            return false;
        }
        
        if (transform.childCount > 0)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
            Debug.Log($"Slot {gameObject.name} berhasil direset");
            return true;
        }
        return false;
    }
    
    // Method untuk mendapatkan informasi tentang command di slot ini
    public string GetSlotCommandInfo()
    {
        if (transform.childCount == 0)
            return "Empty";
        
        // Ambil command dari child pertama
        Transform child = transform.GetChild(0);
        string childName = child.name.ToLower();
        
        if (childName.Contains("move")) return "Move";
        if (childName.Contains("turnleft") || childName.Contains("left")) return "TurnLeft";
        if (childName.Contains("turnright") || childName.Contains("right")) return "TurnRight";
        if (childName.Contains("collect") || childName.Contains("trash")) return "CollectTrash";
        if (childName.Contains("deposit") || childName.Contains("bin")) return "DepositTrash";
        
        return "Unknown";
    }
    
    // Method untuk memeriksa apakah slot memiliki command
    public bool HasCommand()
    {
        return transform.childCount > 0;
    }
}