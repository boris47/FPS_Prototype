
using UnityEngine;


public enum ENTITY_TYPE : uint {
	NONE,
	ACTOR,
	HUMAN,
	ROBOT,
	ANIMAL,
	OBJECT
};

/////////////////////////////////////////
/////////////////////////////////////////

public	enum LookTargetType : uint {
	POSITION,
	TRANSFORM
};

public	enum LookTargetMode : uint {
	HEAD_ONLY,
	WITH_BODY
}

public class LookData {
	public	bool			HasLookAtObject		= false;
	public	Vector3			PointToLookAt		= Vector3.zero;
	public	Transform		TrasformToLookAt	= null;
	public	LookTargetType	LookTargetType		= LookTargetType.POSITION;
	public	LookTargetMode	LookTargetMode		= LookTargetMode.HEAD_ONLY;
};

/////////////////////////////////////////
/////////////////////////////////////////

public enum EffectType {
	ENTITY_ON_HIT,
	AMBIENT_ON_HIT,
	ELETTRO,
	PLASMA,
	EXPLOSION,

	MUZZLE,
	SMOKE,
	COUNT
};

/////////////////////////////////////////
/////////////////////////////////////////

[ System.Serializable ]
public enum BrainState {
	EVASIVE		= 0,
	NORMAL		= 1,
	ALARMED		= 2,
	SEEKER		= 3,
	ATTACKER	= 4,
	COUNT		= 5
}

/////////////////////////////////////////
/////////////////////////////////////////

public enum SimMovementType {
	STATIONARY,
	WALK,
	CROUCHED,
	RUN
}

/////////////////////////////////////////
/////////////////////////////////////////

public enum eMotionType {
	None		= 1 << 0,
	Walking		= 1 << 1,
	Flying		= 1 << 2,
	Swimming	= 1 << 3,
	P1ToP2		= 1 << 4
};

/////////////////////////////////////////
/////////////////////////////////////////

[System.Serializable]
public class TargetInfo {
	public	bool	HasTarget;
	public	IEntity	CurrentTarget;
	public	float	TargetSqrDistance;

	public	void	Update( TargetInfo Infos )
	{
		HasTarget			= Infos.HasTarget;
		CurrentTarget		= Infos.CurrentTarget;
		TargetSqrDistance	= Infos.TargetSqrDistance;
	}

	public	void	Reset()
	{
		HasTarget			= false;
		CurrentTarget		= null;
		TargetSqrDistance	= 0.0f;
	}
}

/////////////////////////////////////////
/////////////////////////////////////////

public struct EntityEvents {
	public	delegate	void		HitWithBullet( IBullet bullet );
	public	delegate	void		HitDetailsEvent( Vector3 startPosition, Entity whoRef, float damage, bool canPenetrate = false );
	public	delegate	void		TargetEvent( TargetInfo targetInfo );
	public	delegate	void		NavigationEvent( Vector3 Destination );
	public	delegate	void		KilledEvent();
}

/////////////////////////////////////////
/////////////////////////////////////////

public enum BulletMotionType {
	INSTANT,
	DIRECT,
	PARABOLIC
}

/////////////////////////////////////////
/////////////////////////////////////////
public enum WeaponState {
	DRAWED, STASHED
}

/////////////////////////////////////////
/////////////////////////////////////////

public enum FireModes {
	SINGLE, BURST, AUTO
}
