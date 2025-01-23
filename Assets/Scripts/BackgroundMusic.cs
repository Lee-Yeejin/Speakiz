using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BackgroundMusicManager : MonoBehaviour
{
    public static BackgroundMusicManager instance;
    public AudioSource backgroundMusic;
    public Toggle musicToggle;

    public Canvas ptCanvas;
    public Canvas btCanvas;
    public Canvas ftCanvas;
    public Canvas btFeedback;
    public Canvas ptFeedback;
    public Canvas ftFeedback;

    private string[] gameScenes = { "BreathGame", "PronunciationGame", "FluencyGame" };

    private bool isMusicPausedByCanvas = false; // 캔버스에 의해 음악이 일시정지되었는지 추적

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        if (!backgroundMusic.isPlaying)
        {
            backgroundMusic.Play();
        }

        if (musicToggle != null)
        {
            musicToggle.isOn = backgroundMusic.isPlaying;
            musicToggle.onValueChanged.RemoveAllListeners();
            musicToggle.onValueChanged.AddListener(ToggleMusic);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (IsGameScene(scene.name))
        {
            if (backgroundMusic.isPlaying)
            {
                backgroundMusic.Pause();
                isMusicPausedByCanvas = true;
            }
        }
        else
        {
            if (!backgroundMusic.isPlaying && !isMusicPausedByCanvas)
            {
                backgroundMusic.UnPause();
            }
        }
    }

    public void ToggleMusic(bool isOn)
    {
        if (isOn)
        {
            if (!isMusicPausedByCanvas) // 캔버스가 음악을 일시정지한 경우 토글이 음악을 재생하지 않도록 함
            {
                backgroundMusic.Play();
            }
        }
        else
        {
            backgroundMusic.Pause();
        }
    }

    // 특정 캔버스 활성화 시 음악 일시정지
    public void CheckCanvasState()
    {
        if (ptCanvas.isActiveAndEnabled ||
            btCanvas.isActiveAndEnabled ||
            ftCanvas.isActiveAndEnabled ||
            btFeedback.isActiveAndEnabled ||
            ptFeedback.isActiveAndEnabled ||
            ftFeedback.isActiveAndEnabled)
        {
            if (backgroundMusic.isPlaying)
            {
                backgroundMusic.Pause();
                isMusicPausedByCanvas = true; // 캔버스에 의해 음악이 정지되었음을 표시
            }
        }
        else
        {
            if (!backgroundMusic.isPlaying && musicToggle.isOn)
            {
                backgroundMusic.UnPause();
                isMusicPausedByCanvas = false;
            }
        }
    }

    void Update()
    {
        // 캔버스 상태를 매 프레임 확인
        CheckCanvasState();
    }

    private bool IsGameScene(string sceneName)
    {
        foreach (string gameScene in gameScenes)
        {
            if (sceneName == gameScene)
            {
                return true;
            }
        }
        return false;
    }
}
