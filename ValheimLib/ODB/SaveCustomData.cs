namespace ValheimLib.ODB
{
    internal static class SaveCustomData
    {
		internal static void Init()
        {
			On.Inventory.Save += SaveModdedItems;
			//On.Inventory.AddItem_string_int_float_Vector2i_bool_int_int_long_string += CheckIfModdedItem;
		}

        private static void SaveModdedItems(On.Inventory.orig_Save orig, Inventory self, ZPackage pkg)
        {
            orig(self, pkg);

            var localPlayer = Player.m_localPlayer;

            string inventoryOwner = "";

            if (localPlayer.m_inventory == self)
            {
                inventoryOwner = localPlayer.m_name;
            }
            else
            {
                var containers = Prefab.Cache.GetPrefabs(typeof(Container));
                foreach (Container container in containers.Values)
                {
                    if (container.m_inventory == self)
                    {
                        inventoryOwner = container.m_name;
                    }
                }
            }

            foreach (var itemData in self.m_inventory)
            {
                if (itemData.m_dropPrefab)
                {
                    foreach (var customItem in ObjectDBHelper.CustomItems)
                    {
                        if (customItem.ItemDrop.TokenName() == itemData.m_shared.m_name)
                        {
                            SaveCustomItemToFile(inventoryOwner, itemData);

                            break;
                        }
                    }
                }
            }
        }

        /*private static bool CheckIfModdedItem(On.Inventory.orig_AddItem_string_int_float_Vector2i_bool_int_int_long_string orig,
            Inventory self, string name, int stack, float durability, Vector2i pos, bool equiped, int quality, int variant, long crafterID, string crafterName)
        {
            var itemPrefab = ObjectDB.instance.GetItemPrefab(name);
            if (itemPrefab == null)
            {

            }
        }*/

        internal static void SaveCustomItemToFile(string inventoryOwner, ItemDrop.ItemData itemData)
        {
			/*pkg.Write(itemData.m_dropPrefab.name);
			pkg.Write(itemData.m_stack);
			pkg.Write(itemData.m_durability);
			pkg.Write(itemData.m_gridPos);
			pkg.Write(itemData.m_equiped);
			pkg.Write(itemData.m_quality);
			pkg.Write(itemData.m_variant);
			pkg.Write(itemData.m_crafterID);
			pkg.Write(itemData.m_crafterName);*/
        }
    }
}
