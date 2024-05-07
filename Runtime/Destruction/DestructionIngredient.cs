#if UNITY_EDITOR
using UnityEngine;

namespace FizzSDK.Destruction
{
    public abstract class DestructionIngredient : MonoBehaviour
    {
        public abstract void UseIngredient(GameObject targetGameObject);
    }
}
#endif