using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CommandArrowIndicator : MonoBehaviour
{
    [Header("REFERENCES")]
    public GameObject commandManagerObject;
    public GameObject commandExecuteObject;
    public GameObject commandContainer;
    
    [Header("ARROW VISUAL")]
    public GameObject arrowPrefab;
    private GameObject arrowInstance;
    private RectTransform arrowRect;
    private Image arrowImage;
    
    [Header("ANIMATION SETTINGS")]
    public float moveSpeed = 10f;
    public float scaleMultiplier = 1.2f;
    public Color activeColor = Color.yellow;
    public Color idleColor = Color.white;
    
    [Header("POSITION OFFSET")]
    public Vector2 positionOffset = new Vector2(0, 60f);
    
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
    private RobotExecutePart3 cachedRobotExecutePart3;
    
    void Start()
    {
        if (commandManagerObject == null)
        {
            CommandManager cmdManager = FindObjectOfType<CommandManager>();
            if (cmdManager != null)
            {
                commandManagerObject = cmdManager.gameObject;
            }
            else
            {
                CommandManagerPart2 cmdManager2 = FindObjectOfType<CommandManagerPart2>();
                if (cmdManager2 != null)
                {
                    commandManagerObject = cmdManager2.gameObject;
                }
                else
                {
                    CommandManagerPart3 cmdManager3 = FindObjectOfType<CommandManagerPart3>();
                    if (cmdManager3 != null)
                    {
                        commandManagerObject = cmdManager3.gameObject;
                    }
                }
            }
        }
            
        if (commandExecuteObject == null)
        {
            CommandExecute cmdExecute = FindObjectOfType<CommandExecute>();
            if (cmdExecute != null)
            {
                commandExecuteObject = cmdExecute.gameObject;
            }
            else
            {
                RobotExecutePart2 robotExecute = FindObjectOfType<RobotExecutePart2>();
                if (robotExecute != null)
                {
                    commandExecuteObject = robotExecute.gameObject;
                }
                else
                {
                    RobotExecutePart3 robotExecute3 = FindObjectOfType<RobotExecutePart3>();
                    if (robotExecute3 != null)
                    {
                        commandExecuteObject = robotExecute3.gameObject;
                    }
                }
            }
        }
            
        if (commandContainer == null)
        {
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
                else
                {
                    CommandManagerPart3 cmdManager3 = GetCommandManagerPart3();
                    if (cmdManager3 != null)
                    {
                        commandContainer = cmdManager3.commandContainer;
                    }
                }
            }
        }
        
        CreateArrowVisual();
        UpdateSlotPositions();
        
        if (arrowInstance != null)
            arrowInstance.SetActive(false);
            
        if (debugMode) Debug.Log("Arrow Indicator initialized");
    }
    
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
    
    private CommandManagerPart2 GetCommandManagerPart2()
    {
        if (cachedCommandManager != null) return null;
        
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
    
    private CommandManagerPart3 GetCommandManagerPart3()
    {
        if (commandManagerObject != null)
        {
            return commandManagerObject.GetComponent<CommandManagerPart3>();
        }
        
        return FindObjectOfType<CommandManagerPart3>();
    }
    
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
    
    private RobotExecutePart3 GetRobotExecutePart3()
    {
        if (cachedRobotExecutePart3 != null) return cachedRobotExecutePart3;
        
        if (commandExecuteObject != null)
        {
            cachedRobotExecutePart3 = commandExecuteObject.GetComponent<RobotExecutePart3>();
        }
        
        if (cachedRobotExecutePart3 == null)
        {
            cachedRobotExecutePart3 = FindObjectOfType<RobotExecutePart3>();
        }
        
        return cachedRobotExecutePart3;
    }
    
    void OnEnable()
    {
        TrySubscribeToRobotEvents();
        
        // Subscribe to Part 3 events
        RobotExecutePart3 robotPart3 = GetRobotExecutePart3();
        if (robotPart3 != null)
        {
            robotPart3.OnCommandExecuted += OnRobotCommandExecuted;
            if (debugMode) Debug.Log("✅ Subscribed to RobotExecutePart3 events");
        }
    }
    
    void OnDisable()
    {
        StopFollowing();
        TryUnsubscribeFromRobotEvents();
        
        // Unsubscribe from Part 3 events
        RobotExecutePart3 robotPart3 = GetRobotExecutePart3();
        if (robotPart3 != null)
        {
            robotPart3.OnCommandExecuted -= OnRobotCommandExecuted;
            if (debugMode) Debug.Log("✅ Unsubscribed from RobotExecutePart3 events");
        }
    }
    
    private void TrySubscribeToRobotEvents()
    {
        RobotExecutePart2 robotExecute = GetRobotExecutePart2();
        if (robotExecute != null)
        {
            var eventField = robotExecute.GetType().GetField("OnCommandExecuted", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            
            if (eventField != null && eventField.FieldType == typeof(System.Action<int>))
            {
                var eventValue = (System.Action<int>)eventField.GetValue(robotExecute);
                eventValue += OnRobotCommandExecuted;
                eventField.SetValue(robotExecute, eventValue);
                
                if (debugMode) Debug.Log("✅ Subscribed to RobotExecutePart2 events");
            }
        }
    }
    
    private void TryUnsubscribeFromRobotEvents()
    {
        RobotExecutePart2 robotExecute = GetRobotExecutePart2();
        if (robotExecute != null)
        {
            var eventField = robotExecute.GetType().GetField("OnCommandExecuted", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            
            if (eventField != null && eventField.FieldType == typeof(System.Action<int>))
            {
                var eventValue = (System.Action<int>)eventField.GetValue(robotExecute);
                eventValue -= OnRobotCommandExecuted;
                eventField.SetValue(robotExecute, eventValue);
            }
        }
    }
    
    private void OnRobotCommandExecuted(int commandIndex)
    {
        if (isActive && arrowRect != null)
        {
            MoveArrowToSlot(commandIndex);
            
            if (debugMode) Debug.Log($"🔄 Arrow pindah ke slot {commandIndex+1} via event");
        }
    }
    
    void CreateArrowVisual()
    {
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
            CreateDefaultArrow();
        }
    }
    
    void CreateDefaultArrow()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Tidak ada Canvas di scene!");
            return;
        }
        
        arrowInstance = new GameObject("CommandArrow");
        arrowInstance.transform.SetParent(canvas.transform, false);
        
        arrowImage = arrowInstance.AddComponent<Image>();
        arrowRect = arrowInstance.GetComponent<RectTransform>();
        
        CreateArrowSprite();
        
        arrowRect.sizeDelta = new Vector2(40, 40);
        arrowImage.color = activeColor;
        
        arrowRect.anchorMin = new Vector2(0.5f, 0.5f);
        arrowRect.anchorMax = new Vector2(0.5f, 0.5f);
        arrowRect.pivot = new Vector2(0.5f, 0.5f);
        
        if (debugMode) Debug.Log("Default arrow created");
    }
    
    void CreateArrowSprite()
    {
        Texture2D texture = new Texture2D(64, 64);
        
        for (int y = 0; y < 64; y++)
        {
            for (int x = 0; x < 64; x++)
            {
                bool isArrow = false;
                
                if (x >= 20 && x <= 44 && y >= 10 && y <= 54)
                {
                    float centerX = 32;
                    float centerY = 32;
                    float distance = Mathf.Abs(x - centerX) + Mathf.Abs(y - centerY);
                    
                    if (y > 32)
                    {
                        if (Mathf.Abs(x - centerX) < (54 - y) * 0.4f)
                            isArrow = true;
                    }
                    else
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
            slotWorldPositions[i] = slot.position;
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
        
        UpdateSlotPositions();
        
        if (slotWorldPositions == null || slotWorldPositions.Length == 0)
        {
            Debug.LogError("Tidak ada slot command ditemukan!");
            return;
        }
        
        arrowInstance.SetActive(true);
        isActive = true;
        currentCommandIndex = 0;
        
        if (debugMode) Debug.Log("🚀 Arrow mulai mengikuti command...");
        
        // Choose the right coroutine based on which robot is active
        if (followRoutine != null)
            StopCoroutine(followRoutine);
        
        // Check if Part 3 (LOOP) is active
        RobotExecutePart3 robotPart3 = GetRobotExecutePart3();
        if (robotPart3 != null && robotPart3.IsExecuting())
        {
            followRoutine = StartCoroutine(FollowExecutionLoop());
        }
        else
        {
            // Use accurate mode for Part 2 or legacy
            followRoutine = StartCoroutine(FollowExecutionAccurate());
        }
    }
    
    public void StopFollowing()
    {
        isActive = false;
        
        if (followRoutine != null)
        {
            StopCoroutine(followRoutine);
            followRoutine = null;
        }
        
        if (arrowInstance != null)
            arrowInstance.SetActive(false);
            
        if (debugMode) Debug.Log("⏹️ Arrow berhenti");
    }
    
    private IEnumerator FollowExecutionAccurate()
    {
        yield return new WaitUntil(() => IsExecuting());
        
        if (debugMode) Debug.Log("👀 Arrow mulai tracking robot (mode akurat)...");
        
        string[] allCommands = GetCommandArray();
        int commandIndex = 0;
        
        while (commandIndex < allCommands.Length && isActive)
        {
            string command = allCommands[commandIndex];
            
            if (command == "Empty") 
            {
                commandIndex++;
                continue;
            }
            
            if (command == "IF_ORGANIK" || command == "IF_ANORGANIK")
            {
                bool conditionTrue = CheckIfCondition(command);
                
                int endIfIndex = FindEndIfIndex(allCommands, commandIndex);
                
                if (debugMode) 
                    Debug.Log($"🔍 IF {command}: Condition = {conditionTrue}, Jump to {endIfIndex}");
                
                if (conditionTrue)
                {
                    commandIndex++;
                    int elseIndex = FindElseIndex(allCommands, commandIndex, endIfIndex);
                    
                    int executeUntil = (elseIndex > commandIndex) ? elseIndex : endIfIndex;
                    
                    while (commandIndex < executeUntil && isActive)
                    {
                        if (allCommands[commandIndex] != "Empty" && 
                            allCommands[commandIndex] != "END_IF")
                        {
                            yield return StartCoroutine(AnimateArrowToSlot(commandIndex));
                            float commandDuration = GetCommandWaitTime(allCommands[commandIndex]);
                            yield return new WaitForSeconds(commandDuration);
                        }
                        commandIndex++;
                    }
                    
                    if (elseIndex > commandIndex)
                    {
                        commandIndex = elseIndex + 1;
                    }
                }
                else
                {
                    commandIndex++;
                    int elseIndex = FindElseIndex(allCommands, commandIndex, endIfIndex);
                    
                    if (elseIndex > commandIndex)
                    {
                        commandIndex = elseIndex + 1;
                        
                        while (commandIndex < endIfIndex && isActive)
                        {
                            if (allCommands[commandIndex] != "Empty" && 
                                allCommands[commandIndex] != "END_IF")
                            {
                                yield return StartCoroutine(AnimateArrowToSlot(commandIndex));
                                float commandDuration = GetCommandWaitTime(allCommands[commandIndex]);
                                yield return new WaitForSeconds(commandDuration);
                            }
                            commandIndex++;
                        }
                    }
                    else
                    {
                        commandIndex = endIfIndex;
                    }
                }
                
                if (commandIndex < allCommands.Length && allCommands[commandIndex] == "END_IF")
                {
                    commandIndex++;
                }
                
                continue;
            }
            
            if (command == "ELSE" || command == "END_IF")
            {
                commandIndex++;
                continue;
            }
            
            yield return StartCoroutine(AnimateArrowToSlot(commandIndex));
            
            if (debugMode) Debug.Log($"   ▶️ Arrow di Slot {commandIndex+1}: {command}");
            
            float waitTime = GetCommandWaitTime(command);
            yield return new WaitForSeconds(waitTime);
            
            commandIndex++;
            
            if (!IsExecuting())
                break;
        }
        
        if (arrowImage != null)
            arrowImage.color = Color.green;
            
        if (debugMode) Debug.Log("✅ Arrow selesai mengikuti (mode akurat)");
    }
    
    private IEnumerator FollowExecutionLoop()
    {
        yield return new WaitUntil(() => IsExecuting());
        
        if (debugMode) Debug.Log("👀 Arrow mulai tracking robot (mode loop)...");
        
        string[] allCommands = GetCommandArray();
        int commandIndex = 0;
        
        while (commandIndex < allCommands.Length && isActive)
        {
            string command = allCommands[commandIndex];
            
            if (command == "Empty") 
            {
                commandIndex++;
                continue;
            }
            
            // Handle LOOP commands for Part 3
            if (command.StartsWith("LOOP"))
            {
                // Show arrow at LOOP command
                // yield return StartCoroutine(AnimateArrowToSlot(commandIndex));
                
                int loopCount = GetLoopCount(command);
                int endLoopIndex = FindEndLoopIndex(allCommands, commandIndex);
                
                if (debugMode) 
                    Debug.Log($"🔄 LOOP {command}: Count = {loopCount}, End at {endLoopIndex}");
                
                if (endLoopIndex > commandIndex)
                {
                    // Execute loop multiple times
                    for (int iteration = 0; iteration < loopCount; iteration++)
                    {
                        if (debugMode) 
                            Debug.Log($"   🔄 Iterasi {iteration+1}/{loopCount}");
                        
                        int j = commandIndex + 1;
                        while (j < endLoopIndex && isActive)
                        {
                            if (allCommands[j] != "Empty" && allCommands[j] != "END_LOOP" && allCommands[j] != "LOOP")
                            {
                                // Move arrow to command inside loop
                                yield return StartCoroutine(AnimateArrowToSlot(j));
                                
                                float commandDuration = GetCommandWaitTime(allCommands[j]);
                                yield return new WaitForSeconds(commandDuration);
                            }
                            j++;
                        }
                        
                        // Return to LOOP position for next iteration
                        if (iteration < loopCount - 1 && isActive)
                        {
                            // yield return StartCoroutine(AnimateArrowToSlot(commandIndex));
                            yield return new WaitForSeconds(0.2f);
                        }
                    }

                    yield return new WaitForSeconds(0.3f);
                    
                    // Move to END_LOOP
                    // yield return StartCoroutine(AnimateArrowToSlot(endLoopIndex));
                    commandIndex = endLoopIndex;
                }
                
                commandIndex++;
                continue;
            }
            
            // Skip END_LOOP (already handled in LOOP)
            if (command == "END_LOOP")
            {
                commandIndex++;
                continue;
            }
            
            // Handle IF commands (from Part 2) if needed
            if (command == "IF_ORGANIK" || command == "IF_ANORGANIK")
            {
                bool conditionTrue = CheckIfCondition(command);
                int endIfIndex = FindEndIfIndex(allCommands, commandIndex);
                
                if (conditionTrue)
                {
                    commandIndex++;
                    int elseIndex = FindElseIndex(allCommands, commandIndex, endIfIndex);
                    int executeUntil = (elseIndex > commandIndex) ? elseIndex : endIfIndex;
                    
                    while (commandIndex < executeUntil && isActive)
                    {
                        if (allCommands[commandIndex] != "Empty" && 
                            allCommands[commandIndex] != "END_IF")
                        {
                            yield return StartCoroutine(AnimateArrowToSlot(commandIndex));
                            float commandDuration = GetCommandWaitTime(allCommands[commandIndex]);
                            yield return new WaitForSeconds(commandDuration);
                        }
                        commandIndex++;
                    }
                }
                else
                {
                    commandIndex = endIfIndex;
                }
                
                if (commandIndex < allCommands.Length && allCommands[commandIndex] == "END_IF")
                {
                    commandIndex++;
                }
                
                continue;
            }
            
            // Skip ELSE and END_IF
            if (command == "ELSE" || command == "END_IF")
            {
                commandIndex++;
                continue;
            }
            
            // Move arrow for normal command
            yield return StartCoroutine(AnimateArrowToSlot(commandIndex));
            
            if (debugMode) Debug.Log($"   ▶️ Arrow di Slot {commandIndex+1}: {command}");
            
            float waitTime = GetCommandWaitTime(command);
            yield return new WaitForSeconds(waitTime);
            
            commandIndex++;
            
            if (!IsExecuting())
                break;
        }
        
        if (arrowImage != null)
            arrowImage.color = Color.green;
            
        if (debugMode) Debug.Log("✅ Arrow selesai mengikuti (mode loop)");
    }
    
    private bool CheckIfCondition(string ifCommand)
    {
        RobotExecutePart2 robotExecute = GetRobotExecutePart2();
        if (robotExecute == null) 
        {
            robotExecute = GetRobotExecutePart3()?.GetComponent<RobotExecutePart2>();
        }
        
        if (robotExecute == null) 
        {
            if (debugMode) Debug.LogWarning("RobotExecute tidak ditemukan untuk cek kondisi IF");
            return false;
        }
        
        string carriedTrash = robotExecute.GetCarriedTrash();
        
        if (ifCommand == "IF_ORGANIK")
        {
            bool result = carriedTrash == "Organik";
            if (debugMode) Debug.Log($"🔍 Cek IF_ORGANIK: Bawa {carriedTrash} -> {result}");
            return result;
        }
        else if (ifCommand == "IF_ANORGANIK")
        {
            bool result = carriedTrash == "Anorganik";
            if (debugMode) Debug.Log($"🔍 Cek IF_ANORGANIK: Bawa {carriedTrash} -> {result}");
            return result;
        }
        
        return false;
    }
    
    private int GetLoopCount(string loopCommand)
    {
        if (loopCommand == "LOOP2") return 2;
        if (loopCommand == "LOOP3") return 3;
        if (loopCommand == "LOOP4") return 4;
        return 2;
    }
    
    private int FindEndLoopIndex(string[] commands, int startIndex)
    {
        for (int i = startIndex + 1; i < commands.Length; i++)
        {
            if (commands[i] == "END_LOOP") 
            {
                if (debugMode) Debug.Log($"   🔎 END_LOOP ditemukan di index {i}");
                return i;
            }
        }
        
        if (debugMode) Debug.LogWarning($"   ⚠️ END_LOOP tidak ditemukan setelah index {startIndex}");
        return commands.Length - 1;
    }
    
    private int FindElseIndex(string[] commands, int startIndex, int endIfIndex)
    {
        for (int i = startIndex; i < endIfIndex; i++)
        {
            if (commands[i] == "ELSE")
            {
                if (debugMode) Debug.Log($"🔍 ELSE ditemukan di index {i}");
                return i;
            }
        }
        if (debugMode) Debug.Log($"🔍 ELSE tidak ditemukan antara {startIndex} dan {endIfIndex}");
        return -1;
    }
    
    private IEnumerator AnimateArrowToSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slotWorldPositions.Length)
            yield break;
        
        if (arrowRect == null)
            yield break;
        
        Vector3 targetPosition = slotWorldPositions[slotIndex];
        Vector3 startPosition = arrowRect.position;
        
        Vector3 originalScale = arrowRect.localScale;
        Vector3 targetScale = originalScale * scaleMultiplier;
        
        float elapsedTime = 0f;
        float animationTime = 0.3f;
        
        while (elapsedTime < animationTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / animationTime;
            
            arrowRect.position = Vector3.Lerp(startPosition, targetPosition, t);
            
            if (t < 0.5f)
                arrowRect.localScale = Vector3.Lerp(originalScale, targetScale, t * 2);
            else
                arrowRect.localScale = Vector3.Lerp(targetScale, originalScale, (t - 0.5f) * 2);
            
            if (arrowImage != null)
                arrowImage.color = Color.Lerp(idleColor, activeColor, Mathf.PingPong(t * 2, 1));
            
            yield return null;
        }
        
        arrowRect.position = targetPosition;
        arrowRect.localScale = originalScale;
        currentCommandIndex = slotIndex;
    }
    
    private void MoveArrowToSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slotWorldPositions.Length)
            return;
        
        if (arrowRect != null)
        {
            arrowRect.position = slotWorldPositions[slotIndex];
            currentCommandIndex = slotIndex;
            
            if (debugMode) Debug.Log($"📍 Arrow pindah ke slot {slotIndex+1}");
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
            // case "AmbilSampah":
            // case "CollectTrash":
            //     return 1.0f;
            // case "BuangSampah":
            //     return 0.8f;
            // case "LOOP2":
            // case "LOOP3":
            // case "LOOP4":
            //     return 0.2f;
            // case "END_LOOP":
            //     return 0.1f;
            default:
                return 0.5f;
        }
    }
    
    public void OnExecuteButtonClicked()
    {
        Invoke("StartFollowing", 0.3f);
    }
    
    public void OnResetButtonClicked()
    {
        StopFollowing();
        
        if (arrowInstance != null && slotWorldPositions != null && slotWorldPositions.Length > 0)
        {
            arrowRect.position = slotWorldPositions[0];
            arrowImage.color = activeColor;
        }
    }
    
    private string[] GetCommandArray()
    {
        CommandManager cmdManager = GetCommandManager();
        if (cmdManager != null)
        {
            return cmdManager.GetCommandArray();
        }
        
        CommandManagerPart2 cmdManager2 = GetCommandManagerPart2();
        if (cmdManager2 != null)
        {
            return cmdManager2.GetCommandArray();
        }
        
        CommandManagerPart3 cmdManager3 = GetCommandManagerPart3();
        if (cmdManager3 != null)
        {
            return cmdManager3.GetCommandArray();
        }
        
        Debug.LogError("Tidak ditemukan CommandManager!");
        return new string[0];
    }
    
    private bool IsExecuting()
    {
        CommandExecute cmdExecute = GetCommandExecute();
        if (cmdExecute != null)
        {
            return cmdExecute.IsExecuting();
        }
        
        RobotExecutePart2 robotExecute = GetRobotExecutePart2();
        if (robotExecute != null)
        {
            return robotExecute.IsExecuting();
        }
        
        RobotExecutePart3 robotExecute3 = GetRobotExecutePart3();
        if (robotExecute3 != null)
        {
            return robotExecute3.IsExecuting();
        }
        
        Debug.LogWarning("Tidak ditemukan CommandExecute atau RobotExecute!");
        return false;
    }
    
    private int FindEndIfIndex(string[] commands, int startIndex)
    {
        for (int i = startIndex + 1; i < commands.Length; i++)
        {
            if (commands[i] == "END_IF") 
            {
                if (debugMode) Debug.Log($"   🔎 END_IF ditemukan di index {i}");
                return i;
            }
        }
        
        if (debugMode) Debug.LogWarning($"   ⚠️ END_IF tidak ditemukan setelah index {startIndex}");
        return commands.Length - 1;
    }
    
    private bool IsBranchingCommand(string command)
    {
        return command == "IF_ORGANIK" || 
               command == "IF_ANORGANIK" || 
               command == "ELSE" || 
               command == "END_IF" ||
               command.StartsWith("LOOP") ||
               command == "END_LOOP";
    }
    
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
        if (debugMode && Input.GetKeyDown(KeyCode.F8))
        {
            TestArrow();
        }
    }
}