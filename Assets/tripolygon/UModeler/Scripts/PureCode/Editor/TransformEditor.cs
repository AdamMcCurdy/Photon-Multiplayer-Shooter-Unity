//// Copyright 2018-2021 tripolygon Inc. All Rights Reserved.

//using UnityEngine;
//using UnityEditor;
//using tripolygon.UModeler;

//namespace TPUModelerEditor
//{
//#if UNITY_2018_1_OR_NEWER
//    [CanEditMultipleObjects, CustomEditor(typeof(Transform))]
//    public class TransformEditor : Editor
//    {
//        private const float FIELD_WIDTH = 212.0f;
//        private const bool WIDE_MODE = true;

//        private const float POSITION_MAX = 100000.0f;

//        private static GUIContent positionGUIContent = new GUIContent(LocalString("Position")
//                                                                     , LocalString("The local position of this Game Object relative to the parent."));
//        private static GUIContent rotationGUIContent = new GUIContent(LocalString("Rotation")
//                                                                     , LocalString("The local rotation of this Game Object relative to the parent."));
//        private static GUIContent scaleGUIContent = new GUIContent(LocalString("Scale")
//                                                                     , LocalString("The local scaling of this Game Object relative to the parent."));

//        private static string positionWarningText = LocalString("Due to floating-point precision limitations, it is recommended to bring the world coordinates of the GameObject within a smaller range.");

//        private SerializedProperty positionProperty;
//        private SerializedProperty rotationProperty;
//        private SerializedProperty rotationHintProperty;
//        private SerializedProperty scaleProperty;

//        private static string LocalString(string text)
//        {
//#if true
//            return text;
//#else
//            return LocalizationDatabase.GetLocalizedString(text);
//#endif
//        }

//        public void OnEnable()
//        {
//            this.positionProperty = this.serializedObject.FindProperty("m_LocalPosition");
//            this.rotationProperty = this.serializedObject.FindProperty("m_LocalRotation");
//            this.rotationHintProperty = this.serializedObject.FindProperty("m_LocalEulerAnglesHint");
//            this.scaleProperty = this.serializedObject.FindProperty("m_LocalScale");

//            CheckMesh((Transform)target);
//            CheckPreviousVersionPrefab((Transform)target);
//        }

//        /// <summary>
//        /// 유모델러가 아닌 오브젝트를 복사 했을 경우 새 Mesh를 할당하기 위해서 돌리는 함수입니다.
//        /// </summary>
//        /// <param name="transform"></param>
//        /// <param name="root"></param>
//        private void CheckMesh(Transform transform)
//        {
//            if (EditorUtil.IsPrefabOnProject(transform.gameObject))
//            {
//                return;
//            }

//            UModeler modeler = transform.gameObject.GetComponent<UModeler>();
//            if (modeler != null)
//            {
//                UModelerEditor.UpdateAndRefreshMesh(modeler);
//            }

//            for (int i = 0; i < transform.childCount; ++i)
//            {
//                Transform childTM = transform.GetChild(i);
//                CheckMesh(childTM);
//            }
//        }
//        /// <summary>
//        /// 이전 버전 프리팹인 경우 
//        /// </summary>
//        /// <param name="transform"></param>
//        private void CheckPreviousVersionPrefab(Transform transform)
//        {
//            if (EditorUtil.IsPartOfPrefabInstance(transform.gameObject))
//            {
//                UModeler[] modelers = transform.GetComponentsInChildren<UModeler>();

//                if (modelers != null)
//                {
//                    foreach (var model in modelers)
//                    {
//                        if (model.mainRenderableMesh == null)
//                        {
//                            UModelerEditor.CheckPrefabAssetRenderableMesh(transform);
//                            return;
//                        }
//                    }
//                }
//            }
//        }

//        public override void OnInspectorGUI()
//        {
//            EditorGUIUtility.wideMode = TransformEditor.WIDE_MODE;
//            EditorGUIUtility.labelWidth = 70;

//            this.serializedObject.Update();

//            EditorGUILayout.PropertyField(this.positionProperty, positionGUIContent);
//            this.RotationPropertyField(rotationGUIContent);
//            EditorGUILayout.PropertyField(this.scaleProperty, scaleGUIContent);

//            if (!ValidatePosition(((Transform)this.target).position))
//            {
//                EditorGUILayout.HelpBox(positionWarningText, MessageType.Warning);
//            }

//            this.serializedObject.ApplyModifiedProperties();
//        }

//        private bool ValidatePosition(Vector3 position)
//        {
//            if (Mathf.Abs(position.x) > TransformEditor.POSITION_MAX) return false;
//            if (Mathf.Abs(position.y) > TransformEditor.POSITION_MAX) return false;
//            if (Mathf.Abs(position.z) > TransformEditor.POSITION_MAX) return false;
//            return true;
//        }

//        private void RotationPropertyField(GUIContent content)
//        {
//            EditorGUI.BeginChangeCheck();
//            Quaternion hintQuaternion = Quaternion.identity;

//            hintQuaternion.eulerAngles = rotationHintProperty.vector3Value;

//            if (hintQuaternion != rotationProperty.quaternionValue)
//            {
//                rotationHintProperty.vector3Value = rotationProperty.quaternionValue.eulerAngles;
//            }
//            EditorGUILayout.PropertyField(this.rotationHintProperty, rotationGUIContent);
//            if (EditorGUI.EndChangeCheck())
//            {
//                Undo.RecordObjects(this.targets, "Rotation Changed");
//                Vector3 eulerAngles = this.rotationHintProperty.vector3Value;

//                for (int i = 0; i < this.targets.Length; ++i)
//                {
//                    Transform t = (Transform)this.targets[i];
//                    t.localEulerAngles = eulerAngles;
//                    rotationProperty.quaternionValue = t.localRotation;

//                    if (this.targets[i] == this.target)
//                    {
//                        continue;
//                    }

//                    var serializedTargetObject = new SerializedObject(this.targets[i]);
//                    var targetLocalRotationProperty = serializedTargetObject.FindProperty("m_LocalEulerAnglesHint");
//                    targetLocalRotationProperty.vector3Value = eulerAngles;
//                    serializedTargetObject.ApplyModifiedProperties();
//                    serializedTargetObject.SetIsDifferentCacheDirty();
//                }

//                rotationHintProperty.serializedObject.SetIsDifferentCacheDirty();
//            }
//        }
//    }
//#endif
//}