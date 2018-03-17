

public abstract class EventInfo {}


public class HitInfo : EventInfo {

	public	Entity			Who			= null;
	public	float			Damage		= 0f;

}

public class HurtInfo : EventInfo {

	public	float			Damage		= 0f;
}