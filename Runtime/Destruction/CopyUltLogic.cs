#if UNITY_EDITOR
using FizzSDK.Tags;
using SLZ.Marrow.Warehouse;
using UltEvents;
using UnityEngine;

namespace FizzSDK.Destruction
{
    [AddComponentMenu("FizzSDK/Destruction Toolkit/Copy Logic")]
    public class CopyUltLogic : DestructionIngredient
    {
        public GameObject template;
        [Tooltip("Optional - leave as none to copy to all game objects")]
        public DataCard tagFilter;

        private static void UltEventReferenceSwap(UltEventBase ultEvent, GameObject from, GameObject to)
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
                object newValue = callTargetType == typeof(GameObject) ? to : to.GetComponent(callTargetType);
                
                // Use reflection to change _Target as it's private
                var field = call.GetType().GetField("_Target", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field.SetValue(call, newValue);
            }
        }
        
        private static void CopyLogic(GameObject logicRoot, GameObject logicTo)
        {
            var logicFrom = logicRoot.transform.GetChild(0).gameObject;
            var logicClone = Instantiate(logicFrom, logicTo.transform, false);
            logicClone.name = logicFrom.name;

            foreach (var ultEventHolder in logicClone.GetComponentsInChildren<UltEventHolder>())
            {
                UltEventReferenceSwap(ultEventHolder.Event, logicRoot, logicTo);
            }
            
            foreach (var delayedUltEventHolder in logicClone.GetComponentsInChildren<DelayedUltEventHolder>())
            {
                UltEventReferenceSwap(delayedUltEventHolder.Event, logicRoot, logicTo);
            }
            
            foreach (var lifeCycleEvents in logicClone.GetComponentsInChildren<LifeCycleEvents>())
            {
                UltEventReferenceSwap(lifeCycleEvents.EnableEvent, logicRoot, logicTo);
                UltEventReferenceSwap(lifeCycleEvents.DisableEvent, logicRoot, logicTo);
                UltEventReferenceSwap(lifeCycleEvents.AwakeEvent, logicRoot, logicTo);
                UltEventReferenceSwap(lifeCycleEvents.StartEvent, logicRoot, logicTo);
                UltEventReferenceSwap(lifeCycleEvents.DestroyEvent, logicRoot, logicTo);
            }

            foreach (var logicBehaviour in logicClone.GetComponentsInChildren<RigidbodyLogicBehaviour>())
            {
                logicBehaviour.RunLogic(logicTo);
            }
        }

        public override void UseIngredient(GameObject targetGameObject)
        {
            foreach (var copyTransform in targetGameObject.GetComponentsInChildren<Transform>())
            {
                var copyToGameObject = copyTransform.gameObject;
                
                if (tagFilter && !copyToGameObject.gameObject.HasFizzTag(tagFilter))
                {
                    continue;
                }
                
                CopyLogic(template, copyToGameObject);
            }
        }
    }
}
#endif