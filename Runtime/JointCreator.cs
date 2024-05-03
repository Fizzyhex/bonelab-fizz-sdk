#if UNITY_EDITOR
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using FizzSDK.Utils;
using System.Linq;

namespace FizzSDK
{
    public class JointCreator : MonoBehaviour, IJointScript
    {
        [SerializeField] private List<GameObject> rigidbodyContainers;
        [SerializeField] private List<Rigidbody> rigidbodies;
        
        [Header("Options")]
        [Tooltip("If checked, new joints will be added in play mode")]
        [SerializeField]
        private bool createAtRuntime = false;

        [Header("Configurable Joint Settings")]
        [SerializeField]
        private float breakForce = Mathf.Infinity;
        [SerializeField] private float breakTorque = Mathf.Infinity;

        private readonly Collider[] _overlapResults = new Collider[10];

        private const float SearchPadding = 0.1f;

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

        private List<Rigidbody> GetAllRigidbodies()
        {
            var foundRigidbodies = SearchUtils.FindAllInContainers<Rigidbody>(rigidbodyContainers);
            foundRigidbodies.AddRange(rigidbodies);

            return foundRigidbodies;
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

        public void MakeJoints()
        {
            var actionGuid = System.Guid.NewGuid().ToString();
            var allRigidbodies = GetAllRigidbodies();
            var rootGameObjects = allRigidbodies.Select(rb => rb.gameObject.transform.root.gameObject).ToList();

            foreach (var rb in allRigidbodies)
            {
                var rootGameObject = rb.gameObject.transform.root.gameObject;

                if (!rootGameObjects.Contains(rootGameObject))
                {
                    rootGameObjects.Add(rootGameObject);
                }
            }

            Debug.Log("RBS: " + allRigidbodies.Count);

            foreach (var rb in allRigidbodies)
            {
                var rbCollider = rb.gameObject.GetComponent<Collider>();
                var searchRadius = (rbCollider.bounds.size / 2) + new Vector3(SearchPadding, SearchPadding, SearchPadding);
                Physics.OverlapBoxNonAlloc(rb.worldCenterOfMass, searchRadius, _overlapResults);

                foreach (var hitCollider in _overlapResults)
                {
                    if (hitCollider.gameObject == rb.gameObject)
                    {
                        continue;
                    }

                    if (!hitCollider.gameObject.TryGetComponent<Rigidbody>(out var hitRigidbody))
                    {
                        continue;
                    }

                    var joint = rb.gameObject.AddComponent<ConfigurableJoint>();

                    joint.anchor = rb.transform.InverseTransformPoint(hitCollider.ClosestPoint(rb.worldCenterOfMass));
                    joint.axis = Vector3.zero;
                    joint.connectedBody = hitRigidbody;

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

                    Debug.Log($"Created joint between {rb.gameObject} & {hitCollider.gameObject}");
                }
            }
        }

        private void Awake()
        {
            if (createAtRuntime)
            {
                MakeJoints();
            }
        }
    }
}
#endif