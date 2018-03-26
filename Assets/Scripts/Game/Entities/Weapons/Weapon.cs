using UnityEngine;
using System.Collections;


public abstract class Weapon : MonoBehaviour {

	public enum FireModes {
		SINGLE, BURST, AUTO
	}

	public	float		Damage			= 20f;
	public	Transform	firePoint1		= null;

	public	FireModes	fireMode		= FireModes.AUTO;
	public	int			magazine		= 25;

}
