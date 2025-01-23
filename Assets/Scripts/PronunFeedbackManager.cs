using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class PronunFeedbackManager : MonoBehaviour
{
    public Text wordText;
    public Text childWordText;
    public Text accuracyText;
    public Text feedbackText;
    public Button nextButton;
    public Button previousButton;
    public Button playTeacherVoiceButton; // 선생님 소리 재생 버튼
    public Button playChildVoiceButton;  // 아동 소리 재생 버튼
    public AudioSource audioSource;       // 오디오 재생을 위한 AudioSource
    public Image accuracyBar;             // 정확도 점수를 표시할 그래프 이미지

    private int currentWordIndex = 0;     // 현재 표시 중인 단어의 인덱스
    private TrainingFetcher trainingFetcher; // TrainingFetcher 인스턴스 참조

    private string teacherAudioFolderPath = "Game_PT/Resources/Mp3/"; // 선생님 소리 경로
    private string childAudioBasePath;    // 아동 소리 기본 경로
    private string sessionFolderName;     // 현재 세션 폴더 이름 (e.g., Session_9)

    void Start()
    {// Unity 런타임에서 persistentDataPath 초기화
        childAudioBasePath = Path.Combine(Application.persistentDataPath, "AudioFiles");

        // PlayerPrefs에서 올바른 세션 번호 가져오기
        int sessionNumber = PlayerPrefs.GetInt("sessionNumber", -1);
        if (sessionNumber == -1)
        {
            Debug.LogError("세션 번호를 가져올 수 없습니다. 디폴트 값인 -1이 반환되었습니다.");
        }

        sessionFolderName = $"Session_{sessionNumber}";
        Debug.Log($"현재 세션 번호: {sessionNumber}");
        Debug.Log($"세션 폴더 경로: {Path.Combine(childAudioBasePath, sessionFolderName)}");

        trainingFetcher = TrainingFetcher.instance;

        if (trainingFetcher != null)
        {
            UpdateWordUI(); // 첫 번째 단어로 UI 업데이트
        }
        else
        {
            Debug.LogError("TrainingFetcher instance is null.");
        }

        nextButton.onClick.AddListener(GoToNextWord);
        previousButton.onClick.AddListener(GoToPreviousWord);
        playTeacherVoiceButton.onClick.AddListener(PlayTeacherVoice); // 선생님 소리 재생
        playChildVoiceButton.onClick.AddListener(PlayChildVoice);    // 아동 소리 재생
    }

    // UI를 현재 단어 인덱스에 따라 업데이트
    private void UpdateWordUI()
    {
        if (trainingFetcher != null && currentWordIndex < trainingFetcher.GetWords().Length)
        {
            wordText.text = trainingFetcher.GetWords()[currentWordIndex];
            childWordText.text = trainingFetcher.GetChildWords()[currentWordIndex];

            UpdateGraph();

            feedbackText.text = trainingFetcher.GetFeedback()[currentWordIndex];
            previousButton.gameObject.SetActive(currentWordIndex > 0);
            nextButton.gameObject.SetActive(currentWordIndex < trainingFetcher.GetWords().Length - 1);
        }
    }

    public void UpdateGraph()
    {
        float accuracy = trainingFetcher.GetScores()[currentWordIndex];

        if (accuracy > 0)
        {
            accuracyBar.fillAmount = Mathf.Clamp01(accuracy / 100f); // 100.0 기준으로 정규화
            accuracyText.text = accuracy.ToString("F1") + "%"; // 소수점 1자리까지 표시
        }
        else
        {
            accuracyBar.fillAmount = 0f;
            accuracyText.text = "데이터 없음";
            Debug.LogWarning("정확도 데이터가 유효하지 않습니다.");
        }
    }

    public void GoToNextWord()
    {
        if (currentWordIndex < trainingFetcher.GetWords().Length - 1)
        {
            currentWordIndex++;
            UpdateWordUI();
        }
    }

    public void GoToPreviousWord()
    {
        if (currentWordIndex > 0)
        {
            currentWordIndex--;
            UpdateWordUI();
        }
    }

    // 훈련 단어에 맞는 mp3 파일 재생 (선생님 소리)
    private void PlayTeacherVoice()
    {
        string currentWord = trainingFetcher.GetWords()[currentWordIndex];
        string audioFilePath = Path.Combine(Application.dataPath, teacherAudioFolderPath, $"{currentWord}.mp3");

        if (!File.Exists(audioFilePath))
        {
            Debug.LogError($"오디오 파일이 존재하지 않습니다: {audioFilePath}");
            return;
        }

        StartCoroutine(LoadAudioClipFromPath(audioFilePath));
    }

    private void PlayChildVoice()
    {
        string currentWord = trainingFetcher.GetWords()[currentWordIndex];
        int trainingOrder = currentWordIndex + 1; // 훈련 순서 (1부터 시작)
        string childAudioFilePath = Path.Combine(childAudioBasePath, sessionFolderName, $"recordedAudio_{currentWord}_{trainingOrder}.wav");

        Debug.Log($"재생하려는 파일 경로: {childAudioFilePath}");

        if (!File.Exists(childAudioFilePath))
        {
            Debug.LogError($"아동 소리 파일이 존재하지 않습니다: {childAudioFilePath}");
            return;
        }

        StartCoroutine(LoadAudioClipFromPath(childAudioFilePath));
    }


    private System.Collections.IEnumerator LoadAudioClipFromPath(string path)
    {
        using (var www = new WWW("file://" + path))
        {
            yield return www;

            if (string.IsNullOrEmpty(www.error))
            {
                audioSource.clip = www.GetAudioClip(false, true);
                audioSource.Play();
                Debug.Log("오디오 재생: " + path);
            }
            else
            {
                Debug.LogError("오디오 로드 실패: " + www.error);
            }
        }
    }
}
