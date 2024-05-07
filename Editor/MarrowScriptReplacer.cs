using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;

namespace FizzSDK.Editor
{
    // Adds a button to Tools that replaces the Prop_Health script
    public class MarrowScriptReplacer : EditorWindow
    {
        [MenuItem("Tools/FizzSDK/Patch Dummy Scripts")]
        public static void ShowWindow()
        {
            ReplaceScripts();
        }

        private static void ReplaceScripts()
        {
            var replacementScriptSources = AssetDatabase.FindAssets("t:ReplacementScriptSource")
                .Select(AssetDatabase.GUIDToAssetPath);
            
            foreach (string source in replacementScriptSources)
            {
                var replacementScriptSource = AssetDatabase.LoadAssetAtPath<ReplacementScriptSource>(source);
                var scriptName = replacementScriptSource.name;
                var replacementScript = replacementScriptSource.source;
                
                var files = Directory.GetFiles(Application.dataPath, $"{scriptName}.cs", SearchOption.AllDirectories);
                
                foreach (string file in files)
                {
                    var scriptPath = file.Replace(Application.dataPath, "Assets");
                    var script = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptPath);

                    File.WriteAllText(file, replacementScript);
                    Debug.Log($"Replaced {scriptName} @ {scriptPath}");
                }
            }
        }
    }
}