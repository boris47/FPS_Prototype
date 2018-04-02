
using UnityEngine;

public class Rifle : Weapon
{

	public Animator anim;
	public AnimationClip fire;
	public AnimationClip reload;
	public AnimationClip draw;

	public	AudioSource	audioSourceFire1;
	public	AudioSource	audioSourceFire2;
	public	float	shotDelay;
	public	float	camDeviation					= 0.8f;
	public	float	fireDispersion					= 0.05f;

	private	float	bulletMaxDamage					= 5f;
	private	float	bulletMinDamage					= 5f;

	private	float	granadeDamage					= 40f;
	private	float	granadeRadius					= 3f;
	private	float	granadeThrowForce				= 5f;
	private	float	granadeExplosionDelay			= 3f;
	

	int	  magazineCapacity		= 25;
	float fire1Timer			= 1f;
	float fire2Timer			= 1f;
	float reloadTimer;

	[SerializeField]
	private	GranadeBase						m_GranadeType					= null;

	private	GameObjectsPool<Bullet>			m_PoolBullets					= null;
	private	GameObjectsPool<GranadeBase>	m_PoolGranade					= null;



	protected	override void	Awake()
	{
		if (fire == null)	Debug.LogError("Please assign a fire aimation in the inspector!");
		if (reload == null)	Debug.LogError("Please assign a reload animation in the inspector!");
		if (draw == null)	Debug.LogError("Please assign a draw animation in the inspector!");

		bool	canPenetrate = false;

		// LOAD CONFIGURATION
		{
			CFG_Reader.Section section = null;
			GameManager.Configs.GetSection( "Rifle", ref section );

			bulletMaxDamage				= section.AsFloat( "DamageMax",		bulletMaxDamage );
			bulletMinDamage				= section.AsFloat( "DamageMix",		bulletMinDamage );
			canPenetrate				= section.AsBool(  "CanPenetrate",	canPenetrate );

			granadeDamage				= section.AsFloat( "GranadeDamage",	granadeDamage );
			granadeRadius				= section.AsFloat( "GranadeRadius",	granadeRadius );
			granadeThrowForce			= section.AsFloat( "GranadeThrowForce",	granadeThrowForce );
			granadeExplosionDelay		= section.AsFloat( "GranadeExplosionDelay",	granadeExplosionDelay );
		}

		// BULLETS POOL CREATION
		{
			if ( m_Bullet1GameObject != null )
			{
				m_FirstFireAvaiable			= true;
				m_PoolBullets = new GameObjectsPool<Bullet>( ref m_Bullet1GameObject, 20, destroyModel : false, actionOnObject : ( Bullet o ) =>
				{
					o.SetActive( false );
					o.Setup( bulletMinDamage, bulletMaxDamage, Player.Instance, this, false );
					Physics.IgnoreCollision( o.Collider, Player.Instance.PhysicCollider, ignore : true );
				} );
				m_PoolBullets.ContainerName = "RifleBulletPool";
			}


			if ( m_Bullet2GameObject != null )
			{
				m_SecondFireAvaiable			= true;
				m_PoolGranade = new GameObjectsPool<GranadeBase>( ref m_Bullet2GameObject, 5, destroyModel : false, actionOnObject : ( GranadeBase o ) =>
				{
					o.SetActive( false );
					o.Setup( granadeDamage, granadeRadius, granadeExplosionDelay, Player.Instance, this );
					Physics.IgnoreCollision( o.Collider, Player.Instance.PhysicCollider, ignore : true );
				} );
				m_PoolGranade.ContainerName = "RifleGranadePool";
			}

		}

		Player.Instance.CurrentWeapon = this;
	}

	private void OnEnable()
	{
		UI_InGame.Instance.UpdateUI();
	}


	// Update is called once per frame
	private void Update()
	{
		fire1Timer -= Time.deltaTime;
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

		if ( m_Bullet1GameObject == null )	m_FirstFireAvaiable  = false;
		if ( m_Bullet2GameObject == null )	m_SecondFireAvaiable = false;


		if ( m_FirstFireAvaiable && magazine > 0 &&
			( ( fireMode == FireModes.AUTO		&& InputManager.Inputs.Fire1Loop ) || 
			  ( fireMode == FireModes.SINGLE	&& InputManager.Inputs.Fire1 ) ) )
		{
			if ( fire1Timer > 0 )
				return;

			fire1Timer = shotDelay;

			anim.Play( fire.name, -1, 0f );
			
			Vector3 dispersion = new Vector3 ( Random.Range( -1f, 1f ), Random.Range( -1f, 1f ), Random.Range( -1f, 1f ) ) * fireDispersion;
			Vector3 direction = ( transform.right + dispersion ).normalized;

			Bullet bullet = m_PoolBullets.GetComponent();
			bullet.transform.position = firePoint1.position;
			bullet.SetVelocity( bullet.transform.forward = direction );
			bullet.SetActive( true );
			
			audioSourceFire1.Play();

			magazine --;

			float finalDispersion = camDeviation;
			finalDispersion	= Player.Instance.IsCrouched	? finalDispersion	* 0.5f : finalDispersion;
			finalDispersion	= Player.Instance.IsMoving		? finalDispersion	* 1.5f : finalDispersion;
			finalDispersion	= Player.Instance.IsRunning		? finalDispersion	* 2.0f : finalDispersion;
			finalDispersion *= ( fireMode == FireModes.SINGLE ) ? 0.5f : 1f;
			CameraControl.Instance.ApplyDispersion( finalDispersion );

			UI_InGame.Instance.UpdateUI();
		}


		fire2Timer -= Time.deltaTime;
		if ( InputManager.Inputs.Fire2Loop && m_SecondFireAvaiable == true )
		{
			if ( fire2Timer > 0 )
				return;

			fire2Timer = 1f;

			GranadeBase granade				= m_PoolGranade.GetComponent();
			granade.transform.position		= firePoint1.position;
			granade.RigidBody.velocity		= transform.right * granadeThrowForce;
			granade.SetActive( true );

			audioSourceFire2.Play();

			CameraControl.Instance.ApplyDispersion( 5 );
		}

		if ( magazine <= 0 || InputManager.Inputs.Reload )
		{
			anim.Play(reload.name);
			reloadTimer = reload.length;
		}
	}

}
