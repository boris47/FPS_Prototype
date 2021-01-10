
using UnityEngine;

namespace CutScene {

	public	enum ECutsceneSubject { ENTITY, CAMERA }

	public class CutsceneEntityManager : MonoBehaviour {
	
		public		bool							IsPlaying					{ get; private set; }

		private		EntityCutsceneController		m_EntityCutsceneController	= new EntityCutsceneController();
		private		CameraCutsceneController		m_CameraCutsceneController	= new CameraCutsceneController();
		private		ECutsceneSubject				m_CutsceneSubject			= ECutsceneSubject.ENTITY;



		//////////////////////////////////////////////////////////////////////////
		private void	Awake()
		{
			enabled						= false;
		}


		//////////////////////////////////////////////////////////////////////////
		public	void	Play( PointsCollectionOnline pointsCollection )
		{
			if ( enabled == true )
				return;

			if ( pointsCollection == null || pointsCollection.Count == 0 )
			{
				return;
			}

			if ( !Utils.Base.SearchComponent(gameObject, out Entity entityParent, ESearchContext.PARENT ) )
			{
				return;
			}

			enabled = true;

			GameManager.UpdateEvents.OnFrame += OnFrameUpdate;

			m_CutsceneSubject = ECutsceneSubject.ENTITY;

			m_EntityCutsceneController.Setup( entityParent, pointsCollection );

			IsPlaying = true;

//			( entityParent as IEntitySimulation ).EnterSimulationState();

			// On start event called
			pointsCollection.OnStart?.Invoke();
		}


		//////////////////////////////////////////////////////////////////////////
		public	void	Play( PathBase cameraPath )
		{
			if ( enabled == true )
				return;

			if ( cameraPath == null )
				return;

			enabled						= true;

			GameManager.UpdateEvents.OnFrame += OnFrameUpdate;

			m_CutsceneSubject = ECutsceneSubject.CAMERA;

			m_CameraCutsceneController.Setup( cameraPath );

			IsPlaying = true;

			// start event called automatically called by path
		}


		//////////////////////////////////////////////////////////////////////////
		private	void	OnFrameUpdate( float DeltaTime )
		{
			if ( GameManager.IsPaused == true )
				return;

			if (IsPlaying == false )
				return;
			
			bool bHasCompleted = true;
			if (m_CutsceneSubject == ECutsceneSubject.ENTITY )
			{
				bHasCompleted = m_EntityCutsceneController.Update();
			}
			else
			{
				bHasCompleted = m_CameraCutsceneController.Update();
			}

			if ( bHasCompleted )
			{
				Terminate();
			}
		}


		//////////////////////////////////////////////////////////////////////////
		public	void	Terminate()
		{
			if (m_CutsceneSubject == ECutsceneSubject.ENTITY )
			{
				m_EntityCutsceneController.Terminate();
			}
			else
			{
				m_CameraCutsceneController.Terminate();
			}

			// Resetting internals
			IsPlaying							= false;

			// to save performance disable this script
			enabled						= false;

			GameManager.UpdateEvents.OnFrame -= OnFrameUpdate;
		}
	
	}


}