#if UNITY_EDITOR
using System.Linq;
using UnityEngine;

namespace FizzSDK
{
    [AddComponentMenu("FizzSDK/Component Counters/ConfigurableJoint Counter")]
    public class ConfigurableJointCounter : ShittyComponentCounter<ConfigurableJoint>
    {
        private void Awake()
        {
            componentName = "ConfigurableJoint";
        }
        
        [ContextMenu("Count!")]
        public void CountButton() => Count();
    }
}
#endif