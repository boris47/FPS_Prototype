
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
			this.enabled						= false;
		}


		//////////////////////////////////////////////////////////////////////////
		public	void	Play( PointsCollectionOnline pointsCollection )
		{
			if ( this.enabled == true )
				return;

			if ( pointsCollection == null || pointsCollection.Count == 0 )
			{
				return;
			}

			if ( !Utils.Base.SearchComponent(this.gameObject, out Entity entityParent, ESearchContext.PARENT ) )
			{
				return;
			}

			this.enabled = true;

			GameManager.UpdateEvents.OnFrame += this.OnFrameUpdate;

			this.m_CutsceneSubject = ECutsceneSubject.ENTITY;

			this.m_EntityCutsceneController.Setup( entityParent, pointsCollection );

			this.IsPlaying = true;

//			( entityParent as IEntitySimulation ).EnterSimulationState();

			// On start event called
			pointsCollection.OnStart?.Invoke();
		}


		//////////////////////////////////////////////////////////////////////////
		public	void	Play( PathBase cameraPath )
		{
			if ( this.enabled == true )
				return;

			if ( cameraPath == null )
				return;

			this.enabled						= true;

			GameManager.UpdateEvents.OnFrame += this.OnFrameUpdate;

			this.m_CutsceneSubject = ECutsceneSubject.CAMERA;

			this.m_CameraCutsceneController.Setup( cameraPath );

			this.IsPlaying = true;

			// start event called automatically called by path
		}


		//////////////////////////////////////////////////////////////////////////
		private	void	OnFrameUpdate( float DeltaTime )
		{
			if ( GameManager.IsPaused == true )
				return;

			if (this.IsPlaying == false )
				return;
			
			bool bHasCompleted = true;
			if (this.m_CutsceneSubject == ECutsceneSubject.ENTITY )
			{
				bHasCompleted = this.m_EntityCutsceneController.Update();
			}
			else
			{
				bHasCompleted = this.m_CameraCutsceneController.Update();
			}

			if ( bHasCompleted )
			{
				this.Terminate();
			}
		}


		//////////////////////////////////////////////////////////////////////////
		public	void	Terminate()
		{
			if (this.m_CutsceneSubject == ECutsceneSubject.ENTITY )
			{
				this.m_EntityCutsceneController.Terminate();
			}
			else
			{
				this.m_CameraCutsceneController.Terminate();
			}

			// Resetting internals
			this.IsPlaying							= false;

			// to save performance disable this script
			this.enabled						= false;

			GameManager.UpdateEvents.OnFrame -= this.OnFrameUpdate;
		}
	
	}


}