

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

		protected	Texture							m_Texture					= null;
		protected	Rect							m_DrawRect					= new Rect();
		protected	bool							m_IsTextureLoaded			= false;


		//--
		bool			IObjective.IsCompleted
		{
			get { return m_IsCompleted; }
		}


		//////////////////////////////////////////////////////////////////////////
		// Init ( Interface )
		void IObjective.Init()
		{			
			ResourceManager.LoadData<Texture2D> loadData = new ResourceManager.LoadData<Texture2D>();
			System.Action<Texture2D> onTextureLoaded = delegate( Texture2D t )
			{
				m_Texture = t;
				m_IsTextureLoaded = true;

				m_DrawRect.width  = m_Texture.width;
				m_DrawRect.height = m_Texture.height;
			};
			ResourceManager.LoadResourceAsync( "Textures/Task_Objectives/BadSmile", loadData, onTextureLoaded );
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

			print( "Completed Objective " + name );
		}


		//////////////////////////////////////////////////////////////////////////
		public	abstract	void		Activate();


		//////////////////////////////////////////////////////////////////////////
		private void OnGUI()
		{
			if ( m_IsTextureLoaded && m_IsCurrentlyActive )
			{
				DrawUIElementOnObjectives( transform, ref m_DrawRect );
				GUI.DrawTexture( m_DrawRect, m_Texture );
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected	static void	DrawUIElementOnObjectives( Transform targetTransform, ref Rect DrawRect )
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
				DrawRect.x = ScreenPoint.x - DrawRect.width*0.5f;
				DrawRect.y = Screen.height - ( ScreenPoint.y + DrawRect.height*0.5f );
			}
			else // Off screen
			{
				bool bIsBehind = ScreenPoint.z < 0.0f;
				if ( bIsBehind )
				{
					ScreenPoint *= -1.0f;
				}

				Vector2 ScreenPoint2D = ScreenPoint;
				Vector2 screenCenter2D = new Vector2( Screen.width, Screen.height ) * 0.5f;

				// NOTE COORDINATE TRASLATED
				// make 0, 0 the center of screen inteead of bottom left
				ScreenPoint2D -= screenCenter2D;

				// Find angle from center of screen to mouse position
				float angle = Mathf.Atan2( ScreenPoint2D.y, ScreenPoint2D.x ) - 90f * Mathf.Deg2Rad;
				float cos = Mathf.Cos( angle );
				float sin = -Mathf.Sin( angle );

				const float amplify = 150f;
				ScreenPoint2D.Set( screenCenter2D.x + sin * amplify, screenCenter2D.y + cos * amplify );

				// y = mx + b format
				float m = cos / sin;

				Vector2 screenBounds = screenCenter2D * 0.9f;

				// Check up and down first
				if ( cos > 0.0f )
				{
					ScreenPoint2D.Set( screenBounds.y/m, screenBounds.y );
				}
				else // down
				{
					ScreenPoint2D.Set( -screenBounds.y/m, -screenBounds.y );
				}

				// If out of bounds, get point on appropriate side
				if ( ScreenPoint2D.x > screenBounds.x ) // out of bounds, must be on right
				{
					ScreenPoint2D.Set( screenBounds.x, screenBounds.x*m );
				}
				else if ( ScreenPoint2D.x < -screenBounds.x ) // out of bounds left
				{
					ScreenPoint2D.Set( -screenBounds.x, -screenBounds.x*m );
				}

				// Remove cooridnate traslation
				ScreenPoint2D += screenCenter2D;

				DrawRect.x = ScreenPoint2D.x - DrawRect.width*0.5f;
				DrawRect.y = Screen.height - ( ScreenPoint2D.y + DrawRect.height*0.5f );
			}
		}
	}

}