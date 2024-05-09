#if UNITY_EDITOR
using FizzSDK.Destruction;
using UnityEngine;

namespace FizzSDK.Tools
{
    [AddComponentMenu("FizzSDK/Tools/Joint Connections Nuke")]
    public class JointConnectionsNuke : MonoBehaviour
    {
        [SerializeField] private GameObject container;
        
        [ContextMenu("NUKE JOINT CONNECTIONS!!!")]
        public void NukeJointConnections()
        {
            foreach (var jointConnections in container.GetComponentsInChildren<JointConnections>())
            {
                DestroyImmediate(jointConnections);
            }
        }
    }
}
#endif