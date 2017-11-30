﻿using UnityEngine;
using System.Text;
using System.IO;
using System;

namespace MSCMP {
#if !PUBLIC_RELEASE
	/// <summary>
	/// Development tools.
	/// </summary>
	class DevTools {
		Vector2 scrollViewVector = new Vector2();
		Texture2D fillText = new Texture2D(1, 1);

		bool devView = false;
		GameObject spawnedGo = null;
		public DevTools() {
			fillText.SetPixel(0, 0, Color.white);
			fillText.wrapMode = TextureWrapMode.Repeat;
			fillText.Apply();
		}

		public void OnGUI(GameObject localPlayer) {
			if (!devView) {
				return;
			}

			foreach (GameObject go in GameObject.FindObjectsOfType<GameObject>()) {

				if (localPlayer) {
					if ((go.transform.position - localPlayer.transform.position).sqrMagnitude > 10) {
						continue;
					}
				}

				//if (go.transform.parent != null) {
				//	continue;
				//}
				Vector3 pos = Camera.main.WorldToScreenPoint(go.transform.position);
				if (pos.z < 0.0f) {
					continue;
				}


				GUI.Label(new Rect(pos.x, Screen.height - pos.y, 500, 20), go.name);
			}

			if (spawnedGo) {
				Transform trans = spawnedGo.GetComponent<Transform>();
				string parentName = trans.parent != null ? trans.parent.name : "(no parent)";
				if (Utils.GetPlaymakerScriptByName(spawnedGo, "LOD")) {
					parentName += " has lod";
				}
				GUI.Label(new Rect(1, 50, 500, 20), "spawnedGo pos: " + trans.position.ToString() + " " + parentName);
			}

			if (localPlayer != null) {
				Transform trans = localPlayer.GetComponent<Transform>();

				GUI.Label(new Rect(1, 30, 500, 20), "Character pos: " + trans.position.ToString());

				GUI.backgroundColor = Color.red;
				scrollViewVector = GUI.BeginScrollView(new Rect(1, 40, 500, 300), scrollViewVector, new Rect(0, 0, 500, 7000));
				int index = 0;
				GUI.color = Color.white;
				Utils.PrintTransformTree(trans, 0, (int level, string text) => {
					GUI.Label(new Rect(level * 10, index * 18, 500, 20), text);
					index++;
				});
				GUI.EndScrollView();

				GUI.color = new Color(0.0f, 0.0f, 0.0f, 0.5f);
				GUI.DrawTexture(new Rect(1, 40, 500, 300), fillText, ScaleMode.StretchToFill, true);
			}
		}

		public void Update() {
			if (Input.GetKeyDown(KeyCode.F3)) {
				devView = !devView;
			}
		}

		public void UpdatePlayer(GameObject localPlayer) {



			// Pseudo AirBrk
			if (Input.GetKeyDown(KeyCode.KeypadPlus) && localPlayer) {
				localPlayer.transform.position = localPlayer.transform.position + Vector3.up * 5.0f;
			}
			if (Input.GetKeyDown(KeyCode.KeypadMinus) && localPlayer) {
				localPlayer.transform.position = localPlayer.transform.position - Vector3.up * 5.0f;
			}
			if (Input.GetKeyDown(KeyCode.Keypad8) && localPlayer) {
				localPlayer.transform.position = localPlayer.transform.position + localPlayer.transform.rotation * Vector3.forward * 5.0f;
			}
			if (Input.GetKeyDown(KeyCode.Keypad2) && localPlayer) {
				localPlayer.transform.position = localPlayer.transform.position - localPlayer.transform.rotation * Vector3.forward * 5.0f;
			}
			if (Input.GetKeyDown(KeyCode.Keypad4) && localPlayer) {
				localPlayer.transform.position = localPlayer.transform.position - localPlayer.transform.rotation * Vector3.right * 5.0f;
			}
			if (Input.GetKeyDown(KeyCode.Keypad6) && localPlayer) {
				localPlayer.transform.position = localPlayer.transform.position + localPlayer.transform.rotation * Vector3.right * 5.0f;
			}

			if (Input.GetKeyDown(KeyCode.G) && localPlayer) {
				PlayMakerFSM fsm = Utils.GetPlaymakerScriptByName(localPlayer, "PlayerFunctions");
				if (fsm != null) {
					fsm.SendEvent("MIDDLEFINGER");
				}
				else {
					GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
					go.transform.position = localPlayer.transform.position + localPlayer.transform.rotation * Vector3.forward * 2.0f;
				}
			}

			if (Input.GetKeyDown(KeyCode.I) && localPlayer) {

				StringBuilder builder = new StringBuilder();
				Utils.PrintTransformTree(localPlayer.transform, 0, (int level, string text) => {

					for (int i = 0; i < level; ++i) builder.Append("    ");
					builder.Append(text + "\n");
				});
				System.IO.File.WriteAllText(Client.GetPath("localPlayer.txt"), builder.ToString());
			}


			if (Input.GetKeyDown(KeyCode.F6) && localPlayer) {


				GameObject prefab = GameObject.Find("JONNEZ ES(Clone)");
				spawnedGo = GameObject.Instantiate(prefab);

				// Remove component that overrides spawn position of JONNEZ.
				PlayMakerFSM fsm = Utils.GetPlaymakerScriptByName(spawnedGo, "LOD");
				GameObject.Destroy(fsm);

				Vector3 direction = localPlayer.transform.rotation * Vector3.forward * 2.0f;
				spawnedGo.transform.position = localPlayer.transform.position + direction;


				/*StringBuilder builder = new StringBuilder();
				PrintTrans(go.transform, 0, (int level, string text) => {

					for (int i = 0; i < level; ++i)	builder.Append("    ");
					builder.Append(text + "\n");
				});
				System.IO.File.WriteAllText("J:\\projects\\MSCMP\\MSCMP\\Debug\\charTree.txt", builder.ToString());*/


			}

			if (Input.GetKeyDown(KeyCode.F5)) {
				DumpWorld();
			}
		}

		public void DumpWorld() {
			GameObject []gos = gos = GameObject.FindObjectsOfType<GameObject>();

			Directory.CreateDirectory(Client.GetPath("WorldDump"));

			StringBuilder builder = new StringBuilder();
			int index = 0;
			foreach (GameObject go in gos) {
				Transform trans = go.GetComponent<Transform>();
				if (trans == null || trans.parent != null) continue;


				StringBuilder bldr = new StringBuilder();
				Utils.PrintTransformTree(trans, 0, (int level, string text) => {

					for (int i = 0; i < level; ++i) bldr.Append("    ");
					bldr.Append(text + "\n");
				});

				string SanitizedName = go.name;
				MPController.logFile.WriteLine(SanitizedName);
				SanitizedName = SanitizedName.Replace(@"\", "_SLASH_");
				MPController.logFile.WriteLine(SanitizedName);
				string dumpFilePath = Client.GetPath("WorldDump/" + SanitizedName + ".txt");

				try {
					System.IO.File.WriteAllText(dumpFilePath, bldr.ToString());
				}
				catch (Exception e) {
					builder.Append("Unable to dump objects: " + SanitizedName + "\n");
					builder.Append(e.Message + "\n");
				}

				builder.Append(go.name + " (" + SanitizedName + "), Trans: " + trans.position.ToString() + "\n");
				++index;
			}

			System.IO.File.WriteAllText(Client.GetPath("WorldDump/dumpLog.txt"), builder.ToString());
		}
	}
#endif
}
