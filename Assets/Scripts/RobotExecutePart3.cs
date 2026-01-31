using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotExecutePart3 : MonoBehaviour
{
    [Header("REFERENCES")]
    public CommandManagerPart3 commandManager;
    
    [Header("ARROW INDICATOR")]
    public CommandArrowIndicator arrowIndicator;
    
    [Header("MOVEMENT SETTINGS")]
    public float moveDistance = 1f;
    public float moveDuration = 0.6f;
    public float rotationDuration = 0.3f;
    
    [Header("COLLECTION SETTINGS")]
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
    
    // Event untuk arrow indicator
    public System.Action<int> OnCommandExecuted;
    
    private bool isExecuting = false;
    private string carriedTrash = "None";
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private int currentCommandIndex = 0;
    
    void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        
        if (commandManager == null)
            commandManager = FindObjectOfType<CommandManagerPart3>();
            
        if (arrowIndicator == null)
            arrowIndicator = FindObjectOfType<CommandArrowIndicator>();
            
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
            
        if (audioSource == null && gameObject.GetComponent<AudioSource>() == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        
        if (debugMode)
        {
            Debug.Log($"🤖 Robot Part 3 Initialize:");
            Debug.Log($"- Position: {initialPosition}");
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
        currentCommandIndex = 0;
        
        // Start arrow indicator
        if (arrowIndicator != null)
        {
            arrowIndicator.StartFollowing();
        }
        
        // Tunggu sedikit agar arrow indicator siap
        yield return new WaitForSeconds(0.1f);
        
        string[] commands = commandManager.GetCommandArray();
        
        Debug.Log("🚀 Robot Part 3 mulai eksekusi...");
        
        for (int i = 0; i < commands.Length; i++)
        {
            string cmd = commands[i];
            if (cmd == "Empty") continue;
            
            Debug.Log($"   [{i+1}] {cmd}");
            
            // HANDLE LOOP - JANGAN trigger arrow untuk LOOP command
            if (cmd.StartsWith("LOOP"))
            {
                int loopCount = GetLoopCount(cmd);
                Debug.Log($"   🔄 LOOP {loopCount}x dimulai");
                
                int endLoopIndex = FindEndLoopIndex(commands, i);
                
                if (endLoopIndex == -1)
                {
                    Debug.LogError("END_LOOP tidak ditemukan!");
                    break;
                }
                
                // JANGAN trigger arrow untuk LOOP command
                // Hanya log saja tanpa memindahkan arrow
                Debug.Log($"   📍 Posisi LOOP di index {i+1} (arrow tidak dipindah)");
                
                // Eksekusi blok loop
                for (int iteration = 0; iteration < loopCount; iteration++)
                {
                    Debug.Log($"   🔄 Iterasi {iteration+1}/{loopCount}");
                    
                    int j = i + 1;
                    while (j < endLoopIndex)
                    {
                        if (commands[j] != "Empty" && commands[j] != "END_LOOP")
                        {
                            Debug.Log($"     [{iteration+1}.{j-i}] {commands[j]}");
                            
                            // HANYA trigger arrow untuk command yang dieksekusi dalam loop
                            TriggerArrowEvent(j);
                            
                            // Tunggu sebelum eksekusi untuk memberi waktu animasi arrow
                            yield return new WaitForSeconds(0.2f);
                            
                            // Eksekusi command
                            yield return ExecuteSingleCommand(commands[j]);
                        }
                        j++;
                    }
                    
                    // Jeda antar iterasi, arrow tetap di command terakhir dalam loop
                    if (iteration < loopCount - 1)
                    {
                        yield return new WaitForSeconds(0.3f);
                        Debug.Log($"   ⏸️ Siap untuk iterasi {iteration+2}");
                    }
                }
                
                i = endLoopIndex;
                
                // JANGAN trigger arrow untuk END_LOOP juga
                Debug.Log($"   📍 Posisi END_LOOP di index {i+1} (arrow tidak dipindah)");
                    
                continue;
            }
            
            // SKIP END_LOOP - tidak perlu diproses karena sudah dihandle dalam LOOP
            if (cmd == "END_LOOP") 
            {
                continue;
            }
            
            // EXECUTE SINGLE COMMAND (bukan LOOP/END_LOOP)
            TriggerArrowEvent(i);
            yield return new WaitForSeconds(0.2f); // Beri waktu untuk arrow bergerak
            yield return ExecuteSingleCommand(cmd);
        }
        
        Debug.Log("✅ Eksekusi selesai!");
        
        // Stop arrow indicator setelah delay
        if (arrowIndicator != null)
        {
            yield return new WaitForSeconds(0.5f);
            arrowIndicator.StopFollowing();
        }
            
        isExecuting = false;
    }
    
    private void TriggerArrowEvent(int commandIndex)
    {
        currentCommandIndex = commandIndex;
        OnCommandExecuted?.Invoke(commandIndex);
        
        if (debugMode)
            Debug.Log($"   🎯 Arrow di command index: {commandIndex+1}");
    }
    
    int GetLoopCount(string loopCommand)
    {
        if (loopCommand == "LOOP2") return 2;
        if (loopCommand == "LOOP3") return 3;
        if (loopCommand == "LOOP4") return 4;
        return 2;
    }
    
    int FindEndLoopIndex(string[] commands, int startIndex)
    {
        for (int i = startIndex + 1; i < commands.Length; i++)
        {
            if (commands[i] == "END_LOOP") return i;
        }
        return -1;
    }
    
    IEnumerator ExecuteSingleCommand(string command)
    {
        switch (command)
        {
            case "Move":
                PlaySound(moveSound, moveSoundVolume);
                yield return StartCoroutine(MoveForward());
                break;
                
            case "TurnLeft":
                PlaySound(turnSound, turnSoundVolume);
                yield return StartCoroutine(Rotate(-90));
                break;
                
            case "TurnRight":
                PlaySound(turnSound, turnSoundVolume);
                yield return StartCoroutine(Rotate(90));
                break;
                
            case "AmbilSampah":
                yield return StartCoroutine(CollectTrash());
                break;
                
            case "BuangSampah":
                yield return StartCoroutine(DepositTrash());
                break;
        }
        
        yield return new WaitForSeconds(0.1f);
    }
    
    IEnumerator CollectTrash()
    {
        if (debugMode) Debug.Log("   🔍 Mencari sampah...");

        yield return new WaitForSeconds(0.2f);
        
        ConditionalTrash[] allTrash = FindObjectsOfType<ConditionalTrash>();
        
        Debug.Log($"   📊 Found {allTrash.Length} trash objects in scene");
        
        bool foundTrash = false;
        
        foreach (ConditionalTrash trash in allTrash)
        {
            if (trash != null && trash.IsCollectable())
            {
                float distance = Vector3.Distance(transform.position, trash.transform.position);
                
                Debug.Log($"   📏 Checking trash '{trash.name}': distance = {distance:F2}, collectable = {trash.IsCollectable()}");
                
                if (distance <= collectRange)
                {
                    PlaySound(collectSound);
                    carriedTrash = trash.trashType.ToString();
                    trash.Collect();
                    
                    Debug.Log($"   ✅ Mengambil sampah: {carriedTrash}");
                    foundTrash = true;
                    
                    yield return StartCoroutine(PlayCollectAnimation());
                    yield break;
                }
                else
                {
                    Debug.Log($"   ❌ Too far: {distance:F2} > {collectRange}");
                }
            }
        }
        
        if (!foundTrash)
        {
            Debug.Log("   ❌ Tidak ada sampah di dekat robot!");
            PlaySound(failSound);
            yield return StartCoroutine(PlayFailAnimation());
        }
    }
    
    IEnumerator DepositTrash()
    {
        if (carriedTrash == "None")
        {
            Debug.Log("   ❌ Tidak membawa sampah!");
            PlaySound(failSound);
            yield return StartCoroutine(PlayFailAnimation());
            yield break;
        }
        
        Debug.Log($"   🗑️ Mencari tempat sampah untuk {carriedTrash}...");
        
        TrashBin[] allBins = FindObjectsOfType<TrashBin>();
        Debug.Log($"   📊 Found {allBins.Length} trash bins");
        
        foreach (TrashBin bin in allBins)
        {
            float distance = Vector3.Distance(transform.position, bin.transform.position);
            
            Debug.Log($"   📏 Checking bin '{bin.name}': distance = {distance:F2}, type = {bin.binType}");
            
            if (distance <= bin.depositRange)
            {
                bool jenisCocok = (carriedTrash == "Organik" && bin.binType == TrashBin.BinType.Organik) ||
                                 (carriedTrash == "Anorganik" && bin.binType == TrashBin.BinType.Anorganik);
                
                if (jenisCocok)
                {
                    PlaySound(depositSound);
                    bin.PlaySuccessEffect();
                    carriedTrash = "None";
                    
                    Debug.Log("   ✅ Sampah dibuang!");
                    yield return StartCoroutine(PlayCollectAnimation());
                    yield break;
                }
                else
                {
                    Debug.Log($"   ❌ Jenis tidak cocok: {carriedTrash} vs {bin.binType}");
                }
            }
        }
        
        Debug.Log("   ❌ Tidak ada tempat sampah yang cocok!");
        PlaySound(failSound);
        yield return StartCoroutine(PlayFailAnimation());
    }
    
    IEnumerator MoveForward()
    {
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

        // yield return new WaitForSeconds(0.1f);
    }
    
    IEnumerator Rotate(float angle)
    {
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
    
    private void PlaySound(AudioClip clip, float volume = 1.0f)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
        else if (debugMode)
        {
            Debug.LogWarning($"Sound effect tidak ditemukan: {clip?.name}");
        }
    }
    
    // Method untuk UI
    public void OnExecuteButton()
    {
        ExecuteCommands();
    }
    
    public void OnResetButtonClicked()
    {
        ResetRobot();
        
        if (arrowIndicator != null)
        {
            arrowIndicator.OnResetButtonClicked();
        }
    }
    
    // Public methods
    public bool IsExecuting()
    {
        return isExecuting;
    }
    
    public string GetCarriedTrash()
    {
        return carriedTrash;
    }
    
    public int GetCurrentCommandIndex()
    {
        return currentCommandIndex;
    }
    
    public void ResetRobot()
    {
        StopAllCoroutines();
        isExecuting = false;
        currentCommandIndex = 0;
        carriedTrash = "None";
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        transform.localScale = Vector3.one;
        
        ConditionalTrash[] allTrash = FindObjectsOfType<ConditionalTrash>();
        foreach (ConditionalTrash trash in allTrash)
        {
            trash.ResetTrash();
        }
        
        TrashBin[] allBins = FindObjectsOfType<TrashBin>();
        foreach (TrashBin bin in allBins)
        {
            bin.StopEffects();
        }
        
        Debug.Log("🔄 Robot dan lingkungan direset");
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, collectRange);
    }
}