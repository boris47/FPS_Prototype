
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
			this.m_CameraPath = cameraPath;
			this.m_OldParent = CameraControl.Instance.Transform.parent;
			CameraControl.Instance.Transform.SetParent( null );

			this.m_PrevWeaponState = WeaponManager.Instance.CurrentWeapon.WeaponState;

			if (this.m_PrevWeaponState == EWeaponState.DRAWED )
				this.m_TimeToWait = WeaponManager.Instance.CurrentWeapon.Stash();
		}


		/// <summary>
		/// return true if completed, otherwise false
		/// </summary>
		/// <returns></returns>
		public	bool	Update()
		{
			if (this.m_TimeToWait > 0.0f )
			{
				this.m_TimeToWait -= Time.deltaTime;
				return false;
			}

			Transform cameraTransform = CameraControl.Instance.Transform;
			bool completed = this.m_CameraPath.Move( ref cameraTransform, null, null );

			return completed;
		}


		public	void	Terminate()
		{
			CameraControl.Instance.Transform.SetParent(this.m_OldParent );
			CameraControl.Instance.Transform.localPosition = Vector3.zero;
			CameraControl.Instance.Transform.localRotation = Quaternion.identity;

			if (this.m_PrevWeaponState == EWeaponState.DRAWED )
				WeaponManager.Instance.CurrentWeapon.Draw();

			this.m_TimeToWait		= 0.0f;
			this.m_CameraPath		= null;
			this.m_OldParent			= null;
		}

	}

}