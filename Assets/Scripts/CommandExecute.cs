using System.Collections;
using UnityEngine;

public class CommandExecute : MonoBehaviour
{
    [Header("REFERENCES")]
    public CommandManager commandManager;
    
    [Header("MOVEMENT SETTINGS")]
    public float moveDistance = 1f;
    public float moveDuration = 0.5f;
    public float rotationDuration = 0.3f;
    
    [Header("DEBUG")]
    public bool debugMode = true;
    
    private bool isExecuting = false;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    
    void Start()
    {
        // Simpan posisi dan rotasi awal
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        
        // Jika CommandManager belum diassign, cari di scene
        if (commandManager == null)
        {
            commandManager = FindObjectOfType<CommandManager>();
        }
    }
    
    public void ExecuteAllCommands()
    {
        if (isExecuting)
        {
            Debug.LogWarning("⚠️ Robot sedang mengeksekusi command!");
            return;
        }
        
        if (commandManager == null)
        {
            Debug.LogError("❌ CommandManager belum diassign!");
            return;
        }
        
        StartCoroutine(ExecuteCommandsCoroutine());
    }
    
    private IEnumerator ExecuteCommandsCoroutine()
    {
        isExecuting = true;
        
        string[] commands = commandManager.GetCommandArray();
        
        if (debugMode)
        {
            Debug.Log("🚀 **MULAI EKSEKUSI COMMAND**");
            Debug.Log($"📋 Total command: {commands.Length}");
        }
        
        for (int i = 0; i < commands.Length; i++)
        {
            string command = commands[i];
            
            if (command == "Empty") 
            {
                if (debugMode) Debug.Log($"   ⏭️ Slot {i + 1}: Empty - Dilewati");
                continue;
            }
            
            if (debugMode) Debug.Log($"   ▶️ Slot {i + 1}: {command}");
            
            switch (command)
            {
                case "Move":
                    yield return StartCoroutine(MoveForward());
                    break;
                    
                case "TurnLeft":
                    yield return StartCoroutine(TurnLeft());
                    break;
                    
                case "TurnRight":
                    yield return StartCoroutine(TurnRight());
                    break;
                    
                default:
                    if (debugMode) Debug.LogWarning($"   ⚠️ Command tidak dikenali: {command}");
                    break;
            }
            
            // Jeda kecil antara command
            yield return new WaitForSeconds(0.1f);
        }
        
        if (debugMode) Debug.Log("✅ **EKSEKUSI COMMAND SELESAI**");
        
        isExecuting = false;
    }
    
    private IEnumerator MoveForward()
    {
        if (debugMode) Debug.Log("      🚶 Bergerak maju...");
        
        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition + transform.forward * moveDistance;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < moveDuration)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / moveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        transform.position = endPosition;
    }
    
    private IEnumerator TurnLeft()
    {
        if (debugMode) Debug.Log("      ↪️ Belok kiri...");
        
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = startRotation * Quaternion.Euler(0, -90, 0);
        
        float elapsedTime = 0f;
        
        while (elapsedTime < rotationDuration)
        {
            transform.rotation = Quaternion.Lerp(startRotation, endRotation, elapsedTime / rotationDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        transform.rotation = endRotation;
    }
    
    private IEnumerator TurnRight()
    {
        if (debugMode) Debug.Log("      ↪️ Belok kanan...");
        
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = startRotation * Quaternion.Euler(0, 90, 0);
        
        float elapsedTime = 0f;
        
        while (elapsedTime < rotationDuration)
        {
            transform.rotation = Quaternion.Lerp(startRotation, endRotation, elapsedTime / rotationDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        transform.rotation = endRotation;
    }
    
    public void ResetRobot()
    {
        if (isExecuting)
        {
            Debug.LogWarning("⚠️ Tidak bisa reset saat robot sedang bergerak!");
            return;
        }
        
        StopAllCoroutines();
        isExecuting = false;
        
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        
        if (debugMode) Debug.Log("🔄 Robot direset ke posisi awal");
    }
    
    // Method untuk UI Button - Execute
    public void ExecuteCommandsButton()
    {
        ExecuteAllCommands();
    }
    
    // Method untuk UI Button - Reset
    public void ResetRobotButton()
    {
        ResetRobot();
    }
    
    // Method untuk mengubah kecepatan eksekusi
    public void SetExecutionSpeed(float speedMultiplier)
    {
        moveDuration = 0.5f / speedMultiplier;
        rotationDuration = 0.3f / speedMultiplier;
        
        if (debugMode) Debug.Log($"🎚️ Kecepatan eksekusi diubah: {speedMultiplier}x");
    }
    
    // Method untuk mengecek status eksekusi
    public bool IsExecuting()
    {
        return isExecuting;
    }
}