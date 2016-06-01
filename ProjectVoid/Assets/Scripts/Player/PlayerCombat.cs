using UnityEngine;
using System.Collections;

public class PlayerCombat : PlayerScript {

    public enum AttackState
    {
        READY,
        IN_PROGRESS,
        INTERRUPTED
    }
    public AttackState attackState;

    private enum SelectAttack
    {
        DASH_ATTACK,
        SPECIAL_ATTACK,
        AIR_ATTACK,
        NO_ATTACK,
        SWING_01,
        SWING_02,
        COMBO_FINISHER
    }
    private SelectAttack selectedAttack;

    private bool bShieldBlock;
    private bool bAirAttack; //tracks if the air attack is in progress to perform the dive motion.

	void Start () {
        selectedAttack = SelectAttack.NO_ATTACK;
	}
	
	void Update () {

        //Combo_ready occurs at certain frames during an attack, allowing the player to chain swings
        if (attackState == AttackState.READY)
        {
            //AREA ATTACK
            if (Input.GetKeyDown(KeyCode.R) && !bShieldBlock) 
            {

                ReadyASpeacialAttack();//Set the next attack to be a special if the player has enough stamina

            }
                
            //SWING ATTACK
            if (Input.GetButtonDown("Fire1") && !bShieldBlock) {
                AttackCheck();
            }

            //DASH ATTACK
            if (Input.GetKeyDown(KeyCode.F) && player.groundCheck.IsGrounded()) {
                ReadyADashAttack(); //Set the next attack to be a dash one
            }

        }

        //SHIELD
        if (Input.GetKey(KeyCode.Mouse1) && player.groundCheck.IsGrounded()) {

            bShieldBlock = true;
            player.animationController.SetShieldBlock(true);

            //Adjust player camera based on controller being used
            PlayerCameraCorrection();
        }else{
            bShieldBlock = false;
            player.animationController.SetShieldBlock(false);
        }
	}

    private void AttackCheck(){

        //Adjust player camera based on controller being used
        PlayerCameraCorrection();

        //If player is grounded do a ground attack, else do an air attack
        if (player.groundCheck.IsGrounded()) 
        {
            DoGroundAttack();
        }
        else if(bAirAttack == false)
        {
            if (selectedAttack == SelectAttack.SPECIAL_ATTACK) {
                return;
            }
            bAirAttack = true;
            attackState = AttackState.IN_PROGRESS;

            player.animationController.SetAirAttack();
        }
    }

    //Ground attacks
    private void DoGroundAttack ()
    {
        //stop plyer from spamming attacks
        attackState = AttackState.IN_PROGRESS;

        //combo counter
        if ((int)selectedAttack >= (int)SelectAttack.NO_ATTACK)
        {
            selectedAttack++;
        }
       
        //there is only 3 attacks in the combo
        if ((int)selectedAttack > (int)SelectAttack.COMBO_FINISHER) {
            selectedAttack = SelectAttack.SWING_01;
        }

        if (selectedAttack == SelectAttack.SWING_01) 
        {
            if (player.animationController.CompareAnimationState(PlayerAnimationController.StateInfo.SWING_01)) 
            {
                //if the attack is ready but the character is still in this animation, then he is stuck.
                //force play the attack again to carry on with the animations and set mecanim free.
                player.animationController.ForcePlaySwing01();
                return;
            }
            player.animationController.SetSwing01();
        }
        else if (selectedAttack == SelectAttack.SWING_02) 
        {
            //This attack is meant to be a combo sequence. If the previous possible attacks are not happening, then this attack wont either.
            if (player.animationController.CompareAnimationState(PlayerAnimationController.StateInfo.SWING_01) == false
                && player.animationController.CompareAnimationState(PlayerAnimationController.StateInfo.CHARGE) == false)
            {
                return;
            }
            player.animationController.SetSwing02();
        }
        else if (selectedAttack == SelectAttack.COMBO_FINISHER) 
        {
            //This attack is meant to be a combo sequence. If the previous possible attacks are not happening, then this attack wont either.
            if (player.animationController.CompareAnimationState(PlayerAnimationController.StateInfo.SWING_02) == false) {
                return;
            }
            player.animationController.SetComboFinisher();
        }
        else if (selectedAttack == SelectAttack.SPECIAL_ATTACK) 
        {
            player.animationController.SetSpecialAttack();
            ResetAttackChain();
        }
        else if (selectedAttack == SelectAttack.DASH_ATTACK) 
        {
            player.animationController.SetDashAttack();
            ResetAttackChain();
        }
    }

    /// <summary>
    /// Resets the attack chain.
    /// </summary>
    public void ResetAttackChain()
    {
        //resets back to the first attack in the combo.
        selectedAttack = SelectAttack.NO_ATTACK;
    }

    /// <summary>
    /// Sets the state of the attack.
    /// </summary>
    /// <param name="attackState">Attack state.</param>
    public void SetAttackState(AttackState attackState)
    {
        this.attackState = attackState;
    }

    //Set the next attack to be a special if the player has enough stamina
    private void ReadyASpeacialAttack()
    {
        if (player.stats.GetCurrentMagic() >= 30) 
        {
            selectedAttack = SelectAttack.SPECIAL_ATTACK;
            player.stats.AddMagic(-30f);
            AttackCheck();
        }
    }

    //Set the next attack to be a dash one
    private void ReadyADashAttack()
    {
        selectedAttack = SelectAttack.DASH_ATTACK;
        AttackCheck();
    }

    /*
    * If the player is using a controller, rotates the character towards the nearest enemy,
    * if he is using mouse and keyboard, rotate him towards the camera direction
    */
    void PlayerCameraCorrection()
    {
        /*if (!bControllerActive) {
            TurnTowardsCameraForward ();
        }else{
            TurnTowardsNearEnemy ();
        }*/
    }
}
