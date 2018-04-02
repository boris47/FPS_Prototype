using UnityEngine;
using System.Collections;


public abstract class Weapon : MonoBehaviour {

	public enum FireModes {
		SINGLE, BURST, AUTO
	}

	public		float					Damage					= 20f;
	public		Transform				firePoint1				= null;

	public		FireModes				fireMode				= FireModes.AUTO;
	public		int						magazine				= 25;
	

	public		GameObject				m_Bullet1GameObject		= null;
	public		GameObject				m_Bullet2GameObject		= null;


	[SerializeField,Range(0.1f, 2f)]
	protected	float					m_SlowMotionCoeff		= 1f;
	public		float					SlowMotionCoeff
	{
		get { return m_SlowMotionCoeff; }
	}

	protected	bool					m_FirstFireAvaiable		= false;
	public		bool					FirstFireAvaiable
	{
		set { m_FirstFireAvaiable = value; }
	}

	protected	bool					m_SecondFireAvaiable	= false;
	public		bool					SecondFireAvaiable
	{
		set { m_SecondFireAvaiable = value; }
	}


	protected	virtual	void	Awake()
	{
		if ( m_Bullet1GameObject == null )
		{
			print( "Wepaon " + name + " need a defined bullet to use " );
			enabled = false;
		}

	}



}
