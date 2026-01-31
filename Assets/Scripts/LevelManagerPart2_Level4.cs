using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LevelManagerPart2_Level4 : MonoBehaviour
{
    [Header("LEVEL INFO")]
    public int levelNumber = 7;
    public string levelName = "Multiple Conditions";
    
    [Header("REFERENCES")]
    public CommandManagerPart2 commandManager;
    public RobotExecutePart2 robot;
    
    [Header("DUA SAMPAH BERURUTAN")]
    public ConditionalTrash sampahPertama;    // Organik
    public ConditionalTrash sampahKedua;      // Anorganik
    
    [Header("TEMPAT SAMPAH")]
    public TrashBin tongOrganik;      // Posisi kanan
    public TrashBin tongAnorganik;    // Posisi kiri
    
    [Header("UI ELEMENTS")]
    public Text levelTitleText;
    public Text feedbackText;
    public Text instruksiText;
    public GameObject successPanel;
    public GameObject failedPanel;
    
    [Header("SOUND EFFECTS")]
    public AudioSource audioSource;
    public AudioClip successSound;
    public AudioClip failedSound;
    
    [Header("VALIDASI")]
    public bool requireBothIF = true;
    public bool requireCorrectOrder = true;
    
    [Header("DEBUG")]
    public bool debugMode = true;
    
    private bool levelCompleted = false;
    private bool sampah1Dibuang = false;
    private bool sampah2Dibuang = false;
    
    void Start()
    {
        SetupLevel();
    }
    
    void SetupLevel()
    {
        // Setup UI
        if (levelTitleText != null)
            levelTitleText.text = $"Level {levelNumber}: {levelName}";
        
        if (instruksiText != null)
        {
            instruksiText.text = "<b>INSTRUKSI:</b>\n" +
                                "1. Ambil Sampah Organik pertama\n" +
                                "2. Buang ke Tong Organik (kanan)\n" +
                                "3. Ambil Sampah Anorganik kedua\n" +
                                "4. Buang ke Tong Anorganik (kiri)";
        }
        
        if (feedbackText != null)
            feedbackText.text = "Pilah DUA sampah berurutan!";
        
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
        
        if (debugMode)
        {
            Debug.Log($"🎮 Level {levelNumber} siap!");
            Debug.Log($"Target: Pilah 2 sampah berurutan (Organik → Anorganik)");
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
            Debug.Log("🔍 Memulai validasi Level 4...");
        
        // Reset status
        sampah1Dibuang = false;
        sampah2Dibuang = false;
        
        // Simpan status awal sampah
        bool sampah1Awal = sampahPertama.IsCollectable();
        bool sampah2Awal = sampahKedua.IsCollectable();
        
        // Jalankan robot
        robot.ExecuteCommands();
        
        // Tunggu robot selesai
        while (robot.IsExecuting())
        {
            yield return null;
        }
        
        yield return new WaitForSeconds(1f);
        
        // Update status
        sampah1Dibuang = !sampahPertama.IsCollectable();
        sampah2Dibuang = !sampahKedua.IsCollectable();
        
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
            Debug.Log("📋 COMMAND ARRAY LEVEL 4:");
            for (int i = 0; i < commands.Length; i++)
            {
                Debug.Log($"  [{i}] {commands[i]}");
            }
        }
        
        // 1. Validasi struktur algoritma
        bool algorithmValid = ValidateAlgorithmStructure(commands);
        
        // 2. Validasi urutan
        bool orderValid = ValidateCommandOrder(commands);
        
        // 3. Validasi hasil
        bool resultValid = ValidateResult();
        
        if (debugMode)
            Debug.Log($"📊 Validasi: Algoritma={algorithmValid}, Urutan={orderValid}, Hasil={resultValid}");
        
        return algorithmValid && orderValid && resultValid;
    }
    
    bool ValidateAlgorithmStructure(string[] commands)
    {
        bool hasIFOrganik = false;
        bool hasIFAnorganik = false;
        bool hasTwoAmbil = false;
        bool hasTwoBuang = false;
        
        int ambilCount = 0;
        int buangCount = 0;
        int ifOrganikIndex = -1;
        int ifAnorganikIndex = -1;
        
        for (int i = 0; i < commands.Length; i++)
        {
            string cmd = commands[i];
            
            if (cmd == "IF_ORGANIK")
            {
                hasIFOrganik = true;
                ifOrganikIndex = i;
            }
            else if (cmd == "IF_ANORGANIK")
            {
                hasIFAnorganik = true;
                ifAnorganikIndex = i;
            }
            else if (cmd == "AmbilSampah")
            {
                ambilCount++;
            }
            else if (cmd == "BuangSampah")
            {
                buangCount++;
            }
        }
        
        hasTwoAmbil = (ambilCount >= 2);
        hasTwoBuang = (buangCount >= 2);
        
        // Tampilkan analisis jika debug mode aktif
        if (debugMode)
        {
            Debug.Log($"📊 Struktur Algoritma:");
            Debug.Log($"- IF_ORGANIK: {hasIFOrganik} (index: {ifOrganikIndex})");
            Debug.Log($"- IF_ANORGANIK: {hasIFAnorganik} (index: {ifAnorganikIndex})");
            Debug.Log($"- AmbilSampah: {ambilCount}x");
            Debug.Log($"- BuangSampah: {buangCount}x");
        }
        
        // Validasi Level 4 requirements
        if (requireBothIF && (!hasIFOrganik || !hasIFAnorganik))
        {
            ShowMessage("Gunakan kedua IF (Organik DAN Anorganik)!", Color.red);
            return false;
        }
        
        if (!hasTwoAmbil)
        {
            ShowMessage("Ambil kedua sampah!", Color.red);
            return false;
        }
        
        if (!hasTwoBuang)
        {
            ShowMessage("Buang kedua sampah!", Color.red);
            return false;
        }
        
        // Validasi urutan IF (IF_ORGANIK harus sebelum IF_ANORGANIK)
        if (requireCorrectOrder && hasIFOrganik && hasIFAnorganik)
        {
            if (ifAnorganikIndex < ifOrganikIndex)
            {
                ShowMessage("IF_ORGANIK harus SEBELUM IF_ANORGANIK!", Color.red);
                return false;
            }
        }
        
        return true;
    }
    
    bool ValidateCommandOrder(string[] commands)
    {
        // Validasi urutan yang logis untuk dua sampah
        
        int ambil1Index = -1;
        int ambil2Index = -1;
        int buang1Index = -1;
        int buang2Index = -1;
        int ifOrganikIndex = -1;
        int ifAnorganikIndex = -1;
        
        int ambilCount = 0;
        
        // Catat semua index penting
        for (int i = 0; i < commands.Length; i++)
        {
            switch (commands[i])
            {
                case "AmbilSampah":
                    if (ambilCount == 0)
                        ambil1Index = i;
                    else
                        ambil2Index = i;
                    ambilCount++;
                    break;
                    
                case "BuangSampah":
                    if (buang1Index == -1)
                        buang1Index = i;
                    else
                        buang2Index = i;
                    break;
                    
                case "IF_ORGANIK":
                    ifOrganikIndex = i;
                    break;
                    
                case "IF_ANORGANIK":
                    ifAnorganikIndex = i;
                    break;
            }
        }
        
        if (debugMode)
        {
            Debug.Log($"📊 Urutan Command:");
            Debug.Log($"- Ambil1: {ambil1Index}, Ambil2: {ambil2Index}");
            Debug.Log($"- Buang1: {buang1Index}, Buang2: {buang2Index}");
            Debug.Log($"- IF_Organik: {ifOrganikIndex}, IF_Anorganik: {ifAnorganikIndex}");
        }
        
        // Validasi urutan dasar
        bool validOrder = true;
        
        // 1. Ambil pertama harus sebelum buang pertama
        if (ambil1Index > buang1Index && buang1Index != -1)
        {
            ShowMessage("Ambil sampah sebelum membuang!", Color.red);
            validOrder = false;
        }
        
        // 2. Ambil kedua harus sebelum buang kedua
        if (ambil2Index > buang2Index && ambil2Index != -1 && buang2Index != -1)
        {
            ShowMessage("Ambil sampah kedua sebelum membuangnya!", Color.red);
            validOrder = false;
        }
        
        // 3. Buang pertama harus sebelum ambil kedua
        if (buang1Index > ambil2Index && ambil2Index != -1)
        {
            ShowMessage("Buang sampah pertama sebelum ambil kedua!", Color.red);
            validOrder = false;
        }
        
        // 4. IF_ORGANIK harus dekat dengan buang pertama
        if (ifOrganikIndex != -1 && buang1Index != -1)
        {
            // Cek apakah BuangSampah ada dalam blok IF_ORGANIK
            bool buangDalamIFOrganik = false;
            int endifIndex = FindMatchingEndIf(commands, ifOrganikIndex);
            
            for (int i = ifOrganikIndex + 1; i < endifIndex; i++)
            {
                if (commands[i] == "BuangSampah")
                {
                    buangDalamIFOrganik = true;
                    break;
                }
            }
            
            if (!buangDalamIFOrganik)
            {
                ShowMessage("Buang sampah pertama dalam blok IF_ORGANIK!", Color.red);
                validOrder = false;
            }
        }
        
        // 5. IF_ANORGANIK harus dekat dengan buang kedua
        if (ifAnorganikIndex != -1 && buang2Index != -1)
        {
            // Cek apakah BuangSampah ada dalam blok IF_ANORGANIK
            bool buangDalamIFAnorganik = false;
            int endifIndex = FindMatchingEndIf(commands, ifAnorganikIndex);
            
            for (int i = ifAnorganikIndex + 1; i < endifIndex; i++)
            {
                if (commands[i] == "BuangSampah")
                {
                    buangDalamIFAnorganik = true;
                    break;
                }
            }
            
            if (!buangDalamIFAnorganik)
            {
                ShowMessage("Buang sampah kedua dalam blok IF_ANORGANIK!", Color.red);
                validOrder = false;
            }
        }
        
        return validOrder;
    }
    
    int FindMatchingEndIf(string[] commands, int startIndex)
    {
        int depth = 0;
        for (int i = startIndex; i < commands.Length; i++)
        {
            if (commands[i] == "IF_ORGANIK" || commands[i] == "IF_ANORGANIK")
            {
                depth++;
            }
            else if (commands[i] == "END_IF")
            {
                depth--;
                if (depth == 0)
                {
                    return i;
                }
            }
        }
        return commands.Length - 1;
    }
    
    bool ValidateResult()
    {
        // Validasi hasil akhir
        bool bothTrashDisposed = sampah1Dibuang && sampah2Dibuang;
        bool robotEmpty = (robot.GetCarriedTrash() == "None");
        
        if (debugMode)
        {
            Debug.Log($"📊 Hasil Level 4:");
            Debug.Log($"- Sampah 1 dibuang: {sampah1Dibuang}");
            Debug.Log($"- Sampah 2 dibuang: {sampah2Dibuang}");
            Debug.Log($"- Robot kosong: {robotEmpty}");
            Debug.Log($"- Kedua sampah terpilah: {bothTrashDisposed}");
        }
        
        // Berikan feedback spesifik
        if (!sampah1Dibuang && !sampah2Dibuang)
        {
            ShowMessage("Ambil dan buang kedua sampah!", Color.red);
        }
        else if (!sampah1Dibuang)
        {
            ShowMessage("Sampah pertama belum dibuang!", Color.red);
        }
        else if (!sampah2Dibuang)
        {
            ShowMessage("Sampah kedua belum dibuang!", Color.red);
        }
        else if (!robotEmpty)
        {
            ShowMessage("Robot masih membawa sampah!", Color.red);
        }
        
        return bothTrashDisposed && robotEmpty;
    }
    
    void LevelSuccess()
    {
        levelCompleted = true;
        
        if (debugMode)
            Debug.Log("🎉 LEVEL 4 BERHASIL!");
        
        if (successPanel != null)
        {
            successPanel.SetActive(true);
        }
        
        ShowMessage("HEBAT! Kamu berhasil memilah dua sampah berurutan!", Color.green);
        
        // Play success sound
        PlaySound(successSound);
        
        // Save progress
        PlayerPrefs.SetInt("LevelPart2", levelNumber);
        PlayerPrefs.Save();
    }
    
    void LevelFailed()
    {
        if (debugMode)
            Debug.Log("❌ LEVEL 4 GAGAL");
        
        if (failedPanel != null)
        {
            failedPanel.SetActive(true);
        }
        
        ShowMessage("Perbaiki algoritma untuk dua sampah!", Color.red);
        
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
        
        // Reset robot
        if (robot != null) robot.ResetRobot();
        
        // Reset sampah
        if (sampahPertama != null)
        {
            sampahPertama.ResetTrash();
            sampah1Dibuang = false;
        }
        
        if (sampahKedua != null)
        {
            sampahKedua.ResetTrash();
            sampah2Dibuang = false;
        }
        
        // Reset command UI
        CommandReset reset = FindObjectOfType<CommandReset>();
        if (reset != null) reset.ResetAllCommands();
        
        // Reset tempat sampah effects
        if (tongOrganik != null) tongOrganik.StopEffects();
        if (tongAnorganik != null) tongAnorganik.StopEffects();
        
        // Close panels
        CloseSuccessPanel();
        CloseFailedPanel();
        
        // Reset feedback
        if (feedbackText != null)
        {
            feedbackText.text = "Level 4 direset. Pilah kedua sampah berurutan!";
            feedbackText.color = Color.white;
        }
        
        if (debugMode)
            Debug.Log("🔄 Level 4 direset");
    }
    
    // DEBUG
    public void PrintLevelStatus()
    {
        Debug.Log($"=== LEVEL 4 STATUS ===");
        Debug.Log($"Completed: {levelCompleted}");
        Debug.Log($"Sampah 1: {sampah1Dibuang}");
        Debug.Log($"Sampah 2: {sampah2Dibuang}");
        Debug.Log($"Robot carrying: {robot.GetCarriedTrash()}");
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