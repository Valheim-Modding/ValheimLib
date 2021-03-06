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

    public static class MockRequirement
    {
        public static Piece.Requirement Create(string name, int amount = 1, bool recover = true)
        {
            var requirement = new Piece.Requirement
            {
                m_recover = recover,
                m_amount = amount
            };

            requirement.m_resItem = Mock<ItemDrop>.Create(name);

            return requirement;
        }
    }
}