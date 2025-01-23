using UnityEngine;

public class DebugManager : MonoBehaviour
{
    public void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("FT_PlayerPrefs 데이터가 모두 삭제되었습니다.");
    }
}
