using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FizzSDK.Utils
{
    internal static class SearchUtils
    {
        public static List<T> FindAllInContainers<T>(List<GameObject> containers) where T : Component
        {
            List<T> matches = new();

            foreach (GameObject container in containers)
            {
                matches.AddRange(container.GetComponentsInChildren<T>());
            }

            return matches;
        }

        public static List<GameObject> GetRootGameObjects(List<GameObject> gameObjects)
        {
            List<GameObject> rootGameObjects = new();

            foreach (GameObject gameObject in gameObjects)
            {
                GameObject rootGameObject = gameObject.transform.root.gameObject;

                if (!rootGameObjects.Contains(rootGameObject))
                {
                    rootGameObjects.Add(rootGameObject);
                }
            }

            return rootGameObjects;
        }
    }
}