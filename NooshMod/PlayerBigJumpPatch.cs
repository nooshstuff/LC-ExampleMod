using UnityEngine; // you would need this to do stuff like get Time.deltatime
using GameNetcodeStuff;

namespace NooshMod
{
	[HarmonyPatch(typeof(PlayerControllerB))]	// if you didnt do this up here you could do it on the indivual functions. when you do this you cant patch other classes
	internal class PlayerBigJumpPatch // having classes related to patches have the name Patch might be useful
	{
		[HarmonyPostfix] // theres also [HarmonyPrefix]
		[HarmonyPatch(nameof(PlayerControllerB.Update))] // every patch needs a class type and class name. this could also be a string but whatever too complicated
		public static void ApplyBigJumpValue( PlayerControllerB __instance) // double underscore is a naming convention https://harmony.pardeike.net/articles/patching-injections.html
		{
			//var dt = Time.deltaTime;
			if (Plugin.configBigJumpEnabled != null)
			{
				if (Plugin.configBigJumpEnabled.Value)
				{
					__instance.jumpForce = 100; // this is a float but its implicitly cast. remember to use f when you're using decimals in math
					return;
				}
			}
			__instance.jumpForce = 5;
		}
	}
}
