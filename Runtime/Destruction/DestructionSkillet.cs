#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace FizzSDK.Destruction
{
    [AddComponentMenu("FizzSDK/Destruction Toolkit/Destruction Skillet")]
    public class DestructionSkillet : MonoBehaviour
    {
        [SerializeField] private List<DestructionIngredient> ingredients = new();
        [Tooltip("The GameObject that will be saved as a prefab with added ingredients.")]
        public GameObject targetGameObject;
        [Tooltip("If true, ingredients will be added when in play mode for testing purposes.")]
        [SerializeField] private bool cookAtRuntime = false;
        
        public void SaveDishToPrefab(string prefabPath)
        {
            var prefabName = Path.GetFileName(prefabPath);
            
            var newPrefab = Instantiate(targetGameObject);
            newPrefab.name = prefabName;

            RefreshIngredients();
            
            foreach (var ingredient in ingredients)
            {
                ingredient.UseIngredient(newPrefab);
            }
            
            PrefabUtility.SaveAsPrefabAsset(newPrefab, prefabPath);
            
            DestroyImmediate(newPrefab);
            Debug.Log($"Prefab saved to {prefabPath}!");
        }
        
        public void RefreshIngredients()
        {
            var allIngredients = GetComponents<DestructionIngredient>();
            
            foreach (var ingredient in allIngredients)
            {
                if (!ingredients.Contains(ingredient))
                {
                    ingredients.Add(ingredient);
                }
            }
            
            ingredients.RemoveAll(ingredient => !ingredient);
        }

        private void Start()
        {
            if (!cookAtRuntime) return;
            foreach (var ingredient in ingredients)
            {
                ingredient.UseIngredient(targetGameObject);
            }
        }
    }
}
#endif