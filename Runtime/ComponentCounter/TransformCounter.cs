#if UNITY_EDITOR
using System.Linq;
using UnityEngine;

namespace FizzSDK
{
    [AddComponentMenu("FizzSDK/Component Counters/Transform Counter")]
    public class TransformCounter : ShittyComponentCounter<Transform>
    {
        private void Awake()
        {
            componentName = "Transform";
        }
        
        [ContextMenu("Count!")]
        public void CountButton() => Count();
    }
}
#endif