using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using ValheimLib.Util.Reflection;
using UnityObject = UnityEngine.Object;

namespace ValheimLib
{
    public static class Prefab
    {
        public const string ModdedPrefabsParentName = "ModdedPrefabs";
        public const string MockPrefix = "VLmock_";

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

        public static GameObject InstantiateClone(this GameObject gameObject, string nameToSet, bool zNetRegister = true)
        {
            const char separator = '_';

            var methodBase = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod();
            var id = methodBase.DeclaringType.Assembly.GetName().Name + separator + methodBase.DeclaringType.Name + separator + methodBase.Name;

            var prefab = UnityObject.Instantiate(gameObject, Parent.transform);
            prefab.name = nameToSet + separator + id;

            if (zNetRegister)
            {
                var zNetScene = ZNetScene.instance;
                if (zNetScene)
                {
                    zNetScene.m_namedPrefabs.Add(prefab.name.GetStableHashCode(), prefab);
                }
            }

            return prefab;
        }

        public static UnityObject GetRealPrefabFromMock(UnityObject unityObject, Type mockObjectType)
        {
            if (unityObject)
            {
                var unityObjectName = unityObject.name;
                var isMock = unityObjectName.StartsWith(MockPrefix);
                if (isMock)
                {
                    unityObjectName = unityObjectName.Substring(MockPrefix.Length);
                    return Cache.GetPrefab(mockObjectType, unityObjectName);
                }
            }

            return null;
        }

        public static T GetRealPrefabFromMock<T>(UnityObject unityObject) where T : UnityObject
        {
            return (T)GetRealPrefabFromMock(unityObject, typeof(T));

            GameObject a = new GameObject();
            a.FixReferences();
        }

        // Thanks for not using the Resources folder IronGate
        // There is probably some oddities in there
        public static void FixReferences(this object objectToFix)
        {
            var type = objectToFix.GetType();

            const BindingFlags flags = ReflectionHelper.AllBindingFlags & ~BindingFlags.Static;
            var fields = type.GetFields(flags);

            foreach (var field in fields)
            {
                var fieldType = field.FieldType;

                var isUnityObject = fieldType.IsSameOrSubclass(typeof(UnityObject));
                if (isUnityObject)
                {
                    var mock = (UnityObject)field.GetValue(objectToFix);
                    var realPrefab = GetRealPrefabFromMock(mock, field.FieldType);
                    if (realPrefab)
                    {
                        field.SetValue(objectToFix, realPrefab);
                    }
                }
                else
                {
                    var enumeratedType = fieldType.GetEnumeratedType();
                    var isEnumerableOfUnityObjects = enumeratedType?.IsSameOrSubclass(typeof(UnityObject)) == true;
                    if (isEnumerableOfUnityObjects)
                    {
                        var currentValues = (IEnumerable<UnityObject>)field.GetValue(objectToFix);
                        if (currentValues != null)
                        {
                            var isArray = fieldType.IsArray;
                            var newI = isArray ? (IEnumerable<UnityObject>)Array.CreateInstance(enumeratedType, currentValues.Count()) : (IEnumerable<UnityObject>)Activator.CreateInstance(fieldType);
                            var list = new List<UnityObject>();
                            foreach (var unityObject in currentValues)
                            {
                                var realPrefab = GetRealPrefabFromMock(unityObject, enumeratedType);
                                if (realPrefab)
                                {
                                    list.Add(realPrefab);
                                }
                            }

                            if (list.Count > 0)
                            {
                                if (isArray)
                                {
                                    var toArray = ReflectionHelper.Cache.EnumerableToArray;
                                    var toArrayT = toArray.MakeGenericMethod(enumeratedType);

                                    // mono...
                                    var cast = ReflectionHelper.Cache.EnumerableCast;
                                    var castT = cast.MakeGenericMethod(enumeratedType);
                                    var correctTypeList = castT.Invoke(null, new object[] { list });

                                    var array = toArrayT.Invoke(null, new object[] { correctTypeList });
                                    field.SetValue(objectToFix, array);
                                }
                                else
                                {
                                    field.SetValue(objectToFix, newI.Concat(list));
                                }
                            }
                        }
                    }
                    else if (enumeratedType?.IsClass == true)
                    {
                        var currentValues = (IEnumerable<object>)field.GetValue(objectToFix);
                        foreach (var value in currentValues)
                        {
                            value.FixReferences();
                        }
                    }
                    else if (fieldType.IsClass)
                    {
                        field.GetValue(objectToFix)?.FixReferences();
                    }
                }
            }
        }

        public static void FixReferences(this GameObject gameObject)
        {
            foreach (var component in gameObject.GetComponents<Component>())
            {
                component.FixReferences();
            }
        }

        public static void CloneFields(this GameObject gameObject, GameObject objectToClone)
        {
            const BindingFlags flags = ReflectionHelper.AllBindingFlags;

            var fieldValues = new Dictionary<FieldInfo, object>();
            var origComponents = objectToClone.GetComponentsInChildren<Component>();
            foreach (var origComponent in origComponents)
            {
                foreach (var fieldInfo in origComponent.GetType().GetFields(flags))
                {
                    if (!fieldInfo.IsLiteral && !fieldInfo.IsInitOnly)
                        fieldValues.Add(fieldInfo, fieldInfo.GetValue(origComponent));
                }
            }

            var clonedComponents = gameObject.GetComponentsInChildren<Component>();
            foreach (var clonedComponent in clonedComponents)
            {
                foreach (var fieldInfo in clonedComponent.GetType().GetFields(flags))
                {
                    if (fieldValues.TryGetValue(fieldInfo, out var fieldValue))
                    {
                        fieldInfo.SetValue(clonedComponent, fieldValue);
                    }
                }
            }
        }

        public static class Cache
        {
            private static readonly Dictionary<Type, Dictionary<string, UnityObject>> DictionaryCache =
                new Dictionary<Type, Dictionary<string, UnityObject>>();

            internal static ConditionalWeakTable<Inventory, Container> InventoryToContainer = new ConditionalWeakTable<Inventory, Container>();

            private static void InitCache(Type type, Dictionary<string, UnityObject> map = null)
            {
                map ??= new Dictionary<string, UnityObject>();
                foreach (var unityObject in Resources.FindObjectsOfTypeAll(type))
                {
                    map[unityObject.name] = unityObject;
                }

                DictionaryCache[type] = map;
            }

            public static UnityObject GetPrefab(Type type, string name)
            {
                if (DictionaryCache.TryGetValue(type, out var map))
                {
                    if (map.Count == 0 || !map.Values.First())
                    {
                        InitCache(type, map);
                    }

                    if (map.TryGetValue(name, out var unityObject))
                    {
                        return unityObject;
                    }
                }
                else
                {
                    InitCache(type);
                    return GetPrefab(type, name);
                }

                return null;
            }

            public static T GetPrefab<T>(string name) where T : UnityObject
            {
                return (T)GetPrefab(typeof(T), name);
            }

            public static Dictionary<string, UnityObject> GetPrefabs(Type type)
            {
                if (DictionaryCache.TryGetValue(type, out var map))
                {
                    if (map.Count == 0 || !map.Values.First())
                    {
                        InitCache(type, map);
                    }

                    return map;
                }
                else
                {
                    InitCache(type);
                    return GetPrefabs(type);
                }
            }
        }
    }
}
