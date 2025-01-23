using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>(); // Animator 컴포넌트를 가져옵니다.
    }

    // 이동 애니메이션이 끝나면 호출할 메서드
    public void OnMovementEnd()
    {
        animator.SetTrigger("hello"); // 인사 애니메이션으로 전환하는 트리거 설정
    }
}
