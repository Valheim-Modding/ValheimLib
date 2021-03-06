﻿using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace ValheimLib.ODB
{
    public class CustomRecipe
    {
        public Recipe Recipe;
        public bool FixReference;
        public bool FixRequirementReferences;

        public CustomRecipe(Recipe recipe, bool fixReference, bool fixRequirementReferences)
        {
            Recipe = recipe;
            FixReference = fixReference;
            FixRequirementReferences = fixRequirementReferences;
        }
    }

    public static class MockCraftingStation
    {
        public static CraftingStation Create(string name)
        {
            var g = new GameObject(name + "_" + nameof(MockCraftingStation));
            UnityObject.DontDestroyOnLoad(g);
            g.transform.SetParent(Prefab.Parent.transform);
            g.SetActive(false);

            var craftingStation = g.AddComponent<CraftingStation>();
            craftingStation.name = Prefab.MockPrefix + name;

            return craftingStation;
        }
    }

    public static class MockItemDrop
    {
        public static ItemDrop Create(string name)
        {
            var g = new GameObject(name + "_" + nameof(MockItemDrop));
            UnityObject.DontDestroyOnLoad(g);
            g.transform.SetParent(Prefab.Parent.transform);
            g.SetActive(false);

            var itemDrop = g.AddComponent<ItemDrop>();
            itemDrop.name = Prefab.MockPrefix + name;

            return itemDrop;
        }
    }

    public static class MockRequirement
    {
        public static Piece.Requirement Create(string name, int amount = 1, bool recover = true)
        {
            var requirement = new Piece.Requirement
            {
                m_recover = recover,
                m_amount = amount
            };

            requirement.m_resItem = MockItemDrop.Create(name);

            return requirement;
        }
    }
}