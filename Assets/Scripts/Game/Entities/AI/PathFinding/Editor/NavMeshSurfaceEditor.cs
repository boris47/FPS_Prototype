using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine.AI;
using UnityEngine;

namespace UnityEditor.AI
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(NavMeshSurface))]
    class NavMeshSurfaceEditor : Editor
    {
        SerializedProperty m_AgentTypeID;
        SerializedProperty m_BuildHeightMesh;
        SerializedProperty m_Center;
        SerializedProperty m_CollectObjects;
        SerializedProperty m_DefaultArea;
        SerializedProperty m_LayerMask;
        SerializedProperty m_OverrideTileSize;
        SerializedProperty m_OverrideVoxelSize;
        SerializedProperty m_Size;
        SerializedProperty m_TileSize;
        SerializedProperty m_UseGeometry;
        SerializedProperty m_VoxelSize;

        class Styles
        {
            public readonly GUIContent m_LayerMask = new GUIContent("Include Layers");

            public readonly GUIContent m_ShowInputGeom = new GUIContent("Show Input Geom");
            public readonly GUIContent m_ShowVoxels = new GUIContent("Show Voxels");
            public readonly GUIContent m_ShowRegions = new GUIContent("Show Regions");
            public readonly GUIContent m_ShowRawContours = new GUIContent("Show Raw Contours");
            public readonly GUIContent m_ShowContours = new GUIContent("Show Contours");
            public readonly GUIContent m_ShowPolyMesh = new GUIContent("Show Poly Mesh");
            public readonly GUIContent m_ShowPolyMeshDetail = new GUIContent("Show Poly Mesh Detail");
        }

        struct AsyncBakeOperation
        {
            public NavMeshSurface surface;
            public NavMeshData bakeData;
            public AsyncOperation bakeOperation;
        }

        static List<AsyncBakeOperation> s_BakeOperations = new List<AsyncBakeOperation>();

        static Styles s_Styles;

//        static bool s_ShowDebugOptions;

        static Color s_HandleColor = new Color(127f, 214f, 244f, 100f) / 255;
        static Color s_HandleColorSelected = new Color(127f, 214f, 244f, 210f) / 255;
        static Color s_HandleColorDisabled = new Color(127f * 0.75f, 214f * 0.75f, 244f * 0.75f, 100f) / 255;

        BoxBoundsHandle m_BoundsHandle = new BoxBoundsHandle(/*02*/);

        bool editingCollider
        {
            get { return EditMode.editMode == EditMode.SceneViewEditMode.Collider && EditMode.IsOwner(this); }
        }

        void OnEnable()
        {
			this.m_AgentTypeID = this.serializedObject.FindProperty("m_AgentTypeID");
			this.m_BuildHeightMesh = this.serializedObject.FindProperty("m_BuildHeightMesh");
			this.m_Center = this.serializedObject.FindProperty("m_Center");
			this.m_CollectObjects = this.serializedObject.FindProperty("m_CollectObjects");
			this.m_DefaultArea = this.serializedObject.FindProperty("m_DefaultArea");
			this.m_LayerMask = this.serializedObject.FindProperty("m_LayerMask");
			this.m_OverrideTileSize = this.serializedObject.FindProperty("m_OverrideTileSize");
			this.m_OverrideVoxelSize = this.serializedObject.FindProperty("m_OverrideVoxelSize");
			this.m_Size = this.serializedObject.FindProperty("m_Size");
			this.m_TileSize = this.serializedObject.FindProperty("m_TileSize");
			this.m_UseGeometry = this.serializedObject.FindProperty("m_UseGeometry");
			this.m_VoxelSize = this.serializedObject.FindProperty("m_VoxelSize");

            NavMeshVisualizationSettings.showNavigation++;
        }

        void OnDisable()
        {
            NavMeshVisualizationSettings.showNavigation--;
        }

        static string GetAndEnsureTargetPath(NavMeshSurface surface)
        {
			// Create directory for the asset if it does not exist yet.
			string activeScenePath = surface.gameObject.scene.path;

			string targetPath = "Assets";
            if (!string.IsNullOrEmpty(activeScenePath))
                targetPath = Path.Combine(Path.GetDirectoryName(activeScenePath), Path.GetFileNameWithoutExtension(activeScenePath));
            if (!Directory.Exists(targetPath))
                Directory.CreateDirectory(targetPath);
            return targetPath;
        }

        static void CreateNavMeshAsset(NavMeshSurface surface)
        {
			string targetPath = GetAndEnsureTargetPath(surface);

			string combinedAssetPath = Path.Combine(targetPath, "NavMesh-" + surface.name + ".asset");
            combinedAssetPath = AssetDatabase.GenerateUniqueAssetPath(combinedAssetPath);
            AssetDatabase.CreateAsset(surface.navMeshData, combinedAssetPath);
        }

        static NavMeshData GetNavMeshAssetToDelete(NavMeshSurface navSurface)
        {
    //        var prefabType = PrefabUtility.GetPrefabType(navSurface);
    //        if (prefabType == PrefabType.PrefabInstance || prefabType == PrefabType.DisconnectedPrefabInstance)
    //        {
				///*
    //            // Don't allow deleting the asset belonging to the prefab parent
    //            var parentSurface = PrefabUtility.GetCorrespondingObjectFromSource(navSurface) as NavMeshSurface;
    //            if (parentSurface && navSurface.navMeshData == parentSurface.navMeshData)
    //                return null;
				//*/
    //        }
            return navSurface.navMeshData;
        }

        void ClearSurface(NavMeshSurface navSurface)
        {
			NavMeshData assetToDelete = GetNavMeshAssetToDelete(navSurface);
            navSurface.RemoveData();
            navSurface.navMeshData = null;
            EditorUtility.SetDirty(navSurface);

            if (assetToDelete)
            {
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(assetToDelete));
                EditorSceneManager.MarkSceneDirty(navSurface.gameObject.scene);
            }
        }

        Bounds GetBounds()
        {
			NavMeshSurface navSurface = (NavMeshSurface)this.target;
            return new Bounds(navSurface.transform.position, navSurface.size);
        }

        public override void OnInspectorGUI()
        {
            if (s_Styles == null)
                s_Styles = new Styles();

			this.serializedObject.Update();

			NavMeshBuildSettings bs = NavMesh.GetSettingsByID(this.m_AgentTypeID.intValue);

            if (bs.agentTypeID != -1)
            {
                // Draw image
                const float diagramHeight = 80.0f;
                Rect agentDiagramRect = EditorGUILayout.GetControlRect(false, diagramHeight);
                NavMeshEditorHelpers.DrawAgentDiagram(agentDiagramRect, bs.agentRadius, bs.agentHeight, bs.agentClimb, bs.agentSlope);
            }
            NavMeshComponentsGUIUtility.AgentTypePopup("Agent Type", this.m_AgentTypeID);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(this.m_CollectObjects);
            if ((CollectObjects)this.m_CollectObjects.enumValueIndex == CollectObjects.Volume)
            {
                EditorGUI.indentLevel++;

                EditMode.DoEditModeInspectorModeButton(EditMode.SceneViewEditMode.Collider, "Edit Volume",
                    EditorGUIUtility.IconContent("EditCollider"), this.GetBounds/*()*/, this);
                EditorGUILayout.PropertyField(this.m_Size);
                EditorGUILayout.PropertyField(this.m_Center);

                EditorGUI.indentLevel--;
            }
            else
            {
                if (this.editingCollider)
                    EditMode.QuitEditMode();
            }

            EditorGUILayout.PropertyField(this.m_LayerMask, s_Styles.m_LayerMask);
            EditorGUILayout.PropertyField(this.m_UseGeometry);

            EditorGUILayout.Space();

            EditorGUILayout.Space();

			this.m_OverrideVoxelSize.isExpanded = EditorGUILayout.Foldout(this.m_OverrideVoxelSize.isExpanded, "Advanced");
            if (this.m_OverrideVoxelSize.isExpanded)
            {
                EditorGUI.indentLevel++;

                NavMeshComponentsGUIUtility.AreaPopup("Default Area", this.m_DefaultArea);

                // Override voxel size.
                EditorGUILayout.PropertyField(this.m_OverrideVoxelSize);

                using (new EditorGUI.DisabledScope(!this.m_OverrideVoxelSize.boolValue || this.m_OverrideVoxelSize.hasMultipleDifferentValues))
                {
                    EditorGUI.indentLevel++;

                    EditorGUILayout.PropertyField(this.m_VoxelSize);

                    if (!this.m_OverrideVoxelSize.hasMultipleDifferentValues)
                    {
                        if (!this.m_AgentTypeID.hasMultipleDifferentValues)
                        {
                            float voxelsPerRadius = this.m_VoxelSize.floatValue > 0.0f ? (bs.agentRadius / this.m_VoxelSize.floatValue) : 0.0f;
                            EditorGUILayout.LabelField(" ", voxelsPerRadius.ToString("0.00") + " voxels per agent radius", EditorStyles.miniLabel);
                        }
                        if (this.m_OverrideVoxelSize.boolValue)
                            EditorGUILayout.HelpBox("Voxel size controls how accurately the navigation mesh is generated from the level geometry. A good voxel size is 2-4 voxels per agent radius. Making voxel size smaller will increase build time.", MessageType.None);
                    }
                    EditorGUI.indentLevel--;
                }

                // Override tile size
                EditorGUILayout.PropertyField(this.m_OverrideTileSize);

                using (new EditorGUI.DisabledScope(!this.m_OverrideTileSize.boolValue || this.m_OverrideTileSize.hasMultipleDifferentValues))
                {
                    EditorGUI.indentLevel++;

                    EditorGUILayout.PropertyField(this.m_TileSize);

                    if (!this.m_TileSize.hasMultipleDifferentValues && !this.m_VoxelSize.hasMultipleDifferentValues)
                    {
                        float tileWorldSize = this.m_TileSize.intValue * this.m_VoxelSize.floatValue;
                        EditorGUILayout.LabelField(" ", tileWorldSize.ToString("0.00") + " world units", EditorStyles.miniLabel);
                    }

                    if (!this.m_OverrideTileSize.hasMultipleDifferentValues)
                    {
                        if (this.m_OverrideTileSize.boolValue)
                            EditorGUILayout.HelpBox("Tile size controls the how local the changes to the world are (rebuild or carve). Small tile size allows more local changes, while potentially generating more data in overal.", MessageType.None);
                    }
                    EditorGUI.indentLevel--;
                }


                // Height mesh
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.PropertyField(this.m_BuildHeightMesh);
                }

                EditorGUILayout.Space();
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

			this.serializedObject.ApplyModifiedProperties();

			bool hadError = false;
			bool multipleTargets = this.targets.Length > 1;
            foreach (NavMeshSurface navSurface in this.targets)
            {
				NavMeshBuildSettings settings = navSurface.GetBuildSettings();
				// Calculating bounds is potentially expensive when unbounded - so here we just use the center/size.
				// It means the validation is not checking vertical voxel limit correctly when the surface is set to something else than "in volume".
				Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
                if (navSurface.collectObjects == CollectObjects.Volume)
                {
                    bounds = new Bounds(navSurface.center, navSurface.size);
                }

				string[] errors = settings.ValidationReport(bounds);
                if (errors.Length > 0)
                {
                    if (multipleTargets)
                        EditorGUILayout.LabelField(navSurface.name);
                    foreach (string err in errors)
                    {
                        EditorGUILayout.HelpBox(err, MessageType.Warning);
                    }
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(EditorGUIUtility.labelWidth);
                    if (GUILayout.Button("Open Agent Settings...", EditorStyles.miniButton))
                        NavMeshEditorHelpers.OpenAgentSettings(navSurface.agentTypeID);
                    GUILayout.EndHorizontal();
                    hadError = true;
                }
            }

            if (hadError)
                EditorGUILayout.Space();

            using (new EditorGUI.DisabledScope(Application.isPlaying || this.m_AgentTypeID.intValue == -1))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(EditorGUIUtility.labelWidth);
                if (GUILayout.Button("Clear"))
                {
                    foreach (NavMeshSurface s in this.targets)
						this.ClearSurface(s);
                    SceneView.RepaintAll();
                }

                if (GUILayout.Button("Bake"))
                {
                    // Remove first to avoid double registration of the callback
                    EditorApplication.update -= UpdateAsyncBuildOperations;
                    EditorApplication.update += UpdateAsyncBuildOperations;

                    foreach (NavMeshSurface surf in this.targets)
                    {
						AsyncBakeOperation oper = new AsyncBakeOperation();

                        oper.bakeData = InitializeBakeData(surf);
                        oper.bakeOperation = surf.UpdateNavMesh(oper.bakeData);
                        oper.surface = surf;

                        s_BakeOperations.Add(oper);
                    }
                }

                GUILayout.EndHorizontal();
            }

            // Show progress for the selected targets
            for (int i = s_BakeOperations.Count - 1; i >= 0; --i)
            {
                if (!this.targets.Contains(s_BakeOperations[i].surface))
                    continue;

				AsyncOperation oper = s_BakeOperations[i].bakeOperation;
                if (oper == null)
                    continue;

				float p = oper.progress;
                if (oper.isDone)
                {
                    SceneView.RepaintAll();
                    continue;
                }

                GUILayout.BeginHorizontal();

                if (GUILayout.Button("Cancel", EditorStyles.miniButton))
                {
					NavMeshData bakeData = s_BakeOperations[i].bakeData;
                    UnityEngine.AI.NavMeshBuilder.Cancel(bakeData);
                    s_BakeOperations.RemoveAt(i);
                }

                EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), p, "Baking: " + (int)(100 * p) + "%");
                if (p <= 1)
					this.Repaint();

                GUILayout.EndHorizontal();
            }
        }

        static NavMeshData InitializeBakeData(NavMeshSurface surface)
        {
			List<NavMeshBuildSource> emptySources = new List<NavMeshBuildSource>();
			Bounds emptyBounds = new Bounds();
            return UnityEngine.AI.NavMeshBuilder.BuildNavMeshData(surface.GetBuildSettings(), emptySources, emptyBounds
                , surface.transform.position, surface.transform.rotation);
        }

        static void UpdateAsyncBuildOperations()
        {
            foreach (AsyncBakeOperation oper in s_BakeOperations)
            {
                if (oper.surface == null || oper.bakeOperation == null)
                    continue;

                if (oper.bakeOperation.isDone)
                {
					NavMeshSurface surface = oper.surface;
					NavMeshData delete = GetNavMeshAssetToDelete(surface);
                    if (delete != null)
                        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(delete));

                    surface.RemoveData();
                    surface.navMeshData = oper.bakeData;
                    if (surface.isActiveAndEnabled)
                        surface.AddData();
                    CreateNavMeshAsset(surface);
                    EditorSceneManager.MarkSceneDirty(surface.gameObject.scene);
                }
            }
            s_BakeOperations.RemoveAll(o => o.bakeOperation == null || o.bakeOperation.isDone);
            if (s_BakeOperations.Count == 0)
                EditorApplication.update -= UpdateAsyncBuildOperations;
        }

        [DrawGizmo(GizmoType.Selected | GizmoType.Active | GizmoType.Pickable)]
        static void RenderBoxGizmoSelected(NavMeshSurface navSurface, GizmoType gizmoType)
        {
            RenderBoxGizmo(navSurface, gizmoType, true);
        }

        [DrawGizmo(GizmoType.NotInSelectionHierarchy | GizmoType.Pickable)]
        static void RenderBoxGizmoNotSelected(NavMeshSurface navSurface, GizmoType gizmoType)
        {
            if (NavMeshVisualizationSettings.showNavigation > 0)
                RenderBoxGizmo(navSurface, gizmoType, false);
            else
                Gizmos.DrawIcon(navSurface.transform.position, "NavMeshSurface Icon", true);
        }

        static void RenderBoxGizmo(NavMeshSurface navSurface, GizmoType gizmoType, bool selected)
        {
			Color color = selected ? s_HandleColorSelected : s_HandleColor;
            if (!navSurface.enabled)
                color = s_HandleColorDisabled;

			Color oldColor = Gizmos.color;
			Matrix4x4 oldMatrix = Gizmos.matrix;

			// Use the unscaled matrix for the NavMeshSurface
			Matrix4x4 localToWorld = Matrix4x4.TRS(navSurface.transform.position, navSurface.transform.rotation, Vector3.one);
            Gizmos.matrix = localToWorld;

            if (navSurface.collectObjects == CollectObjects.Volume)
            {
                Gizmos.color = color;
                Gizmos.DrawWireCube(navSurface.center, navSurface.size);

                if (selected && navSurface.enabled)
                {
					Color colorTrans = new Color(color.r * 0.75f, color.g * 0.75f, color.b * 0.75f, color.a * 0.15f);
                    Gizmos.color = colorTrans;
                    Gizmos.DrawCube(navSurface.center, navSurface.size);
                }
            }
            else
            {
                if (navSurface.navMeshData != null)
                {
					Bounds bounds = navSurface.navMeshData.sourceBounds;
                    Gizmos.color = Color.grey;
                    Gizmos.DrawWireCube(bounds.center, bounds.size);
                }
            }

            Gizmos.matrix = oldMatrix;
            Gizmos.color = oldColor;

            Gizmos.DrawIcon(navSurface.transform.position, "NavMeshSurface Icon", true);
        }

        void OnSceneGUI()
        {
            if (!this.editingCollider)
                return;

			NavMeshSurface navSurface = (NavMeshSurface)this.target;
			Color color = navSurface.enabled ? s_HandleColor : s_HandleColorDisabled;
			Matrix4x4 localToWorld = Matrix4x4.TRS(navSurface.transform.position, navSurface.transform.rotation, Vector3.one);
            using (new Handles.DrawingScope(color, localToWorld))
            {
				this.m_BoundsHandle.center = navSurface.center;
				this.m_BoundsHandle.size = navSurface.size;

                EditorGUI.BeginChangeCheck();
				this.m_BoundsHandle.DrawHandle();
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(navSurface, "Modified NavMesh Surface");
                    Vector3 center = this.m_BoundsHandle.center;
                    Vector3 size = this.m_BoundsHandle.size;
                    navSurface.center = center;
                    navSurface.size = size;
                    EditorUtility.SetDirty(this.target);
                }
            }
        }

        [MenuItem("GameObject/AI/NavMesh Surface", false, 2000)]
        public static void CreateNavMeshSurface(MenuCommand menuCommand)
        {
			GameObject parent = menuCommand.context as GameObject;
			GameObject go = NavMeshComponentsGUIUtility.CreateAndSelectGameObject("NavMesh Surface", parent);
            go.AddComponent<NavMeshSurface>();
			SceneView view = SceneView.lastActiveSceneView;
            if (view != null)
                view.MoveToView(go.transform);
        }
    }
}
