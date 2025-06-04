using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Animator))]
public class CharacterAnimator : MonoBehaviour
{
    private Animator animator;
    private Transform characterTransform;
    
    // Animator parametre isimleri
    private const string IS_RUNNING_PARAM = "IsRunning";
    
    // Hareket yönü için
    private Vector3 lastPosition;
    private Vector3 movementDirection;
    
    private bool isRunning=false;
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
        characterTransform = transform;
        lastPosition = characterTransform.position;
    }

    private void FixedUpdate()
    {
        CalculateMovementDirection();
        UpdateRotation();
    }

    public void SetRunning(bool canRunning)
    {
        animator.SetBool(IS_RUNNING_PARAM, canRunning);
        isRunning = canRunning;
    }

    private void CalculateMovementDirection()
    {
        if (Vector3.Distance(characterTransform.position, lastPosition) > 0.01f&&isRunning)
        {
            movementDirection = (characterTransform.position - lastPosition).normalized;
            lastPosition = characterTransform.position;
        }
    }

    private void UpdateRotation()
    {
        if (movementDirection != Vector3.zero && isRunning)
        {
            // Yalnızca y ekseninde dönüş (karakterin yatay düzlemde dönmesi)
            Quaternion targetRotation = Quaternion.LookRotation(movementDirection);
            characterTransform.rotation = Quaternion.Slerp(
                characterTransform.rotation,
                targetRotation,
                Time.deltaTime * 10f
            );
        }
    }
    public void ResetRotation()
    {
        // Yalnızca y eksenini sıfırla (karakterin ön yüzünün default yöne bakmasını sağla)
        characterTransform.rotation = Quaternion.Euler(0, 0, 0); // Veya istediğiniz default açı
    }
}