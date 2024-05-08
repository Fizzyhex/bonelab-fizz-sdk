using UnityEngine;
using UnityEngine.Serialization;

namespace FizzSDK.Tools
{
    public class VisualiseColliderBounds : MonoBehaviour
    {
        [SerializeField] private Vector3 additionalSize = new Vector3(0, 0, 0);
        private readonly Collider[] _overlapResults = new Collider[20];
        
        private void OnDrawGizmos()
        {
            var ourCollider = GetComponent<Collider>();
            var rb = GetComponent<Rigidbody>();
            if (ourCollider == null) return;
            if (!rb) return;
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(rb.worldCenterOfMass, ourCollider.bounds.size + additionalSize);
            
            // visualise ourCollider.bounds.center
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(rb.worldCenterOfMass, Vector3.one * 0.1f);
            
            var size = Physics.OverlapBoxNonAlloc(rb.worldCenterOfMass, (ourCollider.bounds.size + additionalSize) / 2, _overlapResults);
            
            for (var i = 0; i < size; i++)
            {
                var hit = _overlapResults[i];
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(hit.bounds.center, hit.bounds.size);
            }
        }
    }
}