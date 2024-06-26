﻿#if UNITY_EDITOR
using System.Linq;
using UltEvents;
using UnityEngine;

namespace FizzSDK.Destruction
{
    [AddComponentMenu("FizzSDK/Destruction Toolkit/Copy Rigidbody Logic")]
    public class CopyRigidbodyLogic : DestructionIngredient
    {
        public GameObject template;

        private static void UltEventReferenceSwap(UltEventBase ultEvent, GameObject from, GameObject to)
        {
            foreach (var call in ultEvent.PersistentCallsList)
            {
                // Replace references to logicRoot with logicTo
                if (call.Target is not Component targetComponent) continue;
                    
                if (targetComponent.gameObject != from)
                {
                    continue;
                }
                        
                var newC = to.GetComponent(call.Target.GetType());
                        
                // use reflection to change _Target as it's private
                var field = call.GetType().GetField("_Target", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field.SetValue(call, newC);
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
            foreach (var rb in targetGameObject.GetComponentsInChildren<Rigidbody>())
            {
                CopyLogic(template, rb.gameObject);
            }
        }
    }
}
#endif