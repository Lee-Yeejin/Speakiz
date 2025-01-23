using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class WordData
{
    public List<string> pt_word;          // 선택된 단어
    public List<string> pt_text;          // 인식된 텍스트 목록
    public List<float> pt_score;          // 정확도 점수 목록
    public List<string> pt_feedback;      // 피드백 목록
}

public class WordDataManager : MonoBehaviour
{
    private static string jsonFilePath = Path.Combine(Application.persistentDataPath, "SaveData.json");

    // PlayerPrefs에 데이터를 저장하는 메서드
    public static void SaveData(List<string> selectedWords, List<string> recognizedTexts, List<float> accuracyScores, List<string> feedbacks)
    {
        // WordData 객체를 생성하여 필요한 데이터를 담기
        WordData wordData = new WordData
        {
            pt_word = selectedWords,
            pt_text = recognizedTexts,
            pt_score = accuracyScores,
            pt_feedback = feedbacks
        };

        // PlayerPrefs에 데이터 저장
        PlayerPrefs.SetString("pt_word", JsonUtility.ToJson(new StringListWrapper { items = wordData.pt_word }));
        PlayerPrefs.SetString("pt_text", JsonUtility.ToJson(new StringListWrapper { items = wordData.pt_text }));
        PlayerPrefs.SetString("pt_score", JsonUtility.ToJson(new FloatListWrapper { items = wordData.pt_score }));
        PlayerPrefs.SetString("pt_feedback", JsonUtility.ToJson(new StringListWrapper { items = wordData.pt_feedback }));
        PlayerPrefs.Save();

        // PlayerPrefs에 저장된 데이터를 디버그 메시지로 출력
        Debug.Log("PlayerPrefs 저장 내용:");
        Debug.Log("pt_word: " + PlayerPrefs.GetString("pt_word"));
        Debug.Log("pt_text: " + PlayerPrefs.GetString("pt_text"));
        Debug.Log("pt_score: " + PlayerPrefs.GetString("pt_score"));
        Debug.Log("pt_feedback: " + PlayerPrefs.GetString("pt_feedback"));

        // JSON 데이터를 파일에 저장
        string json = JsonUtility.ToJson(wordData);
        File.WriteAllText(jsonFilePath, json);

        Debug.Log("선택된 단어, 인식된 텍스트, 정확도 점수, 피드백 JSON으로 저장 완료: " + json);
    }

    [System.Serializable]
    public class StringListWrapper
    {
        public List<string> items;
    }

    [System.Serializable]
    public class FloatListWrapper
    {
        public List<float> items;
    }

    // 로컬에 데이터 저장하는 메서드
    public void SaveDataLocally(List<string> selectedWords, List<string> recognizedTexts, List<float> accuracyScores, List<string> feedbacks)
    {
        // 로컬에 JSON 파일 저장
        SaveData(selectedWords, recognizedTexts, accuracyScores, feedbacks);
    }
}