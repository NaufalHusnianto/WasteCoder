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
    
    [Header("TRASH SETTINGS")]
    public float collectDistance = 1.5f;

    [Header("SOUND EFFECTS")]
    public AudioSource audioSource;
    public AudioClip moveSound;
    public AudioClip turnSound;
    public AudioClip collectSound;
    public AudioClip failSound;
    public float moveSoundVolume = 0.5f;
    public float turnSoundVolume = 0.3f;
    
    [Header("DEBUG")]
    public bool debugMode = true;

    [Header("ARROW INDICATOR")]
    public CommandArrowIndicator arrowIndicator;
    
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
        
        if (debugMode)
        {
            Debug.Log($"🤖 Robot Initialize:");
            Debug.Log($"- Position: {initialPosition}");
            Debug.Log($"- Move Distance: {moveDistance}");
            Debug.Log($"- Collect Distance: {collectDistance}");
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

        // Notify arrow indicator
        if (arrowIndicator == null)
            arrowIndicator = FindObjectOfType<CommandArrowIndicator>();
            
        if (arrowIndicator != null)
            arrowIndicator.StartFollowing();
        
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
                    
                case "CollectTrash":
                    yield return StartCoroutine(CollectTrash());
                    break;
            }
            
            yield return new WaitForSeconds(0.3f);
        }
        
        if (debugMode) Debug.Log("✅ **EKSEKUSI COMMAND SELESAI**");
        
        isExecuting = false;

        // Optional: Stop arrow after delay
        if (arrowIndicator != null)
            Invoke("StopArrowIndicator", 1f);
    }

    private void StopArrowIndicator()
    {
        if (arrowIndicator != null)
            arrowIndicator.StopFollowing();
    }

    public void OnExecuteButton()
    {
        // Eksekusi command
        ExecuteAllCommands();
        
        // Start arrow
        if (arrowIndicator != null)
            arrowIndicator.OnExecuteButtonClicked();
    }
    
    private IEnumerator MoveForward()
    {
        if (debugMode) Debug.Log("      🚶 Bergerak maju...");

        PlaySound(moveSound, moveSoundVolume);
        
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
        if (debugMode) Debug.Log("      ↩️ Belok kiri...");

        PlaySound(turnSound, turnSoundVolume);
        
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = startRotation * Quaternion.Euler(0, -90, 0);
        
        float elapsedTime = 0f;
        while (elapsedTime < rotationDuration)
        {
            transform.rotation = Quaternion.Lerp(startRotation, endRotation, elapsedTime / rotationDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.2f);
        
        transform.rotation = endRotation;
    }
    
    private IEnumerator TurnRight()
    {
        if (debugMode) Debug.Log("      ↪️ Belok kanan...");

        PlaySound(turnSound, turnSoundVolume);
        
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = startRotation * Quaternion.Euler(0, 90, 0);
        
        float elapsedTime = 0f;
        while (elapsedTime < rotationDuration)
        {
            transform.rotation = Quaternion.Lerp(startRotation, endRotation, elapsedTime / rotationDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.2f);
        
        transform.rotation = endRotation;
    }
    
    // COLLECT TRASH: Sederhana untuk Level 1-3
    private IEnumerator CollectTrash()
    {
        if (debugMode) Debug.Log("      🗑️ Mencari sampah...");

        // delay time
        yield return new WaitForSeconds(0.2f);

        PlaySound(collectSound);
        
        // Cari semua sampah di scene
        SimpleTrash[] simpleTrashArray = FindObjectsOfType<SimpleTrash>();
        
        if (debugMode) 
            Debug.Log($"      🔍 Found: {simpleTrashArray.Length} SimpleTrash");
        
        bool foundTrash = false;

        
        // Cek SimpleTrash
        foreach (SimpleTrash trash in simpleTrashArray)
        {
            if (trash.isCollectable)
            {
                foundTrash = true;
                float distance = Vector3.Distance(transform.position, trash.transform.position);
                
                if (debugMode) 
                    Debug.Log($"      📏 Distance to {trash.name}: {distance:F2} (Max: {collectDistance})");
                
                if (distance <= collectDistance)
                {
                    trash.Collect();
                    if (debugMode) Debug.Log($"      ✅ {trash.name} dikumpulkan");
                    
                    yield return StartCoroutine(PlayCollectAnimation());
                    yield break;
                }
            }
        }
        
        if (!foundTrash)
        {
            if (debugMode) Debug.Log("      ❌ Tidak ada sampah di scene!");
        }
        else
        {
            if (debugMode) Debug.Log("      ❌ Semua sampah terlalu jauh!");
        }
        
        yield return StartCoroutine(PlayFailAnimation());
    }
    
    // ANIMASI
    private IEnumerator PlayCollectAnimation()
    {
        Vector3 originalScale = transform.localScale;
        transform.localScale = originalScale * 1.2f;
        yield return new WaitForSeconds(0.5f);
        transform.localScale = originalScale;
    }
    
    private IEnumerator PlayFailAnimation()
    {
        PlaySound(failSound);
        for (int i = 0; i < 2; i++)
        {
            transform.localScale = Vector3.one * 0.9f;
            yield return new WaitForSeconds(0.1f);
            transform.localScale = Vector3.one;
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    // RESET: Reset semua objek di scene
    public void ResetRobotAndEnvironment()
    {
        if (isExecuting)
        {
            Debug.LogWarning("⚠️ Tidak bisa reset saat robot sedang bergerak!");
            return;
        }
        
        StopAllCoroutines();
        isExecuting = false;
        
        // 1. Reset robot
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        transform.localScale = Vector3.one;
        
        if (debugMode) Debug.Log("🔄 Robot direset ke posisi awal");
        
        // 2. Reset semua SimpleTrash di scene
        ResetAllSimpleTrash();
    }
    
    private void ResetAllSimpleTrash()
    {
        SimpleTrash[] allSimpleTrash = FindObjectsOfType<SimpleTrash>();
        foreach (SimpleTrash trash in allSimpleTrash)
        {
            trash.ResetTrash();
        }
        
        if (debugMode && allSimpleTrash.Length > 0)
            Debug.Log($"🔄 {allSimpleTrash.Length} SimpleTrash direset");
    }
    
    // Public method untuk UI
    public void ResetRobot()
    {
        ResetRobotAndEnvironment();
    }
    
    public void ExecuteCommandsButton()
    {
        ExecuteAllCommands();
    }
    
    public bool IsExecuting()
    {
        return isExecuting;
    }

    private void PlaySound(AudioClip clip, float volume = 1.0f)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
        else
        {
            if (debugMode) Debug.LogWarning($"Sound effect tidak ditemukan: {clip?.name}");
        }
    }
}