#if UNITY_EDITOR
using FizzSDK.Tags;
using SLZ.Marrow.Warehouse;
using UnityEditor;
using UnityEngine;

namespace FizzSDK.Destruction
{
    [AddComponentMenu("FizzSDK/Destruction Toolkit/Joint All To Object")]
    public class JointAllToObject : DestructionIngredient
    {
        public string jointTargetName = "";
        [Tooltip("An optional template ConfigurableJoint to copy the fields from.")]
        public ConfigurableJoint templateJoint;
        public DataCard tagFilter;
        
        public override void UseIngredient(GameObject targetGameObject)
        {
            var jointTarget = targetGameObject.transform.Find(jointTargetName).GetComponent<Rigidbody>();
            var allRigidbodies = targetGameObject.GetComponentsInChildren<Rigidbody>();
            
            foreach (var targetRigidbody in allRigidbodies)
            {
                if (tagFilter && !targetRigidbody.gameObject.HasFizzTag(tagFilter))
                {
                    continue;
                }
                
                var joint = targetRigidbody.gameObject.AddComponent<ConfigurableJoint>();

                if (templateJoint)
                {
                    EditorUtility.CopySerialized(templateJoint, joint);
                }
                
                joint.connectedBody = jointTarget;
            }
        }
    }
}
#endif