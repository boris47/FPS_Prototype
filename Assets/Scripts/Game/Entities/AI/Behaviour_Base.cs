
using UnityEngine;

public abstract class AIBehaviour {

	protected	Entity	m_BaseEntity;

	public	abstract	void	Enable();
	public	abstract	void	Disable();

	public	abstract	void Setup( Entity BaseEntity );
}
