using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_ComInterface : MonoBehaviour, IStateDefiner {

	private	const	int			MAX_ELEMENTS = 05;

	private		GameObjectsPool<Text>				m_NotificationsPool						= null;

	private		RectTransform						m_NotificationsPanel					= null;

	private		float								m_NotificationsDuration					= 5f;

	// STORE DATA
	[System.Serializable]
	private class ComInterfaceNotification {
		public	float	CurrentTime;
		public	Text	TextComponent;
		public	Color	Color;
	}
	private	List<ComInterfaceNotification> m_Notifications	= new List<ComInterfaceNotification>();

	// REQUESTS
	[System.Serializable]
	private	struct NotificationRequest {
		public	string	Text;
		public	Color	Color;
	}
	[SerializeField]
	private	Queue<NotificationRequest>					m_Requests					= new Queue<NotificationRequest>();

	// SECTION DATA
	[System.Serializable]
	private class UI_NotificationsSectionData {
		public	float	NotificationsDuration = 5f;
	}
	[SerializeField]
	private		UI_NotificationsSectionData			m_NotificationsSectionData = new UI_NotificationsSectionData();


	// INITIALIZATION
	private	bool			m_bIsInitialized	= false;
	bool IStateDefiner.IsInitialized
	{
		get { return m_bIsInitialized; }
	}

	string IStateDefiner.StateName
	{
		get { return name; }
	}


	//////////////////////////////////////////////////////////////////////////
	// Initialize
	IEnumerator IStateDefiner.Initialize()
	{
		if ( m_bIsInitialized == true )
			yield break;

		CoroutinesManager.AddCoroutineToPendingCount( 1 );

		m_NotificationsPanel = transform.Find( "NotificationsPanel" ) as RectTransform;
		UnityEngine.Assertions.Assert.IsNotNull( m_NotificationsPanel );

		yield return null;

		bool resourcesLoaded = true;

		// Section Data
		UnityEngine.Assertions.Assert.IsTrue
		(
			GlobalManager.Configs.bGetSection( "Notifications", m_NotificationsSectionData ),
			"UI_ComInterface::Initialize:Cannot load m_NotificationsSectionData"
		);

		yield return null;

		m_NotificationsDuration = m_NotificationsSectionData.NotificationsDuration;

		// A prefab where the sprites will be set
		ResourceManager.LoadedData<GameObject> notificationPrefab = new ResourceManager.LoadedData<GameObject>();
		yield return ResourceManager.LoadResourceAsyncCoroutine
		(
			ResourcePath:			"Prefabs/UI/UI_Notification",
			loadedResource:			notificationPrefab,
			OnResourceLoaded :		_ => resourcesLoaded = true,
			OnFailure:				_ => resourcesLoaded = false
		);

		if ( resourcesLoaded )
		{
			Destroy( m_NotificationsPanel.GetChild(0).gameObject );
			m_NotificationsPanel.DetachChildren();

			yield return null;

			System.Action<Text> onItemAction = delegate( Text t )
			{
				t.gameObject.SetActive(false);

				RectTransform rectTransform = t.transform as RectTransform;

				t.transform.SetParent( parent: m_NotificationsPanel, false );
//				rectTransform.transform.localPosition = Vector3.zero;
//				rectTransform.anchorMin = Vector2.zero;
//				rectTransform.anchorMax = Vector2.zero;
			};

			// Pool Creation
			GameObjectsPoolConstructorData<Text> data = new GameObjectsPoolConstructorData<Text>()
			{
				Model			= notificationPrefab.Asset,
				Size			= MAX_ELEMENTS,
				ContainerName	= "UI_NotificationsPool",
				ActionOnObject	= onItemAction,
				IsAsyncBuild	= true
			};
			m_NotificationsPool = new GameObjectsPool<Text>( data );

			yield return data.CoroutineEnumerator;

			UI_Graphics.OnResolutionChanged += UI_Graphics_OnResolutionChanged;

			m_bIsInitialized = true;

			yield return null;

			CoroutinesManager.RemoveCoroutineFromPendingCount( 1 );
		}
		else
		{
			Debug.LogError( "UI_ComInterface: Bad initialization!!!" );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private void UI_Graphics_OnResolutionChanged( float newWidth, float newHeight )
	{
		foreach( Text textComponent in m_NotificationsPool )
		{

		}
	}



	//////////////////////////////////////////////////////////////////////////
	// ReInit
	IEnumerator IStateDefiner.ReInit()
	{
		yield return null;
	}


	//////////////////////////////////////////////////////////////////////////
	// Finalize
	bool IStateDefiner.Finalize()
	{
		return m_bIsInitialized;
	}

	
	//////////////////////////////////////////////////////////////////////////
	// ShowLabel
	private void OnEnable()
	{
		Debug.Log( name + " OnEnable " + Time.time);
		GameManager.UpdateEvents.OnThink += UpdateRequestQueue;
	}


	//////////////////////////////////////////////////////////////////////////
	// ShowLabel
	private void OnDisable()
	{
		Debug.Log( name + " OnDisable " + Time.time);
		GameManager.UpdateEvents.OnThink -= UpdateRequestQueue;
	}


	//////////////////////////////////////////////////////////////////////////
	// SendNotification
	public	void	SendNotification( string text, Color textColor )
	{
		NotificationRequest request = new NotificationRequest()
		{
			Text = text,
			Color = textColor
		};
		m_Requests.Enqueue( request );
	}	


	//////////////////////////////////////////////////////////////////////////
	// UpdateNotifications
	private	void	UpdateRequestQueue()
	{
		if ( m_bIsInitialized == false )
			return;

		if ( m_Requests.Count > 0 )
		{
			NotificationRequest request = m_Requests.Dequeue();
			EnableNotificationInternal( request );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// Update
	private void Update()
	{
		if ( m_bIsInitialized == false )
			return;
		
		// Disable ranged overflow
		if ( m_Notifications.Count >= MAX_ELEMENTS )
		{
			int delta = Mathf.Max( 1, m_Notifications.Count - MAX_ELEMENTS );
			for ( int i = 0; i < delta && i < m_Notifications.Count; i++ )
			{
				ComInterfaceNotification notification = m_Notifications[i];
				notification.TextComponent.gameObject.SetActive( false );

			}
			m_Notifications.RemoveRange( 0, delta );
		}
		

		for ( int i = m_Notifications.Count - 1; i > -1; i-- )
		{
			ComInterfaceNotification notification = m_Notifications[i];

			// Update time and color
			notification.CurrentTime -= Time.deltaTime;
			notification.TextComponent.color = Color.Lerp( notification.Color, Color.clear, 1.0f - Mathf.Pow( notification.CurrentTime, 2f ) / m_NotificationsDuration );

			// Remove if out of date
			if ( notification.CurrentTime < 0.0f )
			{
				notification.TextComponent.gameObject.SetActive( false );
				m_Notifications.RemoveAt( i );
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// UpdatePositions
	private	void	UpdatePositions()
	{
		Canvas canvas = transform.root.GetComponent<Canvas>();
		float scaleFactor = ( canvas.scaleFactor < 1.0f ) ? 1.0f : 1f / canvas.scaleFactor;

		int count = m_Notifications.Count - 1;
		for ( int i = count; i > -1; i-- )
		{
			ComInterfaceNotification notification = m_Notifications[i];

			notification.TextComponent.rectTransform.localPosition = 
				Vector2.up * 
				notification.TextComponent.rectTransform.rect.height * scaleFactor *
				( count - i );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// EnableIndicator
	private	bool	EnableNotificationInternal( NotificationRequest request )
	{
		// Set components properties
		Text textComponent = m_NotificationsPool.GetNextComponent();
		textComponent.text = request.Text;
		textComponent.color = request.Color;
		textComponent.gameObject.SetActive( true );

		// notification to active list
		ComInterfaceNotification activeNotification = new ComInterfaceNotification()
		{
			Color			= request.Color,
			CurrentTime		= m_NotificationsDuration,
			TextComponent	= textComponent
		};
		m_Notifications.Add( activeNotification );

		UpdatePositions();
		return true;
	}

}
