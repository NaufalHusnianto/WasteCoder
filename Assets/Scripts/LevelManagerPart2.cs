using UnityEngine;
using UnityEngine.UI;

public class LevelManagerPart2 : MonoBehaviour
{
    [Header("LEVEL INFO")]
    public int levelNumber = 1;
    public string levelName = "Percabangan IF";
    
    [Header("REFERENCES")]
    public CommandManagerPart2 commandManager;
    public RobotExecutePart2 robot;
    
    [Header("TARGET OBJECTS")]
    public ConditionalTrash targetTrash;
    public TrashBin targetBin;
    
    [Header("UI ELEMENTS")]
    public Text levelTitleText;
    public Text feedbackText;
    public GameObject successPanel;
    public GameObject failedPanel;
    
    [Header("LEVEL GOAL")]
    public bool requireIfCommand = true;
    public bool requireCorrectSorting = true;
    
    [Header("SOUND EFFECTS")]
    public AudioSource audioSource;
    public AudioClip successSound;
    public AudioClip failedSound;
    
    [Header("DEBUG")]
    public bool debugMode = true;
    
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
            feedbackText.text = "Pilah sampah dengan IF statement!";
        
        // Hide panels
        if (successPanel != null) successPanel.SetActive(false);
        if (failedPanel != null) failedPanel.SetActive(false);
        
        // Auto find references
        if (commandManager == null) commandManager = FindObjectOfType<CommandManagerPart2>();
        if (robot == null) robot = FindObjectOfType<RobotExecutePart2>();
        
        // Find AudioSource jika belum di-set
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null && debugMode)
                Debug.LogWarning("AudioSource tidak ditemukan di GameObject ini");
        }
        
        // Simpan status awal
        if (targetTrash != null)
        {
            trashWasCollectable = targetTrash.IsCollectable();
        }
        
        Debug.Log($"🎮 Level {levelNumber} siap!");
        Debug.Log($"Target: Sampah {targetTrash?.GetTrashType()} -> Tempat {targetBin?.binType}");
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
        Debug.Log("🔍 Memulai validasi level...");
        
        // Simpan status awal
        if (targetTrash != null)
        {
            trashWasCollectable = targetTrash.IsCollectable();
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
        if (debugMode)
        {
            Debug.Log("📋 COMMAND ARRAY UNTUK VALIDASI:");
            for (int i = 0; i < commands.Length; i++)
            {
                Debug.Log($"  [{i}] {commands[i]}");
            }
        }
        
        // 1. Validasi algoritma
        bool algorithmValid = ValidateAlgorithm(commands);
        
        // 2. Validasi logika IF
        bool logicValid = ValidateIfLogic(commands);
        
        // 3. Validasi hasil
        bool resultValid = ValidateResult();
        
        if (debugMode)
            Debug.Log($"📊 Validasi Final: Algoritma={algorithmValid}, Logic={logicValid}, Result={resultValid}");
        
        return algorithmValid && logicValid && resultValid;
    }

    bool ValidateIfLogic(string[] commands)
    {
        // Cek apakah IF_ANORGANIK digunakan untuk sampah organik
        bool hasIfAnorganik = false;
        bool hasIfOrganik = false;
        
        foreach (string cmd in commands)
        {
            if (cmd == "IF_ANORGANIK") hasIfAnorganik = true;
            if (cmd == "IF_ORGANIK") hasIfOrganik = true;
        }
        
        // Jika target sampah Organik, harus pakai IF_ORGANIK
        if (targetTrash != null && targetTrash.GetTrashType() == ConditionalTrash.TrashType.Organik)
        {
            if (hasIfAnorganik && !hasIfOrganik)
            {
                ShowMessage("Salah! Sampah Organik butuh IF_ORGANIK!", Color.red);
                return false;
            }
        }
        
        return true;
    }
    
    bool ValidateAlgorithm(string[] commands)
    {
        bool hasIf = false;
        bool hasCollect = false;
        bool hasDeposit = false;
        int ifCount = 0;
        int endIfCount = 0;
        
        foreach (string cmd in commands)
        {
            if (cmd == "IF_ORGANIK" || cmd == "IF_ANORGANIK")
            {
                hasIf = true;
                ifCount++;
            }
            else if (cmd == "AmbilSampah") hasCollect = true;
            else if (cmd == "BuangSampah") hasDeposit = true;
            else if (cmd == "END_IF") endIfCount++;
        }
        
        // Tampilkan analisis jika debug mode aktif
        if (debugMode)
        {
            Debug.Log($"📊 Analisis Algoritma:");
            Debug.Log($"- IF Command: {hasIf} ({ifCount}x)");
            Debug.Log($"- AmbilSampah: {hasCollect}");
            Debug.Log($"- BuangSampah: {hasDeposit}");
            Debug.Log($"- END_IF: {endIfCount}x");
        }
        
        // Cek kesalahan
        if (ifCount > endIfCount)
        {
            ShowMessage("ERROR: IF tanpa END_IF!", Color.red);
            return false;
        }
        
        // Validasi requirement
        bool valid = true;
        
        if (requireIfCommand && !hasIf)
        {
            ShowMessage("Gunakan IF statement!", Color.red);
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
        // Cek apakah sampah berhasil dipilah
        if (targetTrash == null) return true; // Skip jika tidak ada target
        
        bool trashCollected = !targetTrash.IsCollectable(); // Sampah sudah diambil
        bool correctSorting = true;
        
        // Jika ada target bin, cek apakah jenis sampah cocok
        if (targetBin != null)
        {
            // Cek apakah robot membawa sampah yang sesuai
            string carriedTrash = robot.GetCarriedTrash();
            correctSorting = (carriedTrash == "None"); // Robot tidak membawa sampah = sudah dibuang
        }
        
        if (debugMode)
        {
            Debug.Log($"📊 Hasil Pembersihan:");
            Debug.Log($"- Sampah diambil: {trashCollected}");
            Debug.Log($"- Robot bawa sampah: {robot.GetCarriedTrash()}");
            Debug.Log($"- Sorting benar: {correctSorting}");
        }
        
        return trashCollected && correctSorting;
    }
    
    void LevelSuccess()
    {
        levelCompleted = true;
        
        if (debugMode)
            Debug.Log("🎉 LEVEL BERHASIL!");
        
        if (successPanel != null)
        {
            successPanel.SetActive(true);
        }
        
        ShowMessage("SUKSES! Algoritma IF benar dan sampah terpilah!", Color.green);
        
        // Play success sound
        PlaySound(successSound);
        
        // Save progress
        PlayerPrefs.SetInt("LevelPart2", levelNumber);
        PlayerPrefs.Save();
    }
    
    void LevelFailed()
    {
        if (debugMode)
            Debug.Log("❌ LEVEL GAGAL");
        
        if (failedPanel != null)
        {
            failedPanel.SetActive(true);
        }
        
        ShowMessage("Coba perbaiki algoritma!", Color.red);
        
        // Play failed sound
        PlaySound(failedSound);
    }
    
    void ShowMessage(string message, Color color)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = color;
        }
        
        if (debugMode)
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
        
        // Reset command UI
        CommandReset reset = FindObjectOfType<CommandReset>();
        if (reset != null) reset.ResetAllCommands();
        
        // Reset status awal
        if (targetTrash != null)
        {
            trashWasCollectable = targetTrash.IsCollectable();
            targetTrash.ResetTrash();
        }
        
        // Close panels
        CloseSuccessPanel();
        CloseFailedPanel();
        
        // Reset feedback
        if (feedbackText != null)
        {
            feedbackText.text = "Level direset. Coba lagi!";
            feedbackText.color = Color.white;
        }
        
        if (debugMode)
            Debug.Log("🔄 Level direset");
    }
    
    // Untuk debug
    public void PrintLevelStatus()
    {
        Debug.Log($"=== STATUS LEVEL {levelNumber} ===");
        Debug.Log($"Completed: {levelCompleted}");
        Debug.Log($"Robot executing: {robot.IsExecuting()}");
        Debug.Log($"Robot carrying: {robot.GetCarriedTrash()}");
        
        if (targetTrash != null)
        {
            Debug.Log($"Target trash: {targetTrash.GetTrashType()} (Collectable: {targetTrash.IsCollectable()})");
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
            if (debugMode) 
                Debug.LogWarning($"Sound effect tidak ditemukan atau AudioSource belum di-set: {clip?.name}");
        }
    }
}