#if UNITY_EDITOR
using FizzSDK.Tags;
using SLZ.Marrow.Warehouse;
using SLZ.Marrow.Zones;
using UltEvents;
using UnityEngine;

namespace FizzSDK.Destruction
{
    [AddComponentMenu("FizzSDK/Destruction Toolkit/Copy Ult Logic")]
    public class CopyUltLogic : DestructionIngredient
    {
        public GameObject template;
        [Tooltip("Optional - leave as none to copy to all game objects")]
        public DataCard tagFilter;
        
        public static void UltEventReferenceSwap(UltEventBase ultEvent, GameObject from, GameObject to)
        {
            foreach (var call in ultEvent.PersistentCallsList)
            {
                // Replace references to logicRoot with logicTo
                var targetComponent = call.Target as Component;
                    
                if (targetComponent && targetComponent.gameObject != from)
                {
                    continue;
                }

                var callTargetType = call.Target.GetType();
                
                if (callTargetType == typeof(GameObject) && call.Target != from)
                {
                    continue;
                }
                
                object newValue = callTargetType == typeof(GameObject) ? to : to.GetComponent(callTargetType);
                
                // Use reflection to change _Target as it's private
                var field = call.GetType().GetField("_Target", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field.SetValue(call, newValue);
            }
        }
        
        public static GameObject CopyLogic(GameObject logicRoot, GameObject logicTo)
        {
            var logicFrom = logicRoot.transform.GetChild(0).gameObject;
            var logicClone = Instantiate(logicFrom, logicTo.transform, false);
            logicClone.name = logicFrom.name;
            
            // Needs to be re-done to support fields of all components, rather than specific ones

            foreach (var ultEventHolder in logicClone.GetComponentsInChildren<UltEventHolder>(true))
            {
                UltEventReferenceSwap(ultEventHolder.Event, logicRoot, logicTo);
            }
            
            foreach (var delayedUltEventHolder in logicClone.GetComponentsInChildren<DelayedUltEventHolder>(true))
            {
                UltEventReferenceSwap(delayedUltEventHolder.Event, logicRoot, logicTo);
            }
            
            foreach (var lifeCycleEvents in logicClone.GetComponentsInChildren<LifeCycleEvents>(true))
            {
                UltEventReferenceSwap(lifeCycleEvents.EnableEvent, logicRoot, logicTo);
                UltEventReferenceSwap(lifeCycleEvents.DisableEvent, logicRoot, logicTo);
                UltEventReferenceSwap(lifeCycleEvents.AwakeEvent, logicRoot, logicTo);
                UltEventReferenceSwap(lifeCycleEvents.StartEvent, logicRoot, logicTo);
                UltEventReferenceSwap(lifeCycleEvents.DestroyEvent, logicRoot, logicTo);
            }

            foreach (var zoneEvents in logicClone.GetComponentsInChildren<ZoneEvents>(true))
            {
                UltEventReferenceSwap(zoneEvents.onZoneEnter, logicRoot, logicTo);
                UltEventReferenceSwap(zoneEvents.onZoneExit, logicRoot, logicTo);
            }

            return logicClone;
        }

        public override void UseIngredient(GameObject targetGameObject)
        {
            foreach (var copyTransform in targetGameObject.GetComponentsInChildren<Transform>(true))
            {
                var copyToGameObject = copyTransform.gameObject;
                
                if (tagFilter && !copyToGameObject.gameObject.HasFizzTag(tagFilter))
                {
                    continue;
                }
                
                var logic = CopyLogic(template, copyToGameObject);
            }
        }
    }
}
#endif