using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using ArchiSteamFarm.Steam.Data;

namespace ItemDispenser;

[SuppressMessage("ReSharper", "ClassCannotBeInstantiated")]
internal sealed class DispenseItem {
	[JsonInclude]
	[JsonRequired]
	internal uint AppID { get; private init; }
		
	[JsonInclude]
	[JsonRequired]
	internal ulong ContextID { get; private init; }

	[JsonInclude]
	internal ImmutableHashSet<Asset.EType> Types { get; private init; } = ImmutableHashSet<Asset.EType>.Empty;
	
	[JsonConstructor]
	private DispenseItem() { }
}