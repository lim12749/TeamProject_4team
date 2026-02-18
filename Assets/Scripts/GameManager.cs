using UnityEngine;
using UnityEngine.SceneManagement;

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

    bool isPaused;

    // ? Infima Character 캐시
    InfimaGames.LowPolyShooterPack.Character infimaCharacter;

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (isPlaying && Input.GetKeyDown(KeyCode.Escape))
            TogglePause();

        if (isPlaying && !isPaused)
            surviveTime += Time.deltaTime;
    }

    public void StartGame()
    {
        // ? 시작은 항상 정상
        Time.timeScale = 1f;
        isPaused = false;

        Cleanup();

        surviveTime = 0f;
        killCount = 0;
        isPlaying = true;

        ui.ShowGameplay();
        ui.ShowPause(false);
        ui.SetAssetHud(true);

        SpawnPlayer();

        // ? 시작 시 커서 락(Infima 내부 상태까지)
        SetCursorPaused(false);
    }

    void SpawnPlayer()
    {
        currentPlayer = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);

        var ph = currentPlayer.GetComponent<PlayerHealth>();
        if (ph != null)
            ph.ui = ui;

        // ? Infima Character 가져오기 (루트에 붙어있다고 했지)
        infimaCharacter = currentPlayer.GetComponent<InfimaGames.LowPolyShooterPack.Character>();
    }

    public void GameOver()
    {
        if (!isPlaying) return;
        isPlaying = false;

        Time.timeScale = 1f;
        isPaused = false;

        ui.ShowPause(false);
        ui.SetAssetHud(false);

        // ? 게임오버면 커서 보이게(Infima 내부 포함)
        SetCursorPaused(true);

        // ? 중요: Destroy 대신 비활성화 (CameraLook가 파괴된 Rigidbody 읽는 에러 방지)
        if (currentPlayer != null)
        {
            currentPlayer.SetActive(false);
            currentPlayer = null;
            infimaCharacter = null;
        }

        ui.ShowGameOver(surviveTime, killCount);
    }

    public void TogglePause()
    {
        if (!isPlaying) return;

        isPaused = !isPaused;

        Time.timeScale = isPaused ? 0f : 1f;

        ui.ShowPause(isPaused);
        ui.SetAssetHud(!isPaused);

        // ? 커서 상태 + Infima 내부 cursorLocked 동기화
        SetCursorPaused(isPaused);
    }

    public void ResumeGame()
    {
        if (!isPaused) return;
        TogglePause();
    }

    public void ResetGame()
    {
        Time.timeScale = 1f;

        // 커서 보이게(메뉴에서 클릭 가능)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 현재 씬 다시 로드 (플레이어/서비스/카메라 모두 초기화)
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }


    public void RestartGame()
    {
        ResetGame();
    }


    void Cleanup()
    {
        // ? 혹시 이전 라운드에서 비활성화만 해둔 플레이어가 남아있을 수 있어서 정리
        // currentPlayer가 null이어도 "Player" 태그로 남아있을 수 있음
        var players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var p in players)
            Destroy(p);

        currentPlayer = null;
        infimaCharacter = null;

        // 몬스터 정리
        var monsters = Object.FindObjectsByType<SimpleFollow>(FindObjectsSortMode.None);
        foreach (var m in monsters)
            Destroy(m.gameObject);

        ui.SetAssetHud(false);
        ui.ShowPause(false);
    }

    public void AddKill() => killCount++;

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // ? 커서/입력 동기화 함수
    void SetCursorPaused(bool paused)
    {
        // paused=true → 커서 보이기 + 잠금 해제
        Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = paused;

        // Infima 내부 cursorLocked도 같이 맞추기
        if (infimaCharacter != null)
            infimaCharacter.ForceCursor(paused);
    }
}
