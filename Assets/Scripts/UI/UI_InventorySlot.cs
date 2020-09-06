using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class UI_InventorySlot : MonoBehaviour, IPointerClickHandler, IStateDefiner {

	private		Texture2D			m_Texture			= null;
	private		Image				m_Image				= null;


	private		bool				m_IsSet				= false;
	public		bool	IsSet
	{
		get { return this.m_IsSet; }
	}

	private		Database.Section	m_ItemSection		= null;
	public	Database.Section	Section
	{
		get { return this.m_ItemSection; }
	}

	private	bool			m_IsInitialized			= false;
	bool IStateDefiner.IsInitialized
	{
		get { return this.m_IsInitialized; }
	}

	string IStateDefiner.StateName
	{
		get { return this.name; }
	}


	//////////////////////////////////////////////////////////////////////////
	// Initialize
	IEnumerator IStateDefiner.Initialize()
	{
		if (this.m_IsInitialized == true )
			yield break;

		CoroutinesManager.AddCoroutineToPendingCount( 1 );

		this.m_IsInitialized = true;
		{
			this.m_IsInitialized &= this.transform.SearchComponent<Image>( ref this.m_Image, ESearchContext.LOCAL );
		}

		if (this.m_IsInitialized )
		{
			CoroutinesManager.RemoveCoroutineFromPendingCount( 1 );
		}
		else
		{
			Debug.LogError( "UI_MatrixItem: Bad initialization!!!" );
		}
	}

	//////////////////////////////////////////////////////////////////////////
	// ReInit
	IEnumerator	IStateDefiner.ReInit()
	{
		yield return null;
	}


	//////////////////////////////////////////////////////////////////////////
	// Finalize
	bool	 IStateDefiner.Finalize()
	{
		return this.m_IsInitialized;
	}

	

	//////////////////////////////////////////////////////////////////////////
	// OnPointerClick
	void IPointerClickHandler.OnPointerClick( PointerEventData eventData )
	{
		if ( eventData.button == PointerEventData.InputButton.Left )
		{
			
		}
		if ( eventData.button == PointerEventData.InputButton.Middle )
		{
			
		}
		if ( eventData.button == PointerEventData.InputButton.Right )
		{
			
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public	void	Reset()
	{
		this.m_Texture = null;
		this.m_ItemSection = null;
		this.m_IsSet = false;
	}


	//////////////////////////////////////////////////////////////////////////
	public	bool	TrySet( Texture2D texture, Database.Section section )
	{
		bool result = true;
		{
			result &= texture != null;		// texture must be valid
			result &= section.Lines() > 0;	// section must contain some info
		}

		if (this.m_IsInitialized && result )
		{
			this.m_Texture = texture;
			this.m_ItemSection = section;
			this.m_IsSet = true;

			this.m_Image.sprite = Sprite.Create(this.m_Texture, Rect.MinMaxRect(0, 0, this.m_Texture.width, this.m_Texture.height ), new Vector2( 0.5f, 0.5f ) );
		}
		return result;
	}
}
