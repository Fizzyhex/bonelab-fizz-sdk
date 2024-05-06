using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using FizzSDK.Utils;
using SLZ.Props;
using UltEvents;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine.Events;
using Vector3 = UnityEngine.Vector3;

namespace FizzSDK.Destruction
{
    [AddComponentMenu("FizzSDK/Generate Reset Event Holder")]
    public class GenerateResetUltEventHolder : DestructionIngredient
    {
        [Tooltip("The name of the GameObject that'll hold the UltEventHolder. A new one will be created if it doesn't exist.")]
        [SerializeField] private string outputGameObjectName = "ResetEverything";
        
        public override void UseIngredient(GameObject targetGameObject) => GeneratePoolingUltEvent(targetGameObject);

        private UltEventHolder FindUltEventHolder(GameObject parent)
        {
            var holderTransform = parent.transform.Find(outputGameObjectName);
            if (holderTransform) return holderTransform.GetOrAddComponent<UltEventHolder>();

            var newHolder = new GameObject(outputGameObjectName)
            {
                transform =
                {
                    parent = parent.transform
                }
            };
                
            return newHolder.AddComponent<UltEventHolder>();
        }
        
        private void GeneratePoolingUltEvent(GameObject targetGameObject)
        {
            var ultEventHolder = FindUltEventHolder(targetGameObject);
            var allRigidbodies = targetGameObject.GetComponentsInChildren<Rigidbody>();
            
            ultEventHolder.Event.Clear();

            foreach (var rb in allRigidbodies)
            {
                var setPositionAndRotationMethod = typeof(Transform).GetMethod("SetPositionAndRotation", new Type[] { typeof(Vector3), typeof(Quaternion) });
                var methodDelegate = Delegate.CreateDelegate(typeof(Action<Vector3, Quaternion>), rb.transform, setPositionAndRotationMethod);
                var setPositionCall = ultEventHolder.Event.AddPersistentCall(methodDelegate);
                setPositionCall.SetArguments(rb.transform.position, rb.transform.rotation);
                
                var propHealth = rb.gameObject.AddOrGetComponent<Prop_Health>();

                if (propHealth)
                {
                    // Call propHealth.ReceiveHeal with max_Health
                    var receiveHealMethod = typeof(Prop_Health).GetMethod("ReceiveHeal", new Type[] { typeof(float) });
                    var receiveHealDelegate = Delegate.CreateDelegate(typeof(Action<float>), propHealth, receiveHealMethod);
                    var receiveHealCall = ultEventHolder.Event.AddPersistentCall(receiveHealDelegate);
                    receiveHealCall.SetArguments(propHealth.max_Health);

                    var enableHealthMethod = typeof(Behaviour).GetMethod("set_enabled", new Type[] { typeof(bool) });
                    var enableHealthDelegate = Delegate.CreateDelegate(typeof(Action<bool>), propHealth, enableHealthMethod);
                    var enableHealthCall = ultEventHolder.Event.AddPersistentCall(enableHealthDelegate);
                    enableHealthCall.SetArguments(true);
                }
                
                EditorUtility.SetDirty(ultEventHolder);
            }
        }
    }
}