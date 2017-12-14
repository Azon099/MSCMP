﻿namespace MSCMPMessages.Messages {
	[NetMessageDesc(MessageIds.PickupableSpawn)]
	class PickupableSpawnMessage {
		/// <summary>
		/// Network id of the pickupable to spawn.
		/// </summary>
		ushort				id;

		/// <summary>
		/// The prefab used to create given pickupable.
		/// </summary>
		int					prefabId;

		/// <summary>
		/// The spawn transformation of the pickupable.
		/// </summary>
		TransformMessage	transform;
	}
}
