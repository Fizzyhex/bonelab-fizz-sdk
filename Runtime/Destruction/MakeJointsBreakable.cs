#if UNITY_EDITOR
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using SLZ.Props;
using UltEvents;
using SLZ.Marrow.Data;
using UnityEditor.Events;
using SLZ.Bonelab;

using UnityEditor;
using FizzSDK.Utils;

namespace FizzSDK.Destruction
{
    [AddComponentMenu("FizzSDK/Destruction Toolkit/Make Joints Breakable")]
    public class MakeJointsBreakable : DestructionIngredient
    {
        [Tooltip("Template for Prop_Health component to be added to all objects with a Rigidbody")]
        public Prop_Health propHealthTemplate;
        
        private const int DefaultMaxHealth = 15;
        private const AttackType DefaultAttackType = AttackType.Blunt;
        private const float DefaultAttackTypeDamageMultiplier = 0.75f;
        
        public override void UseIngredient(GameObject targetGameObject) => MakeJoints(targetGameObject);

        public static void ApplyDefaultPropHealthValues(Prop_Health propHealth)
        {
            propHealth.RESETABLE = true;
            propHealth.Pooled = true;
            propHealth.max_Health = DefaultMaxHealth;
            propHealth.cur_Health = DefaultMaxHealth;
            propHealth.damageFromAttack = true;
            propHealth.damageFromImpact = true;
            propHealth.mod_Attack = 1;
            propHealth.mod_Impact = 1;
            propHealth.thr_Impact = 1;
            propHealth.mod_Type = DefaultAttackType;
            propHealth.mod_TypeDamage = DefaultAttackTypeDamageMultiplier;
        }
        
        private void MakeJoints(GameObject targetGameObject)
        {
            var allRigidbodies = targetGameObject.GetComponentsInChildren<Rigidbody>();

            foreach (var rb in allRigidbodies)
            {
                if (!rb.gameObject.TryGetComponent<JointConnections>(out var jointConnections))
                {
                    continue;
                }
                
                var jointsToBreak = rb.gameObject.GetComponents<ConfigurableJoint>().ToList();
                var ultEventHolder = rb.gameObject.AddOrGetComponent<UltEventHolder>();
                
                UnityAction ourWakeUpAction = rb.WakeUp;
                ultEventHolder.Event.AddPersistentCall(ourWakeUpAction);
                
                foreach (var joint in jointConnections.joints)
                {
                    if (joint is ConfigurableJoint configJoint)
                    {
                        jointsToBreak.Add(configJoint);
                        
                        var jointRb = joint.gameObject.GetComponent<Rigidbody>();
                        if (!jointRb) continue;
                        UnityAction action = jointRb.WakeUp;
                        ultEventHolder.Event.AddPersistentCall(action);
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
                rb.gameObject.AddOrGetComponent<Prop_DamageReceiver>();

                if (!rb.gameObject.TryGetComponent<Prop_Health>(out var propHealth))
                {
                    propHealth = rb.gameObject.AddComponent<Prop_Health>();
                    
                    if (propHealthTemplate)
                    {
                        EditorUtility.CopySerialized(propHealthTemplate, propHealth);
                    }
                    else
                    {
                        ApplyDefaultPropHealthValues(propHealth);
                    }
                }
                
                if (!rb.gameObject.TryGetComponent<UltEventHolder>(out var ultEventHolder))
                {
                    continue;
                }
                
                // FUTURE: Swap out references to the Prop_Health's GameObject to allow custom logic on the BreakEvent?

                // :(
                propHealth.BreakEvent ??= new UnityEvent();

                UnityEventTools.AddVoidPersistentListener(propHealth.BreakEvent, ultEventHolder.Invoke);
                EditorUtility.SetDirty(propHealth);
            }
        }
    }

}
#endif