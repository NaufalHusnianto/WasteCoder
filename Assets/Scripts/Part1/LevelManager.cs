using UnityEngine;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    [Header("LEVEL SETTINGS")]
    public int levelNumber = 1;
    public int requiredForwardCommands = 5;
    
    [Header("REFERENCES")]
    public CommandManager commandManager;
    public CommandExecute commandExecute;
    
    [Header("UI POPUPS")]
    public GameObject successPopup;
    public GameObject failedPopup;
    public Text successText;
    public Text failedText;
    
    [Header("AUDIO")]
    public AudioClip successSound;
    public AudioClip failedSound;
    private AudioSource audioSource;
    
    [Header("DEBUG")]
    public bool debugMode = true;
    
    [Header("DIM LAYER")]
    public GameObject dimLayer; // GameObject untuk membuat layar gelap
    public float dimAlpha = 0.7f; // Tingkat kegelapan (0-1)
    
    private bool isLevelCompleted = false;
    private bool isChecking = false;
    
    void Start()
    {
        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Setup dim layer jika belum ada
        if (dimLayer == null)
        {
            CreateDimLayer();
        }
        
        // Hide popups dan dim layer initially
        if (successPopup != null) successPopup.SetActive(false);
        if (failedPopup != null) failedPopup.SetActive(false);
        if (dimLayer != null) dimLayer.SetActive(false);
        
        // Setup UI text
        if (successText != null) 
            successText.text = $"Level {levelNumber} Selesai!\nRobot berhasil bergerak {requiredForwardCommands} langkah!";
        
        if (failedText != null)
            failedText.text = $"Level {levelNumber} Gagal!\nDibutuhkan tepat {requiredForwardCommands} command \"Move\"";
    }
    
    // Method utama - Jalankan robot DULU, baru validasi SETELAH selesai
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
        
        // Mulai coroutine untuk eksekusi dan validasi
        StartCoroutine(ExecuteAndValidateCoroutine());
    }
    
    private System.Collections.IEnumerator ExecuteAndValidateCoroutine()
    {
        isChecking = true;
        
        if (debugMode) Debug.Log("🚀 Memulai eksekusi robot...");
        
        // Simpan posisi awal robot
        Vector3 startPosition = commandExecute.transform.position;
        Quaternion startRotation = commandExecute.transform.rotation;
        
        // Jalankan semua command
        commandExecute.ExecuteAllCommands();
        
        // TUNGGU sampai robot selesai bergerak
        while (commandExecute.IsExecuting())
        {
            yield return null;
        }
        
        if (debugMode) Debug.Log("✅ Robot selesai bergerak");
        
        // Tunggu 0.5 detik untuk efek visual
        yield return new WaitForSeconds(0.5f);
        
        // 1. Validasi apakah robot berhasil menyelesaikan level
        // Untuk level 1: Validasi posisi akhir robot (5 langkah ke depan)
        bool levelSuccess = ValidateRobotPosition(startPosition);
        
        // 2. Tampilkan hasil berdasarkan validasi
        if (levelSuccess)
        {
            ShowLevelSuccess();
        }
        else
        {
            ShowLevelFailed();
        }
        
        // Reset robot ke posisi awal setelah 2 detik
        yield return new WaitForSeconds(2f);
        
        if (commandExecute != null)
        {
            commandExecute.transform.position = startPosition;
            commandExecute.transform.rotation = startRotation;
        }
        
        isChecking = false;
    }
    
    // Validasi posisi robot setelah eksekusi
    private bool ValidateRobotPosition(Vector3 startPosition)
    {
        Vector3 endPosition = commandExecute.transform.position;
        
        // Hitung jarak yang ditempuh robot
        float distanceTravelled = Vector3.Distance(startPosition, endPosition);
        
        // Untuk level 1: robot harus bergerak tepat 5 langkah ke depan
        float expectedDistance = 5 * commandExecute.moveDistance;
        
        // Toleransi kecil untuk floating point errors
        float tolerance = 0.1f;
        
        if (debugMode)
        {
            Debug.Log($"Validasi Posisi Robot:");
            Debug.Log($"- Posisi awal: {startPosition}");
            Debug.Log($"- Posisi akhir: {endPosition}");
            Debug.Log($"- Jarak tempuh: {distanceTravelled}");
            Debug.Log($"- Jarak yang diharapkan: {expectedDistance}");
        }
        
        return Mathf.Abs(distanceTravelled - expectedDistance) < tolerance;
    }
    
    private void ShowLevelSuccess()
    {
        if (debugMode) Debug.Log("🎉 LEVEL BERHASIL!");
        
        isLevelCompleted = true;
        
        // Aktifkan dim layer
        if (dimLayer != null)
        {
            dimLayer.SetActive(true);
        }
        
        // Show success popup
        if (successPopup != null)
        {
            successPopup.SetActive(true);
            // TIDAK ADA AUTOHIDE - popup akan tetap terbuka
        }
        
        // Play sound
        if (successSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(successSound);
        }
        
        // Save level progress
        PlayerPrefs.SetInt("LevelCompleted", levelNumber);
        PlayerPrefs.Save();
    }
    
    private void ShowLevelFailed()
    {
        if (debugMode) Debug.Log("❌ LEVEL GAGAL!");
        
        // Aktifkan dim layer
        if (dimLayer != null)
        {
            dimLayer.SetActive(true);
        }
        
        // Show failed popup
        if (failedPopup != null)
        {
            failedPopup.SetActive(true);
            // TIDAK ADA AUTOHIDE - popup akan tetap terbuka
        }
        
        // Play sound
        if (failedSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(failedSound);
        }
    }
    
    // Method untuk menutup popup (bisa dipanggil dari button di popup)
    public void CloseSuccessPopup()
    {
        if (successPopup != null)
            successPopup.SetActive(false);
        
        // Nonaktifkan dim layer
        if (dimLayer != null)
        {
            dimLayer.SetActive(false);
        }
    }
    
    public void CloseFailedPopup()
    {
        if (failedPopup != null)
            failedPopup.SetActive(false);
        
        // Nonaktifkan dim layer
        if (dimLayer != null)
        {
            dimLayer.SetActive(false);
        }
    }
    
    // Reset level untuk mencoba lagi
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
        
        // Reset robot
        if (commandExecute != null)
        {
            commandExecute.ResetRobot();
        }
        
        // Close popups
        CloseSuccessPopup();
        CloseFailedPopup();
        
        if (debugMode) Debug.Log("🔄 Level direset");
    }
    
    // Untuk debug info
    public void PrintLevelInfo()
    {
        if (commandManager == null) return;
        
        commandManager.InitializeCommandArray();
        string[] commands = commandManager.GetCommandArray();
        
        Debug.Log($"📊 LEVEL {levelNumber} INFO:");
        Debug.Log($"Dibutuhkan: {requiredForwardCommands} command Move");
        
        int forwardCount = 0;
        int totalCommands = 0;
        
        for (int i = 0; i < commands.Length; i++)
        {
            if (commands[i] != "Empty")
            {
                totalCommands++;
                if (commands[i] == "Move")
                {
                    forwardCount++;
                    Debug.Log($"✅ Slot {i+1}: Move");
                }
                else
                {
                    Debug.Log($"❌ Slot {i+1}: {commands[i]} (salah)");
                }
            }
        }
        
        Debug.Log($"Total Move: {forwardCount}/{requiredForwardCommands}");
        Debug.Log($"Total Command: {totalCommands}");
        
        bool isValid = (forwardCount == requiredForwardCommands && totalCommands == requiredForwardCommands);
        Debug.Log($"Status: {(isValid ? "VALID" : "TIDAK VALID")}");
    }
    
    // Method untuk membuat dim layer secara otomatis jika belum ada
    private void CreateDimLayer()
    {
        // Cari Canvas utama
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;
        
        // Buat GameObject untuk dim layer
        dimLayer = new GameObject("DimLayer");
        dimLayer.transform.SetParent(canvas.transform, false);
        
        // Setup RectTransform
        RectTransform rectTransform = dimLayer.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.localScale = Vector3.one;
        
        // Tambahkan Image component
        Image image = dimLayer.AddComponent<Image>();
        image.color = new Color(0, 0, 0, dimAlpha);
        
        // Pastikan dim layer di belakang popup
        dimLayer.transform.SetAsFirstSibling();
        
        // Nonaktifkan raycast target agar tidak menghalangi klik ke popup
        image.raycastTarget = false;
    }
    
    // Method untuk mengatur opacity dim layer
    public void SetDimOpacity(float opacity)
    {
        dimAlpha = Mathf.Clamp01(opacity);
        
        if (dimLayer != null)
        {
            Image image = dimLayer.GetComponent<Image>();
            if (image != null)
            {
                image.color = new Color(0, 0, 0, dimAlpha);
            }
        }
    }
}