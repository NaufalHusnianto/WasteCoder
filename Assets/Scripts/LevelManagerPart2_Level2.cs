using UnityEngine;
using UnityEngine.UI;

public class LevelManagerPart2_Level2 : MonoBehaviour
{
    [Header("LEVEL INFO")]
    public int levelNumber = 5;
    public string levelName = "Percabangan IF: Sampah Anorganik";
    
    [Header("REFERENCES")]
    public CommandManagerPart2 commandManager;
    public RobotExecutePart2 robot;
    
    [Header("TARGET OBJECTS")]
    public ConditionalTrash anorganikTrash;
    public TrashBin organikBin;
    public TrashBin anorganikBin;
    
    [Header("UI ELEMENTS")]
    public Text levelTitleText;
    public Text feedbackText;
    public GameObject successPanel;
    public GameObject failedPanel;
    
    private bool levelCompleted = false;
    private bool trashWasCollectable = false;
    
    void Start()
    {
        SetupLevel();
    }
    
    void SetupLevel()
    {
        // Setup UI
        if (levelTitleText != null)
            levelTitleText.text = $"Level {levelNumber}: {levelName}";
        
        if (feedbackText != null)
            feedbackText.text = "Pilah sampah Anorganik ke tempat yang benar!";
        
        // Hide panels
        if (successPanel != null) successPanel.SetActive(false);
        if (failedPanel != null) failedPanel.SetActive(false);
        
        // Auto find references
        if (commandManager == null) commandManager = FindObjectOfType<CommandManagerPart2>();
        if (robot == null) robot = FindObjectOfType<RobotExecutePart2>();
        
        // Simpan status awal
        if (anorganikTrash != null)
        {
            trashWasCollectable = anorganikTrash.IsCollectable();
        }
        
        Debug.Log($"🎮 Level {levelNumber} siap!");
        Debug.Log($"Target: Sampah Anorganik -> Tempat Sampah Anorganik");
    }
    
    // DIPANGGIL DARI BUTTON "JALANKAN"
    public void ExecuteAndCheck()
    {
        if (levelCompleted)
        {
            ShowMessage("Level sudah selesai!", Color.yellow);
            return;
        }
        
        if (robot.IsExecuting())
        {
            ShowMessage("Robot sedang bergerak!", Color.yellow);
            return;
        }
        
        StartCoroutine(ExecuteAndCheckRoutine());
    }
    
    System.Collections.IEnumerator ExecuteAndCheckRoutine()
    {
        Debug.Log("🔍 Memulai validasi Level 2...");
        
        // Simpan status awal
        if (anorganikTrash != null)
        {
            trashWasCollectable = anorganikTrash.IsCollectable();
        }
        
        // Jalankan robot
        robot.ExecuteCommands();
        
        // Tunggu robot selesai
        while (robot.IsExecuting())
        {
            yield return null;
        }
        
        yield return new WaitForSeconds(1f);
        
        // Validasi level
        bool isValid = ValidateLevel();
        
        // Tampilkan hasil
        if (isValid)
        {
            LevelSuccess();
        }
        else
        {
            LevelFailed();
        }
    }
    
    bool ValidateLevel()
    {
        if (commandManager == null || robot == null)
        {
            Debug.LogError("References tidak lengkap!");
            return false;
        }
        
        string[] commands = commandManager.GetCommandArray();
        
        // DEBUG: Tampilkan command array
        Debug.Log("📋 COMMAND ARRAY LEVEL 2:");
        for (int i = 0; i < commands.Length; i++)
        {
            Debug.Log($"  [{i}] {commands[i]}");
        }
        
        // 1. Validasi algoritma
        bool algorithmValid = ValidateAlgorithm(commands);
        
        // 2. Validasi logika IF
        bool logicValid = ValidateIfLogic(commands);
        
        // 3. Validasi hasil
        bool resultValid = ValidateResult();
        
        Debug.Log($"📊 Validasi Final: Algoritma={algorithmValid}, Logic={logicValid}, Result={resultValid}");
        
        return algorithmValid && logicValid && resultValid;
    }

    bool ValidateIfLogic(string[] commands)
    {
        bool hasIfAnorganik = false;
        bool hasIfOrganik = false;
        
        foreach (string cmd in commands)
        {
            if (cmd == "IF_ANORGANIK") hasIfAnorganik = true;
            if (cmd == "IF_ORGANIK") hasIfOrganik = true;
        }
        
        // Level 2 khusus sampah Anorganik
        if (hasIfOrganik)
        {
            ShowMessage("Salah! Sampah Anorganik butuh IF_ANORGANIK!", Color.red);
            return false;
        }
        
        if (!hasIfAnorganik)
        {
            ShowMessage("Gunakan IF_ANORGANIK untuk sampah ini!", Color.red);
            return false;
        }
        
        return true;
    }
    
    bool ValidateAlgorithm(string[] commands)
    {
        bool hasIfAnorganik = false;
        bool hasCollect = false;
        bool hasDeposit = false;
        int ifCount = 0;
        int endIfCount = 0;
        
        foreach (string cmd in commands)
        {
            if (cmd == "IF_ANORGANIK")
            {
                hasIfAnorganik = true;
                ifCount++;
            }
            else if (cmd == "AmbilSampah") hasCollect = true;
            else if (cmd == "BuangSampah") hasDeposit = true;
            else if (cmd == "END_IF") endIfCount++;
        }
        
        // Tampilkan analisis
        Debug.Log($"📊 Analisis Algoritma Level 2:");
        Debug.Log($"- IF_ANORGANIK: {hasIfAnorganik}");
        Debug.Log($"- AmbilSampah: {hasCollect}");
        Debug.Log($"- BuangSampah: {hasDeposit}");
        Debug.Log($"- END_IF: {endIfCount}x");
        
        // Cek kesalahan
        if (ifCount > endIfCount)
        {
            ShowMessage("ERROR: IF tanpa END_IF!", Color.red);
            return false;
        }
        
        // Validasi requirement
        bool valid = true;
        
        if (!hasIfAnorganik)
        {
            ShowMessage("Gunakan IF_ANORGANIK!", Color.red);
            valid = false;
        }
        
        if (!hasCollect)
        {
            ShowMessage("Tambahkan AmbilSampah!", Color.red);
            valid = false;
        }
        
        if (!hasDeposit)
        {
            ShowMessage("Tambahkan BuangSampah!", Color.red);
            valid = false;
        }
        
        return valid;
    }
    
    bool ValidateResult()
    {
        // Cek apakah sampah anorganik sudah diambil
        if (anorganikTrash == null) 
        {
            Debug.LogError("Sampah anorganik tidak ditemukan!");
            return false;
        }
        
        bool trashCollected = !anorganikTrash.IsCollectable(); // Sampah sudah diambil
        bool robotEmpty = (robot.GetCarriedTrash() == "None"); // Robot tidak membawa sampah
        
        // Jika robot masih membawa sampah, cek jenisnya
        if (!robotEmpty)
        {
            string carriedTrash = robot.GetCarriedTrash();
            ShowMessage($"Robot masih membawa sampah {carriedTrash}!", Color.red);
        }
        
        Debug.Log($"📊 Hasil Level 2:");
        Debug.Log($"- Sampah diambil: {trashCollected}");
        Debug.Log($"- Robot kosong: {robotEmpty}");
        
        return trashCollected && robotEmpty;
    }
    
    void LevelSuccess()
    {
        levelCompleted = true;
        Debug.Log("🎉 LEVEL 2 BERHASIL!");
        
        if (successPanel != null)
        {
            successPanel.SetActive(true);
        }
        
        ShowMessage("SUKSES! Sampah Anorganik dipilah dengan benar!", Color.green);
        
        // Save progress
        PlayerPrefs.SetInt("LevelPart2", levelNumber);
        PlayerPrefs.Save();
    }
    
    void LevelFailed()
    {
        Debug.Log("❌ LEVEL 2 GAGAL");
        
        if (failedPanel != null)
        {
            failedPanel.SetActive(true);
        }
        
        ShowMessage("Coba perbaiki algoritma!", Color.red);
    }
    
    void ShowMessage(string message, Color color)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = color;
        }
        Debug.Log(message);
    }
    
    // DIPANGGIL DARI BUTTON UI
    public void CloseSuccessPanel()
    {
        if (successPanel != null) successPanel.SetActive(false);
    }
    
    public void CloseFailedPanel()
    {
        if (failedPanel != null) failedPanel.SetActive(false);
    }
    
    public void ResetLevel()
    {
        levelCompleted = false;
        
        // Reset robot dan lingkungan
        if (robot != null) robot.ResetRobot();
        
        // Reset command UI (jika ada CommandReset script)
        CommandReset reset = FindObjectOfType<CommandReset>();
        if (reset != null) reset.ResetAllCommands();
        
        // Reset status awal
        if (anorganikTrash != null)
        {
            trashWasCollectable = anorganikTrash.IsCollectable();
            anorganikTrash.ResetTrash();
        }
        
        // Reset tempat sampah effects
        if (organikBin != null) organikBin.StopEffects();
        if (anorganikBin != null) anorganikBin.StopEffects();
        
        // Close panels
        CloseSuccessPanel();
        CloseFailedPanel();
        
        // Reset feedback
        if (feedbackText != null)
        {
            feedbackText.text = "Level 2 direset. Masukkan sampah Anorganik ke tempat Anorganik!";
            feedbackText.color = Color.white;
        }
        
        Debug.Log("🔄 Level 2 direset");
    }
    
    // Untuk debug
    public void PrintLevelStatus()
    {
        Debug.Log($"=== STATUS LEVEL {levelNumber} ===");
        Debug.Log($"Completed: {levelCompleted}");
        Debug.Log($"Robot executing: {robot.IsExecuting()}");
        Debug.Log($"Robot carrying: {robot.GetCarriedTrash()}");
        
        if (anorganikTrash != null)
        {
            Debug.Log($"Anorganik trash: {anorganikTrash.GetTrashType()} (Collectable: {anorganikTrash.IsCollectable()})");
        }
        
        if (organikBin != null)
        {
            Debug.Log($"Organik bin: {organikBin.binType}");
        }
        
        if (anorganikBin != null)
        {
            Debug.Log($"Anorganik bin: {anorganikBin.binType}");
        }
    }
}