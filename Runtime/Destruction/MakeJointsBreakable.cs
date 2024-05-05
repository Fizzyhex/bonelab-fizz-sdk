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
    [AddComponentMenu("FizzSDK/Make Joints Breakable")]
    public class MakeJointsBreakable : DestructionIngredient
    {
        [Header("Prop Health")]
        public int maxHealth = 100;
        public AttackType attackType = AttackType.Blunt;
        public float attackTypeDamage = 0.75f;

        public override void UseIngredient(GameObject targetGameObject) => MakeJoints(targetGameObject);

        public void MakeJoints(GameObject targetGameObject)
        {
            var allRbs = targetGameObject.GetComponentsInChildren<Rigidbody>();

            foreach (var rb in allRbs)
            {
                if (!rb.gameObject.TryGetComponent<JointConnections>(out var jointConnections))
                {
                    continue;
                }
                
                var jointsToBreak = rb.gameObject.GetComponents<ConfigurableJoint>().ToList();
                
                foreach (var joint in jointConnections.joints)
                {
                    if (joint is ConfigurableJoint configJoint)
                    {
                        jointsToBreak.Add(configJoint);
                    }
                }
                
                var ultEventHolder = rb.gameObject.AddOrGetComponent<UltEventHolder>();

                foreach (var joint in jointsToBreak)
                {
                    var configJointExtendedEvents = rb.gameObject.AddComponent<ConfigJointExtendedEvents>();
                    configJointExtendedEvents.joint = joint;
                    UnityAction action = configJointExtendedEvents.SetAllFree;
                    ultEventHolder.Event.AddPersistentCall(action);
                }
            }

            foreach (var rb in allRbs)
            {
                rb.gameObject.AddOrGetComponent<Prop_DamageReceiver>();

                if (!rb.gameObject.TryGetComponent<Prop_Health>(out var propHealth))
                {
                    propHealth = rb.gameObject.AddComponent<Prop_Health>();
                    propHealth.RESETABLE = true;
                    propHealth.Pooled = true;
                    propHealth.max_Health = maxHealth;
                    propHealth.damageFromAttack = true;
                    propHealth.damageFromImpact = true;
                    propHealth.mod_Attack = 1;
                    propHealth.mod_Impact = 1;
                    propHealth.thr_Impact = 1;

                    // set private cur_health field using reflection
                    var curHealthField = typeof(Prop_Health).GetField("cur_Health",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (curHealthField != null) curHealthField.SetValue(propHealth, maxHealth);

                    propHealth.mod_Type = attackType;
                    propHealth.mod_TypeDamage = attackTypeDamage;
                }
                
                if (!rb.gameObject.TryGetComponent<UltEventHolder>(out var ultEventHolder))
                {
                    continue;
                }

                // :(
                propHealth.BreakEvent ??= new UnityEvent();

                UnityEventTools.AddVoidPersistentListener(propHealth.BreakEvent, ultEventHolder.Invoke);

                #if UNITY_EDITOR
                EditorUtility.SetDirty(propHealth);
                #endif
            }
        }
    }

}
#endif