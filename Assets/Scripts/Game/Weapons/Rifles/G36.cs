
using UnityEngine;

public class G36 : Weapon {

	[Header("G36 Properties")]
	private	bool asd;

	protected override string OtherInfo
	{
		get {
			return "";
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected	override	void			Awake()
	{
		base.Awake();
	}


	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	public		override	bool			CanChangeWeapon()
	{
		return base.CanChangeWeapon();
	}


	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected	override	StreamingUnit	OnLoad( StreamingData streamingData )
	{
		return base.OnLoad( streamingData );
	}


	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected	override	StreamingUnit	OnSave( StreamingData streamingData )
	{
		return base.OnSave( streamingData );
	}


	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected	override	void			Update()
	{
		
	}

	
	//////////////////////////////////////////////////////////////////////////
	// FireSingleMode
	public		override	void			OnWeaponChange()
	{
		base.OnWeaponChange();
	}


	//////////////////////////////////////////////////////////////////////////
	// OnSecondaryFire
	private					void			OnSecondaryFire()
	{
		if ( WeaponManager.Instance.Zoomed == false )
			WeaponManager.Instance.ZoomIn( this, m_ZoomOffset, m_ZoomingTime );
		else
			WeaponManager.Instance.ZoomOut();
	}
}
