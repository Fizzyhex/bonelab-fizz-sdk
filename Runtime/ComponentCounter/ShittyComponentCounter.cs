using System.Linq;
using UnityEngine;

namespace FizzSDK
{
    public abstract class ShittyComponentCounter<T> : MonoBehaviour where T : Component
    {
        [HideInInspector] public string componentName;
        [Tooltip("If set, only count components that are children of this object.")]
        [SerializeField] private GameObject container;
        
        [ContextMenu("Count!")]
        public void Count()
        {
            var components = FindObjectsOfType<T>();

            if (container != null)
            {
                components = components.Where(c => c.gameObject.transform.IsChildOf(container.transform)).ToArray();
            }
            
            Debug.Log($"found {components.Length} {componentName}(s).");
        }
    }
}