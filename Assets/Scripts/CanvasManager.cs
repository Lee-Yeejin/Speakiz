using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CanvasManager : MonoBehaviour
{
    public static CanvasManager instance;

    // 모든 캔버스를 선언
    public Canvas startCanvas;
    public Canvas loginCanvas;
    public Canvas signUpCanvas;
    public Canvas sufinishCanvas;
    public Canvas test1Canvas;
    public Canvas test2Canvas;
    public Canvas test3Canvas;
    public Canvas test4Canvas;
    public Canvas test5Canvas;
    public Canvas levelCanvas;
    public Canvas placeCanvas;
    public Canvas training1Canvas;
    public Canvas training2Canvas;
    public Canvas feedBackCanvas;
    public Canvas BreathFB;
    public Canvas PronunciationFB;
    public Canvas FluencyFB;
    public Canvas parentPasswordCanvas;
    public Canvas informationCanvas;
    public Canvas BT_Canvas;
    public Canvas PT_Canvas;
    public Canvas FT_Canvas;

    //public string BT = "BT_Scene";
    //public string PT = "PT_Scene";
    //public string FT = "FT_Scene";

    private Canvas currentCanvas;
    private Stack<Canvas> canvasStack = new Stack<Canvas>(); // 뒤로가기 스택 추가
    private bool returnToTrainingCanvas = false; // 훈련 캔버스로 돌아가는 플래그

    // 싱글톤 패턴 설정 및 초기화
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            instance = this;
            InitializeFirstCanvas(); // 처음 캔버스 설정
            SceneManager.sceneLoaded += OnSceneLoaded; // 씬이 로드될 때 이벤트 연결
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeFirstCanvas()
    {
        currentCanvas = startCanvas; // 처음 활성화될 캔버스 설정
        currentCanvas.gameObject.SetActive(true);
    }

    // 기존의 캔버스 전환 메서드에 스택 추가
    private void SwitchCanvas(Canvas currentCanvas, Canvas newCanvas)
    {
        if (currentCanvas != null)
        {
            currentCanvas.gameObject.SetActive(false);
            canvasStack.Push(currentCanvas); // 현재 캔버스를 스택에 추가하여 추적
        }

        if (newCanvas != null)
        {
            newCanvas.gameObject.SetActive(true);
            AlignCanvasTransform(newCanvas, currentCanvas);
            this.currentCanvas = newCanvas; // 현재 캔버스 업데이트
        }
    }

    // 뒤로가기를 위한 메서드 추가
    public void GoBack()
    {
        if (canvasStack.Count > 0)
        {
            Canvas previousCanvas = canvasStack.Pop(); // 스택에서 가장 최근 캔버스 추출
            SwitchCanvas(currentCanvas, previousCanvas); // 스택에 추가하지 않고 전환
        }
    }

    // 캔버스 전환 시 위치, 회전, 크기 동기화
    private void AlignCanvasTransform(Canvas newCanvas, Canvas oldCanvas)
    {
        if (oldCanvas != null)
        {
            newCanvas.transform.position = oldCanvas.transform.position;
            newCanvas.transform.rotation = oldCanvas.transform.rotation;
            newCanvas.transform.localScale = oldCanvas.transform.localScale;
        }
    }

    // 로그인 -> Training1으로 전환
    public void SwitchToTraining1()
    {
        SwitchCanvas(currentCanvas, training1Canvas);
    }

    // 로그인 -> SignUp으로 전환
    public void SwitchToSignUp()
    {
        SwitchCanvas(currentCanvas, signUpCanvas);
    }

    // Start -> Login Canvas 전환
    public void SwitchToLogin()
    {
        SwitchCanvas(currentCanvas, loginCanvas);
    }

    // sufinish -> Test1 Canvas 전환
    public void SwitchToTest1()
    {
        SwitchCanvas(currentCanvas, test1Canvas);
    }

    // Test1 -> Test2 Canvas 전환
    public void SwitchToTest2()
    {
        SwitchCanvas(currentCanvas, test2Canvas);
    }

    // Test2 -> Test3 Canvas 전환
    public void SwitchToTest3()
    {
        SwitchCanvas(currentCanvas, test3Canvas);
    }

    // Test3 -> Test4 Canvas 전환
    public void SwitchToTest4()
    {
        SwitchCanvas(currentCanvas, test4Canvas);
    }

    // Test4 -> Test5 Canvas 전환
    public void SwitchToTest5()
    {
        SwitchCanvas(currentCanvas, test5Canvas);
    }

    // Test5 -> Level Canvas 전환
    public void SwitchToLevel()
    {
        SwitchCanvas(currentCanvas, levelCanvas);
    }

    // Place -> Training1 Canvas 전환
    public void ShowTraining1Canvas()
    {
        SwitchCanvas(currentCanvas, training1Canvas);
    }

    // Place -> Training2 Canvas 전환
    public void ShowTraining2Canvas()
    {
        SwitchCanvas(currentCanvas, training2Canvas);
    }

    // Feedback Canvas에서 목표 Canvas들로 전환
    public void SwitchToBreathFB()
    {
        SwitchCanvas(feedBackCanvas, BreathFB);
    }

    public void SwitchToPronunciationFB()
    {
        SwitchCanvas(feedBackCanvas, PronunciationFB);
    }

    public void SwitchToFluencyFB()
    {
        SwitchCanvas(feedBackCanvas, FluencyFB);
    }

    // 학부모 비밀번호 캔버스 -> 피드백 캔버스 전환
    public void SwitchToFeedbackCanvas()
    {
        SwitchCanvas(currentCanvas, feedBackCanvas);
    }

    // SignUp -> sufinish 캔버스 전환
    public void SwitchToSufinish()
    {
        SwitchCanvas(currentCanvas, sufinishCanvas);
    }

    // Place Canvas로 전환
    public void SwitchToPlace()
    {
        SwitchCanvas(currentCanvas, placeCanvas);
    }

    // information Canvas로 전환
    public void SwitchToInformationCanvas()
    {
        SwitchCanvas(currentCanvas, informationCanvas);

        // ImageSwitcher가 있다면 OnEnable 호출
        var imageSwitcher = informationCanvas.GetComponent<ImageSwitcher>();
        if (imageSwitcher != null)
        {
            imageSwitcher.enabled = true;
        }
    }


    // 게임 씬으로 전환하고 돌아올 때 특정 캔버스 활성화
    public void SwitchToGameScene1()
    {
        /* returnToTrainingCanvas = true; // 돌아올 때 TrainingCanvas를 활성화하도록 설정
         SceneManager.LoadScene(BT);*/
        SwitchCanvas(currentCanvas, BT_Canvas);
    }
    public void SwitchToGameScene2()
    {
        //returnToTrainingCanvas = true;
        //SceneManager.LoadScene(PT);
        SwitchCanvas(currentCanvas, PT_Canvas);
    }
    public void SwitchToGameScene3()
    {
        // TrainingCanvas 비활성화
        GameObject trainingCanvas = GameObject.Find("Training"); // TrainingCanvas 이름 확인 필요
        if (trainingCanvas != null)
        {
            trainingCanvas.SetActive(false);
            Debug.Log("TrainingCanvas 비활성화 완료");
        }
        else
        {
            Debug.LogWarning("TrainingCanvas를 찾을 수 없습니다.");
        }
        // FT_Canvas 활성화
        if (FT_Canvas != null)
        {
            FT_Canvas.gameObject.SetActive(true); // FT_Canvas 활성화

            // FT_Canvas 안의 Canvas(1)로 전환
            Transform firstCanvas = FT_Canvas.transform.Find("Canvas (1)");
            if (firstCanvas != null)
            {
                firstCanvas.gameObject.SetActive(true); // Canvas(1) 활성화
                currentCanvas = firstCanvas.GetComponent<Canvas>(); // 현재 캔버스 업데이트
            }
            else
            {
                Debug.LogError("Canvas(1)가 FT_Canvas 안에 존재하지 않습니다.");
            }
        }
        else
        {
            Debug.LogError("FT_Canvas가 설정되지 않았습니다.");
        }
    }


    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Main" && returnToTrainingCanvas)
        {
            returnToTrainingCanvas = false;
            ShowTraining1Canvas();
        }
        else if (scene.name == "Main")
        {
            InitializeFirstCanvas();
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

   /* public void ActivateTrainingCanvas()
    {
        if (training1Canvas != null)
        {
            SwitchCanvas(currentCanvas, training1Canvas);
        }
        else
        {
            Debug.LogError("TrainingCanvas가 설정되지 않았습니다.");
        }
    }*/
}
