using System;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ArchiSteamFarm.Core;
using ArchiSteamFarm.Helpers.Json;
using ArchiSteamFarm.Plugins.Interfaces;
using ArchiSteamFarm.Steam;
using ArchiSteamFarm.Steam.Cards;
using ArchiSteamFarm.Steam.Data;
using ArchiSteamFarm.Steam.Exchange;
using ArchiSteamFarm.Steam.Storage;

namespace ItemDispenser;

[Export(typeof(IPlugin))]

internal sealed class ItemDispenser : IBotTradeOffer2, IBotModules {
	private static readonly ConcurrentDictionary<Bot, FrozenDictionary<(uint AppID, ulong ContextID), FrozenSet<EAssetType>>> BotSettings = new();

	public string Name => nameof(ItemDispenser);

	public Version Version => typeof(ItemDispenser).Assembly.GetName().Version ?? new Version("0.0.0.0");

	public Task OnBotInitModules(Bot bot, IReadOnlyDictionary<string, JsonElement>? additionalConfigProperties) {
		ArgumentNullException.ThrowIfNull(bot);

		if (additionalConfigProperties == null) {
			_ = BotSettings.TryRemove(bot, out _);

			return Task.CompletedTask;
		}

		if (!additionalConfigProperties.TryGetValue("Rudokhvist.DispenseItems", out JsonElement jsonElement)) {
			_ = BotSettings.TryRemove(bot, out _);

			return Task.CompletedTask;
		}

		Dictionary<(uint AppID, ulong ContextID), HashSet<EAssetType>> botSettings = [];

		try {
			foreach (DispenseItem? dispenseItem in jsonElement.EnumerateArray().Select(static elem => elem.ToJsonObject<DispenseItem>())) {
				if (dispenseItem == null) {
					continue;
				}

				(uint AppID, ulong ContextID) key = (dispenseItem.AppID, dispenseItem.ContextID);

				if (!botSettings.TryGetValue(key, out HashSet<EAssetType>? types)) {
					types = [];
					botSettings[key] = types;
				}

				types.UnionWith(dispenseItem.Types);
			}

			BotSettings[bot] = botSettings.ToFrozenDictionary(static kv => kv.Key, static kv => kv.Value.ToFrozenSet());
		} catch (Exception e) {
			bot.ArchiLogger.LogGenericException(e);
			bot.ArchiLogger.LogGenericError("Item Dispenser configuration is wrong!");

			_ = BotSettings.TryRemove(bot, out _);
		}

		return Task.CompletedTask;
	}

	public async Task<bool> OnBotTradeOffer(Bot bot, TradeOffer tradeOffer, ParseTradeResult.EResult asfresult) {
		ArgumentNullException.ThrowIfNull(bot);
		ArgumentNullException.ThrowIfNull(tradeOffer);

		if (!BotSettings.TryGetValue(bot, out FrozenDictionary<(uint AppID, ulong ContextID), FrozenSet<EAssetType>>? itemsToDispense)) {
			// Settings not declared for this bot, skip overhead
			return false;
		}

		// If we're receiving something in return, and donations is not accepted - ignore
		if ((tradeOffer.ItemsToReceiveReadOnly.Count > 0) && !bot.BotConfig.TradingPreferences.HasFlag(BotConfig.ETradingPreferences.AcceptDonations)) {
			return false;
		}

		byte? holdDuration = await bot.GetTradeHoldDuration(tradeOffer.OtherSteamID64, tradeOffer.TradeOfferID).ConfigureAwait(false);

		return holdDuration != null && (!(holdDuration > 0) || holdDuration.Value <= (ASF.GlobalConfig?.MaxTradeHoldDuration ?? 0)) && !tradeOffer.ItemsToGiveReadOnly.Any(static item => item.Type is EAssetType.FoilTradingCard or EAssetType.TradingCard && CardsFarmer.SalesBlacklist.Contains(item.RealAppID))
&& tradeOffer.ItemsToGiveReadOnly.All(item => itemsToDispense.TryGetValue((item.AppID, item.ContextID), out FrozenSet<EAssetType>? dispense) && ((dispense.Count == 0) || dispense.Contains(item.Type)));
	}

	public Task OnLoaded() {
		ASF.ArchiLogger.LogGenericInfo("Item Dispenser Plugin by Rudokhvist, powered by ginger cats");

		return Task.CompletedTask;
	}
}
