#if UNITY_EDITOR
using System;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;

namespace FizzSDK.Utils
{
    public static class UnityEventUtils
    {
        public static bool ContainsReferencesTo(this UnityEventBase unityEventBase, GameObject subject)
        {
            var newListeners = new UnityEvent();
            
            for (var i = 0; i < unityEventBase.GetPersistentEventCount(); i++)
            {
                var target = unityEventBase.GetPersistentTarget(i);
                if (target is not Component targetComponent) continue;
                
                if (targetComponent.gameObject == subject)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
#endif