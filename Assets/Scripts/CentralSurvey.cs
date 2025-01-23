using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class CentralSurveyManager : MonoBehaviour
{
    public static CentralSurveyManager instance;

    private int[] scores = new int[5];
    private int currentQuestionIndex = 0;
    public Text resultText;
    /*private string serverUrl = "http://localhost:8080/users/level"; */

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

    public void SetScore(int score)
    {
        scores[currentQuestionIndex] = score;
        currentQuestionIndex++;
    }

    public void CalculateAndDisplayLevel()
    {
        int totalScore = 0;
        for (int i = 0; i < scores.Length; i++)
        {
            totalScore += scores[i];
        }

        float averageScore = totalScore / (float)scores.Length;
        string level = averageScore >= 50 ? "보통" : "기초";

        resultText.text = level;

        // 레벨을 서버로 전송
        /*StartCoroutine(SendLevelToServer(level));*/
    }

    public int GetCurrentQuestionIndex()
    {
        return currentQuestionIndex;
    }

    /*IEnumerator SendLevelToServer(string level)
    {
        UnityWebRequest request = new UnityWebRequest(serverUrl, "POST");
        byte[] bodyRaw = new System.Text.UTF8Encoding().GetBytes("\"" + level + "\"");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("실패" + request.error);
        }
        else
        {
            Debug.Log("성공" + level);
        }
    }*/
}
