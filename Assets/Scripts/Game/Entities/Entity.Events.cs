
public abstract partial class Entity {

	public	abstract	void		OnHit( HitInfo info );

	public	abstract	void		OnHurt( HurtInfo info );

	public	abstract	void		OnKill( HitInfo info = null );

}