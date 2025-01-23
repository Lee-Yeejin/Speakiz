using UnityEngine;
using UnityEngine.UI; // UI 네임스페이스 포함

public class BalloonSliderController : MonoBehaviour
{
    public Slider balloonSlider; // 슬라이더 컴포넌트 참조
    public int totalBalloons = 5; // 총 풍선 개수
    private int currentBalloons = 0; // 현재 풍선 개수

    void Start()
    {
        balloonSlider.maxValue = totalBalloons; // 슬라이더의 최대 값을 총 풍선 개수로 설정
        balloonSlider.value = currentBalloons; // 초기 슬라이더 값 설정
    }

    public void AddBalloon()
    {
        if (currentBalloons < totalBalloons)
        {
            currentBalloons++; // 풍선 개수 증가
            balloonSlider.value = currentBalloons; // 슬라이더 값 업데이트
            Debug.Log("Current Balloons: " + currentBalloons);
            Debug.Log("Slider Value: " + balloonSlider.value);
        }
    }
}
