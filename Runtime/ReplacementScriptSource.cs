using UnityEngine;

namespace FizzSDK
{
    [CreateAssetMenu(fileName = "Replacement Script Source", menuName = "FizzSDK/Replacement Script Source")]
    public class ReplacementScriptSource : ScriptableObject
    {
        [Multiline] public string source = "";
    }
}