using UnityEngine;
using UnityEngine.UI;
using static TrainingFetcher;

public class FluencyUIController : MonoBehaviour
{
    public Text dialogueText; // 피드백 데이터를 표시할 UI
    public Text fluencyScoreText; // 유창성 점수 UI
    public Text pronunciationScoreText; // 발음 점수 UI
    public Text intonationScoreText; // 억양 점수 UI

    public Image fluencyBar; // 유창성 점수 그래프
    public Image pronunciationBar; // 발음 점수 그래프
    public Image intonationBar; // 억양 점수 그래프

    private int currentCanvasIndex = 0;

    void Start()
    {
        LoadAndDisplayAllFeedback(); // 피드백 로드 및 업데이트
        UpdateScores();
    }

    public void LoadAndDisplayAllFeedback()
    {
        string fullDialogue = "";
        var trainingFetcher = TrainingFetcher.instance;

        for (int canvasIndex = 1; canvasIndex <= 3; canvasIndex++) // Canvas 1, 2, 3의 데이터
        {
            string key = $"Canvas_{canvasIndex}_Dialogue";
            string jsonData = PlayerPrefs.GetString(key, "");

            if (!string.IsNullOrEmpty(jsonData))
            {
                var dialogueData = JsonUtility.FromJson<DialogueData>(jsonData);

                fullDialogue += $"<b><size=42><color=#8388FF>-- [장면 {canvasIndex}] --\n</color></size></b>";
                fullDialogue += "\n";
                for (int i = 0; i < dialogueData.Questions.Count; i++)
                {
                    fullDialogue += $"<b>피키 :</b>  {dialogueData.Questions[i]}\n";
                    if (i < dialogueData.Answers.Count)
                    {
                        fullDialogue += $"<b>나의 답변 :</b>  {dialogueData.Answers[i]}\n";
                    }
                }

                fullDialogue += "\n";
            }
            else
            {
                fullDialogue += $"---[장면 {canvasIndex}]---\n데이터 없음\n\n";
            }
        }

        // 대화 내용 업데이트
        dialogueText.text = fullDialogue;
    }
    // 점수 데이터 업데이트
    public void UpdateScores()
    {
        var trainingFetcher = TrainingFetcher.instance;
        if (trainingFetcher != null)
        {
            // PlayerPrefs에서 점수 불러오기
            trainingFetcher.LoadScoresFromPlayerPrefs();

            // 점수 텍스트 업데이트
            float fluencyScore = trainingFetcher.GetFluencyScore();
            float pronunciationScore = trainingFetcher.GetPronunciationScore();
            float intonationScore = trainingFetcher.GetIntonationScore();

            fluencyScoreText.text = $"{fluencyScore:F1}점";
            pronunciationScoreText.text = $"{pronunciationScore:F1}점";
            intonationScoreText.text = $"{intonationScore:F1}점";

            // 점수에 따라 그래프 바 fillAmount 업데이트 (0~1 정규화)
            fluencyBar.fillAmount = Mathf.Clamp01(fluencyScore / 100f);
            pronunciationBar.fillAmount = Mathf.Clamp01(pronunciationScore / 100f);
            intonationBar.fillAmount = Mathf.Clamp01(intonationScore / 100f);

            Debug.Log($"[UI 업데이트] 발음 유창성: {fluencyScore:F1}, 발음 완전성: {pronunciationScore:F1}, 발음 정확도: {intonationScore:F1}");
        }
        else
        {
            Debug.LogError("TrainingFetcher가 존재하지 않습니다.");
            fluencyScoreText.text = "점수를 불러올 수 없습니다.";
            pronunciationScoreText.text = "점수를 불러올 수 없습니다.";
            intonationScoreText.text = "점수를 불러올 수 없습니다.";
        }
    }
}