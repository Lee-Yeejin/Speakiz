using UnityEngine;
using System.Collections;

public class PanelController : MonoBehaviour
{
    public GameObject panel1;
    public GameObject panel2;
    public GameObject panel3;
    public GameObject button;
    private Animator panel1Animator;
    private int repeatCount = 0; // 애니메이션 반복 횟수

    void Start()
    {
        // 초기에는 panel1만 활성화되어 있어야 함
        panel1.SetActive(true);
        panel2.SetActive(false);
        panel3.SetActive(false);
        button.SetActive(false);

        // Animator 컴포넌트 가져오기
        panel1Animator = panel1.GetComponent<Animator>();

        // 애니메이션 이름 확인 (디버그 로그)
        Debug.Log("Panel1 Animation Name: " + panel1Animator.GetCurrentAnimatorClipInfo(0)[0].clip.name);
    }


    // panel1 애니메이션 완료 후 호출될 함수 (버튼 활성화 조건 제거됨)
    public void OnPanel1AnimationEnd()
    {
        // panel1을 활성화하고
        panel1.SetActive(true);

        // panel2와 panel3를 활성화하고
        panel2.SetActive(true);
        panel3.SetActive(true);
    }

    // 버튼 클릭 시 호출될 함수
    public void OnButtonClick()
    {
        if (repeatCount < 2)
        {
            // 현재 활성화된 panel2와 panel3를 비활성화
            panel1.SetActive(false);
            panel2.SetActive(false);
            panel3.SetActive(false);
            button.SetActive(false);


            // panel1을 다시 활성화하고 애니메이션 재생
            panel1.SetActive(true);
            panel1Animator.Play("Panel1Animation"); // "Panel1Animation"을 실제 애니메이션 이름으로 변경
        }
    }
}