using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LevelManagerPart2_Level3 : MonoBehaviour
{
    [Header("LEVEL INFO")]
    public int levelNumber = 6;
    public string levelName = "Percabangan IF-ELSE";
    
    [Header("REFERENCES")]
    public CommandManagerPart2 commandManager;
    public RobotExecutePart2 robot;
    
    [Header("SAMPAH LEVEL 3")]
    public ConditionalTrash targetSampah; // HANYA 1 SAMPAH
    public TrashType sampahType = TrashType.Random; // Bisa di-set
    
    [Header("TEMPAT SAMPAH")]
    public TrashBin tongOrganik;
    public TrashBin tongAnorganik;
    
    [Header("UI ELEMENTS")]
    public Text levelTitleText;
    public Text feedbackText;
    public GameObject successPanel;
    public GameObject failedPanel;
    
    [Header("SOUND EFFECTS")]
    public AudioSource audioSource;
    public AudioClip successSound;
    public AudioClip failedSound;
    
    [Header("LEVEL SETTING")]
    public bool randomTrashEachTry = true; // Sampah random setiap reset
    
    [Header("DEBUG")]
    public bool debugMode = true;
    
    public enum TrashType { Organik, Anorganik, Random }
    
    private bool levelCompleted = false;
    private TrashType currentTrashType;
    
    void Start()
    {
        SetupLevel();
    }
    
    void SetupLevel()
    {
        // Tentukan jenis sampah
        if (sampahType == TrashType.Random)
        {
            currentTrashType = (Random.value > 0.5f) ? TrashType.Organik : TrashType.Anorganik;
        }
        else
        {
            currentTrashType = sampahType;
        }
        
        // Setup UI
        if (levelTitleText != null)
            levelTitleText.text = $"Level {levelNumber}: {levelName}";
        
        if (feedbackText != null)
        {
            feedbackText.text = $"Siapkan IF-ELSE untuk sampah {currentTrashType}!";
        }
        
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
        
        // Setup sampah berdasarkan jenis
        SetupTrashType();
        
        if (debugMode)
        {
            Debug.Log($"🎮 Level {levelNumber} siap!");
            Debug.Log($"Jenis sampah: {currentTrashType}");
        }
    }
    
    void SetupTrashType()
    {
        if (targetSampah != null)
        {
            // Set jenis sampah
            if (currentTrashType == TrashType.Organik)
            {
                targetSampah.trashType = ConditionalTrash.TrashType.Organik;
            }
            else
            {
                targetSampah.trashType = ConditionalTrash.TrashType.Anorganik;
            }
            
            // Reset sampah
            targetSampah.ResetTrash();
            
            // Warna visual (optional)
            Renderer renderer = targetSampah.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = (currentTrashType == TrashType.Organik) 
                    ? Color.green : Color.yellow;
            }
        }
    }
    
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
        if (debugMode)
            Debug.Log($"🔍 Memulai validasi Level 3 (Sampah: {currentTrashType})...");
        
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
        if (commandManager == null || robot == null || targetSampah == null)
        {
            Debug.LogError("References tidak lengkap!");
            return false;
        }
        
        string[] commands = commandManager.GetCommandArray();
        
        // 1. Validasi struktur IF-ELSE
        bool structureValid = ValidateIfElseStructure(commands);
        
        // 2. Validasi hasil akhir
        bool resultValid = ValidateResult();
        
        if (debugMode)
            Debug.Log($"📊 Validasi: Structure={structureValid}, Result={resultValid}");
        
        return structureValid && resultValid;
    }
    
    bool ValidateIfElseStructure(string[] commands)
    {
        bool hasIF = false;
        bool hasELSE = false;
        bool hasENDIF = false;
        
        foreach (string cmd in commands)
        {
            if (cmd == "IF_ORGANIK" || cmd == "IF_ANORGANIK")
            {
                hasIF = true;
            }
            else if (cmd == "ELSE")
            {
                hasELSE = true;
            }
            else if (cmd == "END_IF")
            {
                hasENDIF = true;
            }
        }
        
        // Validasi Level 3: HARUS punya IF, ELSE, dan END_IF
        if (!hasIF)
        {
            ShowMessage("Level 3 butuh perintah IF!", Color.red);
            return false;
        }
        
        if (!hasELSE)
        {
            ShowMessage("Level 3 butuh perintah ELSE!", Color.red);
            return false;
        }
        
        if (!hasENDIF)
        {
            ShowMessage("Level 3 butuh perintah END_IF!", Color.red);
            return false;
        }
        
        // Validasi bahwa ELSE digunakan dengan benar
        // (tidak boleh hanya IF tanpa ELSE)
        bool elseUsedCorrectly = false;
        
        for (int i = 0; i < commands.Length; i++)
        {
            if ((commands[i] == "IF_ORGANIK" || commands[i] == "IF_ANORGANIK") && 
                i + 1 < commands.Length && commands[i + 1] != "ELSE")
            {
                // Cari ELSE setelah IF
                for (int j = i + 1; j < commands.Length; j++)
                {
                    if (commands[j] == "ELSE")
                    {
                        elseUsedCorrectly = true;
                        break;
                    }
                    if (commands[j] == "END_IF") break;
                }
            }
        }
        
        if (!elseUsedCorrectly)
        {
            ShowMessage("Gunakan ELSE setelah blok IF!", Color.red);
            return false;
        }
        
        return true;
    }
    
    bool ValidateResult()
    {
        // Cek apakah sampah sudah dibuang dengan benar
        bool sampahDibuang = !targetSampah.IsCollectable();
        bool robotKosong = (robot.GetCarriedTrash() == "None");
        
        if (debugMode)
        {
            Debug.Log($"📊 Hasil:");
            Debug.Log($"- Sampah dibuang: {sampahDibuang}");
            Debug.Log($"- Robot kosong: {robotKosong}");
            Debug.Log($"- Jenis sampah: {currentTrashType}");
        }
        
        if (!sampahDibuang)
        {
            ShowMessage("Ambil dan buang sampahnya!", Color.red);
            return false;
        }
        
        if (!robotKosong)
        {
            ShowMessage("Robot masih membawa sampah!", Color.red);
            return false;
        }
        
        return true;
    }
    
    void LevelSuccess()
    {
        levelCompleted = true;
        
        if (debugMode)
            Debug.Log($"🎉 LEVEL 3 BERHASIL! (Sampah: {currentTrashType})");
        
        if (successPanel != null)
        {
            successPanel.SetActive(true);
        }
        
        ShowMessage($"SUKSES! IF-ELSE berhasil untuk sampah {currentTrashType}!", Color.green);
        
        // Play success sound
        PlaySound(successSound);
        
        // Save progress
        PlayerPrefs.SetInt("LevelPart2", levelNumber);
        PlayerPrefs.Save();
    }
    
    void LevelFailed()
    {
        if (debugMode)
            Debug.Log($"❌ LEVEL 3 GAGAL (Sampah: {currentTrashType})");
        
        if (failedPanel != null)
        {
            failedPanel.SetActive(true);
        }
        
        ShowMessage("Perbaiki algoritma IF-ELSE-mu!", Color.red);
        
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
    
    // UI BUTTON FUNCTIONS
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
        
        // Random sampah jika di-setting
        if (randomTrashEachTry)
        {
            currentTrashType = (Random.value > 0.5f) ? TrashType.Organik : TrashType.Anorganik;
            SetupTrashType();
        }
        
        // Reset robot
        if (robot != null) robot.ResetRobot();
        
        // Reset command UI
        CommandReset reset = FindObjectOfType<CommandReset>();
        if (reset != null) reset.ResetAllCommands();
        
        // Close panels
        CloseSuccessPanel();
        CloseFailedPanel();
        
        // Reset feedback
        if (feedbackText != null)
        {
            feedbackText.text = $"Level 3 direset. Sampah: {currentTrashType}. Siapkan IF-ELSE!";
            feedbackText.color = Color.white;
        }
        
        if (debugMode)
            Debug.Log($"🔄 Level 3 direset (Sampah: {currentTrashType})");
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