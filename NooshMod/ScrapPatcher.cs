using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;
#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace NooshMod
{
	internal class ScrapPatcher
	{
		public static List<ScrapEntry> _scrapItems = new List<ScrapEntry>();
		public static List<string> _allMoons = new List<string>() { "ExperimentationLevel", "AssuranceLevel", "VowLevel", "OffenseLevel", "MarchLevel", "RendLevel", "DineLevel", "TitanLevel" };
		public static void Activate()
		{
			_scrapItems.Add(new ScrapEntry("Assets/Scrap/Gino/GinoScrap.asset", 5, _allMoons));
			//Activate Patches for Scrap Items
			On.GameNetworkManager.Start += GameNetworkManager_Start;
			On.StartOfRound.Awake += StartOfRound_Awake;
		}

		private static void GameNetworkManager_Start(On.GameNetworkManager.orig_Start orig, GameNetworkManager self)
		{
			foreach (ScrapEntry scrap in _scrapItems)
			{
				self.GetComponent<NetworkManager>().AddNetworkPrefab(scrap.item.spawnPrefab);
			}
		}

		private static void StartOfRound_Awake(On.StartOfRound.orig_Awake orig, StartOfRound self)
		{
			orig(self);

			foreach (SelectableLevel level in self.levels) 
			{
				foreach (ScrapEntry scrap in _scrapItems)
				{
					if (level.spawnableScrap.Any(x => x.spawnableItem == scrap.item)) {continue;}
					if (scrap.levels.Contains(level.name))
					{
						level.spawnableScrap.Add(new SpawnableItemWithRarity(){spawnableItem = scrap.item, rarity = scrap.rarity});
					}
				}
			}

			foreach (ScrapEntry scrap in _scrapItems)
			{
				if (!self.allItemsList.itemsList.Contains(scrap.item))
				{
					Plugin.log.LogInfo($"Registered item: {scrap.item.itemName}");

					self.allItemsList.itemsList.Add(scrap.item);
				}
			}
		}
	}

	public struct ScrapEntry
	{
		public ScrapEntry(string assetpath, int rarity, List<string> levels) : this()
		{

			item = Plugin.NooshAssets.LoadAsset<Item>(assetpath); ;
			this.rarity = rarity;
			this.levels = levels;
		}
		internal Item? item;
		internal int rarity;
		internal List<string> levels;
	}
}
