using System;
using System.Reflection;
using UnityEditor;

namespace FizzSDK.Utils
{
    // Src: https://discussions.unity.com/t/how-to-hide-gizmos-by-script/124767/4
    public static class GizmoUtils
    {
        static MethodInfo setIconEnabled;
        static MethodInfo SetIconEnabled => setIconEnabled = setIconEnabled ??
                                                             Assembly.GetAssembly( typeof(Editor) )
                                                                 ?.GetType( "UnityEditor.AnnotationUtility" )
                                                                 ?.GetMethod( "SetIconEnabled", BindingFlags.Static | BindingFlags.NonPublic );
 
        public static void SetGizmoIconEnabled( Type type, bool on ) {
            if( SetIconEnabled == null ) return;
            const int monoBehaviorClassID = 114; // https://docs.unity3d.com/Manual/ClassIDReference.html
            SetIconEnabled.Invoke( null, new object[] { monoBehaviorClassID, type.Name, on ? 1 : 0 } );
        }
    }
}