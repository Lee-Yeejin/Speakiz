using System.Collections.Generic;
using UnityEngine;

public class FluencyDataSaver : MonoBehaviour
{
    private List<string> savedQuestions = new List<string>(); // 사용자가 저장한 질문 데이터를 담는 리스트
    private List<string> savedAnswers = new List<string>(); // 사용자가 저장한 대답 데이터를 담는 리스트

    // 랜덤 점수를 저장할 변수 추가
    public float fluencyScore; // 유창성 점수
    public float pronunciationScore; // 발음 점수
    public float intonationScore; // 억양 점수

    public void ClearData()
    {
        savedQuestions.Clear();
        savedAnswers.Clear();
        fluencyScore = 0;
        pronunciationScore = 0;
        intonationScore = 0;
        Debug.Log("질문, 답변 및 점수 데이터가 초기화되었습니다.");
    }

    public void SaveQuestion(string question)  // 전달받은 question을 savedquestions 리스트에 저장
    {
        if (!savedQuestions.Contains(question))         // 이미 리스트에 있는 질문인지 확인(중복검사)
        {
            savedQuestions.Add(question);        // 질문 저장
            Debug.Log($"질문 저장: {question}");
        }
        else
        {
            Debug.LogWarning($"중복된 질문이 저장되지 않았습니다: {question}");
        }
    }

    public void SaveAnswer(string answer, bool isCorrect) // 대답을 saveAnswer리스트에 저장
    {
        if (isCorrect && !savedAnswers.Contains(answer)) // 올바른 대답만 저장
        {
            savedAnswers.Add(answer);
            Debug.Log($"답변 저장: {answer}");
        }
        else if (!isCorrect)
        {
            Debug.LogWarning($"올바르지 않은 답변은 저장되지 않습니다: {answer}");
        }
    }

    public bool IsQuestionSaved(string question) => savedQuestions.Contains(question); // 특정 질문이 저장됐는지 확인
    public bool IsAnswerSaved(string answer) => savedAnswers.Contains(answer); // 특정 대답이 저장됐는지 확인

    public void GenerateRandomScores()
    {
        fluencyScore = Random.Range(85f, 100f); // 유창성 점수: 85 ~ 99
        pronunciationScore = Random.Range(85f, 100f); // 발음 점수: 85 ~ 99
        intonationScore = Random.Range(80f, 100f); // 억양 점수: 80 ~ 99

        Debug.Log($"[랜덤 점수 생성] 유창성: {fluencyScore:F1}, 발음: {pronunciationScore:F1}, 억양: {intonationScore:F1}");
    }

    public void SaveScoresToPlayerPrefs()
    {
        // 랜덤 점수를 PlayerPrefs에 저장
        PlayerPrefs.SetFloat("FluencyScore", fluencyScore);
        PlayerPrefs.SetFloat("PronunciationScore", pronunciationScore);
        PlayerPrefs.SetFloat("IntonationScore", intonationScore);
        PlayerPrefs.Save();

        Debug.Log($"[PlayerPrefs 저장 확인] 유창성: {fluencyScore:F1}, 발음: {pronunciationScore:F1}, 억양: {intonationScore:F1}");
    }

    public void LoadScoresFromPlayerPrefs()
    {
        // PlayerPrefs에서 점수를 불러오기
        fluencyScore = PlayerPrefs.GetFloat("FluencyScore", 0f);
        pronunciationScore = PlayerPrefs.GetFloat("PronunciationScore", 0f);
        intonationScore = PlayerPrefs.GetFloat("IntonationScore", 0f);

        Debug.Log($"[PlayerPrefs 불러오기] 유창성: {fluencyScore:F1}, 발음: {pronunciationScore:F1}, 억양: {intonationScore:F1}");
    }

    public void SaveToPlayerPrefs(int canvasIndex)    // savedQuestions와 savedanswers 데이터를 playerprefs에 json 형식으로 저장
    {
        string key = $"Canvas_{canvasIndex}_Dialogue";
        string jsonData = JsonUtility.ToJson(new DialogueData(savedQuestions, savedAnswers));
        PlayerPrefs.SetString(key, jsonData);  // 데이터 저장
        PlayerPrefs.Save();
        // Debug 메시지 
        Debug.Log($"[PlayerPrefs 저장 확인] Scene {canvasIndex} 데이터 저장 완료: {jsonData}");
    }

    public void LoadFluencyTrainingData(int canvasIndex) // playerprefs에서 json 데이터를 불러와 savedquestions와 savedanswers 리스트를 복원
    {
        string key = $"Canvas_{canvasIndex}_Dialogue";// 이 키로 데이터 검색
        string jsonData = PlayerPrefs.GetString(key, "");
        if (!string.IsNullOrEmpty(jsonData))
        {
            DialogueData loadedData = JsonUtility.FromJson<DialogueData>(jsonData);
            savedQuestions = loadedData.Questions ?? new List<string>();
            savedAnswers = loadedData.Answers ?? new List<string>();

            Debug.Log($"[PlayerPrefs 불러오기 확인]Canvas {canvasIndex} 데이터 복원 완료: {jsonData}");
        }
        else
        {
            Debug.Log($"[PlayerPrefs 불러오기 실패]Canvas {canvasIndex}에 저장된 데이터가 없습니다.");
        }
    }

    public int GetDialogueDataCount() // 저장된 질문과 대답의 총 개수를 반환
    {
        return savedQuestions.Count + savedAnswers.Count;
    }
    // 질문 데이터를 반환하는 메서드 추가
    public string[] GetSavedQuestions()
    {
        return savedQuestions.ToArray();
    }

    // 답변 데이터를 반환하는 메서드 추가
    public string[] GetSavedAnswers()
    {
        return savedAnswers.ToArray();
    }

    [System.Serializable]
    private class DialogueData
    {
        public List<string> Questions;
        public List<string> Answers;

        public DialogueData(List<string> questions, List<string> answers)
        {
            Questions = questions ?? new List<string>();
            Answers = answers ?? new List<string>();
        }
    }
}