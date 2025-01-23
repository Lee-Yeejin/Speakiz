using UnityEngine;
using UnityEngine.UI;

public class pronunArrow : MonoBehaviour
{
    public static pronunArrow instance;

    public Text wordText;
    public Text childWordText;
    public Text accuracyText;
    public Text feedbackText;
    public Button nextButton;
    public Button previousButton;

    public Button playTeacherVoiceButton; // 선생님 음성 재생 버튼
    public Button playChildVoiceButton;   // 아동 음성 재생 버튼
    public AudioSource audioSource;       // 음성 재생용 AudioSource

    private int currentWordIndex = 0;
    private string[] words;
    private string[] childWords;
    private float[] accuracies;
    private string[] feedbacks;
    private AudioClip[] teacherVoices;
    private AudioClip[] childVoices;

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
    }

    void Start()
    {
        if (TrainingFetcher.instance != null)
        {
            words = TrainingFetcher.instance.GetWords();
            childWords = TrainingFetcher.instance.GetChildWords();
            accuracies = TrainingFetcher.instance.GetScores();
            feedbacks = TrainingFetcher.instance.GetFeedback();
            teacherVoices = TrainingFetcher.instance.GetTeacherVoices();
            childVoices = TrainingFetcher.instance.GetChildVoices();

            if (words != null && words.Length > 0)
            {
                UpdateWordUI();
            }
            else
            {
                Debug.LogError("Training data is null or empty.");
            }
        }
        else
        {
            Debug.LogError("TrainingFetcher instance is null.");
        }

        // 버튼에 오디오 재생 메서드 연결
        playTeacherVoiceButton.onClick.AddListener(PlayTeacherVoice);
        playChildVoiceButton.onClick.AddListener(PlayChildVoice);
    }

    private void UpdateWordUI()
    {
        if (words != null && currentWordIndex < words.Length)
        {
            wordText.text = words[currentWordIndex];
            childWordText.text = childWords[currentWordIndex];
            accuracyText.text = $"{accuracies[currentWordIndex] * 100:F1}%";
            feedbackText.text = feedbacks[currentWordIndex];

            previousButton.gameObject.SetActive(currentWordIndex > 0);
            nextButton.gameObject.SetActive(currentWordIndex < words.Length - 1);
        }
    }

    public void GoToNextWord()
    {
        if (currentWordIndex < words.Length - 1)
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

    private void PlayTeacherVoice()
    {
        if (teacherVoices[currentWordIndex] != null)
        {
            audioSource.clip = teacherVoices[currentWordIndex];
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("No teacher voice clip found for current word.");
        }
    }

    private void PlayChildVoice()
    {
        if (childVoices[currentWordIndex] != null)
        {
            audioSource.clip = childVoices[currentWordIndex];
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("No child voice clip found for current word.");
        }
    }
}
