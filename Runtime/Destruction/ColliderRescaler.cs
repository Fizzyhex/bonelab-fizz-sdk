using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace FizzSDK.Destruction
{
    internal struct ColliderInfo
    {
        public Vector3 center;
        public Vector3 size;
    }
    
    [AddComponentMenu("FizzSDK/Destruction Toolkit/Collider Rescaler")]
    public class ColliderRescaler : RigidbodyLogicBehaviour
    {
        [SerializeField] private Vector3 sizeMultiplier = new(1, 1, 1);

        private ColliderInfo GetColliderInfo(GameObject subject)
        {
            var boxCollider = subject.GetComponent<BoxCollider>();

            if (boxCollider)
            {
                return new ColliderInfo
                {
                    center = boxCollider.center,
                    size = boxCollider.size
                };
            }
            
            var meshCollider = subject.GetComponent<MeshCollider>();
            
            if (meshCollider)
            {
                return new ColliderInfo
                {
                    center = Vector3.zero,
                    size = meshCollider.bounds.size
                };
            }
            
            return new ColliderInfo
            {
                center = Vector3.zero,
                size = Vector3.zero
            };
        }
        
        public override void RunLogic(GameObject root)
        {
            var colliderInfo = GetColliderInfo(root);
            var colliders = gameObject.GetComponents<BoxCollider>();

            foreach (var targetCollider in colliders)
            {
                var scale = root.transform.localScale;
                var newScale = new Vector3(
                    colliderInfo.size.x * sizeMultiplier.x,
                    colliderInfo.size.y * sizeMultiplier.y,
                    colliderInfo.size.z * sizeMultiplier.z
                );

                targetCollider.center = colliderInfo.center;
                targetCollider.size = newScale;
            }
        }
    }
}