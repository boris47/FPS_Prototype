using UnityEngine;
using System.Collections;

public class WeaponAnimation : MonoBehaviour
{
	public	Transform firePoint;
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

	float fireTimer;


	private	GameObjectsPool m_Pool;



    void Awake()
    {
        if (fire == null) Debug.LogError("Please assign a fire aimation in the inspector!");
        if (reload == null) Debug.LogError("Please assign a reload animation in the inspector!");
        if (draw == null) Debug.LogError("Please assign a draw animation in the inspector!");

		var go = GameObject.CreatePrimitive( PrimitiveType.Sphere );
		var rb = go.AddComponent<Rigidbody>();
		go.transform.localScale = Vector3.one * 0.2f;
		m_Pool = new GameObjectsPool( ref go, 25, true );
    }


    // Update is called once per frame
    void Update()
    {
		fireTimer -= Time.deltaTime;

        if (Input.GetKeyDown(reloadKey))
        {
            if (reload != null) anim.Play(reload.name);
        }

        if (Input.GetKey(fireKey))
        {
			if ( fireTimer > 0 )
				return;
			fireTimer = shotDelay;

            if ( fire != null )
				anim.Play(fire.name, -1, 0f);

			GameObject bullet = m_Pool.Get();
			bullet.transform.position = firePoint.position;
			bullet.GetComponent<Rigidbody>().velocity = transform.right * 20f;
			
			audioSource.Play();

			CameraControl.Instance.ApplyDispersion( fireDispersion );
        }

        if (Input.GetKeyDown(drawKey))
        {
            if (draw != null) anim.Play(draw.name);
        }


    }
}
