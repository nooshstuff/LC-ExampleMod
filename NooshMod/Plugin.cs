using BepInEx.Configuration;
using RuntimeNetcodeRPCValidator;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace NooshMod
{
	[BepInPlugin(GeneratedPluginInfo.Identifier, GeneratedPluginInfo.Name, GeneratedPluginInfo.Version)]
	[BepInDependency(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_VERSION)]
	public class Plugin : BaseUnityPlugin
	{
		internal static AssetBundle? NooshAssets;
		internal static Plugin? Instance;
		internal static BepInEx.Logging.ManualLogSource? log;
		internal static ConfigEntry<bool>? configBigJumpEnabled;
		private NetcodeValidator? netcodeValidator;
	private void Awake()
		{
			if (Instance == null) { Instance = this; }
			log = Logger;
			Assembly OwnAssembly = Assembly.GetExecutingAssembly();
			netcodeValidator = new NetcodeValidator(GeneratedPluginInfo.Identifier);
			netcodeValidator.PatchAll();

		NooshAssets = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(OwnAssembly.Location), "nooshmod"));

			configBigJumpEnabled = Config.Bind("Toggles", "BigJumpEnabled", false, "Enable giga jump");

			Harmony.CreateAndPatchAll(OwnAssembly, GeneratedPluginInfo.Identifier); //this applies everything in this project with HarmonyPatch
			// NOW FOR MONOMOD
			On.GameNetcodeStuff.PlayerControllerB.Update += PlayerControllerB_Update_Funnybusiness;

			ScrapPatcher.Activate();

			Logger.LogInfo($"Plugin {GeneratedPluginInfo.Identifier} is loaded!");
		}
		private void OnDestroy()
		{
			netcodeValidator.Dispose();
		}

	private void PlayerControllerB_Update_Funnybusiness(On.GameNetcodeStuff.PlayerControllerB.orig_Update orig, GameNetcodeStuff.PlayerControllerB self)
		{
			orig(self); // this is like super.update() in other langauges. code's relative location to this is like doing prefix and postfix
			if (self.isSprinting) {
				float downwardsAngle = Vector3.Angle(self.playerEye.transform.forward, Vector3.down); // angle btween the two
				Logger.LogInfo($"Player downwards angle = {downwardsAngle}, Ramming Speed = {(downwardsAngle < 45.0f)}");
				if (downwardsAngle < 45.0f) {
					self.sprintMultiplier = Mathf.Lerp(self.sprintMultiplier, 10.0f, Time.deltaTime * 1f); // funny murder miners (max sprintmult is now 10.0!)
				}
				else { self.sprintMultiplier = Mathf.Lerp(self.sprintMultiplier, 2.25f, Time.deltaTime * 1f); }
			}
			else { self.sprintMultiplier = Mathf.Lerp(self.sprintMultiplier, 1f, 10f * Time.deltaTime); }
		}

		public bool SpawnInItem(string itemname = "GinoScrap", int value = 999)
		{
			Item? item = ScrapPatcher.scrapCatelog[itemname].item;
			if (item == null) { return false; }
			var position = StartOfRound.Instance.allPlayerScripts[0].gameplayCamera.transform.position;
			var obj = GameObject.Instantiate(item.spawnPrefab, position, Quaternion.identity, RoundManager.Instance.spawnedScrapContainer);
			var netObject = obj.GetComponent<NetworkObject>();
			var grabble = obj.GetComponent<GrabbableObject>();
			grabble.transform.rotation = Quaternion.Euler(grabble.itemProperties.restingRotation);
			grabble.fallTime = 0f;
			if (value > 0) { grabble.SetScrapValue(value); }
			netObject.Spawn();
			if (item.isScrap)
			{
				RoundManager.Instance.SyncScrapValuesClientRpc([new NetworkObjectReference(netObject)], [value]);
			}
			return true;
		}
	}
}
