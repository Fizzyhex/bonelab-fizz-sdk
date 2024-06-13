#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace FizzSDK.Tags
{
    [CustomEditor(typeof(FizzTagSO))]
    public class FizzTagSOEditor : UnityEditor.Editor
    {
        bool showInfo = false;
        
        public override void OnInspectorGUI()
        {
            showInfo = EditorGUILayout.Foldout(showInfo, "Information");

            if (showInfo)
            {
                var style = EditorStyles.label;
                style.wordWrap = true;
                
                GUILayout.Label(
                    "Fizz Tags are used to help ingredients know what to apply their behaviour to.\n\nThey can be used interchangeably with other Marrow SDK data card types (e.g MonoDisc and Bone Tags)",
                    style);
            }
            
            EditorGUILayout.Space();
            
            var fizzTagSO = (FizzTagSO)target;
            
            EditorGUILayout.LabelField("Description", EditorStyles.boldLabel);
            fizzTagSO.Description = EditorGUILayout.TextArea(fizzTagSO.Description, EditorStyles.textField);
        }
    }
}
#endif