using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UnitPlayer : MonoBehaviour
{
	static public GameObject goPlayerObject; //player instance

	public bool debug; //activate debug mode
	public bool bControllerActive; //player is using a Xbox controller
	public ParticleSystem shieldHitPar; //shield particle effect
	private bool bShieldHitPar; //when true activates shield hit particle
	//public Flashscreen flashScreen;//screen flash when character gets hit

	#region Stats
	[Header ("Stats")]
	[HideInInspector]public float fCurrentSpeed; //character current speed
	public float fRunSpeed = 2f; //character run speed
	public float fJumpSpeed = 5f; //character jump speed
	public float fDiveSpeed = 10f; //character falling speed when doing a dive attack
	private float fTempRate = 1f; //temporary desceleration rate that is applyed during a charged attack.
	public float fLife;
	[HideInInspector] public float fStamina = 90f; //special attack resource
	[HideInInspector] public bool bPlayerOutOfStamina; //is player out of stamina
	public float fStationaryTurnSpeed = 180;
	public float fMovingTurnSpeed = 360;
	
	[SerializeField] public CombatStats combatStats;
	[System.Serializable]
	public class CombatStats
	{
		public float fComboAttacksRange;
		public float fComboAttackDamage;
		
		public float fAttack01Angle; //regular swing area
		public float fAttack02Angle; //regular swing area for second hit in the sequence
		public float fComboStunDuration; //how long do enemies get stunned when hit by this attack

		public float fDashAttacksRange; //distance to be moved when dashing
		public float fDashAttacksAngle; //dash area of effect

		public float fAreaAttackRange; //special attack range
		public float fAreaAttackDamage; //special attack damage
		public float fAreaHoldTimer; //how long do you have to hold mouse button to activate the special attack
		public float fAreaStunDuration; //stun duration on enemies hit

		public float fAirAttackDamage; //air attack damage
		public float fAirAttackAngle; //air attack area
		public float fAirAttackRange; //air attack reach
		public float fAirStunDuration; //stun duration on enemies hit
	}
	#endregion

	#region Tweaks
	[Header ("Tweaks")]
	public float fAnimationSpeed; //animation speed multiplier
	public bool bAirControl; //set air control active
	private Vector3 v3Move; //character movement vector
	private Vector3 v3LookPosition = Vector3.zero; //look position vector

	#endregion

	#region Combat
	[Header ("Attacks")]
	[HideInInspector]public int iComboCounter = 0; //tracks which attack in the combo sequence is being used. Nowadays I would just make a Enum for this.
	public LayerMask lmTargetLayers; //layers that can be affected by the attack Ray
	private RaycastHit rhHit; 
	private bool bAttackReady = true; //Checked at end of animation
	private bool bComboReady = true; //Checked at the middle of animation. Means that the player can interrupt current animation and blend to the next one immediatly
	private bool bCooldownReady = true; //Checked in code.
	private bool bFiring;
	private bool bAirAttack; //executing air attack
	private bool bButtonStillDown; //if holding button for special attack
	private float fAreaAttackCounter; //how long has the button being held
	[HideInInspector]public bool bShieldBlock; //is the character blocking
	[HideInInspector] public float fBlockingTime = 0f; //how long has he blocking for. This is important because the bouncing back mechanic its a timing one
	private const float fShieldReflectingWindow = 0.7f; //how long can you reflect a projectile after starting blocking
	#endregion

	#region Internal
	[HideInInspector]public bool bHit; //if player's attack hits something
	[Header ("Implementation variables")]
	public Transform tShield; //reference for the shield transform
	private float fRateOfSwing = 0.1f; //how often can you swing the weapon
	[HideInInspector]public bool bEnableMovement = true; //if player is allowed to move
	[HideInInspector]public bool bStunned = false; //check if player is free to act
	[HideInInspector] public float fCurrentLife;
	[HideInInspector] public float fCurrentStamina;
	public GameObject goPlayerGroup; //player gameObject group reference
	[HideInInspector]public Animator anim;
	
	public GameObject goAirAttackParticle; //air attack particle reference
	public GameObject goEnemyBlood; //enemy blood particle reference. Spawns on weapon hit
	public GameObject goHitEffectBuildings; //hit particle for non-living objects.

	private float fTurnAmount;
	private float fForwardAmount;

	public GameObject goDirt;//dirt particle when landing

	public Texture txtCrosshair; //cross hair texture
	private bool bShowCHair; //if cross hair is being displayed
	private bool bIsGrounded;
	private PhysicMaterial pmRegularFriction;
	[HideInInspector]public bool bCancelAttacks; // This tells the player not to use attacks for the frame that he is interacting with objects in the scene.
	private CapsuleCollider capCollider; //player capsule collider
	private GameObject goMainCamera; //reference to the main camera
	private float fMovingTimer; //time spent only moving. If moving for a long time, when the player attacks a different momentum animation plays

	[SerializeField] public DeathVariables deathVariables;
	[System.Serializable]
	public class DeathVariables
	{
		public bool isDead;
		public Rigidbody rRagdoll; //ragdoll reference
		public Image deathScreen; //fade to red
		public Image text1; //death text
	}


	#endregion

	private void Awake(){
		goPlayerObject = this.gameObject;
	}
	
	//Instantiate variables
	private void Start ()
	{
		anim = GetComponentInChildren<Animator>();
		capCollider = GetComponent<CapsuleCollider>();
		pmRegularFriction = Resources.Load ("PhysicMaterials/Regular", typeof(PhysicMaterial)) as PhysicMaterial;
		fCurrentLife = fLife;
		fCurrentStamina = fStamina;
		anim.speed = fAnimationSpeed;
		fCurrentSpeed = fRunSpeed;
		goMainCamera = Camera.main.gameObject;
		
		//Check with the checkpoint manager if player has already aquired the shield. If so spawn him with it.
		if (CheckPointManager._playerHasShield) {
			tShield.gameObject.SetActive(true);
		}
		
	}

	private void Update ()
	{
		//skip while finishing loading
		if (ProjectViking.gameIsLoading == true) {
			return;
		}

		IsGrounded (); //check if grounded

		//If player is dead play audio and reset level uppon hitting return.
		if (deathVariables.isDead) {
			if (Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter)) {
				Application.LoadLevel(1);
			}
			return;
		}
		
		//allow player to attack once animations play out
		if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 1.1f) {
			AttackIsReady();
		}
		
		//If the game is paused then unlock cursor
		if (ProjectViking.hitPause) {
			Screen.lockCursor = false;
			return;
		}else{
			Screen.lockCursor = true;
		}
		
		
		if (fCurrentLife <= 0) {
			Death();
		}
		
		//stamina recovery
		fCurrentStamina += Time.deltaTime * 0.5f;
		if (fCurrentStamina > fStamina) {
			fCurrentStamina = fStamina;
		}

		//JUMP
		if (Input.GetButtonDown("Jump") && bIsGrounded && !ProjectViking.operatingTurret) {
			if (!GetComponent<Rigidbody>().isKinematic) {
				GetComponent<Rigidbody>().velocity += Vector3.up * fJumpSpeed;
			}
			if (!IsInvoking("SetJumpingTrue")) {
				Invoke("SetJumpingTrue", 0.1f);
			}
		}
		
		//If player is not in turret mode or the game is not paused, UpdateAttack
		if (!ProjectViking.operatingTurret && !ProjectViking.hitPause) {
			UpdateAttack ();
		}
		
		
		UpdateShieldBouncingFeedBack ();

	}

	private void FixedUpdate(){
		if (ProjectViking.gameIsLoading == true) {
			return;
		}
		if (deathVariables.isDead) {
			return;
		}
		//if player is not in turret mode then allow movement
		if (!ProjectViking.operatingTurret && !ProjectViking.hitPause) {
			MovementController();
		}else if (ProjectViking.operatingTurret) {
			anim.SetFloat ("fMovementSpeed", 0f);
		}
	}
	
	//Get ragdoll ready
	private void EnableRagdoll(){
		anim.enabled = false;
		capCollider.enabled = false;
		deathVariables.rRagdoll.isKinematic = false;
		bEnableMovement = false;
	}

	//Fade to red upon death
	private IEnumerator FadeToRed(){
		float i = 0;
		deathVariables.deathScreen.gameObject.SetActive (true);
		while (i < 1f) {
			i += Time.deltaTime;
			deathVariables.deathScreen.color = Color.Lerp(deathVariables.deathScreen.color, new Color(deathVariables.deathScreen.color.r, deathVariables.deathScreen.color.g,
			                                                                                          deathVariables.deathScreen.color.b, 0.39f), i);
			yield return null;
		}
		deathVariables.text1.enabled = true;
		AkSoundEngine.SetState ("Mx", "Lose_Mx"); //Play sound
		yield return null;
	}

	//This method handles player death
	private void Death(){
		ProjectViking.bDeath = true;
		ProjectViking.iCurrentPoint -= 1500; //deduct points for dying
		if (ProjectViking.iCurrentPoint < 0) {
			ProjectViking.iCurrentPoint = 0;
		}
		ProjectViking.iDeath++; //tracks number of deaths
		GetComponent<Collider>().material = pmRegularFriction;
		deathVariables.isDead = true;
		GetComponent<Rigidbody> ().isKinematic = true;
		StartCoroutine (FadeToRed()); //fade screen to red
		EnableRagdoll ();
	}

	#region Combat

	private void UpdateAttack(){
		//If stunned player cannot attack
		if (bStunned) {
			return;
		}
		
		//if attack is free to be used. 
		if ((bAttackReady || bComboReady) && bCooldownReady && !bCancelAttacks) {

			//AREA ATTACK
			if (Input.GetButton("Fire1") && !bShieldBlock && CheckPointManager._playerHasMagic) {
				fAreaAttackCounter += Time.deltaTime; //counts how long the button has been held
				if (fAreaAttackCounter > combatStats.fAreaHoldTimer && !bButtonStillDown) {
					//if held for more than the AreaHoldTimer, immediatly fire the special attack
					
					ReadyASpeacialAttack();//Set the next attack to be a special if the player has enough stamina
				}
			}
			
			//CONTROLLER
			if (Input.GetButtonDown("JFire3") && !bShieldBlock && CheckPointManager._playerHasMagic) {
				ReadyASpeacialAttack();//Set the next attack to be a special if the player has enough stamina
			}
			//SWING ATTACK
			if (Input.GetButtonUp("Fire1") && !bShieldBlock) {
				//If button is up before the special attack timer, do a regular attack
				if (fAreaAttackCounter < combatStats.fAreaHoldTimer) {
					AttackCheck();
				}
				fAreaAttackCounter = 0f; //resets the special attack timer
				bButtonStillDown = false; 
			}
			//CONTROLLER
			if (Input.GetButtonDown("JFire1") && !bShieldBlock) {
				AttackCheck();
			}
			//DASH ATTACK
			if (Input.GetKeyDown(KeyCode.F) && CheckPointManager._playerHasShield && bIsGrounded) {
				ReadyADashAttack(); //Set the next attack to be a dash one
			}
			//CONTROLLER
			if (Input.GetButtonDown("JFire2") && !bShieldBlock) {
				ReadyADashAttack(); //Set the next attack to be a dash one
			}

		}

		bCancelAttacks = false;

		//CONTROLLER Move camera
		if (Input.GetButton("JLookFor")) {
			TurnCameraForward();
		}

		//SHIELD
		AnimatorStateInfo currentState = anim.GetCurrentAnimatorStateInfo (0); //tracks animation state
		//if player is on the ground and has already aquired the shield
		if ((Input.GetKey(KeyCode.Mouse1) || Input.GetButton("JShield")) && CheckPointManager._playerHasShield && bIsGrounded) {
			if ((currentState.nameHash == Animator.StringToHash("Base Layer.Shield") ||
			     currentState.nameHash == Animator.StringToHash("Base Layer.Shield Move Tree") ||
			     currentState.nameHash == Animator.StringToHash("Base Layer.UlfarShieldBounce"))) {
				fBlockingTime += Time.deltaTime; //if the player is in one of the shield animations, track how long he've been blocking
			}
			
			//Show a crosshair during a window of time that the shield has been held. This is when the shield can reflect projectiles
			if (fBlockingTime < fShieldReflectingWindow) {
				bShowCHair = true;
			}else{
				bShowCHair = false;
			}
			
			bShieldBlock = true;
			anim.SetBool("bShieldActive", true);
			
			//Adjust player camera based on controller being used
			PlayerCameraCorrection();
		}else{
			bShowCHair = false;
			fBlockingTime = 0f;
			bShieldBlock = false;
			anim.SetBool("bShieldActive", false);
		}
	}
	
	/*
	* If the player is using a controller, rotates the character towards the nearest enemy,
	* if he is using mouse and keyboard, rotate him towards the camera direction
	*/
	void PlayerCameraCorrection()
	{
		if (!bControllerActive) {
			TurnTowardsCameraForward ();
		}else{
			TurnTowardsNearEnemy ();
		}
	}
	
	//Set the next attack to be a special if the player has enough stamina
	private void ReadyASpeacialAttack()
	{
		if (fCurrentStamina >= 30) {
			iComboCounter = -1; //This code will enable the Special attack in the DoGroundAttack method.
			fCurrentStamina -= 30;
			AttackCheck();
		}else{
			bPlayerOutOfStamina = true;
		}
	}
	
	//Set the next attack to be a dash one
	private void ReadyADashAttack()
	{
		iComboCounter = -2;
		AttackCheck();
	}
	
	
	private void UpdateShieldBouncingFeedBack(){
		AnimatorStateInfo currentState = anim.GetCurrentAnimatorStateInfo (0); //tracks animation states
		
		//if the player blocking time is smaller than the fShieldReflectingWindow and he is in a blocking animation, reflect projectile
		if (fBlockingTime > 0f && fBlockingTime < fShieldReflectingWindow  && (currentState.nameHash == Animator.StringToHash("Base Layer.Shield") ||
		                                                    currentState.nameHash == Animator.StringToHash("Base Layer.Shield Move Tree") ||
		                                                    currentState.nameHash == Animator.StringToHash("Base Layer.UlfarShieldBounce"))) {
			AkSoundEngine.PostEvent("Shield_Using",gameObject);//play sound
			UlfarBounceFeedBackshield.instance.Grow();//Increases shield size to hint user when the mechanic is active
		}else{
			UlfarBounceFeedBackshield.instance.Restore();//Decrease shield size to hint the window is gone
		}
	}
	
	//Attack cooldown tracker
	private IEnumerator RateOfSwing(){
		bCooldownReady = false;
		float counter = fRateOfSwing;
		while (counter > 0) {
			counter -= Time.deltaTime;
			yield return null;
		}
		bCooldownReady = true;
		yield return null;
	}
	
	//Player can attack
	public void AttackIsReady(){
		bAttackReady = true;
	}
	
	//Player can interrupt attack animation and blend to next animation in the combo sequence
	public void ComboIsReady(){
		bComboReady = true;
	}
	
	/*
	* This method sets the stage for the next attack
	* 
	*/
	private void AttackCheck(){
		
		//Adjust player camera based on controller being used
		PlayerCameraCorrection();
		
		//Starts the cooldown for the player attack
		StartCoroutine (RateOfSwing());
		
		//If player is grounded do a ground attack, else do an air attack
		if (bIsGrounded) {
			DoGroundAttack();
		}else if(bAirAttack == false){
			if (iComboCounter == -1) {
				return;
			}
			bAirAttack = true;
			bAttackReady = false;

			anim.SetTrigger("tAttackAir");
		}
	}
	
	//Ground attacks
	private void DoGroundAttack ()
	{
		//stop plyer from spamming attacks
		bAttackReady = false;
		bComboReady = false;
		
		//combo counter
		iComboCounter++;
		//there is only 3 attacks in the combo
		if (iComboCounter > 3) {
			iComboCounter = 1;
		}
		
		AnimatorStateInfo currentState = anim.GetCurrentAnimatorStateInfo (0); //track animation state
		
		if (iComboCounter == 1) { //First attack
			if (currentState.nameHash == Animator.StringToHash("Base Layer.UlfarAttack01")) {
				anim.Play("UlfarAttack01", -1, 0f);
				return;
			}
			anim.SetTrigger("tAttack1");
		}else if (iComboCounter == 2) { //Second attack
			if (currentState.nameHash != Animator.StringToHash("Base Layer.UlfarAttack01") && currentState.nameHash != Animator.StringToHash("Base Layer.ChargeAttack")) {
				return;
			}
			anim.SetTrigger("tAttack2");
		}else if (iComboCounter == 3) { //Finisher
			if (currentState.nameHash != Animator.StringToHash("Base Layer.UlfarAttack02")) {
				return;
			}
			anim.SetTrigger("tAttack3");
		}else if (iComboCounter == 0) { //Special Attack! Combo counter will start at -1 to enable this attack
			TutorialPop.instance.Deactivate(2f); //deactivates the tutorial message after 2 seconds that the attack is used for the first time.
			anim.SetTrigger("tAttackStomp");
			bButtonStillDown = true;
		}else if (iComboCounter == -1) {//Dash attack! Combo counter will start at -2 to enable this attack
			anim.SetTrigger("tDashAttack");
			iComboCounter++;
		}
	}

	//This function is called by a animation key event
	public void DoAttack(int iComboCounter){
		Collider[] sphereHit;
		//Find all hittable objects around the character.
		sphereHit = Physics.OverlapSphere (transform.position, combatStats.fComboAttacksRange, lmTargetLayers);
		
		foreach (Collider enemyHit in sphereHit) {
			if (enemyHit.GetComponent<Collider>().tag == "Enemy") {
				bHit = true; //show particle
				
				Vector3 enemyPosition = enemyHit.transform.position - transform.position; //find enemy position relative to the character
				enemyPosition = new Vector3(enemyPosition.x, 0f, enemyPosition.z); //ignores Y axis
				float enemyDot = Vector3.Dot(transform.forward.normalized, enemyPosition.normalized); //find the dot value between characters
				
				//Depending on the attack,the enemy will be affected in a way.
				if(iComboCounter == 0 || iComboCounter == 1) //the first two attacks are the same programatically
				{
					RegularAttackEffect(enemyDot, enemyPosition, enemyHit);
				}
				else if(iComboCounter == 2)
				{
					FinisherAttackEffect(enemyPosition, enemyHit); //Last attack in the combo chain
				}
				else if(iComboCounter == -1)
				{
					DashAttackEffect(enemyDot, enemyPosition, enemyHit); //Dash attack
				}else if(iComboCounter == -2)
				{
					AirAttackEffect(enemyDot, enemyPosition, enemyHit); //Air attack
				}

			}
			//If the player hits an obstacle instead, 
			else if (enemyHit.GetComponent<Collider>().tag.Contains("ObjectWithHealth")) {
				DoCameraShake(5f); //camera shake
				enemyHit.GetComponent<Collider>().GetComponent<EventTakeDamage>().TakeDamage(combatStats.fComboAttackDamage); //apply damage on the event script.
				Barrel bar = enemyHit.GetComponent<Collider>().GetComponent<Barrel>();
				if (bar != null) {
					bar.TakeDamage(35f, (enemyHit.transform.position - transform.position).normalized); //apply knockback
				}
			}
		}

	}
	
	private void DoCameraShake(float intensity)
	{
		UnitySampleAssets.Cameras.ProtectCameraFromWallClip.instance.StartCameraShake (intensity);
	}
	//This function is called by a animation key event
	public void DoDashAttack(int iComboCounter){
		bShieldHitPar = false; //deactivates particle tracker. This will be activated again if the attack lands 
		
		DoAttack(iComboCounter); //Reuse above function for damage and knockback calculations
		
		//set the particle effect to be active
		if(shieldHitPar.gameObject.activeSelf == false)
		{
			shieldHitPar.gameObject.SetActive(true); //Enable particle effect
		}
		//if particle is already active, re-start particle effect and sound
		else if (bShieldHitPar && !shieldHitPar.isPlaying)
		{
			shieldHitPar.Play();
			AkSoundEngine.PostEvent("Shield_Impact",gameObject);
		}
	}
	
	//This function is called by an animation key event
	public void DoAirAttack()
	{
		DoAttack(-2);//Starts the air attack;
	}
	
	//checks if a regular attack hits and apply effects.
	private void RegularAttackEffect(float enemyDot, Vector3 enemyPosition, Collider enemyHit)
	{
		if(enemyDot > combatStats.fAttack01Angle)
		{
			HitEnemy(enemyHit, enemyPosition, combatStats.fComboAttackDamage); //apply damage
			UnitAI enemyFunctions = enemyHit.GetComponent<UnitAI>();
			enemyFunctions.Knockback(enemyPosition, 8f); //apply knockback
		}
	}
	
	//apply effects of a finisher attack. Area is 360, so every enemy around gets hit.
	private void FinisherAttackEffect(Vector3 enemyPosition, Collider enemyHit)
	{
		HitEnemy(enemyHit, enemyPosition, combatStats.fComboAttackDamage * 2f, false, true); //apply damage
		UnitAI enemyFunctions = enemyHit.GetComponent<UnitAI>();
		enemyFunctions.Knockback(enemyPosition, 4f, true); //apply knockback
	}

	//check if dash hits and apply effects.
	private void DashAttackEffect(float enemyDot, Vector3 enemyPosition, Collider enemyHit)
	{
		if (enemyDot > combatStats.fDashAttacksAngle) {
			bShieldHitPar = true; //enables particle tracker
			UnitAI enemyFunctions = enemyHit.GetComponent<UnitAI>();
			HitEnemy(enemyHit, enemyPosition, combatStats.fComboAttackDamage, true); //apply damage
			enemyFunctions.Stun(2.5f); //apply stun
			enemyFunctions.Knockback(enemyPosition, 8f); //apply knockback
		}
	}
	
	//checks if a regular attack hits and apply effects.
	private void AirAttackEffect(float enemyDot, Vector3 enemyPosition, Collider enemyHit)
	{
		if (enemyDot > combatStats.fAirAttackAngle) {
			HitEnemy(enemyHit, enemyPosition, combatStats.fAirAttackDamage);//Apply damage
			UnitAI enemyFunctions = enemyHit.GetComponent<UnitAI>();
			enemyFunctions.Knockback(enemyPosition - transform.position, 150f); //Apply knockback
		}
	}
	
	//This function is called by an animation key event
	public void DoAreaAttack(){

		Collider[] sphereHit;
		//Find all hittable objects around the character.
		sphereHit = Physics.OverlapSphere (transform.position, combatStats.fAreaAttackRange, lmTargetLayers);
		//Do a really strong camera shake
		DoCameraShake(20f);
		
		//Area is 360 so affects all targets
		foreach (Collider enemyHit in sphereHit) {
			if (enemyHit.tag == "Enemy") {
				Vector3 enemyPosition = enemyHit.transform.position - transform.transform.position;
				enemyPosition = new Vector3(enemyPosition.x, 0f, enemyPosition.z);
				HitEnemy(enemyHit, Vector3.up, combatStats.fAreaAttackDamage); //Apply damage.
				UnitAI enemyFunctions = enemyHit.GetComponent<UnitAI>();
				enemyFunctions.Stun(combatStats.fAreaStunDuration); //Apply stun
			}
			//If it hits a obstacle instead
			else if (enemyHit.GetComponent<Collider>().tag.Contains("ObjectWithHealth")) {
				//no camera shake here
				enemyHit.GetComponent<Collider>().GetComponent<EventTakeDamage>().TakeDamage(combatStats.fAreaAttackDamage); //apply damage
				Barrel bar = enemyHit.GetComponent<Collider>().GetComponent<Barrel>();
				if (bar != null) {
					bar.TakeDamage(35f, (enemyHit.transform.position - transform.position).normalized); //apply knockback
				}
			}
		}
	}
	
	//Player blocking getter
	public bool IsPlayerBlocking(){
		return bShieldBlock;
	}

	//Check if player is performing the air attack
	private void CheckForAirAttack()
	{
		if (bAirAttack) {
			if (!GetComponent<Rigidbody>().isKinematic) {
				GetComponent<Rigidbody>().velocity += -Vector3.up * fDiveSpeed * Time.deltaTime; //if player is performing an air attack, use this dive velocity
			}
			if (bIsGrounded) { //when the player reaches the ground, the attack initiates
				anim.SetBool("bAttackAirFinisherReady", true); //play animation
				bAirAttack = false; //update tracker once landed
			}
		}
	}
	
	private void HitEnemy(Collider enemyHit, Vector3 forceDirection, float attackDamage){
		if (enemyHit.tag == "Enemy") {
			DoCameraShake(5f);
			enemyHit.GetComponent<UnitAI>().TakeDamage(attackDamage, forceDirection);
		}
	}
	
	//overloaded method for breaking block
	private void HitEnemy(Collider enemyHit, Vector3 forceDirection, float attackDamage, bool bBreakBlock){
		if (enemyHit.tag == "Enemy") {
			DoCameraShake(5f);
			enemyHit.GetComponent<UnitAI>().TakeDamage(attackDamage, forceDirection, bBreakBlock);
		}
	}
	
	//overloaded method for stopping the counter attack
	private void HitEnemy(Collider enemyHit, Vector3 forceDirection, float attackDamage, bool bBreakBlock, bool bStopCounter){
		if (enemyHit.tag == "Enemy") {
			DoCameraShake(10f);
			enemyHit.GetComponent<UnitAI>().TakeDamage(attackDamage, forceDirection, bBreakBlock, bStopCounter);
		}
	}

	#endregion

	#region General Code and Movement
	
	public void SetEnableMovement(bool enableMovement){
		bEnableMovement = enableMovement;
	}
	
	private bool GetEnableMovement(){
		return bEnableMovement & !bStunned;
	}
	
	//This is used by enemies to damage the player
	public void TakeDamage(float damage, bool bIgnoreHurt = false){
		DoCameraShake(20);
		fCurrentLife -= damage;
		fCurrentStamina += 2f; //gains stamina when player gets hit. This gives player a chance to fight back
		flashScreen.PlayerGetHit ();
		
		//if player is ignoring the hurt animation, he also makes no sound
		if (bIgnoreHurt) {
			return;
		}
		AkSoundEngine.PostEvent ("Ulfar_Hurt", gameObject);
		Hurt ();
	}
	
	//Play random hurt animation
	private void Hurt(){
		if (!ProjectViking.operatingTurret) {
			int i = Random.Range (0, 3);
			//Replace with a string array
			switch (i) {
			case 0:
				anim.SetTrigger("tHurt");
				break;
			case 1:
				anim.SetTrigger("tHurt2");
				break;
			case 2:
				anim.SetTrigger("tHurt3");
				break;
			default:
				break;
			}
		}
	}
	
	//Knockback always use the same hurt animation
	public void PlayKnockBackHurt(){
		anim.SetTrigger ("tHurt3");
	}
	
	//Player gets knockbacked
	public void Kockback(Vector3 dir){
		KnockbackEffect();
		StartCoroutine(Knockback(0.6f, dir));
	}

	//overloaded method to diminish the knockback effect
	public void Kockback(Vector3 dir, float fForceDiscount){
		KnockbackEffect();
		StartCoroutine(Knockback(0.6f, dir, fForceDiscount));
	}
	
	private void KnockbackEffect()
	{
		//cancel combo streak
		iComboCounter = 0;
		bStunned = true;
	}

	private IEnumerator Knockback(float fTime, Vector3 dir){
		while (fTime > 0) {
			fTime -= Time.deltaTime;
			if (!GetComponent<Rigidbody>().isKinematic) {
				GetComponent<Rigidbody>().velocity = new Vector3(dir.x, 0f, dir.z).normalized * 11f;
			}
			yield return null;
		}
		bStunned = false;
		yield return null;
	}
	
	//overloaded courotine with diminished knockback effect
	private IEnumerator Knockback(float fTime, Vector3 dir, float fForceDiscount){
		fTime -= fForceDiscount / 20f; 
		while (fTime > 0) {
			fTime -= Time.deltaTime;
			if (!GetComponent<Rigidbody>().isKinematic) {
				GetComponent<Rigidbody>().velocity = dir.normalized * (13f - fForceDiscount); //this courotine doesn't ignore Y axis
			}
			yield return null;
		}
		bStunned = false; //make sure player doesn't get stunned.
		yield return null;
	}
	
	//When player lands after falling
	public void FinishLanding(){
		anim.SetBool("bHighJump", false);
		GameObject clone = Instantiate (goDirt, transform.position, transform.rotation) as GameObject; //play dirt particle effect
		Destroy (clone, 2f);
	}

	private void IsGrounded(){
		LayerMask lmMask = 1 << 8 | 1 << 9 | 1 << 10; //sets the layerMasks
		lmMask = ~lmMask;
		RaycastHit thisHit;
		//Do a capsule cast to get a more precise result
		Physics.CapsuleCast (transform.position, transform.position - new Vector3(0f, 0.1f, 0f), capCollider.radius, Vector3.down, out thisHit, 1f, lmMask);
		
		//If the player jumps on an enemy head, stuns the enemy and bounce the player off
		if (thisHit.collider && thisHit.collider.tag == "Enemy") {
			if (!GetComponent<Rigidbody>().isKinematic) {
				GetComponent<Rigidbody>().velocity = new Vector3(GetComponent<Rigidbody>().velocity.x, fJumpSpeed * 0.6f, GetComponent<Rigidbody>().velocity.z); //bounce player
			}
			thisHit.collider.GetComponentInParent<UnitAI>().Stun(2f); //stun enemies
			anim.SetTrigger("tStomping"); //play stomping animation
			anim.speed = 3f; //animation multiplier
			anim.Play("Land"); //play land animation when the player hits the enemy head
		}
		if (thisHit.collider) 
		{
			if (!bIsGrounded) 
			{
				anim.Play("Land"); //if player hits anything else, play land animation and set grounded to true.
			}
			bIsGrounded = true;
		}else{
			bIsGrounded = false;
		}
	}

	private void SetJumpingTrue()
	{
		anim.SetBool("bJumping", true); //sets jump animation
	}
	
	//This method controls the character movement
	private void MovementController(){
		
		//if movement is enabled
		if (bAirControl && GetEnableMovement()) {

			Movement ();
		}
		
		if (bIsGrounded) {
			anim.SetBool("bJumping", false);
			//if air control is deactivated maintains momentum
			if (!bAirControl && GetEnableMovement()) {
				Movement ();
			}
			
		}
		
		//Check if air attack is being used in order to do the dive movement
		CheckForAirAttack ();
		
		if (GetEnableMovement()) {
			if (!GetComponent<Rigidbody>().isKinematic) {
				GetComponent<Rigidbody>().velocity = new Vector3(v3Move.x, GetComponent<Rigidbody>().velocity.y, v3Move.z);
			}
		}
	}
	
	//Movement inputs
	void Movement ()
	{
		//if player is stunned, he cant move
		if (bStunned) {
			fMovingTimer = 0f;
			anim.SetBool("bMomentum", false);
			return;
		}
		//get inputs
		float fVerticalMovement = Input.GetAxis("Vertical");
		float fHorizontalMovement = Input.GetAxis("Horizontal");
		//get camera direction
		Vector3 camForward = Vector3.Scale(goMainCamera.GetComponent<Camera>().transform.forward, new Vector3(1, 0, 1)).normalized;
		//walk relative to camera direction
		v3Move = fVerticalMovement * camForward + fHorizontalMovement * goMainCamera.GetComponent<Camera>().transform.right;
		v3Move *= fCurrentSpeed;
		
		
		if (v3Move.magnitude > 3f) { //if player is running for more than 3 seconds, he accumulates momentum
			fMovingTimer += Time.deltaTime;
		}else{
			StopMomentum(); //if he stops the momentum is killed
		}
		if (fMovingTimer > 2f) {
			anim.SetBool("bMomentum", true); //if the momentum timer is higher than 2, the player will have momentum, which changes his attack animation
		}
		
		//if player is doing a charge attack, descelerate slowly as he attacks
		if (anim.GetCurrentAnimatorStateInfo(0).nameHash == Animator.StringToHash("Base Layer.ChargeAttack")) {
			v3Move = transform.forward * fCurrentSpeed * DescelerateRate();
		}else{
			fTempRate = 1f;
		}
		
		anim.SetFloat ("fMovementSpeed", v3Move.magnitude);
		ConvertMoveInput(); // converts the relative move vector into local turn & fwd values
		ApplyExtraTurnRotation ();
	}

	//reduce player speed during a charge attack.
	private float DescelerateRate(){

		fTempRate -= Time.deltaTime * 0.5f;

		if (fTempRate < 0f) {
			fTempRate = 0f;
		}
		return fTempRate;
	}

	//kills momentum
	public void StopMomentum(){
		fMovingTimer = 0f;
		anim.SetBool("bMomentum", false);
	}

	private void ConvertMoveInput()
	{
		// convert the world relative moveInput vector into a local-relative
		// turn amount and forward amount required to head in the desired
		// direction. 
		Vector3 localMove = transform.InverseTransformDirection(v3Move);
		fTurnAmount = Mathf.Atan2(localMove.x, localMove.z);
		fForwardAmount = localMove.z;
	}

	private void TurnTowardsCameraForward()
	{
		transform.eulerAngles = new Vector3 (transform.eulerAngles.x, goMainCamera.GetComponent<Camera>().transform.eulerAngles.y, goMainCamera.GetComponent<Camera>().transform.eulerAngles.z);
	}

	//CONTROLLER
	private void TurnCameraForward(){

		goMainCamera.GetComponent<Camera>().transform.parent.parent.GetComponent<FreeLookCam> ().AdjustCamera (Vector3.Angle(test.transform.forward, transform.forward) * 
		                                                                               AngleDir(transform.forward, test.transform.forward, Vector3.up));
	}

	
	float AngleDir(Vector3 fwd, Vector3 targetDir, Vector3 up) {
		Vector3 perp = Vector3.Cross(fwd, targetDir);
		float dir = Vector3.Dot(perp, up);
		
		if (dir > 0f) {
			return 1f;
		} else if (dir < 0f) {
			return -1f;
		} else {
			return 0f;
		}
	}

	//CONTROLLER
	private void TurnTowardsNearEnemy ()
	{
		//find all enemies in 5 units radius
		Collider[] hitColliders = Physics.OverlapSphere (transform.position, 5f, lmTargetLayers);
		if (hitColliders.Length > 0) {
			int closestEnemyIndex = 0;
			float highestDotProduct = -1f;
			//find the enemy with the highest dot product
			foreach (Collider enemyItem in hitColliders) {
				//crystals ignore this mechanic
				if (enemyItem.tag.Contains("Crystals")) {
					continue;
				}
				if (enemyItem.tag == "Enemy") {
					float currentDot = Vector3.Dot(transform.forward.normalized, (enemyItem.transform.position - transform.position).normalized);
					if (currentDot > highestDotProduct) {
						highestDotProduct = currentDot;
						closestEnemyIndex = System.Array.IndexOf (hitColliders, enemyItem);
					}
				}
			}
			
			if (highestDotProduct == -1f) {
				return;
			}
			//STOPS character from charging in a huge angle making the player move in an unexpected way
			if (highestDotProduct < 0.8f) {
				StopMomentum();
			}
			//turns character towards enemy closest to his front side
			transform.LookAt(new Vector3(hitColliders [closestEnemyIndex].transform.position.x, transform.position.y, hitColliders [closestEnemyIndex].transform.position.z));
			//if distance between the player and the enemy, ignoring Y axis is smaller than 5, then kill momentum so the player doesnt jump forward when already next to the enemy
			if (Vector3.Distance(transform.position, new Vector3(hitColliders [closestEnemyIndex].transform.position.x, transform.position.y, hitColliders [closestEnemyIndex].transform.position.z)) < 5f) {
				StopMomentum();
			}
		}else{
			TurnTowardsCameraForward ();
		}
	}

	private void ApplyExtraTurnRotation()
	{
		// help the character turn faster (this is in addition to root rotation in the animation)
		float turnSpeed = Mathf.Lerp(fStationaryTurnSpeed, fMovingTurnSpeed,
		                             fForwardAmount);
		transform.Rotate(0, fTurnAmount*turnSpeed*Time.deltaTime, 0);
	}
	
	
	void OnTriggerStay(Collider col){
		if (ProjectViking.operatingTurret) {
			return;
		}
		//parent the player to the space ship in the shape of a Viking boat
		if (col.gameObject.tag == "boat") {
			if (transform.parent == null) {
				transform.parent = UnitReiden.urBoat.transform;
			}
		}
	}
	
	void OnTriggerExit(Collider col){
		//set the player free from the ship parent
		if (col.gameObject.tag == "boat") {
			transform.parent = null;
		}
	}

	//Crosshair
	void OnGUI(){
		if (txtCrosshair) {

			GUI.color = Color.white;

			GUI.DrawTexture(new Rect ((Screen.width * 0.5f) - (txtCrosshair.width * 0.5f), (Screen.height * 0.5f) - (txtCrosshair.height * 0.5f), txtCrosshair.width, txtCrosshair.height), txtCrosshair);
		}	
	}

	#endregion

}
