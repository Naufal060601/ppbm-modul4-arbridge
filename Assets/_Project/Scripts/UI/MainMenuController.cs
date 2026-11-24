using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("Tombol")]
    [SerializeField] private Button btnStart;
    [SerializeField] private Button btnContinue;
    [SerializeField] private Button btnAbout;
    [SerializeField] private Button btnCloseAbout;

    [Header("Teks")]
    [SerializeField] private TMP_Text txtPoints;

    [Header("Panel")]
    [SerializeField] private GameObject aboutPanel;

    private void OnEnable()
    {
        if (btnStart != null) btnStart.onClick.AddListener(HandleStart);
        if (btnContinue != null) btnContinue.onClick.AddListener(HandleContinue);
        if (btnAbout != null) btnAbout.onClick.AddListener(HandleAbout);
        if (btnCloseAbout != null) btnCloseAbout.onClick.AddListener(CloseAbout);
    }

    private void OnDisable()
    {
        if (btnStart != null) btnStart.onClick.RemoveListener(HandleStart);
        if (btnContinue != null) btnContinue.onClick.RemoveListener(HandleContinue);
        if (btnAbout != null) btnAbout.onClick.RemoveListener(HandleAbout);
        if (btnCloseAbout != null) btnCloseAbout.onClick.RemoveListener(CloseAbout);
    }

    private void Start()
    {
        if (txtPoints != null)
        {
            txtPoints.text = "Poin: 0";
        }

        if (btnContinue != null)
        {
            btnContinue.interactable = false;
        }

        if (aboutPanel != null)
        {
            aboutPanel.SetActive(false);
        }
    }

    private void HandleStart()
    {
        Debug.Log("[Menu] Tombol Mulai ditekan.");
    }

    private void HandleContinue()
    {
        Debug.Log("[Menu] Tombol Lanjutkan ditekan.");
    }

    private void HandleAbout()
    {
        if (aboutPanel != null) aboutPanel.SetActive(true);
        Debug.Log("[Menu] Panel Tentang dibuka.");
    }

    private void CloseAbout()
    {
        if (aboutPanel != null) aboutPanel.SetActive(false);
        Debug.Log("[Menu] Panel Tentang ditutup.");
    }
}