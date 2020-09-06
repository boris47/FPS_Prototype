using System.Collections.Generic;

namespace UnityEngine.AI
{
    [ExecuteInEditMode]
    [DefaultExecutionOrder(-101)]
    [AddComponentMenu("Navigation/NavMeshLink", 33)]
    [HelpURL("https://github.com/Unity-Technologies/NavMeshComponents#documentation-draft")]
    public class NavMeshLink : MonoBehaviour
    {
        [SerializeField]
        int m_AgentTypeID;
        public int AgentTypeID { get { return this.m_AgentTypeID; } set { this.m_AgentTypeID = value; this.UpdateLink(); } }

        [SerializeField]
        Vector3 m_StartPoint = new Vector3(0.0f, 0.0f, -2.5f);
        public Vector3 StartPoint { get { return this.m_StartPoint; } set { this.m_StartPoint = value; this.UpdateLink(); } }

        [SerializeField]
        Vector3 m_EndPoint = new Vector3(0.0f, 0.0f, 2.5f);
        public Vector3 EndPoint { get { return this.m_EndPoint; } set { this.m_EndPoint = value; this.UpdateLink(); } }

        [SerializeField]
        float m_Width;
        public float Width { get { return this.m_Width; } set { this.m_Width = value; this.UpdateLink(); } }

        [SerializeField]
        int m_CostModifier = -1;
        public int CostModifier { get { return this.m_CostModifier; } set { this.m_CostModifier = value; this.UpdateLink(); } }

        [SerializeField]
        bool m_Bidirectional = true;
        public bool Bidirectional { get { return this.m_Bidirectional; } set { this.m_Bidirectional = value; this.UpdateLink(); } }

        [SerializeField]
        bool m_AutoUpdatePosition;
        public bool AutoUpdate { get { return this.m_AutoUpdatePosition; } set { this.SetAutoUpdate(value); } }

        [SerializeField]
        int m_Area;
        public int Area { get { return this.m_Area; } set { this.m_Area = value; this.UpdateLink(); } }

        NavMeshLinkInstance m_LinkInstance = new NavMeshLinkInstance();

        Vector3 m_LastPosition = Vector3.zero;
        Quaternion m_LastRotation = Quaternion.identity;

        static readonly List<NavMeshLink> s_Tracked = new List<NavMeshLink>();

        void OnEnable()
        {
			this.AddLink();
            if (this.m_AutoUpdatePosition && this.m_LinkInstance.valid)
                AddTracking(this);
        }

        void OnDisable()
        {
            RemoveTracking(this);
			this.m_LinkInstance.Remove();
        }

        public void UpdateLink()
        {
			this.m_LinkInstance.Remove();
			this.AddLink();
        }

        static void AddTracking(NavMeshLink link)
        {
#if UNITY_EDITOR
            if (s_Tracked.Contains(link))
            {
                Debug.LogError("Link is already tracked: " + link);
                return;
            }
#endif

            if (s_Tracked.Count == 0)
                NavMesh.onPreUpdate += UpdateTrackedInstances;

            s_Tracked.Add(link);
        }

        static void RemoveTracking(NavMeshLink link)
        {
            s_Tracked.Remove(link);

            if (s_Tracked.Count == 0)
                NavMesh.onPreUpdate -= UpdateTrackedInstances;
        }

        void SetAutoUpdate(bool value)
        {
            if (this.m_AutoUpdatePosition == value)
                return;
			this.m_AutoUpdatePosition = value;
            if (value)
                AddTracking(this);
            else
                RemoveTracking(this);
        }

        void AddLink()
        {
#if UNITY_EDITOR
            if (this.m_LinkInstance.valid)
            {
                Debug.LogError("Link is already added: " + this);
                return;
            }
#endif

			NavMeshLinkData link = new NavMeshLinkData
			{
				startPosition = this.m_StartPoint,
				endPosition = this.m_EndPoint,
				width = this.m_Width,
				costModifier = this.m_CostModifier,
				bidirectional = this.m_Bidirectional,
				area = this.m_Area,
				agentTypeID = this.m_AgentTypeID
			};
			this.m_LinkInstance = NavMesh.AddLink(link, this.transform.position, this.transform.rotation);
            if (this.m_LinkInstance.valid)
				this.m_LinkInstance.owner = this;

			this.m_LastPosition = this.transform.position;
			this.m_LastRotation = this.transform.rotation;
        }

        bool HasTransformChanged()
        {
            if (this.m_LastPosition != this.transform.position) return true;
            if (this.m_LastRotation != this.transform.rotation) return true;
            return false;
        }

        void OnDidApplyAnimationProperties()
        {
			this.UpdateLink();
        }

        static void UpdateTrackedInstances()
        {
            foreach (NavMeshLink instance in s_Tracked)
            {
                if (instance.HasTransformChanged())
                    instance.UpdateLink();
            }
        }

#if UNITY_EDITOR
        void OnValidate()
        {
			this.m_Width = Mathf.Max(0.0f, this.m_Width);

            if (!this.m_LinkInstance.valid)
                return;

			this.UpdateLink();

            if (!this.m_AutoUpdatePosition)
            {
                RemoveTracking(this);
            }
            else if (!s_Tracked.Contains(this))
            {
                AddTracking(this);
            }
        }
#endif
    }
}
