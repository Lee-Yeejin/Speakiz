using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ImageSwitcher : MonoBehaviour
{
    public Canvas informationCanvas; // Information Canvas 참조
    public Image[] images; // 전환할 이미지들
    public float switchInterval = 3f; // 이미지 전환 간격 (초)

    private int currentImageIndex = 0;
    private Coroutine switchRoutine;

    private void OnEnable()
    {
        // 캔버스가 활성화될 때 전환 시작
        if (informationCanvas != null && informationCanvas.gameObject.activeSelf)
        {
            StartImageSwitching();
        }
    }

    private void OnDisable()
    {
        // 캔버스가 비활성화될 때 전환 중단
        StopImageSwitching();
    }

    private void StartImageSwitching()
    {
        if (images.Length > 0 && switchRoutine == null)
        {
            // 모든 이미지를 초기화 (비활성화)
            foreach (var img in images)
            {
                img.gameObject.SetActive(false);
            }

            // 첫 번째 이미지를 활성화
            images[currentImageIndex].gameObject.SetActive(true);

            // 전환 코루틴 시작
            switchRoutine = StartCoroutine(SwitchImages());
        }
    }

    private void StopImageSwitching()
    {
        if (switchRoutine != null)
        {
            StopCoroutine(switchRoutine);
            switchRoutine = null;
        }
    }

    private IEnumerator SwitchImages()
    {
        while (true)
        {
            yield return new WaitForSeconds(switchInterval);

            // 현재 이미지를 비활성화
            images[currentImageIndex].gameObject.SetActive(false);

            // 다음 이미지로 인덱스 변경
            currentImageIndex = (currentImageIndex + 1) % images.Length;

            // 다음 이미지를 활성화
            images[currentImageIndex].gameObject.SetActive(true);
        }
    }
}
