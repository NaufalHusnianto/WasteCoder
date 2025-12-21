using UnityEngine;

public class CommandManagerPart2 : MonoBehaviour
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
            
            string commandName = child.name.ToLower();
            
            // DEBUG: Tampilkan nama yang dideteksi
            if (debugMode) Debug.Log($"Slot {slotIndex}: Child name = '{child.name}', Lower = '{commandName}'");
            
            // GERAKAN DASAR
            if (commandName.Contains("move")) return "Move";
            else if (commandName.Contains("turnleft") || commandName.Contains("kiri")) return "TurnLeft";
            else if (commandName.Contains("turnright") || commandName.Contains("kanan")) return "TurnRight";
            
            // COMMAND PART 2 - PERCABANGAN
            else if (commandName.Contains("ambil") || commandName.Contains("collect")) return "AmbilSampah";
            else if (commandName.Contains("buang") || commandName.Contains("deposit")) return "BuangSampah";
            
            // FIX: Deteksi IF dengan lebih fleksibel
            if (commandName.Contains("if_anorganik") || 
                (commandName.Contains("if") && commandName.Contains("anorganik")))
            {
                Debug.Log($"✅ Slot {slotIndex}: IF_ANORGANIK");
                return "IF_ANORGANIK";
            }
            else if (commandName.Contains("if_organik") || 
                    (commandName.Contains("if") && commandName.Contains("organik")))
            {
                Debug.Log($"✅ Slot {slotIndex}: IF_ORGANIK");
                return "IF_ORGANIK";
            }
            
            else if (commandName.Contains("else")) return "ELSE";
            else if (commandName.Contains("end") && commandName.Contains("if")) return "END_IF";
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
        string result = "📋 COMMANDS:\n";
        
        for (int i = 0; i < commands.Length; i++)
        {
            result += $"[{i+1}] {commands[i]}\n";
        }
        
        Debug.Log(result);
    }
}