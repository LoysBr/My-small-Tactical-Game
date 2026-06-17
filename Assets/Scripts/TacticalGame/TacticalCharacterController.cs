using System.Collections;
using UnityEngine;

public class TacticalCharacterController : MonoBehaviour
{
    [SerializeField] private Animator m_animator;
    [SerializeField] private float m_walkSpeed = 1f;
    [SerializeField] private float m_stopTargetDistance = 0.1f;
    private float m_sqrStopTargetDistance;

    private Vector3 m_moveToPosition;
    private Coroutine m_resettingTriggerAttack;

    private void Start()
    {
        StopMoving();
        m_sqrStopTargetDistance = m_stopTargetDistance * m_stopTargetDistance;

        m_animator.speed = m_walkSpeed;
    }

    public void MoveCharacterToSelectedPosition(Vector3 moveToPosition)
    {
        m_moveToPosition = moveToPosition;

        Vector3 lookAtPos = new Vector3(moveToPosition.x, transform.position.y, moveToPosition.z);
        transform.LookAt(lookAtPos);

        m_animator.ResetTrigger("TriggerIdle");
        m_animator.ResetTrigger("TriggerAttack");
        m_animator.SetTrigger("TriggerWalk");
    }

    public void PlayAttackAnimation()
    {
        m_animator.ResetTrigger("TriggerIdle");
        m_animator.ResetTrigger("TriggerWalk");
        m_animator.SetTrigger("TriggerAttack");

        if (m_resettingTriggerAttack != null)
        {
            StopCoroutine(m_resettingTriggerAttack);
        }

        m_resettingTriggerAttack = StartCoroutine(ResetTriggerAttackAnimation(0.1f));
    }

    private void Update()
    {
        if (Vector3.SqrMagnitude(m_moveToPosition - transform.position) <= m_sqrStopTargetDistance)
        {
            StopMoving();
        }
    }

    private void StopMoving()
    {
        m_animator.ResetTrigger("TriggerAttack");
        m_animator.ResetTrigger("TriggerWalk");
        m_animator.SetTrigger("TriggerIdle");
    }

    //To be sure once attack it's over Animator will switch back to Idle
    private IEnumerator ResetTriggerAttackAnimation(float delay)
    {
        yield return new WaitForSeconds(delay);
        m_animator.ResetTrigger("TriggerAttack");
        yield return null;
    }
}
