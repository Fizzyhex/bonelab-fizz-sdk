#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SLZ.Marrow;
using UnityEditor;

namespace FizzSDK.Runtime
{
    public static class HandPoseProvider
    {
        private static Dictionary<string, HandPose> _handPoses;

        private static void FindHandPoses()
        {
            _handPoses ??= new Dictionary<string, HandPose>();
            AssetDatabase.FindAssets("t:HandPose").ToList().ForEach(guid =>
            {
                var handPoseAsset = AssetDatabase.LoadAssetAtPath<HandPose>(AssetDatabase.GUIDToAssetPath(guid));
                _handPoses.Add(handPoseAsset.name, handPoseAsset);
            });
        }
        
        [CanBeNull]
        public static HandPose GetHandPose(string handPoseName)
        {
            if (_handPoses != null) return _handPoses.GetValueOrDefault(handPoseName);
            FindHandPoses();
            return _handPoses.GetValueOrDefault(handPoseName);
        }
    }
}
#endif