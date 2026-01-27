using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class LevelManagerPart3_Level2 : MonoBehaviour
{
    [Header("LEVEL INFO")]
    public int levelNumber = 2;
    public string levelName = "Loop Collection";
    
    [Header("REFERENCES")]
    public CommandManagerPart3 commandManager;
    public RobotExecutePart3 robot;
    
    [Header("SAMPAH - Level 2")]
    public ConditionalTrash[] sampahOrganik; // 4 sampah berjajar
    public TrashBin tongOrganik; // Tong di ujung
    
    [Header("POSITION SETTINGS")]
    public float distanceBetweenTrash = 1f; // Jarak antar sampah
    public bool autoPositionTrash = false; // NONAKTIFKAN INI!
    
    [Header("UI ELEMENTS")]
    public Text levelTitleText;
    public Text feedbackText;
    public Text instruksiText;
    public Text hintText;
    public GameObject successPanel;
    public GameObject failedPanel;
    
    [Header("LEVEL REQUIREMENTS")]
    public int requiredTrashCount = 4; // Harus ambil 4 sampah
    public bool requireLoopForCollection = true; // Wajib pakai LOOP
    
    private bool levelCompleted = false;
    private bool[] trashCollected; // Status pengambilan sampah
    private Vector3 robotStartPosition;
    
    void Start()
    {
        SetupLevel();
    }
    
    void SetupLevel()
    {
        // Inisialisasi array status sampah
        trashCollected = new bool[sampahOrganik.Length];
        for (int i = 0; i < trashCollected.Length; i++)
        {
            trashCollected[i] = false;
        }
        
        // Setup UI
        if (levelTitleText != null)
            levelTitleText.text = $"Level {levelNumber}: {levelName}";
        
        if (instruksiText != null)
        {
            instruksiText.text = "<b>TUJUAN LEVEL:</b>\n" +
                                "Ambil 4 sampah organik berurutan\n" +
                                "dan buang ke tong organik.\n\n" +
                                "<b>PETUNJUK:</b>\n" +
                                "1. Gunakan LOOP untuk mengambil semua sampah\n" +
                                "2. Setelah semua sampah terkumpul, buang ke tong\n" +
                                "3. LOOP bisa digunakan dengan AmbilSampah";
        }
        
        if (hintText != null)
        {
            hintText.text = "💡 Tips: LOOP4 [AmbilSampah, Move] END_LOOP";
        }
        
        if (feedbackText != null)
            feedbackText.text = "Ambil 4 sampah organik berjajar menggunakan LOOP!";
        
        // Hide panels
        if (successPanel != null) successPanel.SetActive(false);
        if (failedPanel != null) failedPanel.SetActive(false);
        
        // Auto find references
        if (commandManager == null) commandManager = FindObjectOfType<CommandManagerPart3>();
        if (robot == null) robot = FindObjectOfType<RobotExecutePart3>();
        
        // HAPUS SetupTrashPositions() dari sini!
        // Biarkan posisi sampah seperti yang sudah diatur di Editor
        
        Debug.Log($"🎮 Level {levelNumber} Part 3 siap!");
        Debug.Log($"🎯 Target: Ambil {requiredTrashCount} sampah organik menggunakan LOOP");
        
        // Debug: Tampilkan posisi sampah
        for (int i = 0; i < sampahOrganik.Length; i++)
        {
            if (sampahOrganik[i] != null)
            {
                Debug.Log($"🗑️ Sampah {i+1} di posisi: {sampahOrganik[i].transform.position}");
            }
        }
        
        if (tongOrganik != null)
        {
            Debug.Log($"🗑️ Tong Organik di posisi: {tongOrganik.transform.position}");
        }
    }
    
    // HAPUS METHOD SetupTrashPositions() SEPENUHNYA!
    // Biarkan sampah di posisi yang sudah diatur di Editor
    
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
    
    IEnumerator ExecuteAndCheckRoutine()
    {
        Debug.Log("🔍 Memulai validasi Level 2 Part 3...");
        
        // Reset status sampah
        ResetTrashStatus();
        robotStartPosition = robot.transform.position;
        
        // Jalankan robot
        robot.ExecuteCommands();
        
        // Tunggu robot selesai
        while (robot.IsExecuting())
        {
            yield return null;
        }
        
        yield return new WaitForSeconds(1f); // Tunggu efek animasi
        
        // Update status pengambilan sampah
        UpdateTrashCollectionStatus();
        
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
    
    void ResetTrashStatus()
    {
        for (int i = 0; i < trashCollected.Length; i++)
        {
            trashCollected[i] = false;
        }
    }
    
    void UpdateTrashCollectionStatus()
    {
        for (int i = 0; i < sampahOrganik.Length; i++)
        {
            if (sampahOrganik[i] != null)
            {
                ConditionalTrash trashScript = sampahOrganik[i].GetComponent<ConditionalTrash>();
                if (trashScript != null)
                {
                    trashCollected[i] = !trashScript.IsCollectable();
                }
            }
        }
    }
    
    bool ValidateLevel()
    {
        string[] commands = commandManager.GetCommandArray();
        
        // DEBUG: Tampilkan command array
        Debug.Log("📋 COMMAND ARRAY LEVEL 2:");
        for (int i = 0; i < commands.Length; i++)
        {
            Debug.Log($"  [{i}] {commands[i]}");
        }
        
        // 1. Validasi penggunaan LOOP
        bool hasLoop = HasLoopCommand(commands);
        
        // 2. Validasi struktur LOOP
        bool validLoopStructure = ValidateLoopStructure(commands);
        
        // 3. Validasi algoritma pengambilan sampah
        bool validAlgorithm = ValidateCollectionAlgorithm(commands);
        
        // 4. Validasi hasil akhir
        bool validResult = ValidateFinalResult();
        
        Debug.Log($"📊 Validasi: Loop={hasLoop}, Structure={validLoopStructure}, Algorithm={validAlgorithm}, Result={validResult}");
        
        // Berikan feedback spesifik
        if (!hasLoop && requireLoopForCollection)
        {
            ShowMessage("Gunakan LOOP untuk mengambil sampah!", Color.red);
            if (hintText != null) 
                hintText.text = "💡 Contoh: LOOP4 [AmbilSampah, Move] END_LOOP";
        }
        else if (!validLoopStructure)
        {
            ShowMessage("Struktur LOOP tidak valid!", Color.red);
            if (hintText != null) 
                hintText.text = "💡 Pastikan setiap LOOP ada END_LOOP";
        }
        else if (!validAlgorithm)
        {
            ShowMessage("Algoritma pengambilan sampah salah!", Color.red);
            if (hintText != null) 
                hintText.text = "💡 Ambil sampah dulu, lalu bergerak ke sampah berikutnya";
        }
        else if (!validResult)
        {
            ShowMessage("Tidak semua sampah terkumpul/dibuang!", Color.red);
            if (hintText != null) 
                hintText.text = "💡 Pastikan ambil 4 sampah dan buang ke tong organik";
        }
        
        return hasLoop && validLoopStructure && validAlgorithm && validResult;
    }
    
    bool HasLoopCommand(string[] commands)
    {
        foreach (string cmd in commands)
        {
            if (cmd.StartsWith("LOOP"))
            {
                return true;
            }
        }
        return false;
    }
    
    bool ValidateLoopStructure(string[] commands)
    {
        int loopCount = 0;
        int endLoopCount = 0;
        
        foreach (string cmd in commands)
        {
            if (cmd.StartsWith("LOOP"))
            {
                loopCount++;
            }
            else if (cmd == "END_LOOP")
            {
                endLoopCount++;
                
                if (endLoopCount > loopCount)
                {
                    return false; // END_LOOP tanpa LOOP
                }
            }
        }
        
        return loopCount == endLoopCount;
    }
    
    bool ValidateCollectionAlgorithm(string[] commands)
    {
        // Cari blok LOOP pertama
        int loopStartIndex = -1;
        int loopEndIndex = -1;
        
        for (int i = 0; i < commands.Length; i++)
        {
            if (commands[i].StartsWith("LOOP"))
            {
                loopStartIndex = i;
                break;
            }
        }
        
        if (loopStartIndex == -1) return false;
        
        // Cari END_LOOP yang sesuai
        for (int i = loopStartIndex + 1; i < commands.Length; i++)
        {
            if (commands[i] == "END_LOOP")
            {
                loopEndIndex = i;
                break;
            }
        }
        
        if (loopEndIndex == -1) return false;
        
        // Analisis isi LOOP
        bool hasAmbilSampahInLoop = false;
        
        for (int i = loopStartIndex + 1; i < loopEndIndex; i++)
        {
            if (commands[i] == "AmbilSampah")
            {
                hasAmbilSampahInLoop = true;
                break;
            }
        }
        
        // Harus ada minimal 1 AmbilSampah dalam LOOP
        if (!hasAmbilSampahInLoop)
        {
            ShowMessage("LOOP harus berisi AmbilSampah!", Color.red);
            return false;
        }
        
        return true;
    }
    
    bool ValidateFinalResult()
    {
        // 1. Semua sampah harus terambil
        int collectedCount = 0;
        for (int i = 0; i < trashCollected.Length; i++)
        {
            if (trashCollected[i]) collectedCount++;
        }
        
        bool allTrashCollected = (collectedCount >= requiredTrashCount);
        
        // 2. Robot tidak membawa sampah (sudah dibuang)
        bool robotIsEmpty = true;
        
        // Cek jika ada method GetCarriedTrash di robot
        System.Reflection.MethodInfo getTrashMethod = robot.GetType().GetMethod("GetCarriedTrash");
        if (getTrashMethod != null)
        {
            string carriedTrash = (string)getTrashMethod.Invoke(robot, null);
            robotIsEmpty = (carriedTrash == "None" || string.IsNullOrEmpty(carriedTrash));
        }
        
        Debug.Log($"📊 Hasil akhir: Sampah terkumpul={collectedCount}/{requiredTrashCount}, Robot kosong={robotIsEmpty}");
        
        if (!allTrashCollected)
        {
            ShowMessage($"Kumpulkan {requiredTrashCount} sampah! (Sekarang: {collectedCount})", Color.red);
        }
        else if (!robotIsEmpty)
        {
            ShowMessage("Buang sampah ke tong organik!", Color.red);
        }
        
        return allTrashCollected && robotIsEmpty;
    }
    
    void LevelSuccess()
    {
        levelCompleted = true;
        Debug.Log("🎉 LEVEL 2 PART 3 BERHASIL!");
        
        if (successPanel != null)
        {
            successPanel.SetActive(true);
        }
        
        ShowMessage("SUKSES! Kamu berhasil mengumpulkan semua sampah dengan LOOP!", Color.green);
        
        if (hintText != null)
        {
            hintText.text = "🎉 Luar biasa! Loop membuat pekerjaan berulang jadi mudah!";
        }
        
        // Save progress
        PlayerPrefs.SetInt("LevelPart3", levelNumber);
        PlayerPrefs.Save();
        
        // Play celebration
        StartCoroutine(PlaySuccessAnimation());
    }
    
    void LevelFailed()
    {
        Debug.Log("❌ LEVEL 2 PART 3 GAGAL");
        
        if (failedPanel != null)
        {
            failedPanel.SetActive(true);
        }
        
        ShowMessage("Coba lagi! Perbaiki algoritma LOOP-mu", Color.red);
        
        // Berikan hint spesifik
        ProvideDetailedHint();
    }
    
    void ProvideDetailedHint()
    {
        string[] commands = commandManager.GetCommandArray();
        
        int ambilSampahCount = 0;
        bool hasAmbilInLoop = false;
        
        // Analisis command
        for (int i = 0; i < commands.Length; i++)
        {
            if (commands[i] == "AmbilSampah")
            {
                ambilSampahCount++;
                
                // Cek apakah dalam LOOP
                for (int j = i; j >= 0; j--)
                {
                    if (commands[j].StartsWith("LOOP"))
                    {
                        hasAmbilInLoop = true;
                        break;
                    }
                    else if (commands[j] == "END_LOOP")
                    {
                        break;
                    }
                }
            }
        }
        
        if (ambilSampahCount < requiredTrashCount)
        {
            if (hintText != null)
                hintText.text = $"💡 Butuh {requiredTrashCount}x AmbilSampah. Gunakan LOOP!";
        }
        else if (!hasAmbilInLoop)
        {
            if (hintText != null)
                hintText.text = "💡 Letakkan AmbilSampah di dalam LOOP!";
        }
        else
        {
            if (hintText != null)
                hintText.text = "💡 Pattern: LOOP4 [AmbilSampah, Move, BuangSampah] END_LOOP";
        }
    }
    
    IEnumerator PlaySuccessAnimation()
    {
        // Animasi untuk semua sampah yang terkumpul
        for (int i = 0; i < sampahOrganik.Length; i++)
        {
            if (sampahOrganik[i] != null && trashCollected[i])
            {
                Debug.Log($"✨ Sampah {i+1} berhasil dikumpulkan!");
                yield return new WaitForSeconds(0.3f);
            }
        }
        
        // Animasi tong organik
        if (tongOrganik != null)
        {
            tongOrganik.PlaySuccessEffect();
        }
    }
    
    void ShowMessage(string message, Color color)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = color;
        }
        Debug.Log($"📢 {message}");
    }
    
    // UI BUTTON FUNCTIONS
    public void CloseSuccessPanel()
    {
        if (successPanel != null) 
            successPanel.SetActive(false);
    }
    
    public void CloseFailedPanel()
    {
        if (failedPanel != null) 
            failedPanel.SetActive(false);
    }
    
    public void ResetLevel()
    {
        levelCompleted = false;
        
        // Reset robot
        if (robot != null) 
        {
            System.Reflection.MethodInfo resetMethod = robot.GetType().GetMethod("ResetRobot");
            if (resetMethod != null)
            {
                resetMethod.Invoke(robot, null);
            }
        }
        
        // Reset sampah
        ResetAllTrash();
        
        // Reset command UI
        CommandReset reset = FindObjectOfType<CommandReset>();
        if (reset != null) 
            reset.ResetAllCommands();
        
        // Reset tong
        if (tongOrganik != null)
        {
            tongOrganik.StopEffects();
        }
        
        // Close panels
        CloseSuccessPanel();
        CloseFailedPanel();
        
        // Reset feedback
        if (feedbackText != null)
        {
            feedbackText.text = "Level direset. Ambil 4 sampah organik dengan LOOP!";
            feedbackText.color = Color.white;
        }
        
        if (hintText != null)
        {
            hintText.text = "💡 Tips: LOOP4 [AmbilSampah, Move] END_LOOP";
        }
        
        Debug.Log("🔄 Level 2 Part 3 direset");
    }
    
    void ResetAllTrash()
    {
        for (int i = 0; i < sampahOrganik.Length; i++)
        {
            if (sampahOrganik[i] != null)
            {
                ConditionalTrash trashScript = sampahOrganik[i].GetComponent<ConditionalTrash>();
                if (trashScript != null)
                {
                    trashScript.ResetTrash();
                }
                trashCollected[i] = false;
            }
        }
    }
    
    // DEBUG FUNCTIONS
    [ContextMenu("Print Level Status")]
    public void PrintLevelStatus()
    {
        Debug.Log($"=== LEVEL 2 STATUS ===");
        Debug.Log($"Completed: {levelCompleted}");
        
        int collected = 0;
        for (int i = 0; i < trashCollected.Length; i++)
        {
            Debug.Log($"Sampah {i+1}: {(trashCollected[i] ? "TERKUMPUL" : "BELUM")}");
            if (trashCollected[i]) collected++;
        }
        
        Debug.Log($"Total terkumpul: {collected}/{requiredTrashCount}");
    }
}