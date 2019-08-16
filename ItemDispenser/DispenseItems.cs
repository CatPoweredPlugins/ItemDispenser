using System.Collections.Immutable;
using ArchiSteamFarm.Json;
using Newtonsoft.Json;

namespace ItemDispenser {

	public sealed class DispenseItem {
		public static readonly ImmutableHashSet<Steam.Asset.EType> EmptyTypes = ImmutableHashSet<Steam.Asset.EType>.Empty;

		[JsonProperty(Required = Required.DisallowNull)]
		public readonly uint AppID = 0;
		[JsonProperty(Required = Required.DisallowNull)]
		public readonly ulong ContextID = 0;
		[JsonProperty]
		public readonly ImmutableHashSet<Steam.Asset.EType> Types = EmptyTypes;
	}
}
