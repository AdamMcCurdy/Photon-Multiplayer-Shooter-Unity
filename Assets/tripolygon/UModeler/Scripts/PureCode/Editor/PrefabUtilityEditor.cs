// Copyright 2018-2022 tripolygon Inc. All Rights Reserved.
using UnityEngine;
using System;
using UnityEditor;
using tripolygon.UModeler;
#if UNITY_2021_2_OR_NEWER
using UnityEditor.SceneManagement;
#else
using UnityEditor.Experimental.SceneManagement;
#endif

namespace TPUModelerEditor
{
    public class PrefabUtilityEditor
    {
        private static UModelerPrefabStage ConvertPrefabStage(PrefabStage obj)
        {
            var umodelerPrefabStage = new UModelerPrefabStage();

            umodelerPrefabStage.prefabContentsRoot = obj.prefabContentsRoot;
#if UNITY_2020_1_OR_NEWER
            //umodelerPrefabStage.openedFromInstanceRoot = obj.openedFromInstanceRoot;
            //umodelerPrefabStage.openedFromInstanceObject = obj.openedFromInstanceObject;
            umodelerPrefabStage.assetPath = obj.assetPath;
            umodelerPrefabStage.sceneDirty = obj.scene.isDirty;
#else
            umodelerPrefabStage.assetPath = obj.prefabAssetPath;
            umodelerPrefabStage.sceneDirty = obj.scene.isDirty;

#endif

            return umodelerPrefabStage;
        }

        public static void PrefabStage_prefabStageClosing(PrefabStage obj)
        {
            UModelerPrefabStage.PrefabStageClosing(ConvertPrefabStage(obj));
        }
        public static void PrefabStage_prefabStageOpened(PrefabStage obj)
        {
            UModelerPrefabStage.PrefabStageOpened(ConvertPrefabStage(obj));
        }
        public static void PrefabStage_prefabSaved(GameObject obj)
        {
            UModelerPrefabStage.PrefabSaved(obj);
        }

        public static void PrefabStage_prefabStageDirtied(PrefabStage obj)
        {
            UModelerPrefabStage.PrefabStageDirtied(ConvertPrefabStage(obj));
        }

        internal static void Initialize()
        {
            PrefabStage.prefabStageClosing += PrefabUtilityEditor.PrefabStage_prefabStageClosing;
            PrefabStage.prefabSaving += PrefabUtilityEditor.PrefabStage_prefabSaving;
            PrefabStage.prefabStageDirtied += PrefabUtilityEditor.PrefabStage_prefabStageDirtied;
            PrefabStage.prefabStageOpened += PrefabUtilityEditor.PrefabStage_prefabStageOpened;
            PrefabStage.prefabSaved += PrefabUtilityEditor.PrefabStage_prefabSaved;

            UModelerPrefabStage.applyObject += ApplyObject;
            UModelerPrefabStage.revertPrefabInstance += RevertPrefabInstance;
            UModelerPrefabStage.revertObject += RevertObject;
            UModelerPrefabStage.unpackPrefabInstance += UnpackPrefabInstance;
            UModelerPrefabStage.openPrefabInstance += OpenPrefabInstance;
            UModelerPrefabStage.openPrefabAsset += OpenPrefabAsset;
            UModelerPrefabStage.getPrefabStage += GetPrefabStage;
            UModelerPrefabStage.getCurrentPrefabStage += GetCurrentPrefabStage;
            UModelerPrefabStage.prefabAssetUpdated += PrefabAssetUpdated;

            UMAnalytics.Initialize();
        }

        public static void PrefabStage_prefabSaving(GameObject obj)
        {
            UModelerPrefabStage.PrefabSaving(obj);
        }

        public static UModelerPrefabStage GetCurrentPrefabStage()
        {
            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage != null)
            {
                return ConvertPrefabStage(stage);
            }

            return null;
        }

        public static UModelerPrefabStage GetPrefabStage(GameObject gameObject)
        {
            var stage = PrefabStageUtility.GetPrefabStage(gameObject);
            if (stage != null)
            {
                return ConvertPrefabStage(stage);
            }

            return null;
        }

        public static void RevertObject(UnityEngine.Object componentOrGameObject)
        {
            PrefabUtility.RevertObjectOverride(componentOrGameObject, InteractionMode.AutomatedAction);
        }
        public static void RevertPrefabInstance(GameObject gameObject)
        {
            PrefabUtility.RevertPrefabInstance(gameObject, InteractionMode.AutomatedAction);
        }
        public static void ApplyObject(GameObject gameObject)
        {
            var path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(GetCorrespondingObjectFromOriginalSource(gameObject));
            PrefabUtility.ApplyObjectOverride(gameObject, path, InteractionMode.AutomatedAction);
        }

        public static void UnpackPrefabInstance(GameObject instanceRoot)
        {
            PrefabUtility.UnpackPrefabInstance(instanceRoot, PrefabUnpackMode.Completely, InteractionMode.UserAction);
        }

        public static T GetCorrespondingObjectFromOriginalSource<T>(T openedFromInstance) where T : UnityEngine.Object
        {
            return PrefabUtility.GetCorrespondingObjectFromOriginalSource(openedFromInstance);
        }

        public static T GetCorrespondingObjectFromSource<T>(T openedFromInstance) where T : UnityEngine.Object
        {
            return PrefabUtility.GetCorrespondingObjectFromSource(openedFromInstance);
        }

        public static void OpenPrefabInstance(GameObject openedFromInstance, bool inContent, bool inOriginal)
        {
#if UNITY_2021_2_OR_NEWER
            PrefabStage.Mode prefabStageMode = inContent ? PrefabStage.Mode.InContext : PrefabStage.Mode.InIsolation;
#endif

            if (openedFromInstance != null)
            {
                GameObject openObject;
                if (inOriginal)
                {
                    openObject = GetCorrespondingObjectFromOriginalSource(openedFromInstance);
                }
                else
                {
                    openObject = GetCorrespondingObjectFromSource(openedFromInstance);
                }
#if UNITY_2021_2_OR_NEWER
                var prefabAssetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(openObject);
                PrefabStageUtility.OpenPrefab(prefabAssetPath, openedFromInstance, prefabStageMode);
#else
                AssetDatabase.OpenAsset(openObject);
#endif
            }
        }
        public static void OpenPrefabAsset(GameObject openedFromAsset)
        {
            if (openedFromAsset != null)
            {
                string prefabAssetPath;
                prefabAssetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(openedFromAsset);
#if UNITY_2021_2_OR_NEWER
                PrefabStageUtility.OpenPrefab(prefabAssetPath);
#else
                AssetDatabase.OpenAsset(openedFromAsset);
#endif
            }
        }
        public static void PrefabAssetUpdated(GameObject assetGameObject)
        {
            bool updated = UpdateUModelerMeshAsset(assetGameObject);

            if (updated)
            {
#if UNITY_2021_1_OR_NEWER
                PrefabUtility.SavePrefabAsset(assetGameObject);
#endif
            }
        }

        static private bool UpdateUModelerMeshAsset(GameObject assetGameObject)
        {
            bool updated = false;

            if (assetGameObject != null)
            {
                var modelAsset = assetGameObject.GetComponent<UModeler>();
                var newMesh = SystemUtil.SaveMeshPrefabAsset(modelAsset);
                updated |= newMesh;
                if (newMesh)
                {
                    modelAsset.Build(0, updateToGraphicsAPIImmediately: true);
                    //                    modelAsset.mainRenderableMesh = modelAsset.mainRenderableMesh;
                }
            }

            for (int i = 0; i < assetGameObject.transform.childCount; ++i)
            {
                updated |= UpdateUModelerMeshAsset(assetGameObject.transform.GetChild(i).gameObject);
            }

            return updated;
        }
    }
}