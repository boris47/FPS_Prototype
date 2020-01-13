using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IIdentificable<T> {

	T ID
	{
		get;
	}

}

public abstract class Registerer<T, I> : MonoBehaviour where T : IIdentificable<I> {

	private static  List<T> m_Collection = new List<T>();

	public void RegisterGroup( T item )
	{
		if ( !m_Collection.Contains( item ) )
		{
			m_Collection.Add( item );
		}
	}


	public void UnregisterGroup( T item )
	{
		if ( !m_Collection.Contains( item ) )
		{
			m_Collection.Remove( item );
		}
	}


	public T GetById( I id )
	{
		return m_Collection.Find( i => i.ID.Equals( id ) );
	}

}