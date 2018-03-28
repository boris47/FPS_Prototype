
using UnityEngine;

public class Rifle : Weapon
{

	public Animator anim;
	public AnimationClip fire;
	public AnimationClip reload;
	public AnimationClip draw;
	public KeyCode reloadKey = KeyCode.R;
	public KeyCode fireKey = KeyCode.Mouse0;
	public KeyCode drawKey = KeyCode.D;

	public	AudioSource	audioSource;
	public	float	shotDelay;
	public	float	camDeviation = 0.8f;
	public	float	fireDispersion = 0.05f;

	private	float	bulletMaxDamage = 5f;
	private	float	bulletMinDamage = 5f;
	

	int	  magazineCapacity = 25;
	float fireTimer;
	float reloadTimer;


	private	GameObjectsPool m_Pool;



	private void Awake()
	{
		if (fire == null)	Debug.LogError("Please assign a fire aimation in the inspector!");
		if (reload == null)	Debug.LogError("Please assign a reload animation in the inspector!");
		if (draw == null)	Debug.LogError("Please assign a draw animation in the inspector!");

		bool	canPenetrate = false;

		// LOAD CONFIGURATION
		{
			CFG_Reader.Section section = null;
			GameManager.Configs.GetSection( "Rifle", ref section );

			bulletMaxDamage = section.AsFloat( "DamageMax", bulletMaxDamage );
			bulletMinDamage = section.AsFloat( "DamageMix", bulletMinDamage );
			canPenetrate	= section.AsBool( "CanPenetrate", canPenetrate );
		}

		GameObject go = GameObject.CreatePrimitive( PrimitiveType.Sphere );
		go.name = "PlayerBlt";
		Rigidbody rb = go.AddComponent<Rigidbody>();
		Bullet bullet = go.AddComponent<Bullet>();
		bullet.DamageMax = bulletMaxDamage;
		bullet.DamageMin = bulletMinDamage;
		bullet.WhoRef = Player.Instance;
		bullet.Weapon = this;
		bullet.CanPenetrate = canPenetrate;
		rb.useGravity = false;
		rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
		go.transform.localScale = Vector3.one * 0.2f;

		m_Pool = new GameObjectsPool( ref go, 25, true, 
			actionOnObject : ( GameObject o ) =>
		{
			o.GetComponent<Bullet>().SetActive( false );
			Physics.IgnoreCollision( o.GetComponent<Collider>(), Player.Instance.GetComponent<CapsuleCollider>(), ignore : true );

		} );

		Player.Instance.CurrentWeapon = this;
	}

	private void OnEnable()
	{
		UI_InGame.Instance.UpdateUI();
	}


	// Update is called once per frame
	private void Update()
	{
		fireTimer -= Time.deltaTime;
		if ( reloadTimer > 0 )
		{
			reloadTimer -= Time.deltaTime;
			return;
		}
		else
		if ( reloadTimer != 0 )
		{
			reloadTimer = 0;
			magazine = magazineCapacity;
			UI_InGame.Instance.UpdateUI();
		}

		if ( InputManager.Inputs.ItemAction1 )
		{
			fireMode = ( fireMode == FireModes.AUTO ) ? FireModes.SINGLE : FireModes.AUTO;
			UI_InGame.Instance.UpdateUI();
		}

		if ( ( fireMode == FireModes.AUTO && Input.GetKey( fireKey ) ) || ( fireMode == FireModes.SINGLE && Input.GetKeyDown( fireKey ) ) )
		{
			if ( fireTimer > 0 )
				return;

			fireTimer = shotDelay;

			if ( fire != null )
				anim.Play(fire.name, -1, 0f);

			Bullet bullet = m_Pool.Get<Bullet>();
			bullet.enabled = true;
			bullet.FirePosition = bullet.transform.position = firePoint1.position;
			bullet.MaxLifeTime = 5f;

			Vector3 dispersion = new Vector3
			(
				Random.Range( -1f, 1f ),
				Random.Range( -1f, 1f ),
				Random.Range( -1f, 1f )
			) * fireDispersion;
			
			bullet.SetVelocity( ( transform.right + dispersion ) * 20f );
			bullet.SetActive( true );
			
			audioSource.Play();

			magazine --;

			float finalDispersion = Player.Instance.IsCrouched ? camDeviation * 0.5f : camDeviation;
			finalDispersion = Player.Instance.IsRunning ? finalDispersion * 2.0f : finalDispersion;
			finalDispersion *= ( fireMode == FireModes.SINGLE ) ? 0.5f : 1f;
			CameraControl.Instance.ApplyDispersion( finalDispersion );

			UI_InGame.Instance.UpdateUI();
		}

		if ( magazine <= 0 || Input.GetKeyDown( reloadKey ) )
		{
			anim.Play(reload.name);
			reloadTimer = reload.length;
			
		}
	}

}
