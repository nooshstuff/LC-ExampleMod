using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;
#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace NooshMod
{
	public class ScrapPatcher
	{
		public static Dictionary<string, ScrapEntry> scrapCatelog = new Dictionary<string, ScrapEntry>();
		public static List<ScrapEntry> _scrapItems = [];
		public static List<string> _allMoons = ["ExperimentationLevel", "AssuranceLevel", "VowLevel", "OffenseLevel", "MarchLevel", "RendLevel", "DineLevel", "TitanLevel"];
		public static List<string> _nonEasyMoons = ["ExperimentationLevel", "AssuranceLevel", "VowLevel", "OffenseLevel", "MarchLevel", "RendLevel", "DineLevel", "TitanLevel"];
		public static void Activate()
		{
			scrapCatelog.Add("GinoScrap", new ScrapEntry("Assets/Scrap/Gino/GinoScrap.asset", 5, _allMoons));
			scrapCatelog.Add("GnomeScrap", new ScrapEntry("Assets/Scrap/Gnome/GnomeScrap.asset", 15, _nonEasyMoons));
			scrapCatelog.Add("DollScrap", new ScrapEntry("Assets/Scrap/Doll/DollScrap.asset", 30, _allMoons));
			scrapCatelog.Add("LanternScrap", new ScrapEntry("Assets/Scrap/Lantern/LanternScrap.asset", 25, _nonEasyMoons));

			_scrapItems.AddRange(scrapCatelog.Values);
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
