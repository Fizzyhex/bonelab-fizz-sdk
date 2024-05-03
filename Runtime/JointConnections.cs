#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FizzSDK
{
    public class JointConnections : MonoBehaviour
    {
        private string lastUpdateId;
        [Tooltip("A list of joints that are connected to this object - this is automatically filled out by MakeJoints")]
        public List<Joint> joints = new();

        public string GetUpdateId()
        {
            return lastUpdateId;
        }

        public void SetUpdateId(string guid)
        {
            lastUpdateId = guid;
        }
    }
}
#endif