#if UNITY_EDITOR
using System;
using UnityEngine;
using FizzSDK.Utils;
using SLZ.Bonelab;
using SLZ.Marrow;
using UltEvents;
using Vector3 = UnityEngine.Vector3;

namespace FizzSDK.Destruction
{
    [AddComponentMenu("FizzSDK/Destruction Toolkit/Generate Reset Event Holder")]
    public class GenerateResetUltEventHolder : DestructionIngredient
    {
        [Tooltip("The name of the GameObject that'll hold the UltEventHolder. A new one will be created if it doesn't exist.")]
        public string outputGameObjectName = "ResetEverything";
        private const string RigidbodyResetHolderName = "PoolingReset";
        
        public override void UseIngredient(GameObject targetGameObject) => GeneratePoolingUltEvent(targetGameObject);

        private UltEventHolder FindResetAllUltEventHolder(GameObject parent)
        {
            var holderTransform = parent.transform.Find(outputGameObjectName);
            if (holderTransform) return holderTransform.AddOrGetComponent<UltEventHolder>();

            var newHolder = new GameObject(outputGameObjectName)
            {
                transform =
                {
                    parent = parent.transform
                }
            };
                
            return newHolder.AddComponent<UltEventHolder>();
        }

        private UltEventHolder GetResetUltEventHolder(GameObject parent)
        {
            // Search for 'LockUltEventHolder' GameObject under parent
            var holderTransform = parent.transform.Find(RigidbodyResetHolderName);

            if (!holderTransform)
            {
                // create new
                var newHolder = new GameObject(RigidbodyResetHolderName)
                {
                    transform =
                    {
                        parent = parent.transform
                    }
                };
                
                holderTransform = newHolder.transform;
            }

            var holderGameObject = holderTransform.gameObject;
            var ultEventHolder = holderGameObject.AddOrGetComponent<UltEventHolder>();
            ultEventHolder.Event.Clear();
            
            // re-lock config joints
            foreach (ConfigJointExtendedEvents configJointExtendedEvents in parent.GetComponentsInChildren<ConfigJointExtendedEvents>())
            {
                var setAllLockedMethod = typeof(ConfigJointExtendedEvents).GetMethod("SetAllLocked", new Type[] {});
                var setAllLockedDelegate = Delegate.CreateDelegate(typeof(Action), configJointExtendedEvents, setAllLockedMethod);
                ultEventHolder.Event.AddPersistentCall(setAllLockedDelegate);
            }
            
            // if only we had a newer version of unity...
            // https://docs.unity3d.com/ScriptReference/Transform.SetLocalPositionAndRotation.html
            
            // local position
            var setPositionMethod = typeof(Transform).GetMethod("set_localPosition", new Type[] { typeof(Vector3) });
            var setPositionDelegate = Delegate.CreateDelegate(typeof(Action<Vector3>), parent.transform, setPositionMethod);
            var setPositionCall = ultEventHolder.Event.AddPersistentCall(setPositionDelegate);
            setPositionCall.SetArguments(parent.transform.localPosition);
            
            // local rotation
            var setRotationMethod = typeof(Transform).GetMethod("set_localRotation", new Type[] { typeof(Quaternion) });
            var setRotationDelegate = Delegate.CreateDelegate(typeof(Action<Quaternion>), parent.transform, setRotationMethod);
            var setRotationCall = ultEventHolder.Event.AddPersistentCall(setRotationDelegate);
            setRotationCall.SetArguments(parent.transform.localRotation);
            
            var objectDestructible = parent.AddOrGetComponent<ObjectDestructible>();

            if (objectDestructible)
            {
                // reset health
                //var receiveHealMethod = typeof(ObjectDestructible).GetMethod("SendDamage", new Type[] { typeof(float) });
                //var receiveHealDelegate = Delegate.CreateDelegate(typeof(Action<float>), objectDestructible, receiveHealMethod);
                //var receiveHealCall = ultEventHolder.Event.AddPersistentCall(receiveHealDelegate);
                //receiveHealCall.SetArguments(-objectDestructible.maxHealth);
                
                var resetMethod = typeof(ObjectDestructible).GetMethod("Reset");
                var resetMethodDelegate = Delegate.CreateDelegate(typeof(Action), objectDestructible, resetMethod);
                ultEventHolder.Event.AddPersistentCall(resetMethodDelegate);
                
                // re-enable script
                var enableHealthMethod = typeof(Behaviour).GetMethod("set_enabled", new Type[] { typeof(bool) });
                var enableHealthDelegate = Delegate.CreateDelegate(typeof(Action<bool>), objectDestructible, enableHealthMethod);
                var enableHealthCall = ultEventHolder.Event.AddPersistentCall(enableHealthDelegate);
                enableHealthCall.SetArguments(true);
            }
            
            var rb = parent.GetComponent<Rigidbody>();

            if (rb)
            {
                // reset velocity
                var setVelocityMethod = typeof(Rigidbody).GetMethod("set_velocity", new Type[] { typeof(Vector3) });
                var setVelocityDelegate = Delegate.CreateDelegate(typeof(Action<Vector3>), rb, setVelocityMethod);
                var setVelocityCall = ultEventHolder.Event.AddPersistentCall(setVelocityDelegate);
                setVelocityCall.SetArguments(Vector3.zero);
                
                // reset angular velocity
                var setAngularVelocityMethod = typeof(Rigidbody).GetMethod("set_angularVelocity", new Type[] { typeof(Vector3) });
                var setAngularVelocityDelegate = Delegate.CreateDelegate(typeof(Action<Vector3>), rb, setAngularVelocityMethod);
                var setAngularVelocityCall = ultEventHolder.Event.AddPersistentCall(setAngularVelocityDelegate);
                setAngularVelocityCall.SetArguments(Vector3.zero);
                
                // goto sleep
                var sleepMethod = typeof(Rigidbody).GetMethod("Sleep", new Type[] {});
                var sleepDelegate = Delegate.CreateDelegate(typeof(Action), rb, sleepMethod);
                ultEventHolder.Event.AddPersistentCall(sleepDelegate);

                // re-kinematic
                if (rb.isKinematic)
                {
                    var setIsKinematicMethod = typeof(Rigidbody).GetMethod("set_isKinematic", new Type[] { typeof(bool) });
                    var setIsKinematicDelegate = Delegate.CreateDelegate(typeof(Action<bool>), rb, setIsKinematicMethod);
                    ultEventHolder.Event.AddPersistentCall(setIsKinematicDelegate).SetArguments(true);
                }
            }
            
            return ultEventHolder;
        }
        
        private void GeneratePoolingUltEvent(GameObject targetGameObject)
        {
            var ultEventHolder = FindResetAllUltEventHolder(targetGameObject);
            var allRigidbodies = targetGameObject.GetComponentsInChildren<Rigidbody>();
            
            ultEventHolder.Event.Clear();

            foreach (var rb in allRigidbodies)
            {
                var resetter = GetResetUltEventHolder(rb.gameObject);

                var resetMethod = typeof(UltEventHolder).GetMethod("Invoke", new Type[] {});
                var resetDelegate = Delegate.CreateDelegate(typeof(Action), resetter, resetMethod);
                ultEventHolder.Event.AddPersistentCall(resetDelegate);
            }
        }
    }
}
#endif