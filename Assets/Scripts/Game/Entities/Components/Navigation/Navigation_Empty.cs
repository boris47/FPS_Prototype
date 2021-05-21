﻿using UnityEngine;

public interface INavigation_Empty : IEntityComponent_Navigation
{

}

public class Navigation_Empty : Navigation_Base, INavigation_Empty
{
	//////////////////////////////////////////////////////////////////////////
	protected override void Resolve_Internal(Entity entity, Database.Section entitySection)
	{ }


	//////////////////////////////////////////////////////////////////////////
	public override void RequestMovement(Vector3 Destination)
	{ }


	//////////////////////////////////////////////////////////////////////////
	public override void NavStop()
	{ }


	//////////////////////////////////////////////////////////////////////////
	public override void OnPathSearchTimeOutReached()
	{ }


	//////////////////////////////////////////////////////////////////////////
	public override void NavReset()
	{ }
}