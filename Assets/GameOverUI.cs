using UnityEngine;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    public GameObject panel;
    public TMP_Text timeText;
    public TMP_Text killText;

    void Awake()
    {
        panel.SetActive(false);
    }

    public void Show(float time, int kills)
    {
        panel.SetActive(true);
        timeText.text = $"Time: {time:F1}s";
        killText.text = $"Kill: {kills}";
    }

    public void Hide()
    {
        panel.SetActive(false);
    }
}
