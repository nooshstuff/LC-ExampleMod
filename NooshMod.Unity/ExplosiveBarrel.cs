using GameNetcodeStuff;
using RuntimeNetcodeRPCValidator;
using Unity.Netcode;
using UnityEngine;

namespace NooshMod.Unity
{
	public class ExplosiveBarrel : GrabbableObject, IHittable
	{
		public bool exploded;
		public bool sendingExplodeRPC;

		void IHittable.Hit(int force, Vector3 hitDirection, PlayerControllerB playerWhoHit, bool playHitSFX) // playerWhoHit = null, playHitSFX = false
		{
			// play Conk
			//check chance
			SetOffLocally();
		}

		public override void OnHitGround()
		{
			Plugin.LogSource.LogInfo($"Fall time = {fallTime}");
			base.OnHitGround();
		}

		public void SetOffLocally()
		{
			if (!exploded)
			{
				Detonate();
				sendingExplodeRPC = true;
				SetOffServerRpc();
			}
		}

		[ServerRpc(RequireOwnership = false)]
		public void SetOffServerRpc()
		{
			SetOffClientRpc();
		}

		[ClientRpc]
		public void SetOffClientRpc()
		{
			if (sendingExplodeRPC) { sendingExplodeRPC = false; }
			else { Detonate(); }
		}

		public void Detonate()
		{
			exploded = true;
			Landmine.SpawnExplosion(base.transform.position + Vector3.up, spawnExplosionEffect: true, 5f, 8f);
			RemoveScrapServerRpc();

		}

		[ServerRpc(RequireOwnership = false)]
		private void RemoveScrapServerRpc()
		{
			grabbable = false;
			grabbableToEnemies = false;
			deactivated = true;
			GameObject.Destroy(this.gameObject);
		}
	}
}
