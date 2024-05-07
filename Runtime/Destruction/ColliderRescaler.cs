#if UNITY_EDITOR
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace FizzSDK.Destruction
{
    internal struct ColliderInfo
    {
        public Vector3 Center;
        public Vector3 Size;
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
                    Center = boxCollider.center,
                    Size = boxCollider.size
                };
            }
            
            var meshCollider = subject.GetComponent<MeshCollider>();
            
            if (meshCollider)
            {
                return new ColliderInfo
                {
                    Center = Vector3.zero,
                    Size = meshCollider.bounds.size
                };
            }
            
            return new ColliderInfo
            {
                Center = Vector3.zero,
                Size = Vector3.zero
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
                    colliderInfo.Size.x * sizeMultiplier.x,
                    colliderInfo.Size.y * sizeMultiplier.y,
                    colliderInfo.Size.z * sizeMultiplier.z
                );

                targetCollider.center = colliderInfo.Center;
                targetCollider.size = newScale;
            }
        }
    }
}
#endif