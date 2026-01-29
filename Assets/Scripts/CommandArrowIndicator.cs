using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CommandArrowIndicator : MonoBehaviour
{
    [Header("REFERENCES")]
    public GameObject commandManagerObject; // GameObject yang berisi CommandManager
    public GameObject commandExecuteObject; // GameObject yang berisi CommandExecute
    public GameObject commandContainer;
    
    [Header("ARROW VISUAL")]
    public GameObject arrowPrefab; // Prefab untuk arrow
    private GameObject arrowInstance;
    private RectTransform arrowRect;
    private Image arrowImage;
    
    [Header("ANIMATION SETTINGS")]
    public float moveSpeed = 10f;
    public float scaleMultiplier = 1.2f;
    public Color activeColor = Color.yellow;
    public Color idleColor = Color.white;
    
    [Header("POSITION OFFSET")]
    public Vector2 positionOffset = new Vector2(0, 60f); // Offset di atas slot
    
    [Header("DEBUG")]
    public bool debugMode = true;
    public bool autoStart = true;
    
    private Vector3[] slotWorldPositions;
    private int currentCommandIndex = 0;
    private bool isActive = false;
    private Coroutine followRoutine;
    
    // Cache components
    private CommandManager cachedCommandManager;
    private CommandExecute cachedCommandExecute;
    private RobotExecutePart2 cachedRobotExecutePart2;
    
    void Start()
    {
        // Cari reference jika belum diassign
        if (commandManagerObject == null)
        {
            // Cari CommandManager dulu
            CommandManager cmdManager = FindObjectOfType<CommandManager>();
            if (cmdManager != null)
            {
                commandManagerObject = cmdManager.gameObject;
            }
            else
            {
                // Cari CommandManagerPart2
                CommandManagerPart2 cmdManager2 = FindObjectOfType<CommandManagerPart2>();
                if (cmdManager2 != null)
                {
                    commandManagerObject = cmdManager2.gameObject;
                }
            }
        }
            
        if (commandExecuteObject == null)
        {
            // Cari CommandExecute
            CommandExecute cmdExecute = FindObjectOfType<CommandExecute>();
            if (cmdExecute != null)
            {
                commandExecuteObject = cmdExecute.gameObject;
            }
            else
            {
                // Cari RobotExecutePart2
                RobotExecutePart2 robotExecute = FindObjectOfType<RobotExecutePart2>();
                if (robotExecute != null)
                {
                    commandExecuteObject = robotExecute.gameObject;
                }
            }
        }
            
        if (commandContainer == null)
        {
            // Cari melalui cached CommandManager jika ada
            CommandManager cmdManager = GetCommandManager();
            if (cmdManager != null)
            {
                commandContainer = cmdManager.commandContainer;
            }
            else
            {
                CommandManagerPart2 cmdManager2 = GetCommandManagerPart2();
                if (cmdManager2 != null)
                {
                    commandContainer = cmdManager2.commandContainer;
                }
            }
        }
        
        // Buat arrow jika prefab tersedia
        CreateArrowVisual();
        
        // Update posisi slot
        UpdateSlotPositions();
        
        // Sembunyikan arrow awal
        if (arrowInstance != null)
            arrowInstance.SetActive(false);
            
        if (debugMode) Debug.Log("Arrow Indicator initialized");
    }
    
    // Helper method untuk mendapatkan CommandManager
    private CommandManager GetCommandManager()
    {
        if (cachedCommandManager != null) return cachedCommandManager;
        
        if (commandManagerObject != null)
        {
            cachedCommandManager = commandManagerObject.GetComponent<CommandManager>();
        }
        
        if (cachedCommandManager == null)
        {
            cachedCommandManager = FindObjectOfType<CommandManager>();
        }
        
        return cachedCommandManager;
    }
    
    // Helper method untuk mendapatkan CommandManagerPart2
    private CommandManagerPart2 GetCommandManagerPart2()
    {
        if (cachedCommandManager != null) return null; // Jika sudah punya CommandManager, jangan pakai Part2
        
        if (cachedCommandManager == null && commandManagerObject != null)
        {
            return commandManagerObject.GetComponent<CommandManagerPart2>();
        }
        
        if (cachedCommandManager == null)
        {
            return FindObjectOfType<CommandManagerPart2>();
        }
        
        return null;
    }
    
    // Helper method untuk mendapatkan CommandExecute
    private CommandExecute GetCommandExecute()
    {
        if (cachedCommandExecute != null) return cachedCommandExecute;
        
        if (commandExecuteObject != null)
        {
            cachedCommandExecute = commandExecuteObject.GetComponent<CommandExecute>();
        }
        
        if (cachedCommandExecute == null)
        {
            cachedCommandExecute = FindObjectOfType<CommandExecute>();
        }
        
        return cachedCommandExecute;
    }
    
    // Helper method untuk mendapatkan RobotExecutePart2
    private RobotExecutePart2 GetRobotExecutePart2()
    {
        if (cachedRobotExecutePart2 != null) return cachedRobotExecutePart2;
        
        if (commandExecuteObject != null)
        {
            cachedRobotExecutePart2 = commandExecuteObject.GetComponent<RobotExecutePart2>();
        }
        
        if (cachedRobotExecutePart2 == null)
        {
            cachedRobotExecutePart2 = FindObjectOfType<RobotExecutePart2>();
        }
        
        return cachedRobotExecutePart2;
    }
    
    void OnEnable()
    {
        // Subscribe ke event
        // Kita akan buat custom event nanti
    }
    
    void OnDisable()
    {
        StopFollowing();
    }
    
    void CreateArrowVisual()
    {
        // Jika ada prefab, instantiate
        if (arrowPrefab != null)
        {
            arrowInstance = Instantiate(arrowPrefab, transform);
            arrowRect = arrowInstance.GetComponent<RectTransform>();
            arrowImage = arrowInstance.GetComponent<Image>();
            
            if (arrowRect == null)
            {
                Debug.LogError("Arrow prefab tidak memiliki RectTransform!");
                return;
            }
            
            // Set parent ke Canvas
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                arrowInstance.transform.SetParent(canvas.transform, false);
            }
            
            arrowInstance.name = "CommandArrow";
            
            if (debugMode) Debug.Log("Arrow visual created from prefab");
        }
        else
        {
            // Buat arrow secara manual jika tidak ada prefab
            CreateDefaultArrow();
        }
    }
    
    void CreateDefaultArrow()
    {
        // Cari Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Tidak ada Canvas di scene!");
            return;
        }
        
        // Buat GameObject untuk arrow
        arrowInstance = new GameObject("CommandArrow");
        arrowInstance.transform.SetParent(canvas.transform, false);
        
        // Tambahkan Image component
        arrowImage = arrowInstance.AddComponent<Image>();
        arrowRect = arrowInstance.GetComponent<RectTransform>();
        
        // Buat sprite sederhana (triangle) secara programmatic
        CreateArrowSprite();
        
        // Set ukuran dan warna
        arrowRect.sizeDelta = new Vector2(40, 40);
        arrowImage.color = activeColor;
        
        // Set anchor ke tengah
        arrowRect.anchorMin = new Vector2(0.5f, 0.5f);
        arrowRect.anchorMax = new Vector2(0.5f, 0.5f);
        arrowRect.pivot = new Vector2(0.5f, 0.5f);
        
        if (debugMode) Debug.Log("Default arrow created");
    }
    
    void CreateArrowSprite()
    {
        // Buat sprite panah sederhana
        Texture2D texture = new Texture2D(64, 64);
        
        // Gambar panah pada texture
        for (int y = 0; y < 64; y++)
        {
            for (int x = 0; x < 64; x++)
            {
                // Gambar bentuk panah (segitiga)
                bool isArrow = false;
                
                // Bagian tengah
                if (x >= 20 && x <= 44 && y >= 10 && y <= 54)
                {
                    float centerX = 32;
                    float centerY = 32;
                    float distance = Mathf.Abs(x - centerX) + Mathf.Abs(y - centerY);
                    
                    if (y > 32) // Bagian bawah panah
                    {
                        if (Mathf.Abs(x - centerX) < (54 - y) * 0.4f)
                            isArrow = true;
                    }
                    else // Bagian atas panah
                    {
                        if (Mathf.Abs(x - centerX) < (y - 10) * 0.3f)
                            isArrow = true;
                    }
                }
                
                if (isArrow)
                    texture.SetPixel(x, y, Color.white);
                else
                    texture.SetPixel(x, y, Color.clear);
            }
        }
        
        texture.Apply();
        
        Sprite arrowSprite = Sprite.Create(texture, 
            new Rect(0, 0, 64, 64), 
            new Vector2(0.5f, 0.5f));
        
        arrowImage.sprite = arrowSprite;
    }
    
    void UpdateSlotPositions()
    {
        if (commandContainer == null) 
        {
            if (debugMode) Debug.LogWarning("Command container belum diassign!");
            return;
        }
        
        int slotCount = commandContainer.transform.childCount;
        slotWorldPositions = new Vector3[slotCount];
        
        for (int i = 0; i < slotCount; i++)
        {
            Transform slot = commandContainer.transform.GetChild(i);
            
            // Dapatkan posisi world dari slot
            slotWorldPositions[i] = slot.position;
            
            // Tambahkan offset
            slotWorldPositions[i] += (Vector3)positionOffset;
            
            if (debugMode && i == 0) 
                Debug.Log($"Slot {i} position: {slotWorldPositions[i]}");
        }
        
        if (debugMode) 
            Debug.Log($"Updated {slotCount} slot positions");
    }
    
    public void StartFollowing()
    {
        if (isActive)
        {
            if (debugMode) Debug.Log("Arrow sudah aktif!");
            return;
        }
        
        if (arrowInstance == null)
        {
            CreateArrowVisual();
            if (arrowInstance == null)
            {
                Debug.LogError("Gagal membuat arrow visual!");
                return;
            }
        }
        
        // Update posisi
        UpdateSlotPositions();
        
        if (slotWorldPositions == null || slotWorldPositions.Length == 0)
        {
            Debug.LogError("Tidak ada slot command ditemukan!");
            return;
        }
        
        // Tampilkan arrow
        arrowInstance.SetActive(true);
        isActive = true;
        currentCommandIndex = 0;
        
        if (debugMode) Debug.Log("🚀 Arrow mulai mengikuti command...");
        
        // Mulai coroutine
        if (followRoutine != null)
            StopCoroutine(followRoutine);
            
        followRoutine = StartCoroutine(FollowExecution());
    }
    
    public void StopFollowing()
    {
        isActive = false;
        
        if (followRoutine != null)
        {
            StopCoroutine(followRoutine);
            followRoutine = null;
        }
        
        // Sembunyikan arrow
        if (arrowInstance != null)
            arrowInstance.SetActive(false);
            
        if (debugMode) Debug.Log("⏹️ Arrow berhenti");
    }
    
    private IEnumerator FollowExecution()
    {
        // Tunggu sampai eksekusi dimulai
        yield return new WaitUntil(() => IsExecuting());
        
        if (debugMode) Debug.Log("👀 Arrow mulai tracking robot...");
        
        // Dapatkan command array
        string[] commands = GetCommandArray();
        
        // Posisikan arrow di slot pertama
        if (commands.Length > 0)
        {
            MoveArrowToSlot(0);
        }
        
        // Loop melalui semua command
        for (int i = 0; i < commands.Length; i++)
        {
            string command = commands[i];
            
            // Skip command kosong dan command percabangan (IF, ELSE, END_IF)
            if (command == "Empty" || IsBranchingCommand(command)) 
            {
                if (debugMode && command != "Empty") 
                    Debug.Log($"   ⏭️ Slot {i+1}: {command} (dilompati)");
                continue;
            }
            
            // Update arrow ke slot ini
            currentCommandIndex = i;
            yield return StartCoroutine(AnimateArrowToSlot(i));
            
            if (debugMode) Debug.Log($"   ▶️ Arrow di Slot {i+1}: {command}");
            
            // Tunggu durasi command
            float waitTime = GetCommandWaitTime(command);
            yield return new WaitForSeconds(waitTime);
            
            // Cek jika robot masih bergerak
            if (!IsExecuting())
                break;
        }
        
        // Animasi selesai
        if (arrowImage != null)
            arrowImage.color = Color.green; // Ubah warna saat selesai
            
        if (debugMode) Debug.Log("✅ Arrow selesai mengikuti");
    }
    
    // Helper method untuk mengecek apakah command adalah percabangan
    private bool IsBranchingCommand(string command)
    {
        return command == "IF_ORGANIK" || 
               command == "IF_ANORGANIK" || 
               command == "ELSE" || 
               command == "END_IF";
    }
    
    // Method baru: Dapatkan command array dengan mengabaikan percabangan
    private string[] GetFilteredCommandArray()
    {
        string[] allCommands = GetCommandArray();
        List<string> filteredCommands = new List<string>();
        
        for (int i = 0; i < allCommands.Length; i++)
        {
            string command = allCommands[i];
            
            // Tambahkan hanya jika bukan command kosong atau percabangan
            if (command != "Empty" && !IsBranchingCommand(command))
            {
                filteredCommands.Add(command);
            }
        }
        
        return filteredCommands.ToArray();
    }
    
    // Method baru: Dapatkan slot index dari command array yang difilter
    private int GetOriginalSlotIndex(int filteredIndex, string[] allCommands)
    {
        int filteredCount = 0;
        
        for (int i = 0; i < allCommands.Length; i++)
        {
            string command = allCommands[i];
            
            if (command != "Empty" && !IsBranchingCommand(command))
            {
                if (filteredCount == filteredIndex)
                {
                    return i;
                }
                filteredCount++;
            }
        }
        
        return 0;
    }
    
    // Versi alternatif dari FollowExecution yang lebih akurat
    private IEnumerator FollowExecutionAccurate()
    {
        // Tunggu sampai eksekusi dimulai
        yield return new WaitUntil(() => IsExecuting());
        
        if (debugMode) Debug.Log("👀 Arrow mulai tracking robot (mode akurat)...");
        
        // Dapatkan semua command
        string[] allCommands = GetCommandArray();
        
        // Simulasi eksekusi seperti RobotExecutePart2
        for (int i = 0; i < allCommands.Length; i++)
        {
            string command = allCommands[i];
            
            // Skip command kosong
            if (command == "Empty") 
            {
                continue;
            }
            
            // Handle percabangan IF
            if (command == "IF_ORGANIK" || command == "IF_ANORGANIK")
            {
                // Cari END_IF
                int endIfIndex = FindEndIfIndex(allCommands, i);
                
                // Lompat ke END_IF
                i = endIfIndex;
                continue;
            }
            
            // Skip ELSE dan END_IF
            if (command == "ELSE" || command == "END_IF")
            {
                continue;
            }
            
            // Gerakkan arrow ke slot ini
            currentCommandIndex = i;
            yield return StartCoroutine(AnimateArrowToSlot(i));
            
            if (debugMode) Debug.Log($"   ▶️ Arrow di Slot {i+1}: {command}");
            
            // Tunggu durasi command
            float waitTime = GetCommandWaitTime(command);
            yield return new WaitForSeconds(waitTime);
            
            // Cek jika robot masih bergerak
            if (!IsExecuting())
                break;
        }
        
        // Animasi selesai
        if (arrowImage != null)
            arrowImage.color = Color.green;
            
        if (debugMode) Debug.Log("✅ Arrow selesai mengikuti (mode akurat)");
    }
    
    // Helper method untuk mencari index END_IF
    private int FindEndIfIndex(string[] commands, int startIndex)
    {
        for (int i = startIndex + 1; i < commands.Length; i++)
        {
            if (commands[i] == "END_IF") 
            {
                return i;
            }
        }
        return commands.Length - 1;
    }
    
    private IEnumerator AnimateArrowToSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slotWorldPositions.Length)
            yield break;
        
        if (arrowRect == null)
            yield break;
        
        Vector3 targetPosition = slotWorldPositions[slotIndex];
        Vector3 startPosition = arrowRect.position;
        
        // Animasi scale
        Vector3 originalScale = arrowRect.localScale;
        Vector3 targetScale = originalScale * scaleMultiplier;
        
        float elapsedTime = 0f;
        float animationTime = 0.3f;
        
        while (elapsedTime < animationTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / animationTime;
            
            // Posisi
            arrowRect.position = Vector3.Lerp(startPosition, targetPosition, t);
            
            // Scale (pulse effect)
            if (t < 0.5f)
                arrowRect.localScale = Vector3.Lerp(originalScale, targetScale, t * 2);
            else
                arrowRect.localScale = Vector3.Lerp(targetScale, originalScale, (t - 0.5f) * 2);
            
            // Warna
            if (arrowImage != null)
                arrowImage.color = Color.Lerp(idleColor, activeColor, Mathf.PingPong(t * 2, 1));
            
            yield return null;
        }
        
        // Pastikan posisi tepat
        arrowRect.position = targetPosition;
        arrowRect.localScale = originalScale;
    }
    
    private void MoveArrowToSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slotWorldPositions.Length)
            return;
        
        if (arrowRect != null)
        {
            arrowRect.position = slotWorldPositions[slotIndex];
            currentCommandIndex = slotIndex;
        }
    }
    
    private float GetCommandWaitTime(string command)
    {
        switch (command)
        {
            // case "Move":
            //     return 0.5f;
            // case "TurnLeft":
            // case "TurnRight":
            //     return 0.3f;
            // case "CollectTrash":
            // case "AmbilSampah":
            //     return 1.0f;
            // case "BuangSampah":
            //     return 0.8f;
            default:
                return 0.5f;
        }
    }
    
    // Method untuk dipanggil dari UI
    public void OnExecuteButtonClicked()
    {
        // Mulai arrow 0.5 detik setelah eksekusi dimulai
        Invoke("StartFollowing", 0.5f);
    }
    
    public void OnResetButtonClicked()
    {
        StopFollowing();
        
        // Reset arrow position
        if (arrowInstance != null && slotWorldPositions != null && slotWorldPositions.Length > 0)
        {
            arrowRect.position = slotWorldPositions[0];
        }
    }
    
    // Helper method untuk mendapatkan command array
    private string[] GetCommandArray()
    {
        // Coba dari CommandManager
        CommandManager cmdManager = GetCommandManager();
        if (cmdManager != null)
        {
            return cmdManager.GetCommandArray();
        }
        
        // Coba dari CommandManagerPart2
        CommandManagerPart2 cmdManager2 = GetCommandManagerPart2();
        if (cmdManager2 != null)
        {
            return cmdManager2.GetCommandArray();
        }
        
        Debug.LogError("Tidak ditemukan CommandManager atau CommandManagerPart2!");
        return new string[0];
    }
    
    // Helper method untuk mengecek apakah sedang executing
    private bool IsExecuting()
    {
        // Coba dari CommandExecute
        CommandExecute cmdExecute = GetCommandExecute();
        if (cmdExecute != null)
        {
            return cmdExecute.IsExecuting();
        }
        
        // Coba dari RobotExecutePart2
        RobotExecutePart2 robotExecute = GetRobotExecutePart2();
        if (robotExecute != null)
        {
            return robotExecute.IsExecuting();
        }
        
        Debug.LogWarning("Tidak ditemukan CommandExecute atau RobotExecutePart2!");
        return false;
    }
    
    // Debug method
    public void TestArrow()
    {
        UpdateSlotPositions();
        
        if (slotWorldPositions != null && slotWorldPositions.Length > 0)
        {
            if (arrowInstance == null)
                CreateArrowVisual();
                
            arrowInstance.SetActive(true);
            arrowRect.position = slotWorldPositions[0];
            
            Debug.Log($"Arrow test: Position = {arrowRect.position}");
        }
    }
    
    void Update()
    {
        // Debug shortcut
        if (debugMode && Input.GetKeyDown(KeyCode.F8))
        {
            TestArrow();
        }
    }
}