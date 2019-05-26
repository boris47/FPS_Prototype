using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_MatrixItem : MonoBehaviour, IPointerClickHandler, IStateDefiner {

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

	private	bool			m_bIsInitialized			= true;
	bool IStateDefiner.IsInitialized
	{
		get { return m_bIsInitialized; }
	}


	//////////////////////////////////////////////////////////////////////////
	// Initialize
	bool IStateDefiner.Initialize()
	{
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
		return m_bIsInitialized;
	}

	//////////////////////////////////////////////////////////////////////////
	// ReInit
	bool IStateDefiner.ReInit()
	{
		return m_bIsInitialized;
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








public class UI_Inventory : MonoBehaviour, IStateDefiner {

	private		Transform		m_MainPanel				= null;
	private		UI_MatrixItem[,] m_UI_MatrixItems		= null;

	private	bool			m_bIsInitialized			= true;
	bool IStateDefiner.IsInitialized
	{
		get { return m_bIsInitialized; }
	}

	//////////////////////////////////////////////////////////////////////////
	// Initialize
	bool IStateDefiner.Initialize()
	{
		m_bIsInitialized = true;
		{
			m_MainPanel = transform.Find( "MainPanel" );

			m_UI_MatrixItems = new UI_MatrixItem[ 3, 4 ];

			for ( int i = 0; i < m_MainPanel.childCount; i++ )
			{
				Transform child = m_MainPanel.GetChild(i);
				for ( int j = 0; j < child.childCount; j++ )
				{
					Transform rawChild = child.GetChild(j);
					IStateDefiner item = m_UI_MatrixItems[i, j] = rawChild.gameObject.AddComponent<UI_MatrixItem>();
					item.Initialize();
				}
			}
		}

		if ( m_bIsInitialized )
		{

		}
		else
		{
			Debug.LogError( "UI_Inventory: Bad initialization!!!" );
		}
		return m_bIsInitialized;
	}


	//////////////////////////////////////////////////////////////////////////
	// ReInit
	bool IStateDefiner.ReInit()
	{
		return m_bIsInitialized;
	}


	//////////////////////////////////////////////////////////////////////////
	// Finalize
	bool	 IStateDefiner.Finalize()
	{
		return m_bIsInitialized;
	}
	

	//////////////////////////////////////////////////////////////////////////
	public	bool	AddItem( Texture2D texture, Database.Section section )
	{
		Vector2 position;
		UI_MatrixItem matrixItem = null;
		bool bAllAttempDone = m_UI_MatrixItems.FindByPredicate( ( UI_MatrixItem i ) => i.IsSet == false, ref matrixItem, out position );
		if ( bAllAttempDone )
		{
			bAllAttempDone &= matrixItem.TrySet( texture, section );
		}
		
		return bAllAttempDone;
	}

	


	//////////////////////////////////////////////////////////////////////////
	public	bool	RemoveItem( string itemName )
	{
		Vector2 position;
		UI_MatrixItem matrixItem = null;
		bool bHasBeenFound = m_UI_MatrixItems.FindByPredicate( ( UI_MatrixItem i ) => i.Section.GetName() == itemName, ref matrixItem, out position );
		if ( bHasBeenFound )
		{
			matrixItem.Reset();
		}
		return bHasBeenFound;
	}

	
	private void OnEnable()
	{
		if ( m_bIsInitialized == false )
		{
			return;
		}

		CameraControl.Instance.CanParseInput	= false;
		InputManager.IsEnabled					= false;

		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
	}

	private void OnDisable()
	{
		if ( CameraControl.Instance != null )
		{
			CameraControl.Instance.CanParseInput	= true;
		}
		InputManager.IsEnabled					= true;

		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
	}

}
