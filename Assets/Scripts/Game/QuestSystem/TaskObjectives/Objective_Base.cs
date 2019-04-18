

using UnityEngine;

namespace QuestSystem {

	public interface IObjective {

		void			Init					();

		bool			IsCompleted				{get; }

		void			AddToTask				( ITask task );

		void			RegisterOnCompletion	( System.Action<Objective_Base>	onCompletionCallback );

		void			OnObjectiveCompleted	();
	}


	public abstract class Objective_Base : MonoBehaviour, IObjective {

		[SerializeField]
		private GameEvent							m_OnCompletion				= null;

		protected	System.Action<Objective_Base>	m_OnCompletionCallback		= delegate { };
		protected	bool							m_IsCompleted				= false;
		protected	bool							m_IsCurrentlyActive			= false;

		protected	Transform						m_Signal					= null;


		public	void	Init()
		{
			GameObject o = Resources.Load("Prefabs/UI/Task_Objectives/Task_KillTarget") as GameObject;
			m_Signal = Instantiate( o ).transform;

			m_Signal.SetParent( UI.Instance.InGame.transform );
			m_Signal.gameObject.SetActive( false );
		}

		
		//////////////////////////////////////////////////////////////////////////
		// Update
		private void Update()
		{
			if ( m_IsCurrentlyActive )
			{
				DrawUIElementOnObjectives( transform, m_Signal );
			}
		}

		//--
		bool			IObjective.IsCompleted
		{
			get { return m_IsCompleted; }
		}


		//////////////////////////////////////////////////////////////////////////
		// SetTaskOwner ( Interface )
		void			IObjective.AddToTask( ITask task )
		{
			task.AddObjective( this );
		}


		//////////////////////////////////////////////////////////////////////////
		// RegisterOnCompletion ( Interface )
		void		IObjective.RegisterOnCompletion( System.Action<Objective_Base>	onCompletionCallback )
		{
			m_OnCompletionCallback = onCompletionCallback;
		}




		//////////////////////////////////////////////////////////////////////////
		// OnObjectiveCompleted ( Interface )
		public	void			OnObjectiveCompleted()
		{
			// Internal Flag
			m_IsCompleted = true;

			// Unity Events
			if ( m_OnCompletion != null && m_OnCompletion.GetPersistentEventCount() > 0 )
				m_OnCompletion.Invoke();

			// Internal Delegates
			m_OnCompletionCallback( this );
		}


		//////////////////////////////////////////////////////////////////////////
		public	abstract	void		Enable();


		//////////////////////////////////////////////////////////////////////////
		protected	static void	DrawUIElementOnObjectives( Transform targetTransform, Transform Signal )
		{
			Camera camera = CameraControl.Instance.MainCamera;

		//  Ref: https://www.youtube.com/watch?v=gAQpR1GN0Os
		//  Viewport space is normalized and relative to the camera. The bottom-left of the camera is (0,0); the top-right is (1,1). The z position is in world units from the camera.
		//  Screenspace is defined in pixels. The bottom-left of the screen is (0,0); the right-top is (pixelWidth,pixelHeight). The z position is in world units from the camera.
		//	Vector3 ViewportPoint = camera.WorldToViewportPoint( taretTransform.position );
			Vector3 ScreenPoint = camera.WorldToScreenPoint( targetTransform.position );

			// Normal projection because inside screen
			if ( ScreenPoint.z > 0f && ScreenPoint.x > 0f && ScreenPoint.x < Screen.width && ScreenPoint.y > 0f && ScreenPoint.y < Screen.height )
			{
//				GUI.DrawTexture( new Rect( ScreenPoint, BadSmile.texelSize ), BadSmile );
//				m_CrosshairTransform.position = ScreenPoint;
				Signal.position = ScreenPoint;
			}
			else // Off screen
			{
				bool bIsBehind = ScreenPoint.z < 0.0f;
				if ( bIsBehind )
				{
					ScreenPoint *= -1.0f;
				}

				Vector3 screenCenter = new Vector3( Screen.width, Screen.height, 0.0f ) * 0.5f;

				// NOTE COORDINATE TRASLATED
				// make 0, 0 the center of screen inteead of bottom left
				ScreenPoint -= screenCenter;

				// Find angle from center of screen to mouse position
				float angle = Mathf.Atan2( ScreenPoint.y, ScreenPoint.x ) - 90f * Mathf.Deg2Rad;
				float cos = Mathf.Cos( angle );
				float sin = -Mathf.Sin( angle );

				ScreenPoint = screenCenter + new Vector3( sin * 150f, cos * 150f, 0.0f );

				// y = mx + b format
				float m = cos / sin;

				Vector3 screenBounds = screenCenter * 0.9f;

				// Check up and down first
				if ( cos > 0.0f )
				{
					ScreenPoint = new Vector3( screenBounds.y/m, screenBounds.y, 0f );
				}
				else // down
				{
					ScreenPoint = new Vector3( -screenBounds.y/m, -screenBounds.y, 0f );
				}

				// If out of bounds, get point on appropriate side
				if ( ScreenPoint.x > screenBounds.x ) // out of bounds, must be on right
				{
					ScreenPoint = new Vector3( screenBounds.x, screenBounds.x*m, 0f );
				}
				else if ( ScreenPoint.x < -screenBounds.x ) // out of bounds left
				{
					ScreenPoint = new Vector3( -screenBounds.x, -screenBounds.x*m, 0f );
				}

				// Remove cooridnate traslation
				ScreenPoint += screenCenter;

				Signal.position = ScreenPoint;
			}
		}
	}

}