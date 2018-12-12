
using UnityEngine;

namespace CutScene {

	public	class CameraCutsceneController {

		private		PathBase		m_CameraPath		= null;

		private		Transform		m_OldParent			= null;

		private		WeaponState		m_PrevWeaponState	= WeaponState.DRAWED;


		private		float			m_TimeToWait		= 0.0f;

		public	void	Setup( PathBase cameraPath )
		{
			m_CameraPath = cameraPath;
			m_OldParent = CameraControl.Instance.Transform.parent;
			CameraControl.Instance.Transform.SetParent( null );

			m_PrevWeaponState = WeaponManager.Instance.CurrentWeapon.WeaponState;

			if ( m_PrevWeaponState == WeaponState.DRAWED )
				m_TimeToWait = WeaponManager.Instance.CurrentWeapon.Stash();
		}


		/// <summary>
		/// return true if completed, otherwise false
		/// </summary>
		/// <returns></returns>
		public	bool	Update()
		{
			if ( m_TimeToWait > 0.0f )
			{
				m_TimeToWait -= Time.deltaTime;
				return false;
			}

			Transform cameraTransform = CameraControl.Instance.Transform;
			bool completed = m_CameraPath.Move( ref cameraTransform, null, null );

			return completed;
		}


		public	void	Terminate()
		{
			CameraControl.Instance.Transform.SetParent( m_OldParent );
			CameraControl.Instance.Transform.localPosition = Vector3.zero;
			CameraControl.Instance.Transform.localRotation = Quaternion.identity;

			if ( m_PrevWeaponState == WeaponState.STASHED )
				WeaponManager.Instance.CurrentWeapon.Draw();

			m_PrevWeaponState	= WeaponState.DRAWED;
			m_TimeToWait		= 0.0f;
			m_CameraPath		= null;
			m_OldParent			= null;
		}

	}

}