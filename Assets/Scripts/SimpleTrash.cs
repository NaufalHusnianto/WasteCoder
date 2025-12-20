using UnityEngine;

public class SimpleTrash : MonoBehaviour
{
    [Header("Trash Settings")]
    public bool isCollectable = true;
    
    void Start()
    {
        // Tambahkan collider jika belum ada
        if (GetComponent<Collider>() == null)
        {
            gameObject.AddComponent<BoxCollider>().isTrigger = true;
        }
    }
    
    void Update()
    {
        // Animasi rotasi sederhana
        transform.Rotate(0, 30 * Time.deltaTime, 0);
        
        // Animasi naik turun
        // float newY = Mathf.Sin(Time.time * 4f) * 0.1f;
        // transform.position = new Vector3(
        //     transform.position.x, 
        //     transform.position.y + newY, 
        //     transform.position.z
        // );
    }
    
    // Method untuk collect
    public void Collect()
    {
        if (!isCollectable) return;
        
        isCollectable = false;
        gameObject.SetActive(false);
        Debug.Log($"🗑️ {name} collected!");
    }
    
    // Reset
    public void ResetTrash()
    {
        isCollectable = true;
        gameObject.SetActive(true);
    }
}