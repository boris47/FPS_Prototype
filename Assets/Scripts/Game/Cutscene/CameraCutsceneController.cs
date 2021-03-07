
using UnityEngine;

namespace CutScene {

	[System.Serializable]
	public	class CameraCutsceneController
	{
		[SerializeField, ReadOnly]
		private		PathBase		m_CameraPath		= null;

		[SerializeField, ReadOnly]
		private		Transform		m_OldParent			= null;

		[SerializeField, ReadOnly]
		private		EWeaponState	m_PrevWeaponState	= EWeaponState.DRAWED;

		[SerializeField, ReadOnly]
		private		float			m_TimeToWait		= 0.0f;


		//////////////////////////////////////////////////////////////////////////
		public void Setup(PathBase cameraPath)
		{
			m_CameraPath = cameraPath;
			m_OldParent = FPSEntityCamera.Instance.transform.parent;
			FPSEntityCamera.Instance.transform.SetParent( null );

			m_PrevWeaponState = WeaponManager.Instance.CurrentWeapon.WeaponState;

			if (m_PrevWeaponState == EWeaponState.DRAWED )
			{
				m_TimeToWait = WeaponManager.Instance.CurrentWeapon.Stash();
			}
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> Return true if completed, otherwise false </summary>
		public bool	Update()
		{
			if (m_TimeToWait > 0.0f)
			{
				m_TimeToWait -= Time.deltaTime;
				return false;
			}

			bool completed = m_CameraPath.Move( FPSEntityCamera.Instance.transform, null, null );
			return completed;
		}


		//////////////////////////////////////////////////////////////////////////
		public void	Terminate()
		{
			FPSEntityCamera.Instance.transform.SetParent(m_OldParent );
			FPSEntityCamera.Instance.transform.localPosition = Vector3.zero;
			FPSEntityCamera.Instance.transform.localRotation = Quaternion.identity;

			if (m_PrevWeaponState == EWeaponState.DRAWED )
				WeaponManager.Instance.CurrentWeapon.Draw();

			m_TimeToWait		= 0.0f;
			m_CameraPath		= null;
			m_OldParent			= null;
		}

	}

}