using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CircleTimer : MonoBehaviour
{
    public Image timerImage; // Fill 방식의 Image 컴포넌트 참조
    public float duration = 3f; // 타이머 지속 시간 (초)
    private float timeElapsed;
    private Coroutine timerCoroutine; // 타이머 코루틴을 참조하기 위한 변수
    private bool isSoundDetected = false; // "아" 소리 감지 여부

    void Start()
    {
        timerImage.fillAmount = 0; // 초기화
    }

    public void StartTimer()
    {
        StopTimer(); // 기존에 실행 중인 타이머가 있으면 중단
        timeElapsed = 0; // 시간 초기화
        timerCoroutine = StartCoroutine(UpdateTimer());
    }

    public void StopTimer()
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine); // 실행 중인 코루틴을 중지
            timerCoroutine = null; // 참조 초기화
        }
        timerImage.fillAmount = 0; // 이미지 필러 리셋
        timeElapsed = 0; // 시간 리셋
    }

    // 소리 감지 상태를 설정하는 메소드
    public void SetSoundDetection(bool detected)
    {
        isSoundDetected = detected;
    }

    IEnumerator UpdateTimer()
    {
        while (timeElapsed < duration)
        {
            if (isSoundDetected) // "아" 소리가 감지되면
            {
                timeElapsed += Time.deltaTime; // 시간 증가
                timerImage.fillAmount = Mathf.Clamp01(timeElapsed / duration); // 진행 상황에 따라 fillAmount 조절
            }
            else
            {
                // 소리 감지가 중단되면 타이머 중단
                StopTimer();
                break;
            }
            yield return null;
        }

        if (timeElapsed >= duration)
        {
            timerImage.fillAmount = 1; // 완료 후, 원 완전히 채우기
        }
    }
}
