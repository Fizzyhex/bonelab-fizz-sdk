#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FizzSDK.Destruction
{
    public class DestructionSkillet : MonoBehaviour
    {
        [SerializeField] private List<DestructionIngredient> ingredients;
        [SerializeField] private GameObject targetGameObject;
        
        public void SaveDishToPrefab()
        {
            var newPrefab = Instantiate(targetGameObject);
            newPrefab.name = targetGameObject.name + "_dish";

            RefreshIngredients();
            
            foreach (var ingredient in ingredients)
            {
                ingredient.UseIngredient(newPrefab);
            }
            
            var prefabPath = $"Assets/{newPrefab.name}.prefab";
            var i = 0;
            
            while (AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)))
            {
                prefabPath = $"Assets/{newPrefab.name}_{i}.prefab";
                i++;
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
    }
}
#endif