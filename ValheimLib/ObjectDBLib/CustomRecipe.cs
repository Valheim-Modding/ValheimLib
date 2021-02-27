namespace ValheimLib.ObjectDBLib
{
	public class CustomRecipe
	{
		public ItemDrop ItemToCraft = null;
		public string ItemToCraftPrefabName = null;

		public CustomRequirement[] CustomRequirements = new CustomRequirement[0];

		public int AmountCrafted = 1;

		public CraftingStation CraftingStation = null;
		public string CraftingStationPrefabName = null;

		public CraftingStation RepairStation = null;
		public string RepairStationPrefabName = null;

		public int MinStationLevel = 1;

		public bool Enabled = true;

		public CustomRecipe(ItemDrop ItemToCraft, CustomRequirement[] customRequirements)
		{
			this.ItemToCraft = ItemToCraft;
			CustomRequirements = customRequirements;
		}

		public CustomRecipe(string itemToCraftPrefabName, CustomRequirement[] customRequirements)
        {
			ItemToCraftPrefabName = itemToCraftPrefabName;
			CustomRequirements = customRequirements;
        }
	}
}
