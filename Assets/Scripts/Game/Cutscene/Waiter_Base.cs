using UnityEngine;


public abstract class Waiter_Base : MonoBehaviour
{
	public			bool	HasToWait { get; protected set; } = true;

	public	abstract	void	Wait();

	private void Awake()
	{
		enabled = false;
	}

}