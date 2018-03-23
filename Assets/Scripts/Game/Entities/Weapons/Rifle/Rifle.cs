using UnityEngine;
using System.Collections;

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
	public	float	fireDispersion = 0.8f;
	public	int		magazine = 25;

	int	  magazineCapacity = 25;
	float fireTimer;
	float reloadTimer;


	private	GameObjectsPool m_Pool;



	private void Awake()
	{
		if (fire == null)	Debug.LogError("Please assign a fire aimation in the inspector!");
		if (reload == null)	Debug.LogError("Please assign a reload animation in the inspector!");
		if (draw == null)	Debug.LogError("Please assign a draw animation in the inspector!");

		GameObject go = GameObject.CreatePrimitive( PrimitiveType.Sphere );
		go.name = "PlayerBlt";
		Rigidbody rb = go.AddComponent<Rigidbody>();
		Bullet bullet = go.AddComponent<Bullet>();
		bullet.WhoRef = Player.Instance;
		rb.useGravity = false;
		rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
		go.transform.localScale = Vector3.one * 0.2f;
		m_Pool = new GameObjectsPool( ref go, 25, true );

		Player.Instance.CurrentWeapon = this;
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
		}

		if ( Input.GetKey( fireKey ) )
		{
			if ( fireTimer > 0 )
				return;
			fireTimer = shotDelay;

			if ( fire != null )
				anim.Play(fire.name, -1, 0f);

			Rigidbody bullet = m_Pool.Get<Rigidbody>();
			bullet.transform.position = firePoint1.position;
			bullet.velocity = transform.right * 20f;
			
			audioSource.Play();

			magazine --;

			float finalDispersion = Player.Instance.IsCrouched ? fireDispersion * 0.5f : fireDispersion;
			finalDispersion = Player.Instance.IsRunning ? finalDispersion * 2.0f : finalDispersion;
			CameraControl.Instance.ApplyDispersion( finalDispersion );
		}

		if ( magazine <= 0 || Input.GetKeyDown(reloadKey) )
		{
			anim.Play(reload.name);
			reloadTimer = reload.length;
			
		}
	}

}
