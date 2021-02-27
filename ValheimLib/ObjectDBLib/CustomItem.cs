using System;
using UnityEngine;

namespace ValheimLib.ObjectDBLib
{
    public class CustomItem
    {
        public GameObject ItemPrefab;
        public ItemDrop ItemDrop;

        public CustomItem(GameObject itemPrefab)
        {
            ItemPrefab = itemPrefab;

            ItemDrop = itemPrefab.GetComponent<ItemDrop>();
        }

        public bool IsValid()
        {
            return ItemPrefab && ItemDrop.IsValid();
        }
    }

    public static class ItemDropExtension
    {
        public static string TokenName(this ItemDrop self) => self.m_itemData.m_shared.m_name;

        public static bool IsValid(this ItemDrop self)
        {
            var tokenName = self.TokenName();

            if (tokenName[0] == Language.TokenFirstChar)
            {
                return true;
            }
            else
            {
                throw new Exception($"Item name first char should be $ ! (item name : {tokenName})");
            }
        }
    }
}