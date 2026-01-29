using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotExecutePart2 : MonoBehaviour
{
    [Header("REFERENCES")]
    public CommandManagerPart2 commandManager;
    
    [Header("ARROW INDICATOR")]
    public CommandArrowIndicator arrowIndicator;
    
    [Header("MOVEMENT SETTINGS")]
    public float moveDistance = 1f;
    public float moveDuration = 0.5f;
    public float rotationDuration = 0.3f;
    
    [Header("SENSOR SETTINGS")]
    public float sensorRange = 2f;
    public float collectRange = 1.5f;
    
    [Header("SOUND EFFECTS")]
    public AudioSource audioSource;
    public AudioClip moveSound;
    public AudioClip turnSound;
    public AudioClip collectSound;
    public AudioClip depositSound;
    public AudioClip failSound;
    public float moveSoundVolume = 0.5f;
    public float turnSoundVolume = 0.3f;
    
    [Header("DEBUG")]
    public bool debugMode = true;
    
    private bool isExecuting = false;
    private string carriedTrash = "None"; // "Organik", "Anorganik", "None"
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    
    void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        
        // Cari reference jika belum diassign
        if (commandManager == null)
            commandManager = FindObjectOfType<CommandManagerPart2>();
            
        if (arrowIndicator == null)
            arrowIndicator = FindObjectOfType<CommandArrowIndicator>();
            
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
            
        if (audioSource == null && gameObject.GetComponent<AudioSource>() == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        
        if (debugMode)
        {
            Debug.Log($"🤖 Robot Part 2 Initialize:");
            Debug.Log($"- Position: {initialPosition}");
            Debug.Log($"- Move Distance: {moveDistance}");
            Debug.Log($"- Collect Range: {collectRange}");
            Debug.Log($"- Arrow Indicator: {arrowIndicator != null}");
        }
    }
    
    public void ExecuteCommands()
    {
        if (isExecuting) 
        {
            Debug.LogWarning("🤖 Robot sedang bergerak!");
            return;
        }
        
        StartCoroutine(ExecuteRoutine());
    }
    
    IEnumerator ExecuteRoutine()
    {
        isExecuting = true;
        
        // Start arrow indicator
        if (arrowIndicator != null)
        {
            // Delay sedikit untuk sinkronisasi
            Invoke("StartArrowIndicator", 0.3f);
        }
        
        string[] commands = commandManager.GetCommandArray();
        
        // DEBUG: Tampilkan array
        if (debugMode)
        {
            Debug.Log("📋 COMMAND ARRAY SEBELUM EKSEKUSI:");
            for (int i = 0; i < commands.Length; i++)
            {
                Debug.Log($"  [{i}] {commands[i]}");
            }
            
            Debug.Log("🚀 Robot Part 2 mulai eksekusi...");
        }
        
        for (int i = 0; i < commands.Length; i++)
        {
            string cmd = commands[i];
            if (cmd == "Empty") continue;
            
            if (debugMode) Debug.Log($"   [{i+1}] {cmd}");
            
            // ======== HANDLE PERCABANGAN ========
            if (cmd == "IF_ORGANIK" || cmd == "IF_ANORGANIK")
            {
                string expectedType = cmd == "IF_ORGANIK" ? "Organik" : "Anorganik";
                bool conditionTrue = (carriedTrash == expectedType);
                
                if (debugMode) Debug.Log($"   🔍 IF {expectedType}: Bawa {carriedTrash} -> {conditionTrue}");
                
                // Cari END_IF
                int endIfIndex = FindEndIfIndex(commands, i);
                
                // DEBUG: Tampilkan blok IF
                if (debugMode) Debug.Log($"   📍 Blok IF dari index {i+1} sampai {endIfIndex+1}");
                
                // Eksekusi jika kondisi benar
                if (conditionTrue)
                {
                    if (debugMode) Debug.Log("   ✅ Jalankan blok IF");
                    i++; // Lewati IF command
                    while (i < endIfIndex && commands[i] != "ELSE")
                    {
                        if (commands[i] != "Empty" && commands[i] != "END_IF")
                        {
                            if (debugMode) Debug.Log($"   ▶️ Eksekusi dalam IF: {commands[i]}");
                            yield return ExecuteSingleCommand(commands[i]);
                        }
                        i++;
                    }
                }
                else // Kondisi salah, skip ke ELSE
                {
                    if (debugMode) Debug.Log("   ❌ Skip ke ELSE");
                    i++; // Lewati IF command
                    // Cari ELSE
                    while (i < endIfIndex && commands[i] != "ELSE")
                    {
                        if (debugMode) Debug.Log($"   ⏭️ Skip: {commands[i]}");
                        i++; // Skip semua command sampai ELSE
                    }
                    
                    if (i < endIfIndex && commands[i] == "ELSE")
                    {
                        i++; // Lewati ELSE
                        // Jalankan blok ELSE
                        while (i < endIfIndex)
                        {
                            if (commands[i] != "Empty" && commands[i] != "END_IF")
                            {
                                if (debugMode) Debug.Log($"   ▶️ Eksekusi dalam ELSE: {commands[i]}");
                                yield return ExecuteSingleCommand(commands[i]);
                            }
                            i++;
                        }
                    }
                }
                
                i = endIfIndex; // Loncat ke END_IF
                continue;
            }
            
            // ======== EXECUTE COMMAND BIASA ========
            yield return ExecuteSingleCommand(cmd);
        }
        
        if (debugMode) Debug.Log("✅ Eksekusi selesai!");
        
        // Stop arrow indicator setelah delay
        if (arrowIndicator != null)
            Invoke("StopArrowIndicator", 1f);
            
        isExecuting = false;
    }
    
    // Method untuk memulai arrow indicator
    private void StartArrowIndicator()
    {
        if (arrowIndicator != null)
        {
            arrowIndicator.StartFollowing();
        }
    }
    
    // Method untuk menghentikan arrow indicator
    private void StopArrowIndicator()
    {
        if (arrowIndicator != null)
        {
            arrowIndicator.StopFollowing();
        }
    }

    // FIX method FindEndIfIndex:
    int FindEndIfIndex(string[] commands, int startIndex)
    {
        for (int i = startIndex + 1; i < commands.Length; i++)
        {
            if (commands[i] == "END_IF") 
            {
                if (debugMode) Debug.Log($"   🔎 END_IF ditemukan di index {i}");
                return i;
            }
        }
        Debug.LogWarning($"   ⚠️ END_IF tidak ditemukan setelah index {startIndex}");
        return commands.Length - 1;
    }
    
    IEnumerator ExecuteSingleCommand(string command)
    {
        switch (command)
        {
            case "Move":
                yield return StartCoroutine(MoveForward());
                break;
                
            case "TurnLeft":
                yield return StartCoroutine(Rotate(-90));
                break;
                
            case "TurnRight":
                yield return StartCoroutine(Rotate(90));
                break;
                
            case "AmbilSampah":
                yield return StartCoroutine(CollectTrash());
                break;
                
            case "BuangSampah":
                yield return StartCoroutine(DepositTrash());
                break;
                
            case "ELSE":
            case "END_IF":
                // Do nothing, hanya penanda
                break;
        }
        
        yield return new WaitForSeconds(0.5f);
    }
    
    // COLLECT TRASH dengan ConditionalTrash
    IEnumerator CollectTrash()
    {
        if (debugMode) Debug.Log("   🔍 Mencari sampah...");

        // yield return new WaitForSeconds(0.2f);
        
        // Cari semua ConditionalTrash di scene
        ConditionalTrash[] allTrash = FindObjectsOfType<ConditionalTrash>();
        
        // Jika pakai SimpleConditionalTrash:
        // SimpleConditionalTrash[] allTrash = FindObjectsOfType<SimpleConditionalTrash>();
        
        bool foundTrash = false;
        
        foreach (ConditionalTrash trash in allTrash)
        {
            if (trash != null && trash.IsCollectable() && trash.gameObject.activeInHierarchy)
            {
                float distance = Vector3.Distance(transform.position, trash.transform.position);
                
                if (distance <= collectRange)
                {
                    // Play sound
                    PlaySound(collectSound);
                    
                    // Ambil sampah
                    carriedTrash = trash.GetTrashType().ToString();
                    trash.Collect(); // Ini akan memanggil gameObject.SetActive(false)
                    
                    if (debugMode) Debug.Log($"   📦 Mengambil sampah: {carriedTrash}");
                    foundTrash = true;
                    
                    yield return StartCoroutine(PlayCollectAnimation());
                    yield break;
                }
            }
        }
        
        if (!foundTrash)
        {
            if (debugMode) Debug.Log("   ❌ Tidak ada sampah di dekat robot!");
            PlaySound(failSound);
            yield return StartCoroutine(PlayFailAnimation());
        }
    }
    
    // DEPOSIT TRASH ke TrashBin
    IEnumerator DepositTrash()
    {
        // yield return new WaitForSeconds(0.2f);

        if (carriedTrash == "None")
        {
            if (debugMode) Debug.Log("   ❌ Tidak membawa sampah!");
            PlaySound(failSound);
            yield return StartCoroutine(PlayFailAnimation());
            yield break;
        }
        
        if (debugMode) Debug.Log($"   🗑️ Mencari tempat sampah untuk {carriedTrash}...");
        
        // Cari semua TrashBin
        TrashBin[] allBins = FindObjectsOfType<TrashBin>();
        
        foreach (TrashBin bin in allBins)
        {
            float distance = Vector3.Distance(transform.position, bin.transform.position);
            
            if (distance <= bin.depositRange)
            {
                // Cek jenis sampah cocok
                bool jenisCocok = (carriedTrash == "Organik" && bin.binType == TrashBin.BinType.Organik) ||
                                 (carriedTrash == "Anorganik" && bin.binType == TrashBin.BinType.Anorganik);
                
                if (jenisCocok)
                {
                    // Play sound
                    PlaySound(depositSound);
                    
                    // Buang sampah
                    bin.PlaySuccessEffect();
                    carriedTrash = "None";
                    
                    if (debugMode) Debug.Log($"   ✅ Sampah dibuang di tempat yang benar!");
                    yield return StartCoroutine(PlayCollectAnimation());
                    yield break;
                }
                else
                {
                    if (debugMode) Debug.Log($"   ❌ Jenis sampah tidak cocok!");
                }
            }
        }
        
        if (debugMode) Debug.Log("   ❌ Tidak ada tempat sampah yang cocok!");
        PlaySound(failSound);
        yield return StartCoroutine(PlayFailAnimation());
    }
    
    IEnumerator MoveForward()
    {
        if (debugMode) Debug.Log("      🚶 Bergerak maju...");
        
        PlaySound(moveSound, moveSoundVolume);
        
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + transform.forward * moveDistance;
        
        float elapsed = 0;
        while (elapsed < moveDuration)
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsed / moveDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = endPos;
    }
    
    IEnumerator Rotate(float angle)
    {
        string direction = angle > 0 ? "kanan" : "kiri";
        if (debugMode) Debug.Log($"      ↩️ Belok {direction}...");
        
        PlaySound(turnSound, turnSoundVolume);
        
        Quaternion startRot = transform.rotation;
        Quaternion endRot = startRot * Quaternion.Euler(0, angle, 0);
        
        float elapsed = 0;
        while (elapsed < rotationDuration)
        {
            transform.rotation = Quaternion.Slerp(startRot, endRot, elapsed / rotationDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.rotation = endRot;
        
        // yield return new WaitForSeconds(0.1f);
    }
    
    IEnumerator PlayCollectAnimation()
    {
        Vector3 originalScale = transform.localScale;
        transform.localScale = originalScale * 1.2f;
        yield return new WaitForSeconds(0.2f);
        transform.localScale = originalScale;
    }
    
    IEnumerator PlayFailAnimation()
    {
        for (int i = 0; i < 2; i++)
        {
            transform.localScale = Vector3.one * 0.9f;
            yield return new WaitForSeconds(0.1f);
            transform.localScale = Vector3.one;
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    // Method untuk UI
    public void OnExecuteButton()
    {
        // Eksekusi command
        ExecuteCommands();
        
        // Start arrow dengan delay
        if (arrowIndicator != null)
        {
            Invoke("StartArrowIndicator", 0.5f);
        }
    }
    
    public void OnResetButtonClicked()
    {
        ResetRobot();
        
        // Stop arrow indicator
        if (arrowIndicator != null)
        {
            arrowIndicator.OnResetButtonClicked();
        }
    }
    
    // Method untuk LevelManager
    public bool IsExecuting()
    {
        return isExecuting;
    }
    
    public string GetCarriedTrash()
    {
        return carriedTrash;
    }
    
    public void ResetRobot()
    {
        StopAllCoroutines();
        isExecuting = false;
        carriedTrash = "None";
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        transform.localScale = Vector3.one;
        
        // Reset semua ConditionalTrash
        ConditionalTrash[] allTrash = FindObjectsOfType<ConditionalTrash>();
        foreach (ConditionalTrash trash in allTrash)
        {
            trash.ResetTrash(); // Ini akan memanggil gameObject.SetActive(true)
        }
        
        // Reset semua TrashBin effects
        TrashBin[] allBins = FindObjectsOfType<TrashBin>();
        foreach (TrashBin bin in allBins)
        {
            bin.StopEffects();
        }
        
        if (debugMode) Debug.Log("🔄 Robot dan lingkungan direset");
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