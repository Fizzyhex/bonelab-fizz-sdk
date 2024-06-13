#if UNITY_EDITOR
using System;
using FizzSDK.Tags;
using FizzSDK.Utils;
using SLZ.Marrow.Warehouse;
using UltEvents;
using UnityEditor;
using UnityEngine;

namespace FizzSDK.Destruction
{
    [AddComponentMenu("FizzSDK/Destruction Toolkit/Make Rigid Impostors")]
    public class MakeRigidImpostors : DestructionIngredient
    {
        public DataCard tagFilter;
        
        public override void UseIngredient(GameObject targetGameObject)
        {
            var impostorContainer = new GameObject("Impostor Container");
            impostorContainer.transform.SetParent(targetGameObject.transform);
            
            var colliders = targetGameObject.GetComponentsInChildren<Collider>();

            foreach (var sourceCollider in colliders)
            {
                if (tagFilter && !sourceCollider.gameObject.HasFizzTag(tagFilter))
                {
                    continue;
                }

                var source = sourceCollider.gameObject;

                var impostor = Instantiate(sourceCollider.gameObject, impostorContainer.transform);
                impostor.name = $"{sourceCollider.gameObject.name} (Impostor)";
                impostor.transform.SetPositionAndRotation(source.transform.position, source.transform.rotation);
                
                var impostorCollider = impostor.GetComponent<Collider>();
                
                if (impostorCollider)
                {
                    impostorCollider.isTrigger = false;
                }
                
                var impostorTrigger = source.AddComponent<TriggerEvents3D>();
                
                var setIsActiveMethod = typeof(GameObject).GetMethod("SetActive", new[] {typeof(bool)});
                var sourceSetIsActiveDelegate = Delegate.CreateDelegate(typeof(Action<bool>), source, setIsActiveMethod);
                var impostorSetIsActiveDelegate = Delegate.CreateDelegate(typeof(Action<bool>), impostor, setIsActiveMethod);
                
                impostorTrigger.TriggerExitEvent.AddPersistentCall(sourceSetIsActiveDelegate).SetArguments(false);
                impostorTrigger.TriggerEnterEvent.AddPersistentCall(impostorSetIsActiveDelegate).SetArguments(true);
                
                impostor.SetActive(false);
                
                // copy rigidbody from source to impostor
                
                var sourceRigidbody = source.GetComponent<Rigidbody>();

                if (!sourceRigidbody) continue;
                var impostorRigidbody = impostor.GetComponent<Rigidbody>();
                EditorUtility.CopySerialized(sourceRigidbody, impostorRigidbody);
                DestroyImmediate(sourceRigidbody);
            }
        }
    }
}
#endif