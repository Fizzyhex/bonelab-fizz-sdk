#if UNITY_EDITOR

using SLZ.Marrow.Interaction;
using UnityEngine;

namespace FizzSDK.Destruction
{
    public class Marrowify : DestructionIngredient
    {
        public override void UseIngredient(GameObject target)
        {
            MarrowEntityEditor.PopulateMarrowComponents(target);
        }
        
    }
}

#endif