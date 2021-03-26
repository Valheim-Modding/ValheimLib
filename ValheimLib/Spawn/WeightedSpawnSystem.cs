using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using ValheimLib.Util;

namespace ValheimLib.Spawn
{
    /// <summary>
    /// This class is disabled for now.
    /// </summary>
    public class WeightedSpawnSystem
    {
        public const float SpawnFrequencyRate = 10f;

        internal static ConditionalWeakTable<SpawnSystem, WeightedSpawnSystem> Instances =
            new ConditionalWeakTable<SpawnSystem, WeightedSpawnSystem>();

        internal WeightedList<WeightedSpawnData, SpawnSystem.SpawnData> Spawners;

        internal WeightedSpawnSystem(List<SpawnSystem.SpawnData> spawners)
        {
            Spawners = new WeightedList<WeightedSpawnData, SpawnSystem.SpawnData>();

            PortToSaneSystem(spawners);
        }

        private void PortToSaneSystem(List<SpawnSystem.SpawnData> spawners)
        {
            foreach (var spawner in spawners)
            {
                var weightedSpawnData = new WeightedSpawnData(spawner);
                Spawners.Add(weightedSpawnData);
            }
        }

        internal static void Init()
        {
            On.SpawnSystem.Awake += CacheInstance;
            On.SpawnSystem.OnDestroy += RemoveInstance;

            IL.SpawnSystem.Awake += ChangeSpawningFrequency;

            On.SpawnSystem.UpdateSpawnList += Update;
        }

        private static void CacheInstance(On.SpawnSystem.orig_Awake orig, SpawnSystem self)
        {
            orig(self);

            Instances.Add(self, new WeightedSpawnSystem(self.m_spawners));
        }

        private static void RemoveInstance(On.SpawnSystem.orig_OnDestroy orig, SpawnSystem self)
        {
            orig(self);

            Instances.Remove(self);
        }

        private static void ChangeSpawningFrequency(ILContext il)
		{
			var cursor = new ILCursor(il);

			if (cursor.TryGotoNext(
				i => i.MatchLdcR4(out _),
				i => i.MatchLdcR4(out _),
				i => i.MatchCallOrCallvirt<MonoBehaviour>(nameof(MonoBehaviour.InvokeRepeating))))
            {
				cursor.Next.Operand = SpawnFrequencyRate;
				cursor.Next.Next.Operand = SpawnFrequencyRate;
            }
		}

		private static void Update(On.SpawnSystem.orig_UpdateSpawnList orig, SpawnSystem self, List<SpawnSystem.SpawnData> spawners, DateTime currentTime, bool eventSpawners)
        {
			UpdateSpawners(self, eventSpawners);
		}

        private static void UpdateSpawners(SpawnSystem self, bool eventSpawners)
        {
            if (Instances.TryGetValue(self, out var goodSpawnSystem))
            {
                var activeSpawnersForBiome = goodSpawnSystem.Spawners.List.Where(spawner => spawner.Item.m_enabled && self.m_heightmap.HaveBiome(spawner.Item.m_biome)).ToList();
                var spawner = goodSpawnSystem.Spawners.GetRandomItem(activeSpawnersForBiome);

                if ((!string.IsNullOrEmpty(spawner.m_requiredGlobalKey) && !ZoneSystem.instance.GetGlobalKey(spawner.m_requiredGlobalKey)) ||
                    (spawner.m_requiredEnvironments.Count > 0 && !EnvMan.instance.IsEnvironment(spawner.m_requiredEnvironments)) ||
                    (!spawner.m_spawnAtDay && EnvMan.instance.IsDay()) ||
                    (!spawner.m_spawnAtNight && EnvMan.instance.IsNight()) ||
                    SpawnSystem.GetNrOfInstances(spawner.m_prefab, Vector3.zero, 0f, eventSpawners, false) >= spawner.m_maxSpawned)
                {
                    return;
                }

                if (self.FindBaseSpawnPoint(spawner, self.m_nearPlayers, out var vector, out var player) &&
                    (spawner.m_spawnDistance <= 0f || !SpawnSystem.HaveInstanceInRange(spawner.m_prefab, vector, spawner.m_spawnDistance)))
                {
                    var groupSize = UnityEngine.Random.Range(spawner.m_groupSizeMin, spawner.m_groupSizeMax + 1);
                    var groupRadius = groupSize > 1 ? spawner.m_groupRadius : 0f;

                    var numberOfSpawnedMonsters = 0;
                    for (var i = 0; i < groupSize * 2; i++)
                    {
                        var insideUnitCircle = UnityEngine.Random.insideUnitCircle;
                        var spawnPoint = vector + new Vector3(insideUnitCircle.x, 0f, insideUnitCircle.y) * groupRadius;
                        if (self.IsSpawnPointGood(spawner, ref spawnPoint))
                        {
                            self.Spawn(spawner, spawnPoint + Vector3.up * spawner.m_groundOffset, eventSpawners);

                            numberOfSpawnedMonsters++;
                            if (numberOfSpawnedMonsters >= groupSize)
                            {
                                break;
                            }
                        }
                    }

                    Log.LogInfo($"Spawned {numberOfSpawnedMonsters} {spawner.m_prefab.name}");
                }
			}
            else
            {
                Log.LogWarning($"Failed to find a spawner list for : {self.name}");
            }
        }
    }

    public class WeightedSpawnData : WeightedItem<SpawnSystem.SpawnData>
    {
        public override float Weight { get => Item.m_spawnChance / Item.m_spawnInterval * WeightedSpawnSystem.SpawnFrequencyRate; set => Item.m_spawnChance = value; }

        public WeightedSpawnData(SpawnSystem.SpawnData item) : base(item) { }
    }
}
