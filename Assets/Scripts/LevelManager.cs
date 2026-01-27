using UnityEngine;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    [Header("LEVEL SETTINGS")]
    public int levelNumber = 1;
    
    [Header("LEVEL TYPE")]
    public LevelType levelType = LevelType.BasicMovement;
    
    [Header("LEVEL 1: Basic Movement")]
    public int requiredForwardCommands = 5;
    
    [Header("LEVEL 2-3: Collect Trash")]
    public SimpleTrash targetSimpleTrash;
    
    [Header("REFERENCES")]
    public CommandManager commandManager;
    public CommandExecute commandExecute;
    
    [Header("UI POPUPS")]
    public GameObject successPopup;
    public GameObject failedPopup;
    public Text successText;
    public Text failedText;

    [Header("SOUND EFFECTS")]
    public AudioSource audioSource;
    public AudioClip successSound;
    public AudioClip failedSound;
    
    [Header("DEBUG")]
    public bool debugMode = true;
    
    private bool isLevelCompleted = false;
    private bool isChecking = false;
    
    // Untuk tracking status awal
    private bool trashWasCollectableAtStart = false;
    
    public enum LevelType
    {
        BasicMovement,       // Level 1
        CollectTrash         // Level 2-3
    }
    
    void Start()
    {
        if (successPopup != null) successPopup.SetActive(false);
        if (failedPopup != null) failedPopup.SetActive(false);
        
        SetupLevelText();
        
        Debug.Log($"🎮 LEVEL {levelNumber} READY - Type: {levelType}");
    }
    
    void SetupLevelText()
    {
        switch (levelType)
        {
            case LevelType.BasicMovement:
                if (successText != null) 
                    successText.text = $"Level {levelNumber} Selesai!\nRobot berhasil bergerak {requiredForwardCommands} langkah!";
                if (failedText != null)
                    failedText.text = $"Level {levelNumber} Gagal!\nDibutuhkan tepat {requiredForwardCommands} langkah maju";
                break;
                
            case LevelType.CollectTrash:
                if (successText != null) 
                    successText.text = $"Level {levelNumber} Selesai!\nRobot berhasil mengumpulkan sampah!";
                if (failedText != null)
                    failedText.text = $"Level {levelNumber} Gagal!\nRobot tidak berhasil mengumpulkan sampah";
                break;
        }
    }
    
    public void ExecuteAndValidateLevel()
    {
        if (isLevelCompleted)
        {
            Debug.Log("Level sudah selesai!");
            return;
        }
        
        if (isChecking)
        {
            Debug.LogWarning("Sedang memproses level!");
            return;
        }
        
        if (commandManager == null)
        {
            commandManager = FindObjectOfType<CommandManager>();
            if (commandManager == null)
            {
                Debug.LogError("CommandManager tidak ditemukan!");
                return;
            }
        }
        
        if (commandExecute == null)
        {
            commandExecute = FindObjectOfType<CommandExecute>();
            if (commandExecute == null)
            {
                Debug.LogError("CommandExecute tidak ditemukan!");
                return;
            }
        }
        
        StartCoroutine(ExecuteAndValidateCoroutine());
    }
    
    private System.Collections.IEnumerator ExecuteAndValidateCoroutine()
    {
        isChecking = true;
        
        Debug.Log($"🚀 Memulai eksekusi Level {levelNumber} ({levelType})...");
        
        // Simpan posisi awal robot
        Vector3 startPosition = commandExecute.transform.position;
        Quaternion startRotation = commandExecute.transform.rotation;
        
        // SIMPAN STATUS AWAL SEBELUM EKSEKUSI
        SaveInitialStates();
        
        // Jalankan semua command
        commandExecute.ExecuteAllCommands();
        
        // Tunggu sampai robot selesai bergerak
        while (commandExecute.IsExecuting())
        {
            yield return null;
        }
        
        Debug.Log("✅ Robot selesai bergerak");
        
        yield return new WaitForSeconds(0.5f);
        
        // Validasi berdasarkan tipe level
        bool levelSuccess = false;
        
        switch (levelType)
        {
            case LevelType.BasicMovement:
                levelSuccess = ValidateBasicMovement(startPosition);
                break;
                
            case LevelType.CollectTrash:
                levelSuccess = ValidateCollectTrash();
                break;
        }
        
        // Tampilkan hasil
        if (levelSuccess)
        {
            ShowLevelSuccess();
        }
        else
        {
            ShowLevelFailed();
        }
        
        // Reset robot setelah 2 detik
        yield return new WaitForSeconds(2f);
        
        // Reset robot ke posisi awal
        commandExecute.transform.position = startPosition;
        commandExecute.transform.rotation = startRotation;
        
        isChecking = false;
    }
    
    // Simpan status awal untuk perbandingan
    private void SaveInitialStates()
    {
        switch (levelType)
        {
            case LevelType.CollectTrash:
                if (targetSimpleTrash != null)
                {
                    trashWasCollectableAtStart = targetSimpleTrash.isCollectable;
                }
                break;
        }
    }
    
    private bool ValidateBasicMovement(Vector3 startPosition)
    {
        Vector3 endPosition = commandExecute.transform.position;
        float distance = Vector3.Distance(startPosition, endPosition);
        float expectedDistance = requiredForwardCommands * commandExecute.moveDistance;
        float tolerance = 0.1f;
        
        if (debugMode)
        {
            Debug.Log($"📏 Validasi Jarak:");
            Debug.Log($"- Jarak tempuh: {distance}");
            Debug.Log($"- Jarak harapan: {expectedDistance}");
        }
        
        bool success = Mathf.Abs(distance - expectedDistance) < tolerance;
        
        if (success)
            Debug.Log("✅ Validasi Level 1: Robot bergerak tepat " + requiredForwardCommands + " langkah");
        else
            Debug.Log("❌ Validasi Level 1: Jarak tidak sesuai");
        
        return success;
    }
    
    private bool ValidateCollectTrash()
    {
        // Level 2-3: Cek apakah sampah BERUBAH dari collectable ke tidak collectable
        if (targetSimpleTrash != null)
        {
            bool wasCollectable = trashWasCollectableAtStart;
            bool isCollectableNow = targetSimpleTrash.isCollectable;
            
            Debug.Log($"🗑️ Validasi Level 2-3:");
            Debug.Log($"- Awal: Collectable={wasCollectable}");
            Debug.Log($"- Sekarang: Collectable={isCollectableNow}");
            
            // Sukses jika awalnya collectable, sekarang tidak collectable
            bool success = wasCollectable && !isCollectableNow;
            
            if (success)
                Debug.Log("✅ Validasi Level 2-3: Sampah berhasil dikumpulkan");
            else
                Debug.Log("❌ Validasi Level 2-3: Sampah belum dikumpulkan");
            
            return success;
        }
        
        Debug.Log("❌ Validasi Level 2-3: Target sampah tidak ditemukan");
        return false;
    }
    
    private void ShowLevelSuccess()
    {
        if (debugMode) Debug.Log("🎉 LEVEL BERHASIL!");
        
        isLevelCompleted = true;
        
        if (successPopup != null)
        {
            successPopup.SetActive(true);
        }

        PlaySound(successSound);
        
        // Save level progress
        PlayerPrefs.SetInt("LevelCompleted", levelNumber);
        PlayerPrefs.Save();
    }
    
    private void ShowLevelFailed()
    {
        if (debugMode) Debug.Log("❌ LEVEL GAGAL!");
        
        if (failedPopup != null)
        {
            failedPopup.SetActive(true);
        }

        PlaySound(failedSound);
    }
    
    // Method untuk UI Button
    public void CloseSuccessPopup()
    {
        if (successPopup != null) successPopup.SetActive(false);
    }
    
    public void CloseFailedPopup()
    {
        if (failedPopup != null) failedPopup.SetActive(false);
    }
    
    public void ResetLevel()
    {
        isLevelCompleted = false;
        isChecking = false;
        
        // Reset commands
        CommandReset commandReset = FindObjectOfType<CommandReset>();
        if (commandReset != null)
        {
            commandReset.ResetAllCommands();
        }
        
        // Reset robot dan semua objek
        if (commandExecute != null)
        {
            commandExecute.ResetRobot();
        }
        
        // Close popups
        CloseSuccessPopup();
        CloseFailedPopup();
        
        // Reset status awal
        trashWasCollectableAtStart = false;
        
        if (debugMode) Debug.Log("🔄 Level direset");
    }
    
    public void PrintLevelInfo()
    {
        if (commandManager == null) return;
        
        commandManager.InitializeCommandArray();
        string[] commands = commandManager.GetCommandArray();
        
        Debug.Log($"📊 LEVEL {levelNumber} INFO:");
        Debug.Log($"Type: {levelType}");
        
        int moveCount = 0, collectCount = 0;
        foreach (string cmd in commands)
        {
            switch (cmd)
            {
                case "Move": moveCount++; break;
                case "CollectTrash": collectCount++; break;
            }
        }
        
        Debug.Log($"Commands: Move={moveCount}, Collect={collectCount}");
        
        switch (levelType)
        {
            case LevelType.BasicMovement:
                Debug.Log($"Required Moves: {requiredForwardCommands}");
                break;
                
            case LevelType.CollectTrash:
                if (targetSimpleTrash != null)
                    Debug.Log($"SimpleTrash: Collectable={targetSimpleTrash.isCollectable}");
                break;
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
        else
        {
            if (debugMode) Debug.LogWarning($"Sound effect tidak ditemukan atau AudioSource belum di-set: {clip?.name}");
        }
    }
}