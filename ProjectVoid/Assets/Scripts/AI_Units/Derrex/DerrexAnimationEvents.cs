using UnityEngine;
using System.Collections;

public class DerrexAnimationEvents : MonoBehaviour {

    private UnitDerrex unit;

    [System.Serializable]
    public class AdjustAnimationSpeed
    {
        [Header ("Choose a multiplier for the speeds below:")]
        public float fIdleSpeed = 1f;
        public float fMoveSpeed = 1f;
        public float fHurtSpeed = 1f;
        public float fAttackSpeed = 1f;
    }
    [SerializeField] public AdjustAnimationSpeed adjustSpeed;

	void Start () 
    {
        unit = GetComponentInParent<UnitDerrex>();
	}
	
    public void DoFire()
    {
        unit.DoAttack();
    }

    public void SetIdleSpeed()
    {
        unit.animationController.SetAnimationSpeed(adjustSpeed.fIdleSpeed);
    }

    public void SetMoveSpeed()
    {
        unit.animationController.SetAnimationSpeed(adjustSpeed.fMoveSpeed);
    }

    public void SetHurtSpeed()
    {
        unit.animationController.SetAnimationSpeed(adjustSpeed.fHurtSpeed);
    }

    public void SetAttackSpeed()
    {
        unit.animationController.SetAnimationSpeed(adjustSpeed.fAttackSpeed);
    }

    public void SetStunToFalse()
    {
        unit.animationController.SetStun(false);
    }
    
}
