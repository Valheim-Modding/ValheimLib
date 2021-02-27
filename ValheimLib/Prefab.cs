using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace ValheimLib
{
    public static class Prefab
    {
        public const string ModdedPrefabsParentName = "ModdedPrefabs";

        private static GameObject _parent;
        public static GameObject Parent
        {
            get
            {
                if (!_parent)
                {
                    _parent = new GameObject(ModdedPrefabsParentName);
                    UnityObject.DontDestroyOnLoad(_parent);
                    _parent.SetActive(false);
                }

                return _parent;
            }
        }

        public static GameObject InstantiateClone(this GameObject gameObject, string nameToSet)
        {
            var prefab = UnityObject.Instantiate(gameObject, Parent.transform);
            prefab.name = nameToSet;

            return prefab;
        }

        public static class Cache
        {
            private static CraftingStation[] _craftingStations = new CraftingStation[0];
            public static CraftingStation[] CraftingStations
            {
                get
                {
                    if (_craftingStations.Length == 0)
                    {
                        _craftingStations = Resources.FindObjectsOfTypeAll<CraftingStation>();
                    }

                    return _craftingStations;
                }
            }

            private static Projectile[] _projectiles = new Projectile[0];
            public static Projectile[] Projectiles
            {
                get
                {
                    if (_projectiles.Length == 0)
                    {
                        _projectiles = Resources.FindObjectsOfTypeAll<Projectile>();
                    }

                    return _projectiles;
                }
            }
        }
    }
}
