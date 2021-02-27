namespace ValheimLib.ObjectDBLib
{
    public class CustomRequirement
    {
		public string RequiredItemDropPrefabName;
		internal ItemDrop RequiredItemDrop;

		public int Amount = 1;
		public int AmountPerLevel = 1;

		public bool Recover = true;

        public CustomRequirement(string requiredItemPrefabName)
        {
			RequiredItemDropPrefabName = requiredItemPrefabName;
        }
	}
}
