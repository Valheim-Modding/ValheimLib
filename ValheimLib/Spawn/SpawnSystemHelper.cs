using System;
using System.Collections.Generic;
using UnityEngine;
using ValheimLib.Util.Events;

namespace ValheimLib.Spawn
{
    public static class SpawnSystemHelper
    {
        internal static readonly List<SpawnSystem.SpawnData> CustomSpawnData = new List<SpawnSystem.SpawnData>();

        /// <summary>
        /// Event that get fired after the SpawnSystem get init for the first time.
        /// </summary>
        public static Action<SpawnSystem> OnAfterInit;

        internal static void Init()
        {
            On.ZoneSystem.Awake += AddCustomMobs;
        }

        private static void AddCustomMobs(On.ZoneSystem.orig_Awake orig, ZoneSystem self)
        {
            orig(self);

            var spawnSystem = self.m_zoneCtrlPrefab.GetComponent<SpawnSystem>();

            OnAfterInit.SafeInvoke(spawnSystem);
            OnAfterInit = null;
        }
    }
}
