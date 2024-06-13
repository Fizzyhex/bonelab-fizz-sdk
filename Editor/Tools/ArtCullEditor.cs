#if UNITY_EDITOR
using System.Linq;
using SLZ.Bonelab;
using SLZ.Marrow.Interaction;
using UnityEngine;
using UnityEditor;

namespace FizzSDK.Editor.Tools
{
    [CustomEditor(typeof(ArtCull))]
    public class ArtCullEditor : UnityEditor.Editor
    {
        private bool _showTools = false;
        private GameObject _rendererContainer;
        
        public override void OnInspectorGUI()
        {
            var artCullScript = (ArtCull)target;
            var renderersField = artCullScript.GetType().GetField("_renderers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (renderersField != null)
            {
                var currentRenderers = (Renderer[])renderersField.GetValue(artCullScript);
                
                if (currentRenderers == null)
                {
                    return;
                }
                
                var hasNullRefs = currentRenderers.Any(renderer => !renderer);
                
                if (hasNullRefs)
                {
                    EditorGUILayout.HelpBox(
                        "[FizzSDK] There's missing renderers in your renderers field! This may break the script. You should remove them.",
                        MessageType.Warning);
                }
            }
            
            DrawDefaultInspector();

            _showTools = EditorGUILayout.Foldout(_showTools, "Fizz SDK");
            
            if (!_showTools) return;

            _rendererContainer = (GameObject)EditorGUILayout.ObjectField("Renderer Container", _rendererContainer, typeof(GameObject), true);
            
            if (!GUILayout.Button("Populate Renderers")) return;
            
            EditorGUILayout.EndHorizontal();
            
            var renderers = _rendererContainer.GetComponentsInChildren<Renderer>();
            
            if (renderersField == null) return;
            renderersField.SetValue(artCullScript, renderers);
            
            EditorUtility.SetDirty(artCullScript);
        }
        
    }
}
#endif