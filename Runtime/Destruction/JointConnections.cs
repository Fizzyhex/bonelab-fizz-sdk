#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FizzSDK.Utils;

namespace FizzSDK.Destruction
{
    [ExecuteInEditMode]
    public class JointConnections : MonoBehaviour
    {
        private string _lastUpdateId;
        [Tooltip("A list of joints that are connected to this object.\n\nThis is automatically filled out by the CreateJointsByProximity script and is required for other ingredients.")]
        public List<Joint> joints = new();

        public string GetUpdateId()
        {
            return _lastUpdateId;
        }

        public void SetUpdateId(string guid)
        {
            _lastUpdateId = guid;
        }

        private void Awake()
        {
            GizmoUtils.SetGizmoIconEnabled(typeof(JointConnections), false);
        }
    }
}
#endif