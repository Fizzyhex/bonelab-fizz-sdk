#if UNITY_EDITOR
using UnityEngine;

namespace FizzSDK.Tools
{
    [AddComponentMenu("FizzSDK/Tools/Joint Nuke")]
    public class JointNuke : MonoBehaviour
    {
        [SerializeField] private GameObject container;
        
        [ContextMenu("NUKE JOINTS!!!")]
        public void NukeJoints()
        {
            foreach (var joint in container.GetComponentsInChildren<Joint>())
            {
                DestroyImmediate(joint);
            }
        }
    }
}
#endif