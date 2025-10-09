using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandManager : MonoBehaviour
{
    [Header("COMMAND CONTAINER")]
    public GameObject commandContainer;
    
    [Header("SETTINGS")]
    public bool autoRefreshOnStart = true;
    public bool debugMode = true;
    
    private string[] commandArray;
    
    void Start()
    {
        if (autoRefreshOnStart)
        {
            InitializeCommandArray();
        }
    }
    
    void Update()
    {
        // Refresh dengan tombol R
        if (Input.GetKeyDown(KeyCode.R))
        {
            InitializeCommandArray();
        }
        
        // Print dengan tombol P
        if (Input.GetKeyDown(KeyCode.P))
        {
            PrintCommandArray();
        }
    }
    
    public void InitializeCommandArray()
    {
        if (commandContainer == null)
        {
            Debug.LogError("❌ Command Container belum diassign!");
            return;
        }
        
        // Cek total slot yang ada
        int slotCount = commandContainer.transform.childCount;
        
        if (debugMode)
        {
            Debug.Log($"🔍 Mencari command di {slotCount} slot...");
        }
        
        commandArray = new string[slotCount];
        
        // Loop melalui semua slot
        for (int i = 0; i < slotCount; i++)
        {
            Transform slot = commandContainer.transform.GetChild(i);
            commandArray[i] = FindCommandInSlot(slot, i);
        }
        
        if (debugMode)
        {
            Debug.Log("✅ Command array berhasil diupdate!");
            PrintCommandArray();
        }
    }
    
    private string FindCommandInSlot(Transform slot, int slotIndex)
    {
        // Cek jika slot aktif
        if (!slot.gameObject.activeInHierarchy)
        {
            if (debugMode) Debug.Log($"   Slot {slotIndex}: Non-aktif - Diabaikan");
            return "Empty";
        }
        
        // Cek jika slot memiliki child
        if (slot.childCount == 0)
        {
            if (debugMode) Debug.Log($"   Slot {slotIndex}: Tidak ada child - Empty");
            return "Empty";
        }
        
        // Loop melalui semua child dalam slot
        foreach (Transform child in slot)
        {
            // Cek jika child aktif
            if (!child.gameObject.activeInHierarchy) continue;
            
            string commandName = child.name.ToLower();
            
            // Deteksi command
            if (commandName.Contains("move"))
            {
                if (debugMode) Debug.Log($"   ✅ Slot {slotIndex}: Ditemukan 'Move'");
                return "Move";
            }
            else if (commandName.Contains("turnleft") || commandName.Contains("left"))
            {
                if (debugMode) Debug.Log($"   ✅ Slot {slotIndex}: Ditemukan 'TurnLeft'");
                return "TurnLeft";
            }
            else if (commandName.Contains("turnright") || commandName.Contains("right"))
            {
                if (debugMode) Debug.Log($"   ✅ Slot {slotIndex}: Ditemukan 'TurnRight'");
                return "TurnRight";
            }
        }
        
        if (debugMode) 
        {
            Debug.Log($"   ❌ Slot {slotIndex}: Tidak ada command yang dikenali");
            Debug.Log($"      Child yang ditemukan:");
            foreach (Transform child in slot)
            {
                Debug.Log($"      - '{child.name}' (Aktif: {child.gameObject.activeInHierarchy})");
            }
        }
        
        return "Empty";
    }
    
    public void PrintCommandArray()
    {
        if (commandArray == null || commandArray.Length == 0)
        {
            Debug.LogWarning("⚠️ Command array kosong! Tekan R untuk refresh");
            return;
        }
        
        string result = "🎮 **COMMAND ARRAY ROBOT**\n";
        result += "========================\n";
        
        int commandCount = 0;
        
        for (int i = 0; i < commandArray.Length; i++)
        {
            string command = commandArray[i];
            string status = $"[{i + 1}] {command}";
            
            if (command != "Empty")
            {
                status = $"<color=green>{status}</color>";
                commandCount++;
            }
            else
            {
                status = $"<color=grey>{status}</color>";
            }
            
            result += status + "\n";
        }
        
        result += $"========================\n";
        result += $"Total Command: {commandCount}/{commandArray.Length}";
        
        Debug.Log(result);
    }
    
    // Method untuk mendapatkan array command
    public string[] GetCommandArray()
    {
        if (commandArray == null)
        {
            InitializeCommandArray();
        }
        return commandArray;
    }
    
    // Method untuk mendapatkan command di slot tertentu
    public string GetCommandAtSlot(int slotIndex)
    {
        if (commandArray != null && slotIndex >= 0 && slotIndex < commandArray.Length)
        {
            return commandArray[slotIndex];
        }
        return "Invalid";
    }
    
    // Method untuk mengecek jika slot berisi command
    public bool HasCommandAtSlot(int slotIndex)
    {
        if (commandArray != null && slotIndex >= 0 && slotIndex < commandArray.Length)
        {
            return commandArray[slotIndex] != "Empty";
        }
        return false;
    }
    
    // Method untuk eksekusi command (bisa ditambahkan later)
    public void ExecuteAllCommands()
    {
        InitializeCommandArray();
        
        Debug.Log("🚀 **MENGEKSEKUSI COMMAND**");
        for (int i = 0; i < commandArray.Length; i++)
        {
            if (commandArray[i] != "Empty")
            {
                Debug.Log($"   ▶️ Slot {i + 1}: {commandArray[i]}");
                // Tambahkan logika eksekusi di sini
            }
        }
    }
    
    // Method untuk UI Button - Refresh
    public void RefreshCommands()
    {
        InitializeCommandArray();
    }
    
    // Method untuk UI Button - Print
    public void PrintCommands()
    {
        PrintCommandArray();
    }
    
    // Method untuk UI Button - Execute
    public void ExecuteCommands()
    {
        ExecuteAllCommands();
    }
}