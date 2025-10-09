using UnityEngine;

public class CommandReset : MonoBehaviour
{
    [Header("RESET SETTINGS")]
    public GameObject commandContainer;
    public bool resetOnClick = true;
    public bool playSound = true;
    public AudioClip resetSound;
    
    [Header("VISUAL FEEDBACK")]
    public ParticleSystem resetParticles;
    public float resetAnimationDuration = 0.5f;
    
    private AudioSource audioSource;

    void Start()
    {
        // Get atau add AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && playSound)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    // Method untuk dihubungkan dengan UI Button
    public void ResetAllCommands()
    {
        if (commandContainer == null)
        {
            Debug.LogError("❌ Command Container belum diassign!");
            return;
        }

        StartCoroutine(ResetRoutine());
    }

    private System.Collections.IEnumerator ResetRoutine()
    {
        Debug.Log("🔄 Memulai reset semua command...");

        // Play sound
        if (playSound && resetSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(resetSound);
        }

        // Play particles
        if (resetParticles != null)
        {
            resetParticles.Play();
        }

        int totalDeleted = 0;

        // Loop melalui semua slot
        foreach (Transform slot in commandContainer.transform)
        {
            int childrenInSlot = slot.childCount;
            
            if (childrenInSlot > 0)
            {
                Debug.Log($"🗑️ Menghapus {childrenInSlot} command dari slot {slot.name}");
                
                // Hapus semua child dari slot ini
                for (int i = slot.childCount - 1; i >= 0; i--)
                {
                    Destroy(slot.GetChild(i).gameObject);
                    totalDeleted++;
                }

                // Optional: Beri delay kecil untuk efek visual
                yield return new WaitForSeconds(0.05f);
            }
        }

        Debug.Log($"✅ Reset selesai! {totalDeleted} command dihapus.");

        // Refresh CommandManager jika ada
        RefreshCommandManager();

        // Optional: Beri feedback visual tambahan
        yield return StartCoroutine(PlayResetAnimation());
    }

    private void RefreshCommandManager()
    {
        // Cari dan refresh CommandManager
        CommandManager manager = FindObjectOfType<CommandManager>();
        if (manager != null)
        {
            manager.InitializeCommandArray();
            manager.PrintCommandArray();
        }
        else
        {
            Debug.LogWarning("⚠️ CommandManager tidak ditemukan!");
        }
    }

    private System.Collections.IEnumerator PlayResetAnimation()
    {
        // Animasi sederhana untuk feedback visual
        if (resetAnimationDuration > 0)
        {
            float timer = 0f;
            Vector3 originalScale = transform.localScale;
            
            while (timer < resetAnimationDuration)
            {
                timer += Time.deltaTime;
                float scale = Mathf.PingPong(timer * 2f, 0.2f) + 0.9f;
                transform.localScale = originalScale * scale;
                yield return null;
            }
            
            transform.localScale = originalScale;
        }
    }

    // Method untuk reset tanpa coroutine (lebih sederhana)
    public void ResetAllCommandsInstant()
    {
        if (commandContainer == null)
        {
            Debug.LogError("❌ Command Container belum diassign!");
            return;
        }

        int totalDeleted = 0;

        foreach (Transform slot in commandContainer.transform)
        {
            totalDeleted += slot.childCount;
            
            // Hapus semua child langsung
            foreach (Transform child in slot)
            {
                DestroyImmediate(child.gameObject);
            }
        }

        Debug.Log($"✅ Reset instant! {totalDeleted} command dihapus.");
        RefreshCommandManager();
    }

    // Method untuk debug info
    public void PrintResetInfo()
    {
        if (commandContainer == null) return;

        int totalSlots = commandContainer.transform.childCount;
        int totalCommands = 0;

        foreach (Transform slot in commandContainer.transform)
        {
            totalCommands += slot.childCount;
        }

        Debug.Log($"📊 Reset Info: {totalCommands} command di {totalSlots} slot");
    }

    // Auto-execute ketika button di-click (jika resetOnClick = true)
    public void OnClick()
    {
        if (resetOnClick)
        {
            ResetAllCommands();
        }
    }
}