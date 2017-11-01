namespace Assets.Scripts.Craiel.GDX.AI.Sharp.Pool
{
    using System;
    using Contracts;
    using UnityEngine;

    public class GameObjectPool<T> : TrackedPool<T>
        where T : class, IPoolable
    {
        private GameObject prefab;
        private Func<T, bool> activeUpdateCallback;
        
        private Transform root;

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public void Initialize(GameObject poolPrefab, Func<T, bool> updateCallback, Transform poolRoot = null)
        {
            this.root = poolRoot;

            this.prefab = poolPrefab;
            this.activeUpdateCallback = updateCallback;
        }

        // -------------------------------------------------------------------
        // Protected
        // -------------------------------------------------------------------
        protected override T NewObject()
        {
            GameObject instance = UnityEngine.Object.Instantiate(this.prefab);

            if (this.root != null)
            {
                instance.transform.SetParent(this.root);
            }

            return instance.GetComponentInChildren<T>();
        }

        protected override bool UpdateActiveEntry(T entry)
        {
            return this.activeUpdateCallback(entry);
        }
    }
}
