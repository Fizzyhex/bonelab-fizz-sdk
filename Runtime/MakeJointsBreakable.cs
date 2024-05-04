#if UNITY_EDITOR
using System.Collections.Generic;
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
using Unity.VisualScripting;
using UnityEngine.Serialization;

namespace FizzSDK
{
    [AddComponentMenu("FizzSDK/Make Joints Breakable")]
    public class MakeJointsBreakable : MonoBehaviour, IJointScript
    {
        [SerializeField] private GameObject[] rigidbodyContainers;
        [FormerlySerializedAs("rigidbodies")] [SerializeField] private Rigidbody[] targetRigidbodies;

        [Header("Options")]
        [Tooltip("If checked, the components will be added in play mode")]
        [SerializeField]
        private bool createAtRuntime = false;

        [Header("Prop Health")]
        public int maxHealth = 100;
        public AttackType attackType = AttackType.Blunt;
        public float attackTypeDamage = 0.75f;

        private Rigidbody[] GetAllRigidbodies()
        {
            List<Rigidbody> allRbs = new();

            foreach (var rigidbodyContainer in rigidbodyContainers)
            {
                allRbs.AddRange(rigidbodyContainer.GetComponentsInChildren<Rigidbody>());
            }

            allRbs.AddRange(targetRigidbodies);
            return allRbs.ToArray();
        }

        private static void AddJointDeleter(GameObject holder, GameObject jointSource)
        {
            var joints = jointSource.GetComponents<ConfigurableJoint>();
            var ultEventHolder = holder.AddOrGetComponent<UltEventHolder>();

            foreach (var joint in joints)
            {
                var configJointExtendedEventsSelf = holder.AddComponent<ConfigJointExtendedEvents>();
                configJointExtendedEventsSelf.joint = joint;
                UnityAction action = configJointExtendedEventsSelf.SetAllFree;
                ultEventHolder.Event.AddPersistentCall(action);
#if UNITY_EDITOR
                EditorUtility.SetDirty(ultEventHolder);
                EditorUtility.SetDirty(configJointExtendedEventsSelf);
#endif
            }
        }

        public void MakeJoints()
        {
            var allRbs = GetAllRigidbodies();

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

                // AddJointDeleter(rb.gameObject, rb.gameObject);
                //
                // foreach (var connectedJoint in jointConnections.joints)
                // {
                //     AddJointDeleter(rb.gameObject, connectedJoint.gameObject);
                // }
                
                // foreach (var connectedJoint in jointConnections.joints)
                // {
                //     if (connectedJoint is ConfigurableJoint connectedConfigJoint)
                //     {
                //         var configJointExtendedEvents =
                //             rb.gameObject.AddComponent<ConfigJointExtendedEvents>();
                //         configJointExtendedEvents.joint = connectedConfigJoint;
                //
                //         var ultEventHolder =
                //             rb.gameObject.AddOrGetComponent<UltEventHolder>();
                //
                //         UnityAction action = configJointExtendedEvents.SetAllFree;
                //         ultEventHolder.Event.AddPersistentCall(action);
                //
                //         #if UNITY_EDITOR
                //         // We would need to use https://docs.unity3d.com/ScriptReference/Undo.RecordObject.html to support undo
                //         EditorUtility.SetDirty(ultEventHolder);
                //         EditorUtility.SetDirty(configJointExtendedEvents);
                //         #endif
                //     }
                // }
            }

            foreach (var rb in allRbs)
            {
                ComponentUtils.AddOrGetComponent<Prop_DamageReceiver>(rb.gameObject);

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
                propHealth.BreakEvent ??= new();

                UnityEventTools.AddVoidPersistentListener(propHealth.BreakEvent, ultEventHolder.Invoke);

                #if UNITY_EDITOR
                EditorUtility.SetDirty(propHealth);
                #endif
            }
        }

        private void Start()
        {
            if (createAtRuntime)
            {
                MakeJoints();
            }
        }
    }

}
#endif