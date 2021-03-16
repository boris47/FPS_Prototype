using UnityEngine;
using UnityEngine.UI;

public sealed class UI_Notifications : UI_Base, IStateDefiner
{
	[System.Serializable]
	private class Notification
	{
		public	readonly Text	TextComponent;

		public	float	RemainingTime;
		public	Color	Color;

		public Notification(Text textComponent)
		{
			this.TextComponent = textComponent;
		}
	}

	[System.Serializable]
	private class UI_NotificationsSectionData
	{
		public	float	NotificationsDuration = 5f;
	}

	private	const		uint								MAX_ELEMENTS						= 05u;

	[SerializeField, ReadOnly]
	private				Notification[]						m_Notifications						= new Notification[(int)MAX_ELEMENTS];

	[SerializeField, ReadOnly]
	private				UI_NotificationsSectionData			m_NotificationsSectionData			= new UI_NotificationsSectionData();

	private				RectTransform						m_NotificationsPanel				= null;
	private				float								m_NotificationsDefaultDuration		= 5f;
	private				bool								m_IsInitialized						= false;
						bool								IStateDefiner.IsInitialized			=> m_IsInitialized;


	//////////////////////////////////////////////////////////////////////////
	void IStateDefiner.PreInit()
	{

	}


	//////////////////////////////////////////////////////////////////////////
	void IStateDefiner.Initialize()
	{
		if (!m_IsInitialized)
		{
			CustomAssertions.IsTrue(transform.TrySearchComponentByChildName("NotificationsPanel", out m_NotificationsPanel));

			// Section Data
			if(CustomAssertions.IsTrue
			(
				GlobalManager.Configs.TryGetSection("Notifications", out Database.Section notificationseSection) && GlobalManager.Configs.TrySectionToOuter(notificationseSection, m_NotificationsSectionData),
				"Cannot load UI_NotificationsSectionData"
			))
			{
				m_NotificationsDefaultDuration = m_NotificationsSectionData.NotificationsDuration;
			}

			// Prefab for notification
			CustomAssertions.IsTrue(ResourceManager.LoadResourceSync("Prefabs/UI/UI_Notification", out GameObject notificationPrefab));
			CustomAssertions.IsTrue(notificationPrefab.transform.HasComponent<Text>());

			// Create notification Items
			for (uint i = 0; i < MAX_ELEMENTS; i++)
			{
				GameObject go = Instantiate(notificationPrefab);
				go.SetActive(false);

				go.transform.SetParent(m_NotificationsPanel, worldPositionStays: false);

				var component = go.GetComponent<Text>();
				{
					component.text = string.Empty;
				}
				m_Notifications[(int)i] = new Notification(component);
			}

			// disable navigation for everything
			Navigation noNavigationMode = new Navigation() { mode = Navigation.Mode.None };
			foreach (Selectable s in GetComponentsInChildren<Selectable>())
			{
				s.navigation = noNavigationMode;
			}

			UpdatePositions();

			UserSettings.VideoSettings.OnResolutionChanged += UI_Graphics_OnResolutionChanged;

			m_IsInitialized = true;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	void IStateDefiner.ReInit()
	{
		
	}


	//////////////////////////////////////////////////////////////////////////
	bool IStateDefiner.Finalize()
	{
		return m_IsInitialized;
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnEnable()
	{
		CustomAssertions.IsTrue(m_IsInitialized);
	}


	////////////////////////////////////////////////////////////////////
	private void UI_Graphics_OnResolutionChanged(float newWidth, float newHeight)
	{
		UpdatePositions();
	}


	//////////////////////////////////////////////////////////////////////////
	public void Notify(string text, Color color, float notificationTimeMs = 0f)
	{
		CustomAssertions.IsTrue(m_IsInitialized);

		notificationTimeMs = Mathf.Max(notificationTimeMs, 0f);
		notificationTimeMs = notificationTimeMs == 0f ? m_NotificationsDefaultDuration : notificationTimeMs;

		// In reverse iteration move informations from upper element to current
		for (uint i = MAX_ELEMENTS - 1; i >= 1; i--)
		{
			Notification current = m_Notifications[i];
			Notification upper = m_Notifications[i+1];

			current.Color = upper.Color;
			current.RemainingTime = upper.RemainingTime;
			current.TextComponent.text = upper.TextComponent.text;
		}

		Notification first = m_Notifications[0];
		first.TextComponent.text = text.Length > 0 ? text : "EMPTY TEXT";
		first.Color = color;
		first.RemainingTime = notificationTimeMs;
	}


	//////////////////////////////////////////////////////////////////////////
	private void Update()
	{
		CustomAssertions.IsTrue(m_IsInitialized);

		float deltaTime = Time.deltaTime;
		for (uint i = MAX_ELEMENTS - 1; i >= 1; i--)
		{
			Notification notification = m_Notifications[i];

			// Remove if out of date
			if (notification.RemainingTime < 0f && notification.TextComponent.text.Length > 0)
			{
				notification.TextComponent.gameObject.SetActive(false);
				notification.TextComponent.text = string.Empty;
			}
			else
			{
				// Update time and color
				notification.RemainingTime -= deltaTime;
				notification.TextComponent.color = Color.Lerp(notification.Color, Color.clear, deltaTime);
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private	void	UpdatePositions()
	{
		CustomAssertions.IsTrue(transform.root.TryGetComponent(out Canvas canvas));

		float scaleFactor = (canvas.scaleFactor < 1.0f) ? 1.0f : 1f / canvas.scaleFactor;

		int count = m_Notifications.Length;
		for (int i = 0; i < count; i++)
		{
			Notification notification = m_Notifications[i];

			Vector3 localPosition = Vector2.up * notification.TextComponent.rectTransform.rect.height * scaleFactor * (count - i);

			notification.TextComponent.rectTransform.localPosition = localPosition;
		}
	}
}
