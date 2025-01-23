using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;

public class WordGameController : MonoBehaviour
{
    public static WordGameController Instance;


    // WordDataManager를 인스턴스로 사용
    public WordDataManager wordDataManager;

    public GameObject panel1;
    public RawImage imagePanel1;
    public Button nextButton;
    public Button playButton;
    public GameObject Panel;
    public GameObject cuteBird;
    public GameObject panel2;
    public Slider progressSlider;
    public Text gameOverText;
    public Text wordNameText;
    public GameObject WordNameTextimage;

    public Text buttonText;

    public VideoPlayer videoPlayer;
    public GameObject videopanel; // videopanel 오브젝트 추가

    public RenderTexture newRenderTexture;
    private int currentStage = 1;  // 스테이지 번호를 추적하는 변수

    private int currentWordIndex = 0;
    private List<string> selectedWords;

    public GameObject FCanvas; // F캔버스 참조 추가
    public GameObject currentCanvas; // 현재 캔버스 참조 추가


    private string imagesPath = "Images/";
    private string videosPath = "Videos/";
    private List<string> words = new List<string> { "냉장고", "고등어", "도시락", "토마토", "토스트", "테이블", "에어컨", "청소기", "숟가락", "젓가락", "고양이", "라디오", "물티슈", "바나나", "햄버거", "딸기잼", "꽈배기", "삐에로", "쓰레기" };

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {

        selectedWords = GetRandomWords(3);
        //st<string> videoPaths = new List<string>();   // 비디오 경로 목록
        List<string> recognizedTexts = new List<string>(); // 빈 리스트로 초기화
        //st<AudioClip> audioClips = new List<AudioClip>(); // 빈 오디오 클립 리스트 (현재는 비어있음)
        List<float> accuracyScores = new List<float>();
        List<string> feedbacks = new List<string>();
        foreach (var word in selectedWords)
        {
            //tring videoPath = videosPath + word + ".mp4"; // 동영상 경로 구성
            //ideoPaths.Add(videoPath);
        }

        // 선택된 단어와 동영상 경로를 JSON에 저장 (recognizedTexts와 audioClips를 빈 리스트로 전달)
        WordDataManager.SaveData(selectedWords, recognizedTexts, /*audioClips, videoPaths,*/ accuracyScores, feedbacks); // 클래스 이름으로 호출

        Debug.Log("Data save Success: " + string.Join(",", selectedWords));

        ShowImageAndWord(selectedWords[currentWordIndex]);

        nextButton.onClick.AddListener(OnNextButtonClick);
        nextButton.gameObject.SetActive(false);

        playButton.onClick.AddListener(OnPlayButtonClick);

        buttonText.text = "다음 단어로";

        Panel.gameObject.SetActive(false);
        gameOverText.gameObject.SetActive(false);
        cuteBird.gameObject.SetActive(false);

        RawImage rawImage = panel2.GetComponentInChildren<RawImage>();
        rawImage.texture = newRenderTexture;

        videoPlayer.targetTexture = newRenderTexture;

        PlayVideo(selectedWords[currentWordIndex]);

        UpdateProgressText();

        // VideoPlayer의 재생이 끝났을 때 호출될 이벤트 등록
        videoPlayer.loopPointReached += OnVideoEnd;

        // 처음에 videopanel을 활성화 (비디오가 재생 중이지 않다고 가정)
        videopanel.SetActive(true);
    }

    void Update()
    {
        // SpeechToText 스크립트의 finalFailPanel 또는 panelToActivate가 활성화된 경우 WordNameTextimage를 잠시 비활성화
        if (SpeechToText.Instance != null &&
            (SpeechToText.Instance.finalFailPanel.activeSelf || SpeechToText.Instance.panelToActivate.activeSelf))
        {
            WordNameTextimage.SetActive(false);
        }
        else
        {
            WordNameTextimage.SetActive(true);
        }

        // VideoPlayer가 재생 중인지 확인하여 videopanel을 활성화/비활성화
        if (videoPlayer.isPlaying)
        {
            videopanel.SetActive(false); // 비디오 재생 중일 때 비활성화
        }
        else
        {
            videopanel.SetActive(true); // 비디오 재생 중이 아닐 때 활성화
        }

    }
    // 비디오 재생이 끝났을 때 호출되는 메서드
    private void OnVideoEnd(VideoPlayer vp)
    {
        videopanel.SetActive(true); // 비디오가 끝나면 videopanel 활성화
    }

    // 선택된 단어를 반환하는 메서드
    public List<string> GetSelectedWords()
    {
        return selectedWords;
    }

    // 비디오 경로 목록을 반환하는 메서드
    /*ublic List<string> GetVideoPaths()
     {
         List<string> videoPaths = new List<string>();
         foreach (var word in selectedWords)
         {
             string videoPath = videosPath + word + ".mp4"; // 동영상 경로 생성
             videoPaths.Add(videoPath);
         }
         return videoPaths;
     }*/

    // 현재 스테이지 번호를 반환하는 메서드
    public int GetCurrentStage()
    {
        return currentStage;  // 스테이지 번호 반환
    }

    // 스테이지 번호를 증가시키는 메서드 (필요시 호출)
    public void IncrementStage()
    {
        currentStage++;
    }

    List<string> GetRandomWords(int count)
    {
        List<string> selectedWords = new List<string>();
        while (selectedWords.Count < count)
        {
            string randomWord = words[Random.Range(0, words.Count)];
            if (!selectedWords.Contains(randomWord))
            {
                selectedWords.Add(randomWord);
            }
        }
        return selectedWords;
    }

    void ShowImageAndWord(string word)
    {
        Texture2D texture = Resources.Load<Texture2D>(imagesPath + word);
        if (texture != null)
        {
            imagePanel1.texture = texture;
            wordNameText.text = word;
        }
        else
        {
            Debug.LogWarning("Image not found for word: " + word);
        }
    }

    // 비디오 재생과 서버 전송을 위한 수정된 PlayVideo 메서드
    void PlayVideo(string word)
    {
        string videoPath = videosPath + word; // 확장자 제거

        // 비디오 재생
        VideoClip videoClip = Resources.Load<VideoClip>(videoPath);
        if (videoClip != null)
        {
            videoPlayer.clip = videoClip;
            videopanel.SetActive(false);
            videoPlayer.Play();

            // 선택된 비디오 파일을 서버로 전송
            List<string> recognizedTexts = new List<string>(); // 예시로 빈 텍스트 사용
            List<float> accuracyScores = new List<float>();
            List<string> feedbacks = new List<string>();

            // selectedWords가 null이 아닌지 확인
            if (selectedWords == null)
            {
                Debug.LogError("selectedWords is null!");
                return;
            }

            wordDataManager.SaveDataLocally(selectedWords, recognizedTexts, accuracyScores, feedbacks);
        }
        else
        {
            Debug.LogWarning("Video not found for word: " + word);
        }
    }


    void OnNextButtonClick()
    {
        currentWordIndex++;

        if (currentWordIndex < selectedWords.Count)
        {
            ShowImageAndWord(selectedWords[currentWordIndex]);

            if (currentWordIndex == selectedWords.Count - 1)
            {
                buttonText.text = "게임 종료";
            }

            UpdateProgressText();
            PlayVideo(selectedWords[currentWordIndex]);
        }
        else
        {
            EndGame();
        }

        nextButton.gameObject.SetActive(false);
    }

    void EndGame()
    {
        Debug.Log("게임 종료");

        panel1.SetActive(false);
        nextButton.gameObject.SetActive(false);
        Panel.gameObject.SetActive(true);
        gameOverText.gameObject.SetActive(true);
        wordNameText.gameObject.SetActive(false);
        WordNameTextimage.gameObject.SetActive(false);
        cuteBird.gameObject.SetActive(true);
    }

    void OnPlayButtonClick()
    {
        if (videoPlayer.clip != null)
        {
            videoPlayer.Play();
        }
    }

    void UpdateProgressText()
    {
        progressSlider.value = (float)(currentWordIndex) / selectedWords.Count;
    }

    public string GetCurrentWord()
    {
        return selectedWords[currentWordIndex];
    }
    // 현재 단어의 인덱스를 반환하는 메서드 추가
    public int GetCurrentWordIndex()
    {
        return currentWordIndex;
    }
    public void IncrementSlider()
    {
        // 슬라이더의 값을 한 칸 증가시킴
        progressSlider.value += 1.0f / selectedWords.Count;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void ReturnToMain()
    {
        // PlayerPrefs에 활성화할 캔버스 이름 저장
        PlayerPrefs.SetString("TargetCanvasName", "Training");

        // main 씬 로드
        SceneManager.LoadScene("Main");
    }

    // Main 씬으로 돌아가는 메서드
    public void ReturnMainHome()
    {
        SceneManager.LoadScene("Main");
    }
    public void ExitGame()
    {
        Debug.Log("ExitGame 버튼이 눌렸습니다.");

        // FCanvas 활성화
        FCanvas.SetActive(true);

        // 게임 데이터를 TrainingFetcher로 바로 로드
        TrainingFetcher.instance.LoadPronunciationTrainingData(); // 바로 데이터를 로드

        // 현재 캔버스를 비활성화
        if (currentCanvas != null)
        {
            currentCanvas.SetActive(false);
        }
    }

}