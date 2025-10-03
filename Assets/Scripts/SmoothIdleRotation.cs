using UnityEngine;

public class SmoothIdleRotation : MonoBehaviour
{
    private Vector3 startRotation;
    private float timer = 0f;
    
    void Start()
    {
        // Simpan rotasi awal
        startRotation = transform.eulerAngles;
    }
    
    void Update()
    {
        // Gunakan sinus untuk pergerakan yang smooth dan continuous
        timer += Time.deltaTime * 0.8f; // Kecepatan lebih lambat untuk smoothness
        
        // Sinus memberikan pergerakan yang natural bolak-balik
        float rotation = Mathf.Sin(timer) * 12f; // Sudut rotasi 12 derajat
        
        // Terapkan rotasi pada sumbu Y
        transform.eulerAngles = startRotation + new Vector3(0, rotation, 0);
    }
}