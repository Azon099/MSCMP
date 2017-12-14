﻿using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using System.Collections.Generic;
using UnityEngine;

namespace MSCMP.Game {
	/// <summary>
	/// Database containing prefabs of all pickupables.
	/// </summary>
	class GamePickupableDatabase {
		static GamePickupableDatabase instance;
		public static GamePickupableDatabase Instance {
			get {
				return instance;
			}
		}

		public GamePickupableDatabase() {
			instance = this;

			GameCallbacks.onPlayMakerObjectCreate += (GameObject instance, GameObject prefab) => {
				PrefabDesc descriptor = GetPrefabDesc(prefab);
				if (descriptor != null) {
					var metaDataComponent = instance.AddComponent<Components.PickupableMetaDataComponent>();
					metaDataComponent.prefabId = descriptor.id;

					Logger.Log($"Pickupable has been spawned. ({instance.name})");
				}
			};
		}
		~GamePickupableDatabase() {
			instance = null;
		}

		/// <summary>
		/// Pickupable prefab descriptor.
		/// </summary>
		public class PrefabDesc {
			/// <summary>
			/// The unique id of the prefab.
			/// </summary>
			public int id;

			/// <summary>
			/// Prefab game object.
			/// </summary>
			public GameObject gameObject;

			/// <summary>
			/// Spawn new instance of the given pickupable at given world position.
			/// </summary>
			/// <param name="position">The position where to spawn pickupable at.</param>
			/// <param name="rotation">The rotation to apply on spawned pickupable.</param>
			/// <returns>Newly spawned pickupable game object.</returns>
			public GameObject Spawn(Vector3 position, Quaternion rotation) {
				// HACK: Jonnez is already spawned and there can be only one of it.
				if (gameObject.name.StartsWith("JONNEZ ES")) {
					return GameObject.Find("JONNEZ ES(Clone)");
				}

				GameObject pickupable = (GameObject)Object.Instantiate(gameObject, position, rotation);
				pickupable.SetActive(true);
				pickupable.transform.SetParent(null);

				// Disable loading code on all spawned pickupables.

				PlayMakerFSM fsm = Utils.GetPlaymakerScriptByName(pickupable, "Use");
				if (fsm != null) {
					FsmState loadState = fsm.Fsm.GetState("Load");
					if (loadState != null) {
						var action = new SendEvent();
						action.eventTarget = new FsmEventTarget();
						action.eventTarget.excludeSelf = false;
						action.eventTarget.target = FsmEventTarget.EventTarget.Self;
						action.sendEvent = fsm.Fsm.GetEvent("FINISHED");
						PlayMakerUtils.AddNewAction(loadState, action);

						Logger.Log("Installed skip load hack for prefab " + pickupable.name);
					} else {
						Logger.Log("Failed to find state on " + pickupable.name);
					}

				}

				return pickupable;
			}
		}

		/// <summary>
		/// List containing prefabs.
		/// </summary>
		List<PrefabDesc> prefabs = new List<PrefabDesc>();

		/// <summary>
		/// Collect all pickup-ables from game world.
		/// </summary>
		/// <param name="active">Should it return only active pickupables?</param>
		/// <returns>List containing all pickupable instances in game world.</returns>
		public List<GameObject> CollectAllPickupables(bool active) {
			GameObject []gos = null;
			if (active) {
				gos = Object.FindObjectsOfType<GameObject>();
			}
			else {
				gos = Resources.FindObjectsOfTypeAll<GameObject>();
			}
			List<GameObject> pickupables = new List<GameObject>();
			foreach (var go in gos) {
				if (!IsPickupable(go)) {
					continue;
				}

				if (go.transform.root == MPController.Instance.transform) {
					// Logger.Log($"Skipping {go.name} (id: {go.GetInstanceID()}, hideFlags: {go.hideFlags}, parent: {go.transform.parent})");
					continue;
				}

				pickupables.Add(go);
				// Logger.Log($"Prefab found {go.name} (id: {go.GetInstanceID()}, hideFlags: {go.hideFlags}, parent: {go.transform.parent})");
			}
			return pickupables;
		}

		/// <summary>
		/// Rebuild pickupables database.
		/// </summary>
		public void Rebuild() {
			if (prefabs.Count > 0) {
				return;
			}
			var pickupables = CollectAllPickupables(false);

			prefabs.Clear();

			foreach (var pickupable in pickupables) {
				int prefabId = prefabs.Count;
				var metaDataComponent = pickupable.AddComponent<Components.PickupableMetaDataComponent>();
				metaDataComponent.prefabId = prefabId;

				var prefab = Object.Instantiate(pickupable);
				prefab.SetActive(false);
				prefab.transform.SetParent(MPController.Instance.transform);

				Logger.Log($"Registering {prefab.name} ({prefab.GetInstanceID()}) into pickupable database. (Prefab ID: {prefabId}, Source pickupable id: {pickupable.GetInstanceID()})");

				PrefabDesc desc = new PrefabDesc();
				desc.gameObject = prefab;
				desc.id = prefabId;


				prefabs.Add(desc);
			}
		}

		/// <summary>
		/// Get pickupable prefab by it's id.
		/// </summary>
		/// <param name="prefabId">The id of the prefab to get.</param>
		/// <returns>The pickupable prefab descriptor.</returns>
		public PrefabDesc GetPickupablePrefab(int prefabId) {
			if (prefabId < prefabs.Count) {
				return prefabs[prefabId];
			}
			return null;
		}

		/// <summary>
		/// Get prefab descriptor by prefab game object.
		/// </summary>
		/// <param name="prefab">The prefab game object.</param>
		/// <returns>Prefab descriptor if given prefab is valid.</returns>
		public PrefabDesc GetPrefabDesc(GameObject prefab) {
			foreach (var desc in prefabs) {
				if (desc.gameObject == prefab) {
					return desc;
				}
			}
			return null;
		}

		/// <summary>
		/// Check if given game object is pickupable.
		/// </summary>
		/// <param name="gameObject">The game object to check.</param>
		/// <returns>true if given game object is pickupable, false otherwise</returns>
		static public bool IsPickupable(GameObject gameObject) {
			if (!gameObject.CompareTag("PART") && !gameObject.CompareTag("ITEM")) {
				return false;
			}
			//Transform parent = gameObject.transform.parent;
			//if (parent && IsPickupable(parent.gameObject)) {
			//	return false;
			//}

			if (!gameObject.GetComponent<Rigidbody>()) {
				return false;
			}
			return true;
		}

	}
}
