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
        [SerializeField] List<GameObject> rigidbodyContainers;
        [SerializeField] List<Rigidbody> rigidbodies;

        [Header("Options")]
        [Tooltip("If checked, new joints will be added in play mode")]
        [SerializeField] bool createAtRuntime = false;

        [Header("Configurable Joint Settings")]
        [SerializeField] float breakForce = Mathf.Infinity;
        [SerializeField] float breakTorque = Mathf.Infinity;

        const float searchPadding = 0.1f;

        // Get the bounds of a GameObject including its children
        private Bounds GetBounds(GameObject obj)
        {
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            Bounds bounds = new();

            foreach (Renderer renderer in renderers)
            {
                bounds.Encapsulate(renderer.bounds);
            }

            return bounds;
        }

        List<Rigidbody> GetAllRigidbodies()
        {
            List<Rigidbody> allRbs = SearchUtils.FindAllInContainers<Rigidbody>(rigidbodyContainers);

            foreach (Rigidbody rigidbody in rigidbodies)
            {
                allRbs.Add(rigidbody);
            }

            return allRbs;
        }

        void ConfigureJoint(ConfigurableJoint joint)
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

        List<GameObject> GetRootGameObjects(List<Rigidbody> rbs)
        {
            List<GameObject> rootGameObjects = new();

            foreach (Rigidbody rb in rbs)
            {
                GameObject rootGameObject = rb.gameObject.transform.root.gameObject;

                if (!rootGameObjects.Contains(rootGameObject))
                {
                    rootGameObjects.Add(rootGameObject);
                }
            }

            return rootGameObjects;
        }

        public void MakeJoints()
        {
            string actionGuid = System.Guid.NewGuid().ToString();
            List<Rigidbody> allRbs = GetAllRigidbodies();
            List<GameObject> rootGameObjects = allRbs.Select(rb => rb.gameObject.transform.root.gameObject).ToList();

            foreach (Rigidbody rb in allRbs)
            {
                GameObject rootGameObject = rb.gameObject.transform.root.gameObject;

                if (!rootGameObjects.Contains(rootGameObject))
                {
                    rootGameObjects.Add(rootGameObject);
                }
            }

            Debug.Log("RBS: " + allRbs.Count);

            foreach (Rigidbody rb in allRbs)
            {
                Collider collider = rb.gameObject.GetComponent<Collider>();
                Vector3 searchRadius = (collider.bounds.size / 2) + new Vector3(searchPadding, searchPadding, searchPadding);
                Collider[] hitColliders = Physics.OverlapBox(rb.worldCenterOfMass, searchRadius);

                foreach (Collider hitCollider in hitColliders)
                {
                    if (hitCollider.gameObject == rb.gameObject)
                    {
                        continue;
                    }

                    if (!hitCollider.gameObject.TryGetComponent<Rigidbody>(out var hitRigidbody))
                    {
                        continue;
                    }

                    ConfigurableJoint joint = rb.gameObject.AddComponent<ConfigurableJoint>();

                    joint.anchor = rb.transform.InverseTransformPoint(hitCollider.ClosestPoint(rb.worldCenterOfMass));
                    joint.axis = Vector3.zero;
                    joint.connectedBody = hitRigidbody;

                    JointConnections jointConnections = hitRigidbody.GetComponent<JointConnections>();

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

                    Physics.IgnoreCollision(collider, hitCollider);

                    EditorUtility.SetDirty(joint);

                    ConfigureJoint(joint);

                    Debug.Log($"Created joint between {rb.gameObject} & {hitCollider.gameObject}");
                }
            }
        }

        void Awake()
        {
            if (createAtRuntime)
            {
                MakeJoints();
            }
        }
    }
}
#endif