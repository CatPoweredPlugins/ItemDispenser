using System;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ArchiSteamFarm.Core;
using ArchiSteamFarm.Helpers.Json;
using ArchiSteamFarm.Plugins.Interfaces;
using ArchiSteamFarm.Steam;
using ArchiSteamFarm.Steam.Cards;
using ArchiSteamFarm.Steam.Data;
using ArchiSteamFarm.Steam.Storage;

namespace ItemDispenser;

[Export(typeof(IPlugin))]
[SuppressMessage("ReSharper", "UnusedType.Global")]
internal sealed class ItemDispenser : IBotTradeOffer, IBotModules {
	private static readonly ConcurrentDictionary<Bot, FrozenDictionary<(uint AppID, ulong ContextID), FrozenSet<Asset.EType>>> BotSettings = new();

	public string Name => nameof(ItemDispenser);

	public Version Version => typeof(ItemDispenser).Assembly.GetName().Version ?? new Version("0.0.0.0");

	public Task OnBotInitModules(Bot bot, IReadOnlyDictionary<string, JsonElement>? additionalConfigProperties) {
		ArgumentNullException.ThrowIfNull(bot);

		if (additionalConfigProperties == null) {
			BotSettings.TryRemove(bot, out _);

			return Task.CompletedTask;
		}

		if (!additionalConfigProperties.TryGetValue("Rudokhvist.DispenseItems", out JsonElement jsonElement)) {
			BotSettings.TryRemove(bot, out _);

			return Task.CompletedTask;
		}

		Dictionary<(uint AppID, ulong ContextID), HashSet<Asset.EType>> botSettings = new();

		try {
			foreach (DispenseItem? dispenseItem in jsonElement.EnumerateArray().Select(static elem => elem.ToJsonObject<DispenseItem>())) {
				if (dispenseItem == null) {
					continue;
				}

				(uint AppID, ulong ContextID) key = (dispenseItem.AppID, dispenseItem.ContextID);

				if (!botSettings.TryGetValue(key, out HashSet<Asset.EType>? types)) {
					types = new HashSet<Asset.EType>();
					botSettings[key] = types;
				}

				types.UnionWith(dispenseItem.Types);
			}

			BotSettings[bot] = botSettings.ToFrozenDictionary(static kv => kv.Key, static kv => kv.Value.ToFrozenSet());
		} catch (Exception e) {
			bot.ArchiLogger.LogGenericException(e);
			bot.ArchiLogger.LogGenericError("Item Dispenser configuration is wrong!");

			BotSettings.TryRemove(bot, out _);
		}

		return Task.CompletedTask;
	}

	public async Task<bool> OnBotTradeOffer(Bot bot, TradeOffer tradeOffer) {
		ArgumentNullException.ThrowIfNull(bot);
		ArgumentNullException.ThrowIfNull(tradeOffer);

		if (!BotSettings.TryGetValue(bot, out FrozenDictionary<(uint AppID, ulong ContextID), FrozenSet<Asset.EType>>? itemsToDispense)) {
			// Settings not declared for this bot, skip overhead
			return false;
		}

		// If we're receiving something in return, and donations is not accepted - ignore
		if ((tradeOffer.ItemsToReceiveReadOnly.Count > 0) && !bot.BotConfig.TradingPreferences.HasFlag(BotConfig.ETradingPreferences.AcceptDonations)) {
			return false;
		}

		byte? holdDuration = await bot.GetTradeHoldDuration(tradeOffer.OtherSteamID64, tradeOffer.TradeOfferID).ConfigureAwait(false);

		// If user has a trade hold, we add extra logic
		// If trade hold duration exceeds our max, or user asks for cards with short lifespan, reject the trade
		// If we can't get trade hold duration, ignore
		switch (holdDuration) {
			case null:
			case > 0 when (holdDuration.Value > (ASF.GlobalConfig?.MaxTradeHoldDuration ?? 0)) || tradeOffer.ItemsToGiveReadOnly.Any(static item => item.Type is Asset.EType.FoilTradingCard or Asset.EType.TradingCard && CardsFarmer.SalesBlacklist.Contains(item.RealAppID)):
				return false;
		}

		return tradeOffer.ItemsToGiveReadOnly.All(item => itemsToDispense.TryGetValue((item.AppID, item.ContextID), out FrozenSet<Asset.EType>? dispense) && ((dispense.Count == 0) || dispense.Contains(item.Type)));
	}

	public Task OnLoaded() {
		ASF.ArchiLogger.LogGenericInfo("Item Dispenser Plugin by Rudokhvist, powered by ginger cats");

		return Task.CompletedTask;
	}
}
