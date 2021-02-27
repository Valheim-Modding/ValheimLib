using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ValheimLib.ObjectDBLib
{
    public static class ObjectDBHelper
    {
        private static readonly List<CustomItem> CustomItems = new List<CustomItem>();
        private static readonly List<CustomRecipe> CustomRecipes = new List<CustomRecipe>();

        public static void Init()
        {
            On.ObjectDB.Awake += AddCustomItems;
        }

        internal static bool IsValid(this ObjectDB self)
        {
            return self.m_items.Count > 0;
        }

        public static bool Add(CustomItem customItem)
        {
            if (customItem.IsValid())
            {
                CustomItems.Add(customItem);

                return true;
            }

            return false;
        }

        public static bool Add(CustomRecipe customRecipe)
        {
            CustomRecipes.Add(customRecipe);

            return true;
        }

        private static void AddCustomItemsPrefabs(this ObjectDB self)
        {
            foreach (var customItem in CustomItems)
            {
                self.m_items.Add(customItem.ItemPrefab);
                Log.LogInfo($"Added custom item : {customItem.ItemDrop.TokenName()}");
            }
        }

        private static void AddCustomRecipes(this ObjectDB self)
        {
            foreach (var customRecipe in CustomRecipes)
            {
                var recipe = ScriptableObject.CreateInstance<Recipe>();
                recipe.m_amount = customRecipe.AmountCrafted;
                recipe.m_craftingStation = customRecipe.CraftingStation;
                recipe.m_enabled = customRecipe.Enabled;
                recipe.m_item = customRecipe.ItemToCraft;
                recipe.m_minStationLevel = customRecipe.MinStationLevel;
                recipe.m_repairStation = customRecipe.RepairStation;

                var requiredPieces = new List<Piece.Requirement>();
                foreach (var customRequirement in customRecipe.CustomRequirements)
                {
                    var pieceRequirement = new Piece.Requirement
                    {
                        m_amount = customRequirement.Amount,
                        m_amountPerLevel = customRequirement.AmountPerLevel,
                        m_recover = customRequirement.Recover,
                        m_resItem = customRequirement.RequiredItemDrop
                    };

                    requiredPieces.Add(pieceRequirement);
                }

                recipe.m_resources = requiredPieces.ToArray();

                self.m_recipes.Add(recipe);
                Log.LogInfo($"Added recipe for : {recipe.m_item.TokenName()}");
            }
        }

        private static void RetrieveRecipesReferences(this ObjectDB self)
        {
            var craftingStations = Prefab.Cache.CraftingStations;

            foreach (var customRecipe in CustomRecipes)
            {
                customRecipe.CraftingStation = craftingStations.FirstOrDefault(craftingStation => craftingStation.name == customRecipe.CraftingStationPrefabName);
                customRecipe.RepairStation = craftingStations.FirstOrDefault(craftingStation => craftingStation.name == customRecipe.RepairStationPrefabName);

                foreach (var customRequirement in customRecipe.CustomRequirements)
                {
                    foreach (var item in self.m_items)
                    {
                        var itemName = item.name;

                        if (customRequirement.RequiredItemDropPrefabName == itemName)
                        {
                            customRequirement.RequiredItemDrop = item.GetComponent<ItemDrop>();
                        }

                        if (!customRecipe.ItemToCraft)
                        {
                            if (customRecipe.ItemToCraftPrefabName == itemName)
                            {
                                customRecipe.ItemToCraft = item.GetComponent<ItemDrop>();
                            }
                        }
                    }
                }
            }
        }

        private static void AddCustomItems(On.ObjectDB.orig_Awake orig, ObjectDB self)
        {
            orig(self);

            if (self.IsValid())
            {
                self.AddCustomItemsPrefabs();
                self.RetrieveRecipesReferences();
                self.AddCustomRecipes();
            }

            self.UpdateItemHashes();
        }
    }
}
