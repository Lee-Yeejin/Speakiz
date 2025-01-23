using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class LoginManager : MonoBehaviour
{
    public InputField loginIdInputField;   // 'user_login_id' 입력 필드
    public InputField passwordInputField;  // 'user_password' 입력 필드
    public Button loginButton;

    private void Start()
    {
        loginButton.onClick.AddListener(OnLoginButtonClick);
    }

    private void OnLoginButtonClick()
    {
        string loginId = loginIdInputField.text;
        string password = passwordInputField.text;
        StartCoroutine(Login(loginId, password));
    }

    private IEnumerator Login(string loginId, string password) // 'loginId'로 변수명 수정
    {
        WWWForm form = new WWWForm();
        form.AddField("user_login_id", loginId);  // 'user_login_id'로 변경
        form.AddField("user_password", password);  // 'user_password'로 변경

        UnityWebRequest www = UnityWebRequest.Post("http://localhost:8080/login", form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("로그인 실패: " + www.error);
        }
        else
        {
            Debug.Log("로그인 성공");
        }
    }
}
