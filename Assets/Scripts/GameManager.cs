using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameUI ui;
    public GameObject playerPrefab;
    public Transform spawnPoint;

    GameObject currentPlayer;
    float surviveTime;
    int killCount;
    bool isPlaying;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (isPlaying)
            surviveTime += Time.deltaTime;
    }

    public void StartGame()
    {
        Cleanup();

        surviveTime = 0f;
        killCount = 0;
        isPlaying = true;

        ui.ShowGameplay();
        ui.SetAssetHud(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SpawnPlayer();
    }

    void SpawnPlayer()
    {
        currentPlayer = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);

        var ph = currentPlayer.GetComponent<PlayerHealth>();
        if (ph != null)
            ph.ui = ui;
    }

    public void GameOver()
    {
        if (!isPlaying) return;
        isPlaying = false;

        ui.SetAssetHud(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (currentPlayer != null)
        {
            Destroy(currentPlayer);
            currentPlayer = null;
        }

        ui.ShowGameOver(surviveTime, killCount);
    }

    public void RestartGame()
    {
        StartGame();
    }

    void Cleanup()
    {
        if (currentPlayer != null)
        {
            Destroy(currentPlayer);
            currentPlayer = null;
        }

        var monsters = Object.FindObjectsByType<SimpleFollow>(FindObjectsSortMode.None);
        foreach (var m in monsters)
            Destroy(m.gameObject);

        ui.SetAssetHud(false);
    }

    public void AddKill()
    {
        killCount++;
    }

    // ? 게임오버 패널 "게임종료" 버튼에서 연결할 함수
    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
