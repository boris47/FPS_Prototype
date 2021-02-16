
using UnityEngine;

namespace CutScene {

	[System.Serializable]
	public	class CameraCutsceneController {

		private		PathBase		m_CameraPath		= null;

		private		Transform		m_OldParent			= null;

		private		EWeaponState		m_PrevWeaponState	= EWeaponState.DRAWED;


		private		float			m_TimeToWait		= 0.0f;

		public	void	Setup( PathBase cameraPath )
		{
			m_CameraPath = cameraPath;
			m_OldParent = CameraControl.Instance.transform.parent;
			CameraControl.Instance.transform.SetParent( null );

			m_PrevWeaponState = WeaponManager.Instance.CurrentWeapon.WeaponState;

			if (m_PrevWeaponState == EWeaponState.DRAWED )
				m_TimeToWait = WeaponManager.Instance.CurrentWeapon.Stash();
		}


		/// <summary>
		/// return true if completed, otherwise false
		/// </summary>
		/// <returns></returns>
		public	bool	Update()
		{
			if (m_TimeToWait > 0.0f )
			{
				m_TimeToWait -= Time.deltaTime;
				return false;
			}

			bool completed = m_CameraPath.Move( CameraControl.Instance.transform, null, null );
			return completed;
		}


		public	void	Terminate()
		{
			CameraControl.Instance.transform.SetParent(m_OldParent );
			CameraControl.Instance.transform.localPosition = Vector3.zero;
			CameraControl.Instance.transform.localRotation = Quaternion.identity;

			if (m_PrevWeaponState == EWeaponState.DRAWED )
				WeaponManager.Instance.CurrentWeapon.Draw();

			m_TimeToWait		= 0.0f;
			m_CameraPath		= null;
			m_OldParent			= null;
		}

	}

}