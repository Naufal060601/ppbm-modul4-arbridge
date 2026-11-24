using System.Collections;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;

/// <summary>
/// Memuat scene AR secara aditif, menemukan implementasi IARModule
/// di dalamnya, dan menghubungkan event-nya ke aplikasi utama.
/// </summary>
public class ARBridge : MonoBehaviour
{
    [Header("Konfigurasi")]
    [SerializeField] private string arSceneName = "ARStage";
    [SerializeField] private GameObject mainUIRoot;
    [SerializeField] private GameObject cancelButtonRoot; // Seret Btn_CancelAR ke sini
    [SerializeField] private Camera mainCamera;

    [Header("Pengujian Memori")]
    [SerializeField] private bool enableUnloadUnusedAssets = true;

    private IARModule _module;
    private bool _isLoaded;
    private bool _isClosing;
    private int _cycleCounter;

    public bool IsLoaded => _isLoaded;

    private void Awake()
    {
        // Pastikan tombol Batal tersembunyi saat aplikasi pertama kali jalan
        if (cancelButtonRoot != null)
            cancelButtonRoot.SetActive(false);
    }

    public void OpenAR(string storyId)
    {
        if (_isLoaded || _isClosing)
        {
            Debug.LogWarning("[ARBridge] Sesi AR sedang berjalan atau ditutup, permintaan diabaikan.");
            return;
        }
        StartCoroutine(OpenRoutine(storyId));
    }

    public void CloseAR()
    {
        if (!_isLoaded || _isClosing) return;
        StartCoroutine(CloseRoutine());
    }

    private IEnumerator OpenRoutine(string storyId)
    {
        _cycleCounter++;

        // 1. Memuat scene AR di ATAS scene sekarang.
        AsyncOperation op = SceneManager.LoadSceneAsync(arSceneName, LoadSceneMode.Additive);
        yield return op;

        Scene arScene = SceneManager.GetSceneByName(arSceneName);
        if (!arScene.IsValid())
        {
            Debug.LogError("[ARBridge] Scene AR tidak valid. " +
                           "Pastikan scene terdaftar di Build Settings.");
            yield break;
        }

        // 2. Mencari komponen yang memenuhi kontrak di dalam scene AR.
        foreach (GameObject root in arScene.GetRootGameObjects())
        {
            _module = root.GetComponentInChildren<IARModule>(true);
            if (_module != null) break;
        }

        if (_module == null)
        {
            Debug.LogError("[ARBridge] Tidak ditemukan implementasi IARModule. " +
                           "Periksa kembali kontrak dengan tim AR.");
            yield return SceneManager.UnloadSceneAsync(arSceneName);
            yield break;
        }

        // 3. Berlangganan event modul.
        _module.OnObjectiveCompleted += HandleObjectiveCompleted;
        _module.OnSessionError += HandleSessionError;

        // 4. Menyembunyikan UI utama, mematikan kamera utama, dan menampilkan tombol Batal.
        if (mainUIRoot != null) mainUIRoot.SetActive(false);
        if (mainCamera != null) mainCamera.enabled = false;
        if (cancelButtonRoot != null) cancelButtonRoot.SetActive(true);

        // 5. Menjalankan sesi.
        _module.Initialize(storyId);
        _module.StartSession();

        _isLoaded = true;
        LogAllocatedMemory($"Siklus {_cycleCounter} - Setelah OpenAR");
        Debug.Log("[ARBridge] Modul AR aktif secara aditif.");
    }

    private IEnumerator CloseRoutine()
    {
        _isClosing = true;

        // Sembunyikan tombol Batal segera saat proses penutupan dimulai
        if (cancelButtonRoot != null)
            cancelButtonRoot.SetActive(false);

        // Urutan dibalik dari OpenRoutine: hentikan dulu, lepas terakhir.
        if (_module != null)
        {
            _module.StopSession();
            _module.OnObjectiveCompleted -= HandleObjectiveCompleted;
            _module.OnSessionError -= HandleSessionError;
            _module = null;
        }

        Scene arScene = SceneManager.GetSceneByName(arSceneName);
        if (arScene.IsValid() && arScene.isLoaded)
        {
            yield return SceneManager.UnloadSceneAsync(arSceneName);
        }

        // Membersihkan aset yang tidak lagi dirujuk siapa pun (bisa di-toggle lewat Inspector).
        if (enableUnloadUnusedAssets)
        {
            yield return Resources.UnloadUnusedAssets();
        }

        if (mainUIRoot != null) mainUIRoot.SetActive(true);
        if (mainCamera != null) mainCamera.enabled = true;

        _isLoaded = false;
        _isClosing = false;
        LogAllocatedMemory($"Siklus {_cycleCounter} - Setelah CloseAR");
        Debug.Log("[ARBridge] Modul AR dilepas.");
    }

    private void LogAllocatedMemory(string label)
    {
        long bytes = Profiler.GetTotalAllocatedMemoryLong();
        float mb = bytes / (1024f * 1024f);
        string mode = enableUnloadUnusedAssets ? "UnloadUnusedAssets: ON" : "UnloadUnusedAssets: OFF";
        Debug.Log($"[ARBridge][Memori] {label} ({mode}) -> {mb:F2} MB ({bytes} bytes)");
    }

    private void HandleObjectiveCompleted(string objectiveId, int points)
    {
        Debug.Log($"[ARBridge] Objektif {objectiveId} selesai, +{points} poin.");
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddPoints(points);
        }
        CloseAR();
    }

    private void HandleSessionError(string message)
    {
        Debug.LogWarning($"[ARBridge] Kesalahan sesi AR: {message}");
        CloseAR();
    }
}