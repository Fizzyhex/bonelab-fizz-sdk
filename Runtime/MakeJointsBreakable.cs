#if UNITY_EDITOR
using System.Collections.Generic;
using UltEvents;
using UnityEngine;
using UnityEngine.Events;
using SLZ.Props;
using SLZ.Marrow.Data;
using UnityEditor.Events;
using SLZ.Bonelab;

using UnityEditor;
using FizzSDK.Utils;

namespace FizzSDK
{
    public class MakeJointsBreakable : MonoBehaviour, IJointScript
    {
        [SerializeField] GameObject[] rigidbodyContainers;
        [SerializeField] Rigidbody[] rigidbodies;

        [Header("Options")]
        [Tooltip("If checked, the components will be added in play mode")]
        [SerializeField] bool createAtRuntime = false;

        [Header("Prop Health")]
        public int maxHealth = 100;
        public AttackType attackType = AttackType.Blunt;
        public float attackTypeDamage = 0.75f;

        Rigidbody[] GetAllRigidbodies()
        {
            List<Rigidbody> allRbs = new();

            foreach (GameObject rigidbodyContainer in rigidbodyContainers)
            {
                allRbs.AddRange(rigidbodyContainer.GetComponentsInChildren<Rigidbody>());
            }

            foreach (Rigidbody rigidbody in rigidbodies)
            {
                allRbs.Add(rigidbody);
            }

            return allRbs.ToArray();
        }

        public void MakeJoints()
        {
            Rigidbody[] allRbs = GetAllRigidbodies();

            foreach (Rigidbody rb in allRbs)
            {
                
                if (!rb.gameObject.TryGetComponent<JointConnections>(out var jointConnections))
                {
                    continue;
                }

                foreach (Joint connectedJoint in jointConnections.joints)
                {
                    if (connectedJoint is ConfigurableJoint connectedConfigJoint)
                    {
                        ConfigJointExtendedEvents configJointExtendedEvents =
                            rb.gameObject.AddComponent<ConfigJointExtendedEvents>();

                        configJointExtendedEvents.joint = connectedConfigJoint;

                        UltEventHolder ultEventHolder =
                            ComponentUtils.AddOrGetComponent<UltEventHolder>(rb.gameObject);

                        UnityAction action = configJointExtendedEvents.SetAllFree;
                        ultEventHolder.Event.AddPersistentCall(action);

                        #if UNITY_EDITOR
                        // We would need to use https://docs.unity3d.com/ScriptReference/Undo.RecordObject.html to support undo
                        EditorUtility.SetDirty(ultEventHolder);
                        EditorUtility.SetDirty(configJointExtendedEvents);
                        #endif
                    }
                }
            }

            foreach (Rigidbody rb in allRbs)
            {
                ComponentUtils.AddOrGetComponent<Prop_DamageReceiver>(rb.gameObject);

                if (!rb.gameObject.TryGetComponent<Prop_Health>(out var propHealth))
                {
                    propHealth = rb.gameObject.AddComponent<Prop_Health>();
                    propHealth.RESETABLE = true;
                    propHealth.Pooled = true;
                    propHealth.max_Health = maxHealth;

                    // set private cur_health field using reflection
                    var curHealthField = typeof(Prop_Health).GetField("cur_Health", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    curHealthField.SetValue(propHealth, maxHealth);

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

        void Start()
        {
            if (createAtRuntime)
            {
                MakeJoints();
            }
        }
    }

}
#endif