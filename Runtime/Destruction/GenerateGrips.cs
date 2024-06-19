#if UNITY_EDITOR
using FizzSDK.Runtime;
using FizzSDK.Tags;
using SLZ.Interaction;
using UnityEngine;
using FizzSDK.Utils;
using SLZ.Marrow.Warehouse;

namespace FizzSDK.Destruction
{
    [AddComponentMenu("FizzSDK/Destruction Toolkit/Generate Grips")]
    public class GenerateGrips : DestructionIngredient
    {
        // :((((
        
        private const BoxGrip.Faces AllFaces = BoxGrip.Faces.NegativeX | BoxGrip.Faces.NegativeY |
                                               BoxGrip.Faces.NegativeZ | BoxGrip.Faces.PositiveX |
                                               BoxGrip.Faces.PositiveY | BoxGrip.Faces.PositiveZ;

        private const BoxGrip.Edges AllEdges = BoxGrip.Edges.NegativeXNegativeY | BoxGrip.Edges.NegativeXNegativeZ |
                                               BoxGrip.Edges.NegativeXPositiveY | BoxGrip.Edges.NegativeXPositiveZ |
                                               BoxGrip.Edges.NegativeYNegativeZ | BoxGrip.Edges.NegativeYPositiveZ |
                                               BoxGrip.Edges.NegativeYPositiveZ | BoxGrip.Edges.PositiveXNegativeY |
                                               BoxGrip.Edges.PositiveXNegativeZ | BoxGrip.Edges.PositiveXPositiveY |
                                               BoxGrip.Edges.PositiveXPositiveZ | BoxGrip.Edges.PositiveYNegativeZ |
                                               BoxGrip.Edges.PositiveYPositiveZ;
        
        private const BoxGrip.Corners AllCorners = BoxGrip.Corners.NegativeXNegativeYNegativeZ |
                                                   BoxGrip.Corners.NegativeXNegativeYPositiveZ |
                                                   BoxGrip.Corners.NegativeXPositiveYNegativeZ |
                                                   BoxGrip.Corners.NegativeXPositiveYPositiveZ |
                                                   BoxGrip.Corners.PositiveXNegativeYNegativeZ |
                                                   BoxGrip.Corners.PositiveXNegativeYPositiveZ |
                                                   BoxGrip.Corners.PositiveXPositiveYNegativeZ |
                                                   BoxGrip.Corners.PositiveXPositiveYPositiveZ;

        private const string ColliderHolderName = "GeneratedGrip";
        
        [Tooltip("(Optional) Only objects with this tag will have grips added. Leave as none to add to everything.")]
        public DataCard tagFilter;

        [Tooltip("If true, inactive objects will have grips added.")]
        public bool includeInactive = false;

        [Header("Grip Settings")]
        public float cornerRadius = 0.1f;
        public float faceRadius = 1;
        public bool canBeFaceGrabbed = true;
        
        private static GameObject GenerateColliderHolder(Transform parent)
        {
            var colliderHolder = new GameObject(ColliderHolderName)
            {
                layer = LayerMask.NameToLayer("Interacable"),
                transform =
                {
                    parent = parent,
                    localPosition = Vector3.zero,
                    localRotation = Quaternion.identity,
                    localScale = Vector3.one
                }
            };

            return colliderHolder;
        }
        
        public override void UseIngredient(GameObject targetGameObject)
        {
            foreach (var refBoxCollider in targetGameObject.GetComponentsInChildren<BoxCollider>(includeInactive))
            {
                if (refBoxCollider.isTrigger) continue;
                if (tagFilter && !refBoxCollider.gameObject.HasFizzTag(tagFilter)) continue;
                
                var colliderGameObject = refBoxCollider.gameObject;
                var colliderHolderTransform = colliderGameObject.transform.Find(ColliderHolderName);
                var colliderHolder = colliderHolderTransform
                    ? colliderHolderTransform.gameObject
                    : GenerateColliderHolder(colliderGameObject.transform);

                var gripCollider = colliderHolder.AddOrGetComponent<BoxCollider>();
                gripCollider.size = refBoxCollider.size;
                gripCollider.center = refBoxCollider.center;
                gripCollider.isTrigger = true;
                
                var boxGrip = colliderHolder.AddOrGetComponent<BoxGrip>();

                boxGrip.targetTransform = colliderHolder.transform;

                boxGrip.primaryMovementAxis = new Vector3(0, 0, 1);
                boxGrip.secondaryMovementAxis = Vector3.up;

                boxGrip.gripOptions = InteractionOptions.MultipleHands;

                boxGrip.rotationLimit = 180;
                boxGrip.rotationPriorityBuffer = 20;
                
                boxGrip.isThrowable = true;
                boxGrip.edgePadding = cornerRadius;
                boxGrip.handleAmplifyCurve = AnimationCurve.Linear(0, 1, 1, 1);
                
                boxGrip.sandwichHandPose = HandPoseProvider.GetHandPose("BoxSandwichGrip");
                boxGrip.canBeSandwichedGrabbed = true;
                boxGrip.edgeHandPose = HandPoseProvider.GetHandPose("BoxEdgeGrip");
                boxGrip.edgeHandPoseRadius = cornerRadius;
                boxGrip.canBeEdgeGrabbed = true;
                boxGrip.cornerHandPose = HandPoseProvider.GetHandPose("BoxCornerGrip");
                boxGrip.cornerHandPoseRadius = cornerRadius;
                boxGrip.canBeCornerGrabbed = true;
                boxGrip.faceHandPose = HandPoseProvider.GetHandPose("BoxFaceGrip");
                boxGrip.faceHandPoseRadius = faceRadius;
                boxGrip.canBeFaceGrabbed = canBeFaceGrabbed;

                boxGrip.enabledFaces = AllFaces;
                boxGrip.enabledEdges = AllEdges;
                boxGrip.enabledCorners = AllCorners;

                boxGrip.minBreakForce = 1000;
                boxGrip.maxBreakForce = 2000;
                boxGrip.defaultGripDistance = float.PositiveInfinity;
                
                // did they make these private out of spite of me??
                // patch 4 update: still private... :(
                
                var forceGrabFaceField = boxGrip.GetType().GetField("forceGrabFace",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (forceGrabFaceField != null)
                {
                    forceGrabFaceField.SetValue(boxGrip, BoxGrip.Faces.NegativeX);
                }
                
                var forceGrabTopField = boxGrip.GetType().GetField("forceGrabTop",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (forceGrabTopField != null)
                {
                    forceGrabTopField.SetValue(boxGrip, BoxGrip.Faces.PositiveY);
                }
                
                // BUT THIS ONE IS PUBLIC EVEN THO IT'S CASED LIKE A PRIVATE VARIABLE LOL
                boxGrip._boxCollider = gripCollider;
            }
        }
    }
}
#endif