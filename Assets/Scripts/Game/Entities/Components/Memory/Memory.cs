using UnityEngine;

public interface IEntityComponent_Memory
{
	int				MemoriesCount					{ get; }

	void			EnableMemory					();
	void			DisableMemory					();
	void			UpdateMemory					();

	/// <summary> Validate all the memories checking data valid values </summary>
	void			CleanInvalidMemories			();
	/// <summary> Add a valid entity to memories of this entity </summary>
	bool			Add								(Entity entity);
	/// <summary> Check if memory contains this entity </summary>
	bool			Contains						(uint EntityId);
	/// <summary> Get the last position giving a specific index </summary>
	Vector3			GetLastPositionByIndex			(int index);
	/// <summary> Get the last direction giving a specific index </summary>
	Vector3			GetLastDirectionByIndex			(int index);
	/// <summary> Get the entity giving a specific index </summary>
	Entity			GetEntityByIndex				(int index);
	/// <summary> Return the last position searched by entity index, vector zero otherwise </summary>
	Vector3			GetLastPosition					(uint EntityId);
	/// <summary> Return the entity searched by index, null otherwise </summary>
	Entity			GetEntity						(uint EntityId);
	/// <summary>  </summary>
	bool			Remove							(Entity entity);
	/// <summary> Clear all entity memeory data </summary>
	void			Empty							();
}

public abstract class Memory_Base : EntityComponent, IEntityComponent_Memory
{
	public abstract int MemoriesCount { get; }

	public abstract void EnableMemory();
	public abstract void DisableMemory();
	public abstract void UpdateMemory();

	public abstract void CleanInvalidMemories();
	public abstract bool Add(Entity entity);
	public abstract bool Contains(uint EntityId);
	public abstract Vector3 GetLastPositionByIndex(int index);
	public abstract Vector3 GetLastDirectionByIndex(int index);
	public abstract Entity GetEntityByIndex(int index);
	public abstract Vector3 GetLastPosition(uint EntityId);
	public abstract Entity GetEntity(uint EntityId);
	public abstract bool Remove(Entity entity);
	public abstract void Empty();
}

public class EntityComponentContainer_Memory<T> : EntityComponentContainer where T : Memory_Base, new()
{
	public override System.Type type { get; } = typeof(T);
}
