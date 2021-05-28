using System.Collections.Immutable;
using ArchiSteamFarm.Steam.Data;
using Newtonsoft.Json;

namespace ItemDispenser {

	public sealed class DispenseItem {
		public static readonly ImmutableHashSet<Asset.EType> EmptyTypes = ImmutableHashSet<Asset.EType>.Empty;

		[JsonProperty(Required = Required.Always)]
		public readonly uint AppID;
		[JsonProperty(Required = Required.Always)]
		public readonly ulong ContextID;
		[JsonProperty]
		public readonly ImmutableHashSet<Asset.EType> Types = EmptyTypes;
	}
}
