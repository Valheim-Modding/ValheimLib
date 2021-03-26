using UnityEngine;
using System.Collections.Generic;


namespace ValheimLib.Util
{
    public static class BoneReorder
    {
        private static bool UtilityChanged = false,
            ChestChanged = false,
            HelmetChanged = false,
            LegChanged = false,
            ShoulderChanged = false;

        private static bool Applied = false;
        
        /// <summary>
        /// Corrects any bone mis-orderings caused by unity incorrectly importing ripped assets.
        /// </summary>
        public static void Apply()
        {
            if (!Applied)
            {
                ODB.ObjectDBHelper.OnAfterInit += () =>
                {
                    On.VisEquipment.SetUtilityEquiped += VisEquipmentOnSetUtilityEquiped;
                    On.VisEquipment.SetShoulderEquiped += VisEquipmentOnSetShoulderEquiped;
                    On.VisEquipment.SetChestEquiped += VisEquipmentOnSetChestEquiped;
                    On.VisEquipment.SetHelmetEquiped += VisEquipmentOnSetHelmetEquiped;
                    On.VisEquipment.SetLegEquiped += VisEquipmentOnSetLegEquiped;
                };
            }
        }

        private static bool VisEquipmentOnSetLegEquiped(On.VisEquipment.orig_SetLegEquiped orig, VisEquipment self, int hash)
        {
            var changed = self.m_currentLegItemHash == hash;
            if (changed) LegChanged = false;
            else LegChanged = true;
            var ret = orig(self, hash);
            if(LegChanged && self.m_legItemInstances != null) SetSMRBones(self, hash, self.m_legItemInstances);
            return ret;
        }

        private static bool VisEquipmentOnSetHelmetEquiped(On.VisEquipment.orig_SetHelmetEquiped orig, VisEquipment self, int hash, int hairhash)
        {
            var changed = self.m_currentHelmetItemHash == hash;
            if (changed) HelmetChanged = false;
            else HelmetChanged = true;
            var ret = orig(self, hash, hairhash);
            if(HelmetChanged && self.m_helmetItemInstance != null) SetSMRBones(self, hash, new List<GameObject>{self.m_helmetItemInstance}); //This is a single object instead of a collection, because reasons?
            return ret;
        }

        private static bool VisEquipmentOnSetChestEquiped(On.VisEquipment.orig_SetChestEquiped orig, VisEquipment self, int hash)
        {
            var changed = self.m_currentChestItemHash == hash;
            if (changed) ChestChanged = false;
            else ChestChanged = true;
            var ret = orig(self, hash);
            if(ChestChanged && self.m_chestItemInstances != null) SetSMRBones(self, hash, self.m_chestItemInstances);
            return ret;
        }

        private static bool VisEquipmentOnSetShoulderEquiped(On.VisEquipment.orig_SetShoulderEquiped orig, VisEquipment self, int hash, int variant)
        {
            var changed = self.m_currentShoulderItemHash == hash;
            if (changed) ShoulderChanged = false;
            else ShoulderChanged = true;
            var ret = orig(self, hash, variant);
            if(ShoulderChanged && self.m_shoulderItemInstances != null) SetSMRBones(self, hash, self.m_shoulderItemInstances);
            return ret;
        }
        
        private static bool VisEquipmentOnSetUtilityEquiped(On.VisEquipment.orig_SetUtilityEquiped orig, VisEquipment self, int hash)
        {
            var changed = self.m_currentUtilityItemHash == hash;
            if (changed) UtilityChanged = false;
            else UtilityChanged = true;
            var ret = orig(self, hash);
            if(UtilityChanged && self.m_utilityItemInstances != null) SetSMRBones(self, hash, self.m_utilityItemInstances);
            return ret;
        }
        
        /// <summary>
        /// Reorders incorrect bone ordering caused by importing ripped assets into unity.
        /// Code courtesy of: https://github.com/GoldenJude
        /// </summary>
        /// <param name="ve"></param>
        /// <param name="hash"></param>
        /// <param name="instances"></param>
        private static void SetSMRBones(VisEquipment ve, int hash, List<GameObject> instances)
        {
            Debug.Log($"SMRBones ran");
                Transform skeletonRoot = ve.transform.Find("Visual").Find("Armature").Find("Hips");
                GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(hash);
                if(!skeletonRoot) Debug.Log($"{skeletonRoot} is null.");
                if(!itemPrefab) Debug.Log($"{itemPrefab} is null.");
                int childCount = itemPrefab.transform.childCount;
                int num = 0;
                for (int i = 0; i < childCount; i++)
                {
                    Transform child = itemPrefab.transform.GetChild(i);
                    bool flag = child.name.StartsWith("attach_skin");
                    if (flag)
                    {
                        int num2 = 0;
                        // if(instances == null) Debug.Log($"instance collection was null.");
                        // if(!instances[num]) Debug.Log($"instance was null.");
                        // if(instances[num].GetComponentsInChildren<SkinnedMeshRenderer>() == null) Debug.Log($"SMR was null.");
                        // Debug.Log($"{instances[num].GetComponentsInChildren<SkinnedMeshRenderer>()}");
                         SkinnedMeshRenderer[] componentsInChildren = instances[num].GetComponentsInChildren<SkinnedMeshRenderer>();
                         foreach (SkinnedMeshRenderer smr in child.GetComponentsInChildren<SkinnedMeshRenderer>())
                         {
                             SetBones(componentsInChildren[num2], GetBoneNames(smr), skeletonRoot);
                             num2++;
                         }
                         num++;
                    }
                }
            
        }
        
        /// <summary>
        /// Reorders incorrect bone ordering caused by importing ripped assets into unity.
        /// Code courtesy of: https://github.com/GoldenJude
        /// </summary>
        /// <param name="ve"></param>
        /// <param name="hash"></param>
        /// <param name="instances"></param>
        private static void SetBones(SkinnedMeshRenderer smr, string[] boneNames, Transform skeletonRoot)
        {
            Transform[] array = new Transform[smr.bones.Length];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = _findInChildren(skeletonRoot, boneNames[i]);
            }
            smr.bones = array;
            smr.rootBone = skeletonRoot;
        }
        
        /// <summary>
        /// Returns a list of bone names, given a SkinnedMeshRenderer
        /// Code courtesy of: https://github.com/GoldenJude
        /// </summary>
        /// <param name="ve"></param>
        /// <param name="hash"></param>
        /// <param name="instances"></param>
        private static string[] GetBoneNames(SkinnedMeshRenderer smr)
        {
            List<string> list = new List<string>();
            foreach (Transform transform in smr.bones)
            {
                list.Add(transform.name);
            }
            return list.ToArray();
        }
        
        /// <summary>
        /// Returns a transform matching the given name within the transforms children.
        /// Code courtesy of: https://github.com/GoldenJude
        /// </summary>
        /// <param name="ve"></param>
        /// <param name="hash"></param>
        /// <param name="instances"></param>
        private static Transform _findInChildren(Transform trans, string name)
        {
            bool flag = trans.name == name;
            Transform result;
            if (flag)
            {
                result = trans;
            }
            else
            {
                for (int i = 0; i < trans.childCount; i++)
                {
                    Transform transform = _findInChildren(trans.GetChild(i), name);
                    bool flag2 = transform != null;
                    if (flag2)
                    {
                        return transform;
                    }
                }
                result = null;
            }
            return result;
        }
    }
}