using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_ComInterface : MonoBehaviour, IStateDefiner {

	private	const	int			MAX_ELEMENTS = 30;

	private		GameObjectsPool<Text>				m_NotificationsPool						= null;

	private		RectTransform						m_NotificationsPanel					= null;

	// STORE DATA
	[System.Serializable]
	private class ComInterfaceNotification {
		public	float	Duration;
		public	float	CurrentTime;
		public	Text	TextComponent;
		public	Color	Color;
	}
	private	List<ComInterfaceNotification> m_Notifications	= new List<ComInterfaceNotification>();

	// REQUESTS
	[System.Serializable]
	private	struct NotificationRequest {
		public	float	Duration;
		public	string	Text;
		public	Color	Color;
	}
	[SerializeField]
	private	List<NotificationRequest>					m_Requests					= new List<NotificationRequest>();

	// SECTION DATA
	[System.Serializable]
	private class UI_NotificationsSectionData {
		
	}
	[SerializeField]
	private		UI_NotificationsSectionData			m_NotificationsSectionData = new UI_NotificationsSectionData();


	// INITIALIZATION
	private	bool			m_bIsInitialized	= false;
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

		m_NotificationsPanel = transform.Find( "NotificationsPanel" ) as RectTransform;
		UnityEngine.Assertions.Assert.IsTrue( m_NotificationsPanel != null );

		bool resourcesLoaded = true;

		// Section Data
		UnityEngine.Assertions.Assert.IsTrue
		(
			GlobalManager.Configs.bGetSection( "Notifications", m_NotificationsSectionData ),
			"UI_ComInterface::Initialize:Cannot load m_NotificationsSectionData"
		);

		// A prefab where the sprites will be set
		ResourceManager.LoadedData<GameObject> notificationPrefab = new ResourceManager.LoadedData<GameObject>();
		yield return ResourceManager.LoadResourceAsyncCoroutine
		(
			ResourcePath:			"Prefabs/UI/UI_Notification",
			loadedData:				notificationPrefab,
			OnResourceLoaded :		(a) => { resourcesLoaded &= true; },
			OnFailure:				(p) => resourcesLoaded &= false
		);

		if ( resourcesLoaded )
		{
			System.Action<Text> onItemAction = delegate( Text t )
			{
				t.gameObject.SetActive(false);

				RectTransform rectTransform = t.transform as RectTransform;

				t.transform.SetParent( parent: m_NotificationsPanel );
				rectTransform.transform.localPosition = Vector3.zero;
				rectTransform.anchorMin = Vector2.zero;
				rectTransform.anchorMax = Vector2.zero;
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
			m_bIsInitialized = true;
		}
		else
		{
			Debug.LogError( "UI_ComInterface: Bad initialization!!!" );
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
//		GameManager.UpdateEvents.OnThink += UpdateNotifications;
	}


	//////////////////////////////////////////////////////////////////////////
	// ShowLabel
	private void OnDisable()
	{
//		GameManager.UpdateEvents.OnThink -= UpdateNotifications;
	}


	//////////////////////////////////////////////////////////////////////////
	// SendNotification
	public	void	SendNotification( string text, float duration, Color textColor )
	{
		NotificationRequest request = new NotificationRequest()
		{
			Text = text,
			Duration = duration,
			Color = textColor
		};
		m_Requests.Add( request );
	} 

	
	//////////////////////////////////////////////////////////////////////////
	// ShowLabel
	public	void	ShowLabel( string text )
	{

	}
	


	//////////////////////////////////////////////////////////////////////////
	// UpdateNotifications
	private	void	Update()
	{
		if ( m_bIsInitialized == false )
			return;

		for ( int i = 0; i < m_Requests.Count; i++ )
		{
			NotificationRequest request = m_Requests[i];
			m_Requests.RemoveAt(i);
			EnableNotificationInternal( request );
		}
		m_Requests.Clear();

		InternalCheck();
	}


	//////////////////////////////////////////////////////////////////////////
	// InternalCheck
	/// <summary> Check whetever some data has been invalidated </summary>
	private	void InternalCheck()
	{
		for ( int i = m_Notifications.Count - 1; i >= 0; i-- )
		{
			ComInterfaceNotification n = m_Notifications[i];
			if ( n.CurrentTime < 0.0f )
			{
				m_Notifications[i].TextComponent.gameObject.SetActive( false );
				m_Notifications.RemoveAt( i );
			}
			n.CurrentTime -= Time.deltaTime;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// EnableIndicator
	private	bool	EnableNotificationInternal( NotificationRequest request )
	{
		InternalCheck();

		Text textComponent = m_NotificationsPool.GetNextComponent();

		textComponent.text = request.Text;
		textComponent.color = request.Color;

		ComInterfaceNotification activeNotification = new ComInterfaceNotification()
		{
			Color = request.Color,
			Duration = request.Duration,
			CurrentTime = request.Duration,
			TextComponent = textComponent
		};
		m_Notifications.Add( activeNotification );

		textComponent.gameObject.SetActive( true );
		return true;
	}

}
