using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;

public class ParentPasswordManager : MonoBehaviour
{
    public InputField passwordInputField; // 비밀번호 입력 필드
    public Button submitButton; // 제출 버튼
    public GameObject feedbackCanvas; // 피드백 캔버스
    public GameObject currentCanvas; // 현재 캔버스
    private string serverUrl = "http://localhost:8080/userParentPassword"; // 서버 엔드포인트 URL

    void Start()
    {
        submitButton.onClick.AddListener(OnSubmitButtonClicked);
    }

    void OnSubmitButtonClicked()
    {
        string enteredPassword = passwordInputField.text;

        // 비밀번호 검증 없이 무조건 캔버스 전환
        feedbackCanvas.SetActive(true);
        currentCanvas.SetActive(false);

        // 비밀번호를 서버로 보내는 부분 (주석 해제 가능)
        /*
        StartCoroutine(SendPasswordToServer(enteredPassword));
        */
    }

    // 비밀번호를 서버로 전송하는 코루틴
    IEnumerator SendPasswordToServer(string password)
    {
        WWWForm form = new WWWForm();
        form.AddField("user_parent_password", password); // 서버로 전송할 필드명 변경

        UnityWebRequest www = UnityWebRequest.Post(serverUrl, form);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error: " + www.error);
        }
        else
        {
            string serverResponse = www.downloadHandler.text;
            if (serverResponse == "Success")
            {
                // 비밀번호가 일치하면 피드백 캔버스로 전환
                feedbackCanvas.SetActive(true);
                currentCanvas.SetActive(false);
            }
            else
            {
                Debug.LogError("Invalid password");
                // 비밀번호가 일치하지 않으면 오류 메시지 표시 (추가 가능)
            }
        }
    }
}
