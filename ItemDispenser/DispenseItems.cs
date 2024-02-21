using System.Collections.Immutable;
using System.Text.Json.Serialization;
using ArchiSteamFarm.Steam.Data;

namespace ItemDispenser {

	public sealed class DispenseItem {
		public static readonly ImmutableHashSet<Asset.EType> EmptyTypes = [];
		[JsonInclude]
		[JsonRequired]
		public readonly uint AppID;
		[JsonInclude]
		[JsonRequired]
		public readonly ulong ContextID;
		[JsonInclude]
		public readonly ImmutableHashSet<Asset.EType> Types = EmptyTypes;
	}
}
