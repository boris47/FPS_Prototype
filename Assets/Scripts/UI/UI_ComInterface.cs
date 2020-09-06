using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class UI_ComInterface : MonoBehaviour, IStateDefiner {

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
	private	bool			m_IsInitialized	= false;
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

		this.m_NotificationsPanel = this.transform.Find( "NotificationsPanel" ) as RectTransform;
		UnityEngine.Assertions.Assert.IsNotNull(this.m_NotificationsPanel );

		yield return null;

		bool resourcesLoaded = true;

		// Section Data
		UnityEngine.Assertions.Assert.IsTrue
		(
			GlobalManager.Configs.GetSection( "Notifications", this.m_NotificationsSectionData ),
			"UI_ComInterface::Initialize:Cannot load m_NotificationsSectionData"
		);

		yield return null;

		this.m_NotificationsDuration = this.m_NotificationsSectionData.NotificationsDuration;

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
			Destroy(this.m_NotificationsPanel.GetChild(0).gameObject );
			this.m_NotificationsPanel.DetachChildren();

			yield return null;

			System.Action<Text> onItemAction = delegate( Text t )
			{
				t.gameObject.SetActive(false);

				RectTransform rectTransform = t.transform as RectTransform;

				t.transform.SetParent( parent: this.m_NotificationsPanel, false );
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
			this.m_NotificationsPool = new GameObjectsPool<Text>( data );

			yield return data.CoroutineEnumerator;

			UserSettings.VideoSettings.OnResolutionChanged += this.UI_Graphics_OnResolutionChanged;

			this.m_IsInitialized = true;

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
		foreach( Text textComponent in this.m_NotificationsPool )
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
		return this.m_IsInitialized;
	}

	
	//////////////////////////////////////////////////////////////////////////
	// ShowLabel
	private void OnEnable()
	{
		Debug.Log(this.name + " OnEnable " + Time.time);
		GameManager.UpdateEvents.OnThink += this.UpdateRequestQueue;
	}


	//////////////////////////////////////////////////////////////////////////
	// ShowLabel
	private void OnDisable()
	{
		Debug.Log(this.name + " OnDisable " + Time.time);
		GameManager.UpdateEvents.OnThink -= this.UpdateRequestQueue;
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
		this.m_Requests.Enqueue( request );
	}	


	//////////////////////////////////////////////////////////////////////////
	// UpdateNotifications
	private	void	UpdateRequestQueue()
	{
		if (this.m_IsInitialized == false )
			return;

		if (this.m_Requests.Count > 0 )
		{
			NotificationRequest request = this.m_Requests.Dequeue();
			this.EnableNotificationInternal( request );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// Update
	private void Update()
	{
		if (this.m_IsInitialized == false )
			return;
		
		// Disable ranged overflow
		if (this.m_Notifications.Count >= MAX_ELEMENTS )
		{
			int delta = Mathf.Max( 1, this.m_Notifications.Count - MAX_ELEMENTS );
			for ( int i = 0; i < delta && i < this.m_Notifications.Count; i++ )
			{
				ComInterfaceNotification notification = this.m_Notifications[i];
				notification.TextComponent.gameObject.SetActive( false );

			}
			this.m_Notifications.RemoveRange( 0, delta );
		}
		

		for ( int i = this.m_Notifications.Count - 1; i > -1; i-- )
		{
			ComInterfaceNotification notification = this.m_Notifications[i];

			// Update time and color
			notification.CurrentTime -= Time.deltaTime;
			notification.TextComponent.color = Color.Lerp( notification.Color, Color.clear, 1.0f - Mathf.Pow( notification.CurrentTime, 2f ) / this.m_NotificationsDuration );

			// Remove if out of date
			if ( notification.CurrentTime < 0.0f )
			{
				notification.TextComponent.gameObject.SetActive( false );
				this.m_Notifications.RemoveAt( i );
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// UpdatePositions
	private	void	UpdatePositions()
	{
		Canvas canvas = this.transform.root.GetComponent<Canvas>();
		float scaleFactor = ( canvas.scaleFactor < 1.0f ) ? 1.0f : 1f / canvas.scaleFactor;

		int count = this.m_Notifications.Count - 1;
		for ( int i = count; i > -1; i-- )
		{
			ComInterfaceNotification notification = this.m_Notifications[i];

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
		Text textComponent = this.m_NotificationsPool.GetNextComponent();
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
		this.m_Notifications.Add( activeNotification );

		this.UpdatePositions();
		return true;
	}

}
