
public abstract partial class Entity {

	public	abstract	void		OnHith( HitInfo info );

	public	abstract	void		OnHurt( HurtInfo info );

	public	abstract	void		OnKill( HitInfo info = null );

}