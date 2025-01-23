using UnityEngine;

/// <summary>
/// ///////////////////나중에 삭제할게요!!!!!!!!
/// </summary>
public class PlayerPrefsViewer : MonoBehaviour
{
    void OnGUI()
    {
        GUILayout.Label("PlayerPrefs Data:");

        for (int i = 0; i < 5; i++) // 예: 5개의 Scene 데이터
        {
            string key = $"Scene_{i}_Dialogue";
            if (PlayerPrefs.HasKey(key))
            {
                GUILayout.Label($"{key}: {PlayerPrefs.GetString(key)}");
            }
            else
            {
                GUILayout.Label($"{key}: Not Found");
            }
        }
    }
}
