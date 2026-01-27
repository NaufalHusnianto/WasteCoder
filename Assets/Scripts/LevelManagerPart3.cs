using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class LevelManagerPart3 : MonoBehaviour
{
    [Header("LEVEL INFO")]
    public int levelNumber = 1;
    public string levelName = "Loop Sederhana";
    
    [Header("REFERENCES")]
    public CommandManagerPart3 commandManager;
    public RobotExecutePart3 robot;
    
    [Header("TARGET SETTINGS")]
    public Transform targetPosition; // Posisi target setelah 4 langkah maju
    public float tolerance = 0.5f; // Toleransi posisi
    
    [Header("UI ELEMENTS")]
    public Text levelTitleText;
    public Text feedbackText;
    public Text instruksiText;
    public Text hintText;
    public GameObject successPanel;
    public GameObject failedPanel;
    
    [Header("LEVEL REQUIREMENTS")]
    public int requiredMoveCount = 4; // Harus bergerak 4 langkah
    
    private bool levelCompleted = false;
    private Vector3 robotStartPosition;
    
    void Start()
    {
        SetupLevel();
        SetupTargetPosition(); // Setup target di Start, bukan OnValidate
    }
    
    void SetupLevel()
    {
        // Setup UI
        if (levelTitleText != null)
            levelTitleText.text = $"Level {levelNumber}: {levelName}";
        
        if (instruksiText != null)
        {
            instruksiText.text = "<b>TUJUAN LEVEL:</b>\n" +
                                "Robot harus bergerak 4 langkah maju.\n\n" +
                                "<b>PETUNJUK:</b>\n" +
                                "Gunakan LOOP untuk mengulang command MOVE\n" +
                                "Contoh: LOOP4 [Move] END_LOOP";
        }
        
        if (hintText != null)
        {
            hintText.text = "💡 Tips: LOOP4 akan mengulang command di dalamnya 4 kali";
        }
        
        if (feedbackText != null)
            feedbackText.text = "Susun command dengan LOOP untuk bergerak 4 langkah!";
        
        // Hide panels
        if (successPanel != null) successPanel.SetActive(false);
        if (failedPanel != null) failedPanel.SetActive(false);
        
        // Auto find references
        if (commandManager == null) commandManager = FindObjectOfType<CommandManagerPart3>();
        if (robot == null) robot = FindObjectOfType<RobotExecutePart3>();
        
        Debug.Log($"🎮 Level {levelNumber} Part 3 siap!");
        Debug.Log($"🎯 Target: Bergerak 4 langkah maju menggunakan LOOP");
    }
    
    void SetupTargetPosition()
    {
        // Setup target position jika belum ada
        if (targetPosition == null)
        {
            // Cari di scene
            GameObject targetObj = GameObject.Find("TargetPosition");
            if (targetObj == null)
            {
                // Buat baru jika tidak ditemukan
                targetObj = new GameObject("TargetPosition");
                targetObj.transform.position = CalculateTargetPosition();
            }
            targetPosition = targetObj.transform;
        }
        
        // Pastikan target di posisi yang benar
        if (targetPosition != null)
        {
            targetPosition.position = CalculateTargetPosition();
            Debug.Log($"🎯 Target position set to: {targetPosition.position}");
        }
    }
    
    Vector3 CalculateTargetPosition()
    {
        if (robot != null)
        {
            // Hitung posisi 4 unit di depan robot
            return robot.transform.position + robot.transform.forward * requiredMoveCount;
        }
        
        // Fallback: 4 unit di depan origin
        return Vector3.forward * requiredMoveCount;
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
        Debug.Log("🔍 Memulai validasi Level Part 3...");
        
        // Simpan posisi awal robot
        robotStartPosition = robot.transform.position;
        
        // Update target position berdasarkan posisi awal
        UpdateTargetPosition();
        
        // Jalankan robot
        robot.ExecuteCommands();
        
        // Tunggu robot selesai
        while (robot.IsExecuting())
        {
            yield return null;
        }
        
        yield return new WaitForSeconds(0.5f); // Tunggu sedikit
        
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
    
    void UpdateTargetPosition()
    {
        if (targetPosition != null)
        {
            targetPosition.position = robotStartPosition + robot.transform.forward * requiredMoveCount;
        }
    }
    
    bool ValidateLevel()
    {
        string[] commands = commandManager.GetCommandArray();
        
        // DEBUG: Tampilkan command array
        Debug.Log("📋 COMMAND ARRAY:");
        for (int i = 0; i < commands.Length; i++)
        {
            Debug.Log($"  [{i}] {commands[i]}");
        }
        
        // 1. Validasi apakah menggunakan LOOP
        bool hasLoop = HasLoopCommand(commands);
        
        // 2. Validasi jumlah langkah yang benar
        bool hasCorrectSteps = ValidateStepCount(commands);
        
        // 3. Validasi posisi akhir robot
        bool reachedTarget = ValidateFinalPosition();
        
        // 4. Validasi struktur LOOP sederhana
        bool validStructure = ValidateLoopStructure(commands);
        
        Debug.Log($"📊 Validasi: Loop={hasLoop}, Steps={hasCorrectSteps}, Target={reachedTarget}, Structure={validStructure}");
        
        // Berikan feedback spesifik
        if (!hasLoop)
        {
            ShowMessage("Gunakan command LOOP!", Color.red);
            if (hintText != null) 
                hintText.text = "💡 Drag & drop LOOP2, LOOP3, atau LOOP4";
        }
        else if (!validStructure)
        {
            ShowMessage("LOOP harus ditutup dengan END_LOOP!", Color.red);
            if (hintText != null) 
                hintText.text = "💡 Pastikan ada END_LOOP setelah command";
        }
        else if (!hasCorrectSteps)
        {
            ShowMessage($"Harus bergerak tepat {requiredMoveCount} langkah!", Color.red);
            if (hintText != null) 
                hintText.text = $"💡 Gunakan LOOP{requiredMoveCount} [Move] END_LOOP";
        }
        else if (!reachedTarget)
        {
            ShowMessage("Robot tidak sampai di target!", Color.red);
            if (hintText != null) 
                hintText.text = "💡 Pastikan robot menghadap ke arah yang benar";
        }
        
        return hasLoop && validStructure && hasCorrectSteps && reachedTarget;
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
    
    bool ValidateStepCount(string[] commands)
    {
        int totalMoveCount = CalculateTotalMoves(commands);
        
        Debug.Log($"📊 Jumlah langkah: {totalMoveCount} (dibutuhkan: {requiredMoveCount})");
        
        return totalMoveCount == requiredMoveCount;
    }
    
    int CalculateTotalMoves(string[] commands)
    {
        int totalMoves = 0;
        int currentLoopMultiplier = 1;
        Stack<int> loopMultipliers = new Stack<int>();
        
        for (int i = 0; i < commands.Length; i++)
        {
            string cmd = commands[i];
            
            if (cmd == "Move")
            {
                totalMoves += currentLoopMultiplier;
                Debug.Log($"   ➕ Move * {currentLoopMultiplier} = +{currentLoopMultiplier}");
            }
            else if (cmd.StartsWith("LOOP"))
            {
                int loopCount = GetLoopCount(cmd);
                Debug.Log($"   🔄 LOOP {loopCount}x (multiplier: {currentLoopMultiplier} → {currentLoopMultiplier * loopCount})");
                
                loopMultipliers.Push(currentLoopMultiplier);
                currentLoopMultiplier *= loopCount;
            }
            else if (cmd == "END_LOOP")
            {
                if (loopMultipliers.Count > 0)
                {
                    int previousMultiplier = loopMultipliers.Pop();
                    Debug.Log($"   ✅ END_LOOP (multiplier: {currentLoopMultiplier} → {previousMultiplier})");
                    currentLoopMultiplier = previousMultiplier;
                }
            }
        }
        
        return totalMoves;
    }
    
    int GetLoopCount(string loopCommand)
    {
        if (loopCommand == "LOOP2") return 2;
        if (loopCommand == "LOOP3") return 3;
        if (loopCommand == "LOOP4") return 4;
        
        // Coba parse angka
        string numStr = loopCommand.Replace("LOOP", "");
        if (int.TryParse(numStr, out int count))
            return count;
            
        return 1; // Default
    }
    
    bool ValidateLoopStructure(string[] commands)
    {
        int loopOpen = 0;
        int loopClose = 0;
        
        foreach (string cmd in commands)
        {
            if (cmd.StartsWith("LOOP"))
            {
                loopOpen++;
            }
            else if (cmd == "END_LOOP")
            {
                loopClose++;
                
                // Cek jika END_LOOP tanpa LOOP
                if (loopClose > loopOpen)
                {
                    return false;
                }
            }
        }
        
        // LOOP dan END_LOOP harus seimbang
        return loopOpen == loopClose;
    }
    
    bool ValidateFinalPosition()
    {
        if (targetPosition == null)
        {
            // Jika tidak ada target position, validasi berdasarkan langkah
            float distanceMoved = Vector3.Distance(robotStartPosition, robot.transform.position);
            float expectedDistance = requiredMoveCount * 1.0f; // Asumsi moveDistance = 1
            bool isCorrect = Mathf.Abs(distanceMoved - expectedDistance) < tolerance;
            
            Debug.Log($"📏 Jarak ditempuh: {distanceMoved:F2} (diharapkan: {expectedDistance})");
            return isCorrect;
        }
        
        // Validasi dengan target position
        float distanceToTarget = Vector3.Distance(robot.transform.position, targetPosition.position);
        Debug.Log($"🎯 Jarak ke target: {distanceToTarget:F2} (toleransi: {tolerance})");
        
        return distanceToTarget <= tolerance;
    }
    
    void LevelSuccess()
    {
        levelCompleted = true;
        Debug.Log("🎉 LEVEL PART 3 BERHASIL!");
        
        if (successPanel != null)
        {
            successPanel.SetActive(true);
        }
        
        ShowMessage("SUKSES! Robot berhasil bergerak 4 langkah menggunakan LOOP!", Color.green);
        
        if (hintText != null)
        {
            hintText.text = "🎉 Hebat! Kamu menguasai dasar LOOP!";
        }
        
        // Save progress
        PlayerPrefs.SetInt("LevelPart3", levelNumber);
        PlayerPrefs.Save();
        
        // Visual feedback
        StartCoroutine(PlaySuccessEffects());
    }
    
    void LevelFailed()
    {
        Debug.Log("❌ LEVEL PART 3 GAGAL");
        
        if (failedPanel != null)
        {
            failedPanel.SetActive(true);
        }
        
        ShowMessage("Coba lagi! Periksa algoritma LOOP-mu", Color.red);
        
        // Berikan hint berdasarkan kesalahan
        ProvideSpecificHint();
    }
    
    void ProvideSpecificHint()
    {
        string[] commands = commandManager.GetCommandArray();
        
        // Analisis kesalahan
        int totalMoves = CalculateTotalMoves(commands);
        bool hasLoop = HasLoopCommand(commands);
        bool hasEndLoop = false;
        
        foreach (string cmd in commands)
        {
            if (cmd == "END_LOOP") 
            {
                hasEndLoop = true;
                break;
            }
        }
        
        if (!hasLoop)
        {
            if (hintText != null)
                hintText.text = "💡 Coba: LOOP4 [Move] END_LOOP";
        }
        else if (!hasEndLoop)
        {
            if (hintText != null)
                hintText.text = "💡 Jangan lupa END_LOOP!";
        }
        else if (totalMoves < requiredMoveCount)
        {
            if (hintText != null)
                hintText.text = $"💡 Butuh {requiredMoveCount} langkah. Coba LOOP{requiredMoveCount}";
        }
        else if (totalMoves > requiredMoveCount)
        {
            if (hintText != null)
                hintText.text = $"💡 Terlalu banyak langkah. Cek LOOP-mu";
        }
    }
    
    IEnumerator PlaySuccessEffects()
    {
        // Animasi text
        if (feedbackText != null)
        {
            Vector3 originalScale = feedbackText.transform.localScale;
            float timer = 0f;
            
            while (timer < 1f)
            {
                timer += Time.deltaTime;
                float scale = 1f + Mathf.Sin(timer * 10f) * 0.1f;
                feedbackText.transform.localScale = originalScale * scale;
                yield return null;
            }
            
            feedbackText.transform.localScale = originalScale;
        }
        
        // Highlight target position
        if (targetPosition != null)
        {
            Renderer renderer = targetPosition.GetComponent<Renderer>();
            if (renderer != null)
            {
                Color originalColor = renderer.material.color;
                renderer.material.color = Color.green;
                yield return new WaitForSeconds(0.5f);
                renderer.material.color = originalColor;
            }
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
            else
            {
                // Fallback: cari komponen dengan method ResetRobot
                MonoBehaviour[] scripts = robot.GetComponents<MonoBehaviour>();
                foreach (var script in scripts)
                {
                    if (script.GetType().GetMethod("ResetRobot") != null)
                    {
                        script.GetType().GetMethod("ResetRobot").Invoke(script, null);
                        break;
                    }
                }
            }
        }
        
        // Reset command UI
        CommandReset reset = FindObjectOfType<CommandReset>();
        if (reset != null) 
            reset.ResetAllCommands();
        
        // Close panels
        CloseSuccessPanel();
        CloseFailedPanel();
        
        // Reset feedback
        if (feedbackText != null)
        {
            feedbackText.text = "Level direset. Coba susun algoritma LOOP!";
            feedbackText.color = Color.white;
        }
        
        if (hintText != null)
        {
            hintText.text = "💡 Tips: LOOP4 [Move] END_LOOP = 4 langkah maju";
        }
        
        Debug.Log("🔄 Level Part 3 direset");
    }
    
    // DEBUG FUNCTIONS
    public void PrintCommandAnalysis()
    {
        if (commandManager == null) return;
        
        string[] commands = commandManager.GetCommandArray();
        
        Debug.Log("=== ANALISIS COMMAND ===");
        Debug.Log($"Total commands: {commands.Length}");
        
        int loopCount = 0;
        int moveCount = 0;
        int endLoopCount = 0;
        
        foreach (string cmd in commands)
        {
            if (cmd.StartsWith("LOOP")) loopCount++;
            if (cmd == "Move") moveCount++;
            if (cmd == "END_LOOP") endLoopCount++;
        }
        
        Debug.Log($"Loop: {loopCount}, Move: {moveCount}, EndLoop: {endLoopCount}");
        Debug.Log($"Total moves calculated: {CalculateTotalMoves(commands)}");
    }
    
    // Manual setup target (bisa dipanggil dari editor)
    [ContextMenu("Setup Target Position")]
    public void ManualSetupTarget()
    {
        if (robot == null) 
        {
            robot = FindObjectOfType<RobotExecutePart3>();
        }
        
        if (targetPosition == null)
        {
            GameObject targetObj = GameObject.Find("TargetPosition");
            if (targetObj == null)
            {
                targetObj = new GameObject("TargetPosition");
            }
            targetPosition = targetObj.transform;
        }
        
        if (robot != null && targetPosition != null)
        {
            targetPosition.position = robot.transform.position + robot.transform.forward * requiredMoveCount;
            Debug.Log($"🎯 Target position manually set to: {targetPosition.position}");
        }
    }
    
    // Visual debug di Scene view (hanya di Editor)
    #if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        
        if (targetPosition != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(targetPosition.position, 0.3f);
            
            // Gambar garis dari start ke target
            if (robot != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(robotStartPosition, targetPosition.position);
            }
        }
    }
    #endif
}