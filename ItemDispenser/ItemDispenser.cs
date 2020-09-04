using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using ArchiSteamFarm;
using ArchiSteamFarm.Collections;
using ArchiSteamFarm.Json;
using ArchiSteamFarm.Plugins;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ItemDispenser
{
	[Export(typeof(IPlugin))]
	public class ItemDispenser : IBotTradeOffer, IBotModules {

		private readonly ConcurrentDictionary<Bot, ConcurrentHashSet<DispenseItem>> BotSettings = new ConcurrentDictionary<Bot, ConcurrentHashSet<DispenseItem>>();

		public string Name => nameof(ItemDispenser);

		public Version Version => typeof(ItemDispenser).Assembly.GetName().Version ?? new Version("0");

		public async Task<bool> OnBotTradeOffer([NotNull] Bot bot, [NotNull] Steam.TradeOffer tradeOffer) {
			if (tradeOffer == null) {
				ASF.ArchiLogger.LogNullError(nameof(tradeOffer));
				return false;
			}

			//If we receiveing something in return, and donations is not accepted - ignore.
			if (tradeOffer.ItemsToReceiveReadOnly.Count > 0 && !bot.BotConfig.TradingPreferences.HasFlag(BotConfig.ETradingPreferences.AcceptDonations)) {
				return false;
			}
			byte? holdDuration = await bot.GetTradeHoldDuration(tradeOffer.OtherSteamID64, tradeOffer.TradeOfferID).ConfigureAwait(false);

			if (!holdDuration.HasValue) {
				// If we can't get trade hold duration, ignore
				return false;
			}

			// If user has a trade hold, we add extra logic
			if (holdDuration.Value > 0) {
				// If trade hold duration exceeds our max, or user asks for cards with short lifespan, reject the trade
				if ((holdDuration.Value > (ASF.GlobalConfig?.MaxTradeHoldDuration ?? 0)) || tradeOffer.ItemsToGiveReadOnly.Any(item => ((item.Type == Steam.Asset.EType.FoilTradingCard) || (item.Type == Steam.Asset.EType.TradingCard)) && CardsFarmer.SalesBlacklist.Contains(item.RealAppID))) {
					return false;
				}
			}

			//if we can't get settings for this bot for some reason - ignore
			if (!BotSettings.TryGetValue(bot, out ConcurrentHashSet<DispenseItem>? ItemsToDispense)) {
				return false;
			}

			foreach (Steam.Asset item in tradeOffer.ItemsToGiveReadOnly) {
				if (!ItemsToDispense.Any( sample =>
										(sample.AppID == item.AppID) &&
										(sample.ContextID == item.ContextID) &&
									    ((sample.Types.Count > 0) ? sample.Types.Any(type => type == item.Type) : true)
										)) {
					return false;
				}
			}

			return true;
		}

		public void OnLoaded() => ASF.ArchiLogger.LogGenericInfo("Item Dispenser Plugin by Ryzhehvost, powered by ginger cats");


		public void OnBotInitModules([NotNull] Bot bot, [CanBeNull] IReadOnlyDictionary<string, JToken>? additionalConfigProperties) {

			if (additionalConfigProperties == null) {
				BotSettings.AddOrUpdate(bot, new ConcurrentHashSet<DispenseItem>(), (k, v) => new ConcurrentHashSet<DispenseItem>());
				return;
			}

			if (!additionalConfigProperties.TryGetValue("Ryzhehvost.DispenseItems", out JToken? jToken)) {
				BotSettings.AddOrUpdate(bot, new ConcurrentHashSet<DispenseItem>(), (k, v) => new ConcurrentHashSet<DispenseItem>());
				return;
			}

			ConcurrentHashSet <DispenseItem>? dispenseItems;
			try {
				dispenseItems = jToken.Value<JArray>().ToObject<ConcurrentHashSet<DispenseItem>>();
				if (dispenseItems == null){
					bot.ArchiLogger.LogNullError(nameof(dispenseItems));
					return;
				}
				BotSettings.AddOrUpdate(bot, dispenseItems, (k, v) => dispenseItems);
			} catch {
				bot.ArchiLogger.LogGenericError("Item Dispenser configuration is wrong!");
				BotSettings.AddOrUpdate(bot, new ConcurrentHashSet<DispenseItem>(), (k, v) => new ConcurrentHashSet<DispenseItem>());
			}
			return;
		}

	}
}
