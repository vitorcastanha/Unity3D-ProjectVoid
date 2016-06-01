using UnityEngine;
using System.Collections;

public class PlayerAnimationEvents : MonoBehaviour {


    [System.Serializable]
    public class AdjustAnimationSpeed
    {
        [Header ("Choose a multiplier for the speeds below:")]
        public float fRun = 1f;
        public float fJump = 1f;
        public float fLand = 1f;
        public float fSwing01 = 1f;
        public float fSwing02 = 1f;
        public float fComboFinisher = 1f;
        public float fAirAttack = 1f;
        public float fCharge = 1f;
        public float fDashLanding = 1f;
        public float fSpecial = 1f;
        public float fShield = 1f;
        public float fShieldGrowth = 1f;
        public float fHurt01 = 1f;
        public float fHurt02 = 1f;
        public float fHurt03 = 1f;
    }
    [SerializeField] public AdjustAnimationSpeed adjustSpeed;

    [SerializeField]private SkinnedMeshRenderer[] smShield;
    private const float fShieldMaxSize = 150f; //maximum size of the blend shape
    private float fShieldGrowth; //tracks the shield growth percentage

    private bool bMoveExpeption; //this allows the character to move during certain animation moments

    [Header ("Addon by Shallway:")]
    [SerializeField]private Xft.XWeaponTrail swordTrail; //Weapon Trail created by Shallway

    private Player player;

	private void Start () 
    {
        player = Player.instance;
	}
	
	private void Update () 
    {
        UpdateMovement();
        LockModelPosition();
        UpdateSpeed();
        UpdateShieldSize();
	}

    private void UpdateMovement()
    {
        //If the player is doing an attack, he won't be able to move freely at the same time.
        if (player.animationController.CompareAnimationState(PlayerAnimationController.StateInfo.SWING_01)
            || player.animationController.CompareAnimationState(PlayerAnimationController.StateInfo.SWING_02)
            || player.animationController.CompareAnimationState(PlayerAnimationController.StateInfo.COMBO_FINISHER)
            || player.animationController.CompareAnimationState(PlayerAnimationController.StateInfo.AIR_ATTACK)
            || player.animationController.CompareAnimationState(PlayerAnimationController.StateInfo.SPECIAL_ATTACK)
            || player.animationController.CompareAnimationState(PlayerAnimationController.StateInfo.DASH_ATTACK)
            && bMoveExpeption == false)
        {
            player.stats.SetMove(false);
        }
        else if (player.animationController.CompareAnimationState(PlayerAnimationController.StateInfo.CHARGE))
        {
            //Mantain the movement setting.
        }
        else
        {
            //if no attack is happening, then the player is free to move.
            player.stats.SetMove(true);
        }

    }

    private void UpdateSpeed()
    {
        if (player.animationController.CompareAnimationState(PlayerAnimationController.StateInfo.SWING_01))
        {
            player.animationController.SetAnimationSpeed(adjustSpeed.fSwing01);
        }
        else
        {
            SwordHasBeenPicked();
        }

        if (player.animationController.CompareAnimationState(PlayerAnimationController.StateInfo.SWING_02))
        {
            player.animationController.SetAnimationSpeed(adjustSpeed.fSwing02);
        }
        else if (player.animationController.CompareAnimationState(PlayerAnimationController.StateInfo.COMBO_FINISHER))
        {
            player.animationController.SetAnimationSpeed(adjustSpeed.fComboFinisher);
        }
        else if (player.animationController.CompareAnimationState(PlayerAnimationController.StateInfo.SPECIAL_ATTACK))
        {
            player.animationController.SetAnimationSpeed(adjustSpeed.fSpecial);
        }
        else if (player.animationController.CompareAnimationState(PlayerAnimationController.StateInfo.JUMP))
        {
            player.animationController.SetAnimationSpeed(adjustSpeed.fJump);
        }
        else if (player.animationController.CompareAnimationState(PlayerAnimationController.StateInfo.SHIELD_ATTACK))
        {
            player.animationController.SetAnimationSpeed(adjustSpeed.fShield);
        }
        else if (player.animationController.CompareAnimationState(PlayerAnimationController.StateInfo.MOVEMENT_TREE))
        {
            player.animationController.SetAnimationSpeed(adjustSpeed.fRun);
        }
        else if (player.animationController.CompareAnimationState(PlayerAnimationController.StateInfo.HURT_01))
        {
            player.animationController.SetAnimationSpeed(adjustSpeed.fHurt01);
        }
        else if (player.animationController.CompareAnimationState(PlayerAnimationController.StateInfo.HURT_02))
        {
            player.animationController.SetAnimationSpeed(adjustSpeed.fHurt02);
        }
        else if (player.animationController.CompareAnimationState(PlayerAnimationController.StateInfo.HURT_03))
        {
            player.animationController.SetAnimationSpeed(adjustSpeed.fHurt03);
        }
        else if (player.animationController.CompareAnimationState(PlayerAnimationController.StateInfo.CHARGE))
        {
            player.animationController.SetAnimationSpeed(adjustSpeed.fCharge);
        }
        else if (player.animationController.CompareAnimationState(PlayerAnimationController.StateInfo.DASH_ATTACK))
        {
            player.animationController.SetAnimationSpeed(adjustSpeed.fDashLanding);
        }
        else if (player.animationController.CompareAnimationState(PlayerAnimationController.StateInfo.LANDING))
        {
            player.animationController.SetAnimationSpeed(adjustSpeed.fLand);
        }


    }

    private void UpdateShieldSize(){
        if (player.animationController.CompareAnimationState(PlayerAnimationController.StateInfo.SHIELD_ATTACK))
        {
            IncreaseShield();
        }
        else
            DecreaseShield();
    }

    private void IncreaseShield(){
        fShieldGrowth += adjustSpeed.fShieldGrowth * Time.deltaTime;
        if (fShieldGrowth > fShieldMaxSize) {
            fShieldGrowth = fShieldMaxSize;

        }
        //increase the size off all Skinned Mesh Renderers in the shield.
        for (int i = 0; i < smShield.Length; i++) {
            if (smShield[i].sharedMesh.blendShapeCount > 1) {
                smShield[i].SetBlendShapeWeight(1, fShieldGrowth);
            }
        }
    }

    private void DecreaseShield(){
        fShieldGrowth -= adjustSpeed.fShieldGrowth * 2 * Time.deltaTime; //decreasing shield size should be twice as fast.
        if (fShieldGrowth < 0f) {
            fShieldGrowth = 0f;
        }
        for (int i = 0; i < smShield.Length; i++) {
            if (smShield[i].sharedMesh.blendShapeCount > 1) {
                smShield[i].SetBlendShapeWeight(1, fShieldGrowth);
            }
        }
    }

    //This function is called on a animation event key
    public void ResetAttackChain()
    {
        player.combat.ResetAttackChain();
    }

    //This function is called on a animation event key
    public void SetAttackState(PlayerCombat.AttackState attackState)
    {
        player.combat.SetAttackState(attackState);
    }

    //This function is called on a animation event key
    public void PickUpSword(){
        player.stats.SetMove(true);
        player.animationController.SetPickUpSword(true);
        bMoveExpeption = true;
    }

    //This function is called on a animation event key
    public void SwordHasBeenPicked(){
        bMoveExpeption = false;
        player.animationController.SetPickUpSword(false);
    }

    private void LockModelPosition()
    {
        if (!player.animationController.CompareAnimationState(PlayerAnimationController.StateInfo.CHARGE))
        {
            transform.localPosition = new Vector3(transform.localPosition.x, -1f, transform.localPosition.z);
        }
    }

    public void StartTrail()
    {
        swordTrail.Activate ();
    }
    public void FadeTrail(float t)
    {
        swordTrail.StopSmoothly (t);
    }
    public void StopTrail()
    {
        swordTrail.Deactivate ();
    }
}
