using System.Collections.Immutable;
using System.Text.Json.Serialization;
using ArchiSteamFarm.Steam.Data;

namespace ItemDispenser {

	public sealed class DispenseItem {
		public static readonly ImmutableHashSet<Asset.EType> EmptyTypes = [];
		[JsonInclude]
		[JsonRequired]
		public uint AppID { get; private init; }
		[JsonInclude]
		[JsonRequired]
		public ulong ContextID { get; private init; }

		[JsonInclude]
		public ImmutableHashSet<Asset.EType> Types { get; private init; } = EmptyTypes;
}
}
