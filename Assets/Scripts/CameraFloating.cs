using UnityEngine;

public class CameraFloating : MonoBehaviour
{
    private Vector3 startPosition;
    private float timer = 0f;
    
    void Start()
    {
        startPosition = transform.position;
    }
    
    void Update()
    {
        timer += Time.deltaTime * 0.2f;
        
        // Pergerakan mengambang halus
        float floatX = Mathf.Sin(timer * 0.8f) * 0.1f;
        float floatY = Mathf.Cos(timer * 1.2f) * 0.05f;
        float floatZ = Mathf.Sin(timer * 0.5f) * 0.08f;
        
        transform.position = startPosition + new Vector3(floatX, floatY, floatZ);
    }
}