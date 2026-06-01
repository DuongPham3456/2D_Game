using UnityEngine;

public class CharacterAnimationManager : MonoBehaviour
{
    // play anim tương ứng với trạng thái của player

    [SerializeField] Animator _animator;


    public void UpdateMovingParameter(int x, int y)
    {
        _animator.SetFloat("InputX", x);
        _animator.SetFloat("InputY", y);
    }
}

