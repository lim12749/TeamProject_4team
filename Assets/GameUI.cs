using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUI : MonoBehaviour
{
    public GameObject startPanel;
    public GameObject gameplayUI;
    public GameObject gameOverPanel;

    public Slider hpBar;
    public TMP_Text timeText;
    public TMP_Text killText;

    [Header("Infima HUD")]
    public string assetHudNameContains = "P_LPSP_UI_Canvas";
    GameObject assetHudRoot;

    void Start()
    {
        ShowStart();
        SetAssetHud(false);
    }

    public void ShowStart()
    {
        startPanel.SetActive(true);
        gameplayUI.SetActive(false);
        gameOverPanel.SetActive(false);
    }

    public void ShowGameplay()
    {
        startPanel.SetActive(false);
        gameplayUI.SetActive(true);
        gameOverPanel.SetActive(false);
    }

    public void ShowGameOver(float time, int kills)
    {
        startPanel.SetActive(false);
        gameplayUI.SetActive(false);
        gameOverPanel.SetActive(true);

        timeText.text = $"Time: {time:F1}s";
        killText.text = $"Kill: {kills}";
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
