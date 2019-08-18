using System.Collections.Immutable;
using ArchiSteamFarm.Json;
using Newtonsoft.Json;

namespace ItemDispenser {

	public sealed class DispenseItem {
		public static readonly ImmutableHashSet<Steam.Asset.EType> EmptyTypes = ImmutableHashSet<Steam.Asset.EType>.Empty;

		[JsonProperty(Required = Required.Always)]
		public readonly uint AppID;
		[JsonProperty(Required = Required.Always)]
		public readonly ulong ContextID;
		[JsonProperty]
		public readonly ImmutableHashSet<Steam.Asset.EType> Types = EmptyTypes;
	}
}
