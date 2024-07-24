#if UNITY_EDITOR
using System;
using System.Linq;
using FizzSDK.Tags;
using UnityEngine;
using UnityEngine.Events;
using UltEvents;
using SLZ.Marrow.Data;
using UnityEditor.Events;
using SLZ.Bonelab;

using UnityEditor;
using FizzSDK.Utils;
using SLZ.Marrow;
using SLZ.Marrow.Pool;
using SLZ.Marrow.Warehouse;
using UnityEngine.Serialization;

namespace FizzSDK.Destruction
{
    [AddComponentMenu("FizzSDK/Destruction Toolkit/Make Joints Breakable")]
    public class MakeJointsBreakable : DestructionIngredient
    {
        [FormerlySerializedAs("propHealthTemplate")]
        [Tooltip("Template for ObjectDestructible component to be added to all objects with a Rigidbody.")]
        public ObjectDestructible objectDestructibleTemplate;
        
        [Tooltip("(Optional) Only objects with this tag will have joints made breakable. Leave as none to effect all rigidbodies.")]
        public DataCard tagFilter;

        [Header("Optimisation")]
        [Tooltip("If true, all rigidbodies will become kinematic.")]
        public bool makeEverythingKinematic = false;
        [Tooltip("If true, objects will have kinematic disabled when damaged.")]
        public bool kinematicUntilDamaged = false;
        
        private const int DefaultMaxHealth = 15;
        private const AttackType DefaultAttackType = AttackType.Blunt;
        private const float DefaultAttackTypeDamageMultiplier = 0.75f;
        
        public override void UseIngredient(GameObject targetGameObject) => MakeJoints(targetGameObject);

        public static void ApplyDefaultValues(ObjectDestructible objectDestructible)
        {
            objectDestructible.maxHealth = DefaultMaxHealth;
            objectDestructible.reqHitCount = 1;
            objectDestructible.damageFromAttack = true;
            objectDestructible.damageFromImpact = true;
            objectDestructible.attackMod = 1;
            objectDestructible.modImpact = 1;
            objectDestructible.thrImpact = 1;
            objectDestructible.attackType = DefaultAttackType;
            objectDestructible.modTypeDamage = DefaultAttackTypeDamageMultiplier;
            
            var healthField = typeof(ObjectDestructible).GetField("_health", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            healthField.SetValue(objectDestructible, DefaultMaxHealth);
        }
        
        private void MakeJoints(GameObject targetGameObject)
        {
            var allRigidbodies = targetGameObject.GetComponentsInChildren<Rigidbody>();
            var poolee = targetGameObject.GetComponentInParent<Poolee>();
            
            if (tagFilter)
            {
                allRigidbodies = allRigidbodies.Where(rb => rb.gameObject.HasFizzTag(tagFilter)).ToArray();
            }

            foreach (var rb in allRigidbodies)
            {
                if (makeEverythingKinematic)
                {
                    rb.isKinematic = true;
                }
                
                if (!rb.gameObject.TryGetComponent<JointConnections>(out var jointConnections))
                {
                    continue;
                }
                
                var jointsToBreak = rb.gameObject.GetComponents<ConfigurableJoint>().ToList();
                var ultEventHolder = rb.gameObject.AddOrGetComponent<UltEventHolder>();
                
                UnityAction ourWakeUpAction = rb.WakeUp;
                ultEventHolder.Event.AddPersistentCall(ourWakeUpAction);

                if (kinematicUntilDamaged)
                {
                    var setIsKinematicMethod = typeof(Rigidbody).GetMethod("set_isKinematic", new[] {typeof(bool)});
                    var setIsKinematicDelegate = Delegate.CreateDelegate(typeof(Action<bool>), rb, setIsKinematicMethod);
                    ultEventHolder.Event.AddPersistentCall(setIsKinematicDelegate).SetArguments(false);
                }
                
                foreach (var joint in jointConnections.joints)
                {
                    if (joint is ConfigurableJoint configJoint)
                    {
                        jointsToBreak.Add(configJoint);
                        
                        var jointRb = joint.gameObject.GetComponent<Rigidbody>();
                        if (!jointRb) continue;
                        UnityAction action = jointRb.WakeUp;
                        ultEventHolder.Event.AddPersistentCall(action);

                        if (kinematicUntilDamaged)
                        {
                            var setIsKinematicMethod = typeof(Rigidbody).GetMethod("set_isKinematic", new[] {typeof(bool)});
                            var setIsKinematicDelegate = Delegate.CreateDelegate(typeof(Action<bool>), jointRb, setIsKinematicMethod);
                            ultEventHolder.Event.AddPersistentCall(setIsKinematicDelegate).SetArguments(false);
                        }
                    }
                }

                foreach (var joint in jointsToBreak)
                {
                    var configJointExtendedEvents = rb.gameObject.AddComponent<ConfigJointExtendedEvents>();
                    configJointExtendedEvents.joint = joint;
                    UnityAction action = configJointExtendedEvents.SetAllFree;
                    ultEventHolder.Event.AddPersistentCall(action);
                }
            }

            foreach (var rb in allRigidbodies)
            {
                if (!rb.gameObject.TryGetComponent<ObjectDestructible>(out var objectDestructible))
                {
                    objectDestructible = rb.gameObject.AddComponent<ObjectDestructible>();
                    
                    if (objectDestructibleTemplate)
                    {
                        EditorUtility.CopySerialized(objectDestructibleTemplate, objectDestructible);
                    }
                    else
                    {
                        ApplyDefaultValues(objectDestructible);
                    }

                    var pooleeField = typeof(ObjectDestructible).GetField("_poolee", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    pooleeField.SetValue(objectDestructible, poolee);
                    
                    var rbField = typeof(ObjectDestructible).GetField("_rb", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    rbField.SetValue(objectDestructible, rb);
                }
                
                if (!rb.gameObject.TryGetComponent<UltEventHolder>(out var ultEventHolder))
                {
                    continue;
                }
                
                // FUTURE: Swap out references to the Object Destructible's GameObject to allow custom logic on the BreakEvent?

                objectDestructible.OnDestruct ??= new UnityEvent();

                UnityEventTools.AddVoidPersistentListener(objectDestructible.OnDestruct, ultEventHolder.Invoke);
                EditorUtility.SetDirty(objectDestructible);
            }
        }
    }

}
#endif