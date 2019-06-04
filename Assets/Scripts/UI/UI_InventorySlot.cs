using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_InventorySlot : MonoBehaviour, IPointerClickHandler, IStateDefiner {

	private		Texture2D			m_Texture			= null;
	private		Image				m_Image				= null;


	private		bool				m_IsSet				= false;
	public		bool	IsSet
	{
		get { return m_IsSet; }
	}

	private		Database.Section	m_ItemSection		= null;
	public	Database.Section	Section
	{
		get { return m_ItemSection; }
	}

	private	bool			m_bIsInitialized			= false;
	bool IStateDefiner.IsInitialized
	{
		get { return m_bIsInitialized; }
	}


	//////////////////////////////////////////////////////////////////////////
	// Initialize
	IEnumerator IStateDefiner.Initialize()
	{
		if ( m_bIsInitialized == true )
			yield break;

		m_bIsInitialized = true;
		{
			m_bIsInitialized &= transform.SearchComponent<Image>( ref m_Image, SearchContext.LOCAL );
		}

		if ( m_bIsInitialized )
		{

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
		return m_bIsInitialized;
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
		m_Texture = null;
		m_ItemSection = null;
		m_IsSet = false;
	}


	//////////////////////////////////////////////////////////////////////////
	public	bool	TrySet( Texture2D texture, Database.Section section )
	{
		bool result = true;
		{
			result &= texture != null;		// texture must be valid
			result &= section.Lines() > 0;	// section must contain some info
		}

		if ( m_bIsInitialized && result )
		{
			m_Texture = texture;
			m_ItemSection = section;
			m_IsSet = true;

			m_Image.sprite = Sprite.Create( m_Texture, Rect.MinMaxRect(0, 0, m_Texture.width, m_Texture.height ), new Vector2( 0.5f, 0.5f ) );
		}
		return result;
	}
}
