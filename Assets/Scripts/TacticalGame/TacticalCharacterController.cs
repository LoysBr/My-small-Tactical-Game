using System.Collections;
using UnityEngine;

public class TacticalCharacterController : MonoBehaviour
{
    [SerializeField] private Animator m_Animator;
    [SerializeField] private float m_WalkSpeed = 1f;
    [SerializeField] private float m_StopTargetDistance = 0.1f;
    private float m_SqrStopTargetDistance;

    private Vector3 m_MoveToPosition;
    private Coroutine m_ResettingTriggerAttack;

    private void Start()
    {
        StopMoving();
        m_SqrStopTargetDistance = m_StopTargetDistance * m_StopTargetDistance;

        m_Animator.speed = m_WalkSpeed;
    }

    public void MoveCharacterToSelectedPosition(Vector3 moveToPosition)
    {
        m_MoveToPosition = moveToPosition;

        Vector3 lookAtPos = new Vector3(moveToPosition.x, transform.position.y, moveToPosition.z);
        transform.LookAt(lookAtPos);

        m_Animator.ResetTrigger("TriggerIdle");
        m_Animator.ResetTrigger("TriggerAttack");
        m_Animator.SetTrigger("TriggerWalk");
    }

    public void PlayAttackAnimation()
    {
        m_Animator.ResetTrigger("TriggerIdle");
        m_Animator.ResetTrigger("TriggerWalk");
        m_Animator.SetTrigger("TriggerAttack");

        if (m_ResettingTriggerAttack != null)
        {
            StopCoroutine(m_ResettingTriggerAttack);
        }

        m_ResettingTriggerAttack = StartCoroutine(ResetTriggerAttackAnimation(0.1f));
    }

    private void Update()
    {
        if (Vector3.SqrMagnitude(m_MoveToPosition - transform.position) <= m_SqrStopTargetDistance)
        {
            StopMoving();
        }
    }

    private void StopMoving()
    {
        m_Animator.ResetTrigger("TriggerAttack");
        m_Animator.ResetTrigger("TriggerWalk");
        m_Animator.SetTrigger("TriggerIdle");
    }

    //To be sure once attack it's over Animator will switch back to Idle
    private IEnumerator ResetTriggerAttackAnimation(float delay)
    {
        yield return new WaitForSeconds(delay);
        m_Animator.ResetTrigger("TriggerAttack");
        yield return null;
    }
}
