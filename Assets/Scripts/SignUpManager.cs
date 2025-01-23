using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class SignUpManager : MonoBehaviour
{
    public InputField nameInputField;
    public InputField idInputField;
    public InputField passwordInputField;
    public InputField confirmPasswordInputField;
    public InputField emailInputField;
    public InputField emailVerificationInputField;
    public InputField parentPasswordInputField; // 학부모용 비밀번호 입력 필드
    public Button signUpButton;
    public Text feedbackText;
    private string serverUrl = "http://localhost:8080/users/register";

    void Start()
    {
        signUpButton.onClick.AddListener(OnSignUpButtonClicked);
        confirmPasswordInputField.onValueChanged.AddListener(OnConfirmPasswordChanged);
    }

    void OnSignUpButtonClicked()
    {
        string name = nameInputField.text;
        string id = idInputField.text;
        string password = passwordInputField.text;
        string confirmPassword = confirmPasswordInputField.text;
        string email = emailInputField.text;
        string emailVerification = emailVerificationInputField.text;
        string parentPassword = parentPasswordInputField.text; // 학부모용 비밀번호

        // 비밀번호 확인 로직
        if (password != confirmPassword)
        {
            feedbackText.text = "비밀번호가 일치하지 않습니다.";
            return;
        }

        StartCoroutine(SignUp(name, id, password, email, parentPassword));
    }

    void OnConfirmPasswordChanged(string confirmPassword)
    {
        string password = passwordInputField.text;

        if (password != confirmPassword)
        {
            feedbackText.text = "비밀번호가 일치하지 않습니다.";
        }
        else
        {
            feedbackText.text = "";
        }
    }

    IEnumerator SignUp(string name, string id, string password, string email, string parentPassword)
    {
        User user = new User
        {
            user_name = name,
            user_login_id = id,
            user_password = password,
            user_email = email,
            user_level = 1, // 기본값 설정
            user_parent_password = parentPassword // 학부모용 비밀번호 추가
        };

        string jsonData = JsonUtility.ToJson(user);

        UnityWebRequest request = new UnityWebRequest(serverUrl, "POST");
        byte[] bodyRaw = new System.Text.UTF8Encoding().GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            feedbackText.text = "실패: " + request.error;
        }
        else
        {
            feedbackText.text = "성공";
        }
    }

    [System.Serializable]
    public class User
    {
        public string user_name;            // 사용자 이름
        public string user_login_id;        // 사용자 로그인 ID
        public string user_password;        // 사용자 비밀번호
        public string user_email;           // 사용자 이메일
        public int user_level;              // 사용자 레벨 (기본값 1)
        public string user_parent_password; // 학부모용 비밀번호
    }
}
