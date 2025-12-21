using UnityEngine;

public class TrashBin : MonoBehaviour
{
    public enum BinType { Organik, Anorganik }
    
    [Header("BIN SETTINGS")]
    public BinType binType = BinType.Organik;
    public float depositRange = 2f;
    
    [Header("VISUAL")]
    public Material organikBinMaterial;
    public Material anorganikBinMaterial;
    public ParticleSystem successEffect;
    
    private Renderer binRenderer;
    
    void Start()
    {
        binRenderer = GetComponent<Renderer>();
        
        // Tambahkan trigger collider
        BoxCollider trigger = gameObject.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = new Vector3(depositRange, 2f, depositRange);
        
        SetupVisual();
    }
    
    void SetupVisual()
    {
        if (binRenderer != null)
        {
            // Set material berdasarkan jenis
            binRenderer.material = binType == BinType.Organik 
                ? organikBinMaterial 
                : anorganikBinMaterial;
            
            // Set warna (jika tidak ada material)
            binRenderer.material.color = binType == BinType.Organik 
                ? new Color(0, 0.5f, 0) // Hijau gelap
                : new Color(0.8f, 0.8f, 0); // Kuning
        }
    }
    
    public bool CanAcceptTrash(ConditionalTrash.TrashType trashType)
    {
        return (trashType == ConditionalTrash.TrashType.Organik && binType == BinType.Organik) ||
               (trashType == ConditionalTrash.TrashType.Anorganik && binType == BinType.Anorganik);
    }
    
    public void PlaySuccessEffect()
    {
        if (successEffect != null)
        {
            successEffect.Play();
        }
    }
    
    public void StopEffects()
    {
        if (successEffect != null)
        {
            successEffect.Stop();
            successEffect.Clear();
        }
    }
    
    // Untuk debug
    void OnDrawGizmosSelected()
    {
        // Gambar wire sphere untuk deposit range
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, depositRange);
    }
}