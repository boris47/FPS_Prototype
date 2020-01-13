using System.Collections;
using System.Collections.Generic;
using UnityEngine;





public interface IEntityGroup {

	/// <summary> Return the group this entity belongs to </summary>
	EntityGroup Group { get; }

	void SetGroup( EntityGroup group );

}


public abstract partial class Entity : IEntityGroup {
	
	// INTERFACE START
				EntityGroup					IEntityGroup.Group					{ get { return m_Group; } }
	// INTERFACE END
	
	protected	EntityGroup					m_Group					= null;

	protected	IEntityGroup				m_EntityGroup			= null;



	void IEntityGroup.SetGroup( EntityGroup group )
	{
		m_Group = group;
	}
}