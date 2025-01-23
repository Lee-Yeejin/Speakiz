using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class TrainingFetcher : MonoBehaviour
{
    public static TrainingFetcher instance;

    // 호흡 훈련 데이터
    public float length;                   // 호흡 훈련: 평균 지속시간
    public float[] bt_levels = new float[6]; // 호흡 훈련: 볼륨 값 (6개 구간)
    public int bt_success_cnt;             // 호흡 훈련: 풍선 개수

    // 조음 훈련 데이터
    public string[] pt_words = new string[3];           // 조음 훈련: 훈련 단어
    public string[] pt_text = new string[3];            // 조음 훈련: 아동 발음 텍스트
    public float[] pt_score = new float[3];             // 조음 훈련: 정확도 점수
    public string[] pt_feedback = new string[3];        // 조음 훈련: 피드백
    public AudioClip[] pt_teacher_voice = new AudioClip[3];  // 조음 훈련: 선생님 소리
    public AudioClip[] pt_child_voice = new AudioClip[3];    // 조음 훈련: 아동 소리

    // 유창성 훈련 데이터
    public string[][] ft_texts = new string[3][];      // 유창성 훈련: 각 씬별 대화 5줄
    // 유창성 훈련 점수 데이터
    public float fluencyScore; // 유창성 점수
    public float pronunciationScore; // 발음 점수
    public float intonationScore; // 억양 점수

    private string resourcesPath = "Game_PT/Resources/";

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
        LoadBreathTrainingData();
        LoadPronunciationTrainingData();
        LoadFluencyTrainingData(); // 유창성 훈련 데이터 로드
        Debug.Log("[DEBUG] TrainingFetcher Start 호출됨");
    }

    // 호흡 훈련 데이터 로드
    public void LoadBreathTrainingData()
    {
        length = PlayerPrefs.GetFloat("length", 0.0f);
        Debug.Log($"Loaded length: {length}");

        for (int i = 0; i < bt_levels.Length; i++)
        {
            bt_levels[i] = PlayerPrefs.GetFloat($"bt_level{i + 1}", 0.0f); // bt_level1 ~ bt_level6
            Debug.Log($"Loaded bt_level{i + 1}: {bt_levels[i]}");
        }

        bt_success_cnt = PlayerPrefs.GetInt("bt_success_cnt", 0);
        Debug.Log($"Loaded bt_success_cnt: {bt_success_cnt}");
    }

    // 조음 훈련 데이터 로드
    public void LoadPronunciationTrainingData()
    {
        pt_words = LoadStringArrayFromPrefs("pt_word", new string[] { "", "", "" });
        pt_text = LoadStringArrayFromPrefs("pt_text", new string[] { "", "", "" });

        float[] rawScores = LoadFloatArrayFromPrefs("pt_score", new float[] { 0.0f, 0.0f, 0.0f });
        for (int i = 0; i < pt_score.Length; i++)
        {
            pt_score[i] = Mathf.Round(rawScores[i] * 10f) / 10f;
        }

        pt_feedback = LoadStringArrayFromPrefs("pt_feedback", new string[] { "", "", "" });

        for (int i = 0; i < 3; i++)
        {
            pt_teacher_voice[i] = Resources.Load<AudioClip>($"{resourcesPath}TeacherVoice_{i}") ?? null;
            pt_child_voice[i] = Resources.Load<AudioClip>($"{resourcesPath}ChildVoice_{i}") ?? null;
        }
    }

    // 유창성 훈련 데이터 로드
    public void LoadFluencyTrainingData()
    {
        // ft_texts 배열 초기화
        if (ft_texts == null || ft_texts.Length != 3)
        {
            ft_texts = new string[3][];
            for (int i = 0; i < ft_texts.Length; i++)
            {
                ft_texts[i] = new string[5]; // 씬마다 대화는 5줄
            }
        }

        for (int i = 1; i < 3; i++) // 씬 수에 따라 반복
        {
            string key = $"Canvas_{i}_Dialogue";
            string jsonData = PlayerPrefs.GetString(key, "");

            // [DEBUG] JSON 데이터 확인
            Debug.Log($"[DEBUG] Stored JSON Data for Canvas {i}: {jsonData}");

            if (string.IsNullOrEmpty(jsonData))
            {
                Debug.Log($"[PlayerPrefs 데이터 확인]Canvas_{i}_Dialogue Not Found.");
                continue;
            }

            var dialogueData = JsonUtility.FromJson<DialogueData>(jsonData);
            if (dialogueData != null)
            {
                Debug.Log($"[PlayerPrefs 데이터 확인] Scene {i} Dialogue Loaded: {jsonData}");
                Debug.Log($"Questions: {string.Join(", ", dialogueData.Questions)}");
                Debug.Log($"Answers: {string.Join(", ", dialogueData.Answers)}");
                // 여기에 데이터를 피드백 페이지로 전달하거나 저장
            }
        }

    }
    // TrainingFetcher에서 데이터를 가져와 UI 업데이트///
    public string[] GetDialogueForScene(int canvasIndex)
    {
        string key = $"Canvas_{canvasIndex}_Dialogue";
        string jsonData = PlayerPrefs.GetString(key, "");

        if (string.IsNullOrEmpty(jsonData))
        {
            Debug.Log($"[PlayerPrefs 피드백 확인]Canvas_{canvasIndex}_Dialogue Not Found.");
            return null;
        }

        var dialogueData = JsonUtility.FromJson<DialogueData>(jsonData);
        if (dialogueData != null)
        {
            Debug.Log($"[PlayerPrefs 피드백 확인] Canvas {canvasIndex} Dialogue Found: {jsonData}");
            Debug.Log($"Questions: {string.Join(", ", dialogueData.Questions)}");
            Debug.Log($"Answers: {string.Join(", ", dialogueData.Answers)}");
            return dialogueData.Questions.ToArray();
        }

        return null;
    } ///

    public void LoadScoresFromPlayerPrefs()
    {
        // PlayerPrefs에서 점수를 불러오기
        fluencyScore = PlayerPrefs.GetFloat("FluencyScore", 0f);
        pronunciationScore = PlayerPrefs.GetFloat("PronunciationScore", 0f);
        intonationScore = PlayerPrefs.GetFloat("IntonationScore", 0f);

        Debug.Log($"[PlayerPrefs 점수 불러오기] 유창성: {fluencyScore:F1}, 발음: {pronunciationScore:F1}, 억양: {intonationScore:F1}");
    }


    // Helper to load string array from PlayerPrefs
    private string[] LoadStringArrayFromPrefs(string key, string[] defaultValue)
    {
        string jsonData = PlayerPrefs.GetString(key, "");
        if (string.IsNullOrEmpty(jsonData)) return defaultValue;

        var wrapper = JsonUtility.FromJson<StringArrayWrapper>(jsonData);
        return wrapper?.items ?? defaultValue;
    }

    // Helper to load float array from PlayerPrefs
    private float[] LoadFloatArrayFromPrefs(string key, float[] defaultValue)
    {
        string jsonData = PlayerPrefs.GetString(key, "");
        if (string.IsNullOrEmpty(jsonData)) return defaultValue;

        var wrapper = JsonUtility.FromJson<FloatArrayWrapper>(jsonData);
        return wrapper?.items ?? defaultValue;
    }

    [System.Serializable]
    private class StringArrayWrapper { public string[] items; }

    [System.Serializable]
    private class FloatArrayWrapper { public float[] items; }

    [System.Serializable]
    public class DialogueData
    {
        public List<string> Questions; // 질문 리스트
        public List<string> Answers;   // 답변 리스트

        public DialogueData(List<string> questions, List<string> answers)
        {
            Questions = questions;
            Answers = answers;
        }
    }

    // Getters for 호흡 훈련 데이터
    public float GetAverageDuration() => length;
    public float[] GetVolumeLevels() => bt_levels;
    public int GetBalloonCount() => bt_success_cnt;

    // Getters for 조음 훈련 데이터
    public string[] GetWords() => pt_words;
    public string[] GetChildWords() => pt_text;
    public float[] GetScores() => pt_score;
    public string[] GetFeedback() => pt_feedback;
    public AudioClip[] GetTeacherVoices() => pt_teacher_voice;
    public AudioClip[] GetChildVoices() => pt_child_voice;

    // Getters for 유창성 훈련 데이터
    public string[][] GetFluencyTexts() => ft_texts;
    // Getters for 점수 데이터
    public float GetFluencyScore() => fluencyScore;
    public float GetPronunciationScore() => pronunciationScore;
    public float GetIntonationScore() => intonationScore;
}