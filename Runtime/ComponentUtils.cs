#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FizzSDK.Utils
{
    internal static class ComponentUtils
    {
        public static T AddOrGetComponent<T>(this GameObject gameObject) where T : Component
        {
            return gameObject.TryGetComponent<T>(out var outComponent)
                ? outComponent
                : gameObject.AddComponent<T>();
        }
    }
}
#endif