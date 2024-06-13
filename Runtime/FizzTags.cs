#if UNITY_EDITOR
using System.Collections.Generic;
using SLZ.Marrow.Warehouse;
using UnityEngine;

namespace FizzSDK.Tags
{
    public class FizzTags : MonoBehaviour
    {
        public List<DataCard> tags;

        public static bool HasTag(GameObject gameObject, DataCard queryTag)
        {
            var tags = gameObject.GetComponent<FizzTags>();
            return tags && tags.tags.Contains(queryTag);
        }
    }
    
    public static class FizzTagExtensions
    {
        public static bool HasFizzTag(this GameObject gameObject, DataCard queryTag)
        {
            return FizzTags.HasTag(gameObject, queryTag);
        }
    }
}
#endif