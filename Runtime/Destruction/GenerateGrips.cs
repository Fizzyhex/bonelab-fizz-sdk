﻿#if UNITY_EDITOR
using FizzSDK.Runtime;
using SLZ.Interaction;
using UnityEngine;
using FizzSDK.Utils;

namespace FizzSDK.Destruction
{
    [AddComponentMenu("FizzSDK/Destruction Toolkit/Generate Grips")]
    public class GenerateGrips : DestructionIngredient
    {
        // :((((
        
        private const BoxGrip.Faces allFaces = BoxGrip.Faces.NegativeX | BoxGrip.Faces.NegativeY |
                                               BoxGrip.Faces.NegativeZ | BoxGrip.Faces.PositiveX |
                                               BoxGrip.Faces.PositiveY | BoxGrip.Faces.PositiveZ;

        private const BoxGrip.Edges allEdges = BoxGrip.Edges.NegativeXNegativeY | BoxGrip.Edges.NegativeXNegativeZ |
                                               BoxGrip.Edges.NegativeXPositiveY | BoxGrip.Edges.NegativeXPositiveZ |
                                               BoxGrip.Edges.NegativeYNegativeZ | BoxGrip.Edges.NegativeYPositiveZ |
                                               BoxGrip.Edges.NegativeYPositiveZ | BoxGrip.Edges.PositiveXNegativeY |
                                               BoxGrip.Edges.PositiveXNegativeZ | BoxGrip.Edges.PositiveXPositiveY |
                                               BoxGrip.Edges.PositiveXPositiveZ | BoxGrip.Edges.PositiveYNegativeZ |
                                               BoxGrip.Edges.PositiveYPositiveZ;
        
        private const BoxGrip.Corners allCorners = BoxGrip.Corners.NegativeXNegativeYNegativeZ |
                                                   BoxGrip.Corners.NegativeXNegativeYPositiveZ |
                                                   BoxGrip.Corners.NegativeXPositiveYNegativeZ |
                                                   BoxGrip.Corners.NegativeXPositiveYPositiveZ |
                                                   BoxGrip.Corners.PositiveXNegativeYNegativeZ |
                                                   BoxGrip.Corners.PositiveXNegativeYPositiveZ |
                                                   BoxGrip.Corners.PositiveXPositiveYNegativeZ |
                                                   BoxGrip.Corners.PositiveXPositiveYPositiveZ;

        private const string ColliderHolderName = "GeneratedGrip";
        
        private GameObject GenerateColliderHolder(Transform parent)
        {
            var colliderHolder = new GameObject(ColliderHolderName)
            {
                layer = LayerMask.NameToLayer("Interactable"),
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
            foreach (var refBoxCollider in targetGameObject.GetComponentsInChildren<BoxCollider>())
            {
                if (refBoxCollider.isTrigger) continue;
                
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

                boxGrip.primaryMovementAxis = new Vector3(0, 0, 1);
                boxGrip.secondaryMovementAxis = Vector3.up;

                boxGrip.gripOptions = InteractionOptions.MultipleHands;

                boxGrip.rotationLimit = 180;
                boxGrip.rotationPriorityBuffer = 20;
                
                boxGrip.isThrowable = true;
                boxGrip.edgePadding = 0.1f;
                boxGrip.handleAmplifyCurve = AnimationCurve.Linear(0, 1, 1, 1);
                
                boxGrip.sandwichHandPose = HandPoseProvider.GetHandPose("BoxSandwichGrip");
                boxGrip.canBeSandwichedGrabbed = true;
                boxGrip.edgeHandPose = HandPoseProvider.GetHandPose("BoxEdgeGrip");
                boxGrip.edgeHandPoseRadius = 0.05f;
                boxGrip.canBeEdgeGrabbed = true;
                boxGrip.cornerHandPose = HandPoseProvider.GetHandPose("BoxCornerGrip");
                boxGrip.cornerHandPoseRadius = 0.05f;
                boxGrip.canBeCornerGrabbed = true;
                boxGrip.faceHandPose = HandPoseProvider.GetHandPose("BoxFaceGrip");
                boxGrip.faceHandPoseRadius = 1;
                boxGrip.canBeFaceGrabbed = true;

                boxGrip.enabledFaces = allFaces;
                boxGrip.enabledEdges = allEdges;
                boxGrip.enabledCorners = allCorners;

                boxGrip.minBreakForce = 1000;
                boxGrip.maxBreakForce = 2000;
                boxGrip.defaultGripDistance = float.PositiveInfinity;
                
                // did they make these private out of spite??
                
                var forceGrabFaceField = boxGrip.GetType().GetField("forceGrabFace", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (forceGrabFaceField != null)
                {
                    forceGrabFaceField.SetValue(boxGrip, BoxGrip.Faces.NegativeX);
                }
                
                var forceGrabTopField = boxGrip.GetType().GetField("forceGrabTop", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
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