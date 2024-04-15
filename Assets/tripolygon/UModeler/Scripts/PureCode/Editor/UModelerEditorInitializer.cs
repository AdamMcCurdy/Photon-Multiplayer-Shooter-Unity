// Copyright 2018-2021 tripolygon Inc. All Rights Reserved.

using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using tripolygon.UModeler;
using UnityEditor.SceneManagement;
using System.Reflection;
using System;

namespace TPUModelerEditor
{
    [InitializeOnLoadAttribute]
    public static class UModelerEditorInitializer
    {
        static UModelerEditorInitializer()
        {
            UMContext.Init(new EditorEngine());

            Selection.selectionChanged += HandleOnSelectionChanged;
            EditorUtil.SetSelectedRenderStateCallbackInst += OnSetSelectedRenderStateCallback;
            Builder.modelBuilt += OnMeshBuilt;
            Builder.modelBuilding += OnMeshBuilding;
            UnityEditor.SceneManagement.EditorSceneManager.sceneOpening += OnSceneLoading;
            UnityEditor.SceneManagement.EditorSceneManager.sceneOpened += OnSceneLoaded;
            UnityEditor.SceneManagement.EditorSceneManager.sceneSaving += OnSceneSaving;
            UnityEditor.SceneManagement.EditorSceneManager.sceneSaved += OnSceneSaved;
            //UnityEditor.SceneManagement.EditorSceneManager.sceneOpened += OnSceneLoaded;
            PrefabUtility.prefabInstanceUpdated += PrefabInstanceUpdated;
            EditorUtil.TryGetUModelerComponent += TryGetComponent<UModeler>;
            EditorUtil.TryGetMeshFilterComponent += TryGetComponent<MeshFilter>;
            EditorUtil.GenerateSecondaryUVSet = GenerateSecondaryUVSet;

            PrefabUtilityEditor.Initialize();

#if UNITY_2019_2_OR_NEWER
            Lightmapping.bakeStarted += OnLightmapBake;
#endif

#if UNITY_2019_3_OR_NEWER
            EditorDecl.SettingsBoxHeight = 410;
#else
            EditorDecl.SettingsBoxHeight = 360;
#endif

#if UNITY_2017_2_OR_NEWER
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
#else
            EditorApplication.playmodeStateChanged += HandleOnPlayModeChanged;
#endif

#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui += UModelerEditor.OnScene;
#else
            SceneView.onSceneGUIDelegate += UModelerEditor.OnScene;
#endif
            AssetDatabase.importPackageCompleted += ImportPackageCallback;
            for (int i = 0; i < SceneManager.sceneCount; ++i)
            {
                var scene = SceneManager.GetSceneAt(i);
                OnSceneLoaded(scene, OpenSceneMode.Single);
            }
        }

        private static void ImportPackageCallback(string packageName)
        {
            Debug.Log(packageName);
        }

        private static void OnLightmapBake()
        {
            EditorUtil.RefreshAll();
        }

        private static void CheckSelected()
        {
            if (Selection.activeGameObject != null)
            {
                var target = Selection.activeGameObject.transform;
                CheckMesh(target);
                CheckPreviousVersionPrefab(target);
            }
        }
        /// <summary>
        /// 유모델러가 아닌 오브젝트를 복사 했을 경우 새 Mesh를 할당하기 위해서 돌리는 함수입니다.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="root"></param>
        private static void CheckMesh(Transform transform)
        {
            if (EditorUtil.IsPrefabOnProject(transform.gameObject))
            {
                return;
            }

            UModeler modeler = transform.gameObject.GetComponent<UModeler>();
            if (modeler != null)
            {
                UModelerEditor.UpdateAndRefreshMesh(modeler);
            }

            for (int i = 0; i < transform.childCount; ++i)
            {
                Transform childTM = transform.GetChild(i);
                CheckMesh(childTM);
            }
        }
        /// <summary>
        /// 이전 버전 프리팹인 경우 
        /// </summary>
        /// <param name="transform"></param>
        private static void CheckPreviousVersionPrefab(Transform transform)
        {
            if (EditorUtil.IsPartOfPrefabInstance(transform.gameObject))
            {
                UModeler[] modelers = transform.GetComponentsInChildren<UModeler>();

                if (modelers != null)
                {
                    foreach (var model in modelers)
                    {
                        if (model.mainRenderableMesh == null)
                        {
                            UModelerEditor.CheckPrefabAssetRenderableMesh(transform);
                            return;
                        }
                    }
                }
            }
        }
        public static void HandleOnSelectionChanged()
        {
            UModelerEditor.SendMessage(UModelerMessage.SelectionChanged);

            CheckSelected();

            if (UMContext.activeModeler != null)
            {
                EditorMode.commentaryViewer.AddTitleNoDuplilcation("[" + UMContext.activeModeler.gameObject.name + "] Object has been selected.");
            }
        }

#if UNITY_2017_2_OR_NEWER
        public static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                if (EditorMode.currentTool != null && UMContext.activeModeler != null)
                {
                    EditorMode.currentTool.End();
                    EditorMode.currentTool.Start();
                }

                if (Selection.activeGameObject != null)
                {
                    if (Selection.activeGameObject.GetComponent<UModeler>() != null)
                    {
                        Selection.activeGameObject = null;
                    }
                }

                UModeler.enableDelegate = false;
            }

            if (state == PlayModeStateChange.EnteredEditMode)
            {
                UModeler.enableDelegate = true;

                var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                if (currentScene.isDirty)
                    MenuItems.RefreshAll();
            }

            if (state == PlayModeStateChange.EnteredPlayMode || state == PlayModeStateChange.EnteredEditMode)
            {
                EditorUtil.DisableHasTransformed();
            }
        }
#else
        public static void HandleOnPlayModeChanged()
        {
            bool bExitingEditMode = !EditorApplication.isPlaying &&  EditorApplication.isPlayingOrWillChangePlaymode;
            bool bEnteredEditMode = !EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode;
            bool bEnteredPlayMode =  EditorApplication.isPlaying &&  EditorApplication.isPlayingOrWillChangePlaymode;

            if (bExitingEditMode)
            {
                UModeler.enableDelegate = true;
                if (EditorMode.currentTool != null && UMContext.activeModeler != null)
                {
                    EditorMode.currentTool.End();
                    EditorMode.currentTool.Start();
                }

                if (Selection.activeGameObject != null)
                {
                    if (Selection.activeGameObject.GetComponent<UModeler>() != null)
                    {
                        Selection.activeGameObject = null;
                    }
                }
            }

            if (bEnteredPlayMode)
            {
                UModeler.enableDelegate = false;
            }

            if (bEnteredPlayMode || bEnteredEditMode)
            {
                EditorUtil.RefreshAll(false/*bOutputLog*/);
                EditorUtil.DisableHasTransformed();
            }
        }
#endif

        private static bool TryGetComponent<T>(GameObject go, out T outComponent)
        {
#if UNITY_2019_2_OR_NEWER
            return go.TryGetComponent<T>(out outComponent);
#else
            outComponent = go.GetComponent<T>();
            return outComponent != null;
#endif
        }

        private static void OnSceneLoading(string path, OpenSceneMode mode)
        {
            UModelerEditor.ResetMeshContainer();
        }

        static void OnSceneLoaded(Scene scene, UnityEditor.SceneManagement.OpenSceneMode mode)
        {
            EditorUtil.DisableHasTransformed();
            MenuGUICacheData.Invalidate();
            if (scene != null && scene.isLoaded)
            {
                SystemUtil.CheckAllUModelerPrefabInstance(scene.GetRootGameObjects());
            }
            Selection.activeObject = null;
        }

        static void OnSceneSaving(Scene scene, string path)
        {
            if (UMContext.activeModeler != null && EditorMode.currentTool != null)
            {
                EditorMode.currentTool.End();
                EditorMode.currentTool.Start();
                EditorMode.currentTool.OnSceneSaving();
            }
        }

        static MethodInfo SetSelectedRenderState = typeof(EditorUtility).GetMethod("SetSelectedRenderState");
        private static void OnSetSelectedRenderStateCallback(bool enabled, UModeler modeler)
        {
            if (modeler == null)
            {
                return;
            }

            if (SetSelectedRenderState != null)
            {
                // EditorSelectedRenderState.Highlight : 2
                // EditorSelectedRenderState.Hidden : 0
#if UNITY_2021_1_OR_NEWER
                SetSelectedRenderState.Invoke(null, new object[] { modeler.meshRenderer, enabled ? 2 : 0 });
#else

                SetSelectedRenderState.Invoke(null, new object[] { modeler.meshRenderer, enabled ? 2 : 0 });
#endif
            }
            else
            {
                EditorUtil.EnableWireFrame(modeler.meshRenderer, enabled);
            }
        }

        private static void OnSceneSaved(Scene scene)
        {
            if (UMContext.activeModeler != null && EditorMode.currentTool != null)
            {
                EditorMode.currentTool.OnSceneSaved();
            }
        }

        static void OnMeshBuilt(UModeler modeler, int shelf)
        {
            UModelerEditor.OnChanged();

            if (shelf == 0)
            {
                modeler.editableMesh.IsBuilt = true;

                if (EditorUtil.HasStaticLightmap(modeler))
                {
                    EditorUtil.GenerateUV2(modeler);

                    EditorUtil.SetLightmap(modeler, false);
                    EditorUtil.SetLightmap(modeler, true);
                }
            }
        }

        private static void OnMeshBuilding(UModeler modeler, int shelf)
        {
            if (shelf == 0)
            {
                //UModelerEditor.DisconnectPrefabMeshLink(modeler);
            }
            else if (shelf == 1)
            {
                using (new ShelfHolder(modeler.editableMesh))
                {
                    modeler.editableMesh.shelf = 1;
                    var polygonList = modeler.editableMesh.GetAllPolygons();

                    if (UMContext.activeModeler.autoHotspotLayout)
                    {
                        HotspotLayoutTool.HotspotTexturing(polygonList, true, false);
                        modeler.editableMesh.uvIslandManager.RemoveAllEmpty();
                    }
                }
            }
        }

        public static void PrefabInstanceUpdated(GameObject instance)
        {
            bool updated = UpdateUModelerMeshInstance(instance);

            if (updated)
            {
                var modelAsset = PrefabUtility.GetCorrespondingObjectFromSource(instance);

                //var modelAssetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(instance);
#if UNITY_2021_1_OR_NEWER
                PrefabUtility.SavePrefabAsset(modelAsset);
#endif
                /// Diff가 안생기도록 각 렌더러 메시 등만 리버트. 매터리얼은 바꿀수 있으므로 안됨.
                /// 렌더러 메시와 메시컬라이더 메시만.
                var umodelerComponents = instance.GetComponentsInChildren<UModeler>(includeInactive: true);

                if (umodelerComponents != null)
                {
                    foreach (var umodeler in umodelerComponents)
                    {
                        UModelerPrefabStage.RevertObject(umodeler);
                        var meshFilter = umodeler.gameObject.GetComponent<MeshFilter>();
                        var meshCollider = umodeler.gameObject.GetComponent<MeshCollider>();

                        if (meshFilter != null)
                        {
                            UModelerPrefabStage.RevertObject(meshFilter);
                        }

                        if (meshCollider != null)
                        {
                            UModelerPrefabStage.RevertObject(meshCollider);
                        }
                    }
                }
                //            UModelerPrefabStage.RevertObject(modelerInstance.gameObject);
            }
        }

        static private bool UpdateUModelerMeshInstance(GameObject instance)
        {
            bool updated = false;
            UModeler modelerInstance = instance.GetComponent<UModeler>();
            //Debug.Log($"{instance}");
            //Debug.Log($"IsPartOfPrefabAsset {PrefabUtility.IsPartOfPrefabAsset(instance)} \n IsPartOfAnyPrefab {PrefabUtility.IsPartOfAnyPrefab(instance)} \n IsPartOfPrefabInstance {PrefabUtility.IsPartOfPrefabInstance(instance)}\n" +
            //$"IsPrefabOnProject {EditorUtil.IsPrefabOnProject(instance)} \n IsPrefabOnLoadEditor {EditorUtil.IsPrefabOnLoadEditor(instance)} \n IsPartOfRegularPrefab {PrefabUtility.IsPartOfRegularPrefab(instance)}\n IsPartOfVariantPrefab {PrefabUtility.IsPartOfVariantPrefab(instance)} \n" +
            //$"GetNearestPrefabInstanceRoot {PrefabUtility.GetNearestPrefabInstanceRoot(instance) == instance}");

            if (modelerInstance != null)
            {
                modelerInstance.SetPrefabInstance(true);
                var newMesh = SystemUtil.SaveMeshPrefabInstance(modelerInstance);
                updated |= newMesh;
                //modeler.editableMesh.InvalidateCache();
                if (newMesh)
                {
                    var modelAsset = PrefabUtility.GetCorrespondingObjectFromSource(modelerInstance);
                    modelAsset.Build(0, updateToGraphicsAPIImmediately: true);
                    modelerInstance.mainRenderableMesh = modelAsset.mainRenderableMesh;
                }
            }

            for (int i = 0; i < instance.transform.childCount; ++i)
            {
                updated |= UpdateUModelerMeshInstance(instance.transform.GetChild(i).gameObject);
            }

            return updated;
        }

        static private void GenerateSecondaryUVSet(Mesh src, UnwrapParam settings)
        {
#if UNITY_2022_1_OR_NEWER
            Unwrapping.GenerateSecondaryUVSet(src, settings);
#else
            Unwrapping.GenerateSecondaryUVSet(src, settings);
#endif
        }
    }
}