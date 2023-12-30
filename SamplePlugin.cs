using UnityEngine;

namespace NooshMod
{
	[BepInPlugin(GeneratedPluginInfo.Identifier, GeneratedPluginInfo.Name, GeneratedPluginInfo.Version)]
	public sealed class SamplePlugin : BaseUnityPlugin
	{
		private void Awake()
		{
			// Plugin startup logic
			Logger.LogInfo($"Plugin {GeneratedPluginInfo.Identifier} is loaded!");
			Harmony.CreateAndPatchAll(System.Reflection.Assembly.GetExecutingAssembly(), GeneratedPluginInfo.Identifier); //this applies everything in this project with HarmonyPatch

			// NOW FOR MONOMOD
			On.GameNetcodeStuff.PlayerControllerB.Update += PlayerControllerB_Update_Funnybusiness; // this is kinda cool and kinda silly
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
	}
}
