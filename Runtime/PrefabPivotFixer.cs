#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace FizzSDK
{
    public class PrefabPivotFixer : EditorWindow
    {
        private GameObject prefabTarget;
        const string meshDirectory = "Assets/Resources/FixedMeshes";

        //[MenuItem("Tools/Prefab Pivot Fixer")]
        public static void ShowWindow()
        {
            EditorWindow window = GetWindow(typeof(PrefabPivotFixer));
            window.titleContent = new GUIContent("Prefab Pivot Fixer");
            window.Show();
        }

        void OnGUI()
        {
            GUILayout.Label("Choose a prefab file to modify", EditorStyles.boldLabel);
            prefabTarget = (GameObject)EditorGUILayout.ObjectField("Prefab", prefabTarget, typeof(GameObject), false);

            if (GUILayout.Button("Fix meshes"))
            {
                string prefabPath = AssetDatabase.GetAssetPath(prefabTarget);
                ResetPivot(prefabPath);
            }
        }

        private void CreateMeshDirectoryIfDoesntExist()
        {
            // this sucks. 

            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            if (!AssetDatabase.IsValidFolder(meshDirectory))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "FixedMeshes");
            }
        }

        private void RecenterPivot(GameObject obj)
        {
            MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
            if (meshFilter == null || meshFilter.sharedMesh == null)
            {
                Debug.LogWarning("No mesh filter or mesh found on object: " + obj.name);
                return;
            }

            Vector3 oldScale = obj.transform.localScale;

            if (!obj.TryGetComponent<MeshRenderer>(out var _))
            {
                Debug.LogWarning("No mesh renderer found on object: " + obj.name);
                return;
            }

            if (obj.TryGetComponent<BoxCollider>(out var boxCollider))
            {
                boxCollider.center = Vector3.zero;
            }

            if (obj.TryGetComponent<SphereCollider>(out var sphereCollider))
            {
                sphereCollider.center = Vector3.zero;
            }

            Mesh oldMesh = meshFilter.sharedMesh;
            Mesh modifiedMesh = new()
            {
                vertices = oldMesh.vertices
            };

            Vector3 center = modifiedMesh.bounds.center;
            Vector3 offset = center - obj.transform.localPosition;

            Vector3[] vertices = modifiedMesh.vertices;
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] -= offset;
            }

            modifiedMesh.vertices = vertices;
            modifiedMesh.RecalculateBounds();

            string meshFilePath = $"{meshDirectory}/{meshFilter.transform.name}.asset";
            AssetDatabase.CreateAsset(modifiedMesh, meshFilePath);

            Undo.RecordObject(obj.transform, "Fix transform");
            obj.transform.localPosition = offset;
            obj.transform.localScale = oldScale;

            meshFilter.sharedMesh = modifiedMesh;

            EditorUtility.SetDirty(meshFilter);
            EditorUtility.SetDirty(obj);

            Debug.Log($"applied offset to {obj}: {offset}");
        }

        private void ResetPivot(string prefabPath)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogError("Failed to load prefab file at path: " + prefabPath);
                return;
            }

            GameObject prefabInstance = Instantiate(prefab);
            prefabInstance.name = prefab.name;
            MeshFilter[] meshFilters = prefabInstance.GetComponentsInChildren<MeshFilter>();

            CreateMeshDirectoryIfDoesntExist();

            foreach (MeshFilter meshFilter in meshFilters)
            {
                RecenterPivot(meshFilter.gameObject);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Save changes to the prefab
            PrefabUtility.SaveAsPrefabAsset(prefabInstance, prefabPath);
            DestroyImmediate(prefabInstance);
        }
    }
}

#endif