#if UNITY_EDITOR
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using FizzSDK.Utils;
using System.Linq;
using UnityEngine.Serialization;

namespace FizzSDK.Destruction
{
    [AddComponentMenu("FizzSDK/Destruction Toolkit/Create Joints By Proximity")]
    public class CreateJointsByProximity : DestructionIngredient
    {
        [Header("Configurable Joint Settings")]
        [Tooltip("An optional template ConfigurableJoint to copy the fields from.")]
        public ConfigurableJoint templateJoint; 
        [SerializeField] private float breakForce = Mathf.Infinity;
        [SerializeField] private float breakTorque = Mathf.Infinity;
        [SerializeField] private bool enableCollision = true;

        private readonly Collider[] _overlapResults = new Collider[15];

        private const float SearchPadding = 0.05f;

        public override void UseIngredient(GameObject targetGameObject) => MakeJoints(targetGameObject);

        // Get the bounds of a GameObject including its children
        private Bounds GetBounds(GameObject obj)
        {
            var renderers = obj.GetComponentsInChildren<Renderer>();
            Bounds bounds = new();

            foreach (var rendererMatch in renderers)
            {
                bounds.Encapsulate(rendererMatch.bounds);
            }

            return bounds;
        }

        private void ConfigureJoint(ConfigurableJoint joint)
        {
            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;
            joint.angularXMotion = ConfigurableJointMotion.Locked;
            joint.angularYMotion = ConfigurableJointMotion.Locked;
            joint.angularZMotion = ConfigurableJointMotion.Locked;

            joint.breakForce = breakForce;
            joint.breakTorque = breakTorque;
            
            // enable collision between connected bodies
            joint.enableCollision = enableCollision;

            if (templateJoint != null)
            {
                EditorUtility.CopySerialized(templateJoint, joint);
            }
        }

        private List<GameObject> GetRootGameObjects(List<Rigidbody> rbs)
        {
            List<GameObject> rootGameObjects = new();

            foreach (var rb in rbs)
            {
                var rootGameObject = rb.gameObject.transform.root.gameObject;

                if (!rootGameObjects.Contains(rootGameObject))
                {
                    rootGameObjects.Add(rootGameObject);
                }
            }

            return rootGameObjects;
        }

        public void MakeJoints(GameObject targetGameObject)
        {
            var actionGuid = System.Guid.NewGuid().ToString();
            var allRigidbodies = targetGameObject.GetComponentsInChildren<Rigidbody>().ToList();
            var rootGameObjects = GetRootGameObjects(allRigidbodies);

            Debug.Log($"Rigidbody count: {allRigidbodies.Count}");

            foreach (var rb in allRigidbodies)
            {
                var rbCollider = rb.gameObject.GetComponent<Collider>();

                if (!rbCollider)
                {
                    continue;
                }
                
                var searchRadius = (rbCollider.bounds.size + (Vector3.one * SearchPadding)) / 2;
                var size = Physics.OverlapBoxNonAlloc(rb.worldCenterOfMass, searchRadius, _overlapResults, rb.rotation);

                for (var i = 0; i < size; i++)
                {
                    var hitCollider = _overlapResults[i];
                    
                    if (hitCollider.gameObject == rb.gameObject)
                    {
                        continue;
                    }

                    if (!hitCollider.gameObject.TryGetComponent<Rigidbody>(out var hitRigidbody))
                    {
                        continue;
                    }
                    
                    if (allRigidbodies.All(allowedRb => hitCollider.gameObject != allowedRb.gameObject))
                    {
                        continue;
                    }

                    var joint = rb.gameObject.AddComponent<ConfigurableJoint>();

                    var jointConnections = hitRigidbody.GetComponent<JointConnections>();

                    if (jointConnections)
                    {
                        if (jointConnections.GetUpdateId() != actionGuid)
                        {
                            jointConnections.joints.Clear();
                            jointConnections.SetUpdateId(actionGuid);
                        }
                    }
                    else
                    {
                        jointConnections = hitRigidbody.gameObject.AddComponent<JointConnections>();
                    }

                    jointConnections.joints.Add(joint);

                    Physics.IgnoreCollision(rbCollider, hitCollider);

                    EditorUtility.SetDirty(joint);

                    ConfigureJoint(joint);
                    
                    joint.anchor = rb.transform.InverseTransformPoint(hitCollider.ClosestPoint(rb.worldCenterOfMass));
                    joint.axis = Vector3.zero;
                    joint.connectedBody = hitRigidbody;
                }
            }
        }
    }
}
#endif