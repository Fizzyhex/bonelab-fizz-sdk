#if UNITY_EDITOR
using UnityEngine;

namespace FizzSDK.Destruction
{
    public abstract class RigidbodyLogicBehaviour : MonoBehaviour
    {
        public abstract void RunLogic(GameObject root);
    }
}
#endif