using UnityEngine;

public class ConditionalTrash : MonoBehaviour
{
    public enum TrashType { Organik, Anorganik }
    public TrashType trashType = TrashType.Organik;
    public bool isCollectable = true;
    
    public void Collect()
    {
        if (!isCollectable) return;
        
        isCollectable = false;
        gameObject.SetActive(false); // CUKUP INI SAJA!
        
        Debug.Log($"🗑️ Sampah {trashType} dikumpulkan!");
    }
    
    public void ResetTrash()
    {
        isCollectable = true;
        gameObject.SetActive(true); // AKTIFKAN KEMBALI
        
        Debug.Log($"🔄 Sampah {trashType} direset");
    }
    
    public TrashType GetTrashType() { return trashType; }
    public bool IsCollectable() { return isCollectable; }
}