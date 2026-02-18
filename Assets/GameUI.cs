using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUI : MonoBehaviour
{
    public GameObject startPanel;
    public GameObject gameplayUI;
    public GameObject gameOverPanel;

    // ? ESC ÀÏ½ÃÁ¤Áö ÆÐ³Î
    public GameObject pausePanel;

    public Slider hpBar;
    public TMP_Text timeText;
    public TMP_Text killText;

    [Header("Infima HUD")]
    public string assetHudNameContains = "P_LPSP_UI_Canvas";
    GameObject assetHudRoot;

    void Start()
    {
        ShowStart();
        ShowPause(false);     // ? ½ÃÀÛÇÒ ¶© PausePanel ¼û±è
        SetAssetHud(false);
    }

    public void ShowStart()
    {
        startPanel.SetActive(true);
        gameplayUI.SetActive(false);
        gameOverPanel.SetActive(false);
        ShowPause(false);     // ? È¤½Ã ÄÑÁ®ÀÖÀ¸¸é ¼û±è
    }

    public void ShowGameplay()
    {
        startPanel.SetActive(false);
        gameplayUI.SetActive(true);
        gameOverPanel.SetActive(false);
        ShowPause(false);     // ? °ÔÀÓ Áß¿¡´Â Pause ¼û±è
    }

    public void ShowGameOver(float time, int kills)
    {
        startPanel.SetActive(false);
        gameplayUI.SetActive(false);
        gameOverPanel.SetActive(true);
        ShowPause(false);     // ? °ÔÀÓ¿À¹ö¸é Pause ¼û±è

        timeText.text = $"Time: {time:F1}s";
        killText.text = $"Kill: {kills}";
    }

    // ? PausePanel ÄÑ°í ²ô±â (GameManager°¡ È£ÃâÇÔ)
    public void ShowPause(bool show)
    {
        if (pausePanel != null)
            pausePanel.SetActive(show);
    }

    public void UpdateHP(int cur, int max)
    {
        hpBar.maxValue = max;
        hpBar.value = cur;
    }

    void FindAssetHudIfNeeded()
    {
        if (assetHudRoot != null) return;

        var canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (var c in canvases)
        {
            if (c.gameObject.name.Contains(assetHudNameContains))
            {
                assetHudRoot = c.gameObject;
                break;
            }
        }
    }

    public void SetAssetHud(bool on)
    {
        FindAssetHudIfNeeded();
        if (assetHudRoot != null)
            assetHudRoot.SetActive(on);
    }
}
