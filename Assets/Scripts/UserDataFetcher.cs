using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class UserData
{
    public string user_name; // 서버에서 받을 사용자 이름 필드
}

public class UserDataFetcher : MonoBehaviour
{
    public static UserDataFetcher instance;
    public string user_name;

    private void Awake()
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
        // 앱 시작 시 사용자 데이터를 서버에서 가져옴
        StartCoroutine(FetchUserData());
    }

    IEnumerator FetchUserData()
    {
        UnityWebRequest www = UnityWebRequest.Get("http://localhost:8080/users"); // 서버 URL
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.ConnectionError && www.result != UnityWebRequest.Result.ProtocolError)
        {
            string jsonResult = www.downloadHandler.text;
            UserData userData = JsonUtility.FromJson<UserData>(jsonResult); // JSON 데이터를 파싱하여 UserData 객체로 변환

            if (userData != null)
            {
                user_name = userData.user_name; // 파싱된 user_name 값 저장
                Debug.Log("UserData fetched: " + user_name);
            }
            else
            {
                Debug.LogError("Failed to parse user data");
            }
        }
        else
        {
            Debug.LogError("Failed to fetch user data: " + www.error);
        }
    }

    public string GetUserName()
    {
        return user_name;
    }
}
