using UnityEngine;
using UnityEngine.UI;

public class WebcamDisplay : MonoBehaviour
{
    public RawImage rawImage; // RawImage UI 요소를 Inspector에서 연결해야 합니다.

    void Start()
    {
        // 웹캠을 통해 받은 화면을 표시할 WebCamTexture를 생성합니다.
        WebCamTexture webcamTexture = new WebCamTexture();

        // RawImage UI 요소에 WebCamTexture를 할당하여 화면을 표시합니다.
        rawImage.texture = webcamTexture;

        // 웹캠 화면을 시작합니다.
        webcamTexture.Play();

        // 웹캠 화면의 좌우 반전을 위해 RawImage의 UV 좌표를 조정합니다.
        rawImage.uvRect = new Rect(1, 0, -1, 1); // UV 좌표를 반전하여 좌우 반전 효과를 줍니다.

        float zoomFactor = 2.0f;
        Rect currentUVRect = rawImage.uvRect;
        float zoomWidth = currentUVRect.width / zoomFactor;
        float zoomHeight = currentUVRect.height / zoomFactor;
        rawImage.uvRect = new Rect(
            currentUVRect.x + (currentUVRect.width - zoomWidth) / 2,
            currentUVRect.y + (currentUVRect.height - zoomHeight) / 2,
            zoomWidth,
            zoomHeight
        );
    }
}