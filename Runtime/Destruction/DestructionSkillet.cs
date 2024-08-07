﻿#if UNITY_EDITOR
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
        [Tooltip("The GameObject that will be saved as a prefab with added ingredients.")]
        public GameObject targetGameObject;
        
        [Tooltip("If true, ingredients will be added when in play mode for testing purposes.")]
        
        [SerializeField] private bool cookAtRuntime = false;
        
        [HideInInspector] public string savedPrefabPath = "";

        [SerializeField] public List<DestructionIngredient> ingredients = new();
        
        public GameObject SaveDishToPrefab(string prefabPath)
        {
            var prefabName = Path.GetFileName(prefabPath);
            
            var newPrefab = Instantiate(targetGameObject);
            newPrefab.name = prefabName;
            
            foreach (var ingredient in ingredients)
            {
                ingredient.UseIngredient(newPrefab);
            }
            
            PrefabUtility.SaveAsPrefabAsset(newPrefab, prefabPath);
            
            DestroyImmediate(newPrefab);
            Debug.Log($"Prefab saved to {prefabPath}!");
            
            return newPrefab;
        }

        public GameObject SaveDishIntoScene()
        {
            var newGameObject = Instantiate(targetGameObject);
            newGameObject.name = $"{targetGameObject.name}_destructible";
            
            foreach (var ingredient in ingredients)
            {
                ingredient.UseIngredient(newGameObject);
            }
            
            newGameObject.transform.SetPositionAndRotation(targetGameObject.transform.position,
                targetGameObject.transform.rotation);
            
            return newGameObject;
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