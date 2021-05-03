
public interface IModifiable
{
	void					StartModify				();
	void					AddModifier				(Database.Section modifier);
	void					ResetBaseConfiguration	();
	void					RemoveModifier			(Database.Section modifier);
	void					EndModify				();
}

public class Modifiable: System.IDisposable
{
	private IModifiable m_Modifiable = null;

	public Modifiable(IModifiable modifiable)
	{
		m_Modifiable = modifiable;
		m_Modifiable.StartModify();
	}

	void System.IDisposable.Dispose()
	{
		m_Modifiable.EndModify();
	}

	public void AddModifier				(Database.Section modifier)		=> m_Modifiable.AddModifier(modifier);
	public void ResetBaseConfiguration	()								=> m_Modifiable.ResetBaseConfiguration();
	public void RemoveModifier			(Database.Section modifier)		=> m_Modifiable.RemoveModifier(modifier);
}