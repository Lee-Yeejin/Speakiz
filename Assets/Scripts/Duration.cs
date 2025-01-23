using System.Collections;
using UnityEngine;
using UnityEngine.UI; // UI 관련 네임스페이스 추가

public class Duration : MonoBehaviour
{
    public Image durationBar;       // UI의 막대 그래프를 나타낼 Image
    public Text durationText;       // UI에 표시할 평균 지속시간 텍스트

    void OnEnable()
    {
        UpdateGraph(); // 피드백 화면이 활성화될 때마다 그래프 업데이트
    }

    public void UpdateGraph()
    {
        // TrainingFetcher에서 평균 지속시간 가져오기
        float averageDuration = TrainingFetcher.instance?.GetAverageDuration() ?? 0;

        if (averageDuration > 0)
        {
            durationBar.fillAmount = Mathf.Clamp01(averageDuration / 3f); // 최대 3초로 정규화
            durationText.text = averageDuration.ToString("F1") + "초"; // 소수점 1자리까지 표시
        }
        else
        {
            durationBar.fillAmount = 0f;
            durationText.text = "데이터 없음";
            Debug.LogWarning("평균 지속시간 데이터가 유효하지 않습니다.");
        }
    }
}