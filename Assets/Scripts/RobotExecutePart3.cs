using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotExecutePart3 : MonoBehaviour
{
    [Header("REFERENCES")]
    public CommandManagerPart3 commandManager;
    
    [Header("MOVEMENT SETTINGS")]
    public float moveDistance = 1f;
    public float moveDuration = 0.5f;
    public float rotationDuration = 0.3f;
    
    [Header("COLLECTION SETTINGS")]
    public float collectRange = 1.5f;
    
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
        
        if (commandManager == null)
            commandManager = FindObjectOfType<CommandManagerPart3>();
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
        string[] commands = commandManager.GetCommandArray();
        
        Debug.Log("🚀 Robot Part 3 mulai eksekusi...");
        
        for (int i = 0; i < commands.Length; i++)
        {
            string cmd = commands[i];
            if (cmd == "Empty") continue;
            
            Debug.Log($"   [{i+1}] {cmd}");
            
            // HANDLE LOOP
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
                
                // Eksekusi blok loop
                for (int iteration = 0; iteration < loopCount; iteration++)
                {
                    Debug.Log($"   🔄 Iterasi {iteration+1}/{loopCount}");
                    
                    int j = i + 1;
                    while (j < endLoopIndex)
                    {
                        if (commands[j] != "Empty")
                        {
                            Debug.Log($"     [{iteration+1}.{j-i}] {commands[j]}");
                            yield return ExecuteSingleCommand(commands[j]);
                        }
                        j++;
                    }
                    
                    yield return new WaitForSeconds(0.1f);
                }
                
                i = endLoopIndex;
                continue;
            }
            
            // EXECUTE SINGLE COMMAND
            yield return ExecuteSingleCommand(cmd);
        }
        
        Debug.Log("✅ Eksekusi selesai!");
        isExecuting = false;
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
                
            case "END_LOOP":
                // Do nothing
                break;
        }
        
        yield return new WaitForSeconds(0.1f);
    }
    
    // FIXED: COLLECT TRASH METHOD
    IEnumerator CollectTrash()
    {
        if (debugMode) Debug.Log("   🔍 Mencari sampah...");
        
        // Cari semua ConditionalTrash di scene
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
                    // Ambil sampah
                    carriedTrash = trash.trashType.ToString();
                    trash.Collect();
                    
                    Debug.Log($"   ✅ Mengambil sampah: {carriedTrash}");
                    foundTrash = true;
                    
                    // Animasi sederhana
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
            yield return StartCoroutine(PlayFailAnimation());
        }
    }
    
    // FIXED: DEPOSIT TRASH
    IEnumerator DepositTrash()
    {
        if (carriedTrash == "None")
        {
            Debug.Log("   ❌ Tidak membawa sampah!");
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
    
    // Public methods
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
        
        // Reset semua ConditionalTrash
        ConditionalTrash[] allTrash = FindObjectsOfType<ConditionalTrash>();
        foreach (ConditionalTrash trash in allTrash)
        {
            trash.ResetTrash();
        }
        
        // Reset semua TrashBin
        TrashBin[] allBins = FindObjectsOfType<TrashBin>();
        foreach (TrashBin bin in allBins)
        {
            bin.StopEffects();
        }
        
        Debug.Log("🔄 Robot dan lingkungan direset");
    }
    
    // Debug: Visualize collect range
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, collectRange);
    }
}