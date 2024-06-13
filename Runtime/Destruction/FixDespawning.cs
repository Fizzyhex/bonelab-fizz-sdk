#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using FizzSDK.Utils;
using SLZ.Interaction;
using SLZ.Marrow.Interaction;
using Unity.VisualScripting;
using UnityEngine;

namespace FizzSDK.Destruction
{
    [AddComponentMenu("FizzSDK/Destruction Toolkit/Fix Despawning")]
    public class FixDespawning : DestructionIngredient
    {
        private static void AddHosts(GameObject root)
        {
            List<InteractableHost> interactableHosts = new();
            var interactableHostManager = root.AddOrGetComponent<InteractableHostManager>();
            
            var marrowEntity = root.GetComponentInParent<MarrowEntity>();

            foreach (var rb in root.GetComponentsInChildren<Rigidbody>())
            {
                var interactableHost = rb.AddComponent<InteractableHost>();
                interactableHost.manager = interactableHostManager;

                if (marrowEntity)
                {
                    interactableHost.marrowEntity = marrowEntity;
                }
                
                interactableHosts.Add(interactableHost);
            }

            interactableHostManager.hosts = interactableHosts.ToArray();
        }

        public override void UseIngredient(GameObject target) => AddHosts(target);
    }
}
#endif