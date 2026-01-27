using UnityEngine;

public class CommandManagerPart3 : MonoBehaviour
{
    [Header("COMMAND CONTAINER")]
    public GameObject commandContainer;
    
    [Header("DEBUG")]
    public bool debugMode = true;
    
    private string[] commandArray;
    
    void Start()
    {
        InitializeCommandArray();
    }
    
    void Update()
    {
        // Update secara real-time untuk debug
        InitializeCommandArray();
    }
    
    public void InitializeCommandArray()
    {
        if (commandContainer == null)
        {
            Debug.LogError("❌ Command Container belum diassign!");
            return;
        }
        
        int slotCount = commandContainer.transform.childCount;
        commandArray = new string[slotCount];
        
        for (int i = 0; i < slotCount; i++)
        {
            Transform slot = commandContainer.transform.GetChild(i);
            commandArray[i] = FindCommandInSlot(slot, i);
        }
    }
    
    private string FindCommandInSlot(Transform slot, int slotIndex)
    {
        if (slot.childCount == 0) 
        {
            return "Empty";
        }
        
        foreach (Transform child in slot)
        {
            if (!child.gameObject.activeInHierarchy) continue;
            
            string childName = child.name;
            string commandName = childName.ToLower();
            
            // DEBUG
            if (debugMode) 
                Debug.Log($"Slot {slotIndex}: '{childName}' -> Lower: '{commandName}'");
            
            // GERAKAN DASAR
            if (commandName.Contains("move")) return "Move";
            else if (commandName.Contains("turnleft") || commandName.Contains("kiri")) return "TurnLeft";
            else if (commandName.Contains("turnright") || commandName.Contains("kanan")) return "TurnRight";
            
            // LOOP COMMANDS (PART 3)
            else if (commandName.Contains("loop2")) return "LOOP2";
            else if (commandName.Contains("loop3")) return "LOOP3";
            else if (commandName.Contains("loop4")) return "LOOP4";
            else if (commandName.Contains("endloop") || (commandName.Contains("end") && commandName.Contains("loop"))) 
                return "END_LOOP";
            
            // Jika ada command dari part sebelumnya
            else if (commandName.Contains("ambil") || commandName.Contains("collect")) return "AmbilSampah";
            else if (commandName.Contains("buang") || commandName.Contains("deposit")) return "BuangSampah";
            
            // IF COMMANDS (PART 2) - jika masih dibutuhkan
            else if (commandName.Contains("if_anorganik") || 
                    (commandName.Contains("if") && commandName.Contains("anorganik")))
                return "IF_ANORGANIK";
            else if (commandName.Contains("if_organik") || 
                    (commandName.Contains("if") && commandName.Contains("organik")))
                return "IF_ORGANIK";
            else if (commandName.Contains("else")) return "ELSE";
            else if (commandName.Contains("endif")) return "END_IF";
        }
        
        return "Empty";
    }
    
    public string[] GetCommandArray()
    {
        if (commandArray == null) InitializeCommandArray();
        return commandArray;
    }
    
    public void PrintCommands()
    {
        string[] commands = GetCommandArray();
        string result = "📋 COMMANDS (PART 3):\n";
        
        for (int i = 0; i < commands.Length; i++)
        {
            result += $"[{i+1}] {commands[i]}\n";
        }
        
        Debug.Log(result);
    }
    
    // Validasi struktur LOOP
    public bool ValidateLoopStructure()
    {
        int loopDepth = 0;
        
        foreach (string cmd in commandArray)
        {
            if (cmd.StartsWith("LOOP"))
            {
                loopDepth++;
            }
            else if (cmd == "END_LOOP")
            {
                loopDepth--;
                if (loopDepth < 0)
                {
                    Debug.LogError("❌ END_LOOP tanpa LOOP!");
                    return false;
                }
            }
        }
        
        if (loopDepth != 0)
        {
            Debug.LogError($"❌ LOOP tidak seimbang! Depth sisa: {loopDepth}");
            return false;
        }
        
        return true;
    }
}