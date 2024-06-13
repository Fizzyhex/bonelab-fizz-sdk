#if UNITY_EDITOR
using System.Numerics;
using FizzSDK.Tags;
using SLZ.Marrow.Warehouse;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace FizzSDK.Destruction
{
    [AddComponentMenu("FizzSDK/Destruction Toolkit/Collider Rescaler")]
    public class ColliderRescaler : DestructionIngredient
    {
        [SerializeField] private Vector3 sizeMultiplier = new(1, 1, 1);
        [SerializeField] private DataCard _tagFilter;
        
        public override void UseIngredient(GameObject targetGameObject)
        {
            var colliders = targetGameObject.GetComponentsInChildren<Collider>();

            foreach (var targetCollider in colliders)
            {
                if (_tagFilter && !targetCollider.gameObject.HasFizzTag(_tagFilter))
                {
                    continue;
                }
                
                if (targetCollider is BoxCollider boxCollider)
                {
                    boxCollider.size = new Vector3(
                        boxCollider.size.x * sizeMultiplier.x,
                        boxCollider.size.y * sizeMultiplier.y,
                        boxCollider.size.z * sizeMultiplier.z
                    );
                }
                else if (targetCollider is SphereCollider sphereCollider)
                {
                    sphereCollider.radius *= sizeMultiplier.x;
                }
                else if (targetCollider is CapsuleCollider capsuleCollider)
                {
                    capsuleCollider.radius *= sizeMultiplier.x;
                    capsuleCollider.height *= sizeMultiplier.y;
                }
            }
        }
    }
}
#endif