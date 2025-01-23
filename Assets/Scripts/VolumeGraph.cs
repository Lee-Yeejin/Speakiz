using UnityEngine;
using UnityEngine.UI;

public class VolumeGraph : MonoBehaviour
{
    public LineRenderer lineRenderer;  // LineRenderer 컴포넌트
    public RectTransform graphArea;    // 그래프가 그려질 UI 영역
    private float scaleMultiplier = 50f; // 작은 수치를 확대하기 위한 배율 설정

    void OnEnable()
    {
        UpdateVolumeGraph(); // 피드백 화면이 활성화될 때마다 그래프 업데이트
    }

    public void UpdateVolumeGraph()
    {
        // PlayerPrefs로부터 데이터를 가져오기 전에 로그 추가
        float[] volumes = TrainingFetcher.instance?.GetVolumeLevels() ?? new float[6];
        Debug.Log("Volume levels loaded: " + string.Join(", ", volumes)); // 데이터 확인용 로그

        // 볼륨 데이터가 없거나 유효하지 않을 경우 기본값 설정
        if (volumes != null && volumes.Length > 0)
        {
            DrawGraph(volumes); // LineRenderer의 좌표 설정
        }
        else
        {
            Debug.LogError("Volume data not found or empty.");
        }
    }

    private void DrawGraph(float[] volumes)
    {
        int pointCount = volumes.Length;
        Vector3[] points = new Vector3[pointCount];

        // RectTransform의 크기 가져오기
        Vector2 sizeDelta = graphArea.sizeDelta;

        // 그래프를 그릴 때 사용될 크기 비율 설정
        float xScaleFactor = sizeDelta.x / (pointCount - 1); // x축은 포인트 간의 간격
        float yScaleFactor = sizeDelta.y * scaleMultiplier;  // y축은 볼륨 크기를 배율에 따라 확대

        for (int i = 0; i < pointCount; i++)
        {
            // x 위치는 포인트의 순서, y 위치는 볼륨 값에 비례
            float xPosition = i * xScaleFactor;
            float yPosition = Mathf.Clamp(volumes[i] * yScaleFactor, 0, sizeDelta.y);

            // RectTransform 내부의 좌표를 월드 좌표로 변환
            Vector2 localPoint = new Vector2(xPosition, yPosition);
            Vector3 worldPoint = graphArea.TransformPoint(localPoint);

            points[i] = worldPoint;
        }

        // LineRenderer에 설정된 좌표 반영
        lineRenderer.positionCount = points.Length;
        lineRenderer.SetPositions(points);

        // LineRenderer의 Sorting Layer 설정 (UI 레이어 위로 보이도록)
        lineRenderer.sortingLayerName = "UI";
        lineRenderer.sortingOrder = 5;
    }
}