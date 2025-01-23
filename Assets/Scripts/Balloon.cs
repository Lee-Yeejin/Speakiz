using UnityEngine;
using UnityEngine.UI;

public class Balloon : MonoBehaviour
{
    public Text balloonCountText;  // 풍선 개수를 표시할 UI 텍스트

    void OnEnable()
    {
        // TrainingFetcher에서 데이터를 가져와 풍선 개수를 업데이트
        int balloonCount = TrainingFetcher.instance?.GetBalloonCount() ?? 0;
        UpdateBalloonCount(balloonCount);
    }

    public void UpdateBalloonCount(int balloonCount)
    {
        balloonCountText.text = balloonCount.ToString();
    }
}
