using System;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using ArchiSteamFarm;
using ArchiSteamFarm.Localization;
using ArchiSteamFarm.Core;
using ArchiSteamFarm.Helpers.Json;
using ArchiSteamFarm.IPC.Controllers.Api;
using ArchiSteamFarm.Plugins.Interfaces;
using ArchiSteamFarm.Steam;
using ArchiSteamFarm.Steam.Cards;
using ArchiSteamFarm.Steam.Data;
using ArchiSteamFarm.Steam.Exchange;
using ArchiSteamFarm.Steam.Storage;
using ArchiSteamFarm.Web.GitHub.Data;
using ArchiSteamFarm.Web.GitHub;

namespace ItemDispenser;

[Export(typeof(IPlugin))]

internal sealed class ItemDispenser : IBotTradeOffer2, IBotModules, IGitHubPluginUpdates {
	private static readonly ConcurrentDictionary<Bot, FrozenDictionary<(uint AppID, ulong ContextID), FrozenSet<EAssetType>>> BotSettings = new();

	public string Name => nameof(ItemDispenser);

	public Version Version => typeof(ItemDispenser).Assembly.GetName().Version ?? new Version("0.0.0.0");

	public string RepositoryName => "CatPoweredPlugins/ItemDispenser";

	public async Task<Uri?> GetTargetReleaseURL(Version asfVersion, string asfVariant, bool asfUpdate, bool stable, bool forced) {
		ArgumentNullException.ThrowIfNull(asfVersion);
		ArgumentException.ThrowIfNullOrEmpty(asfVariant);

		if (string.IsNullOrEmpty(RepositoryName)) {
			ASF.ArchiLogger.LogGenericError(string.Format(CultureInfo.CurrentCulture, Strings.WarningFailedWithError, (nameof(RepositoryName))));

			return null;
		}

		ReleaseResponse? releaseResponse = await GitHubService.GetLatestRelease(RepositoryName, stable).ConfigureAwait(false);

		if (releaseResponse == null) {
			return null;
		}

		Version newVersion = new(releaseResponse.Tag);

		if (!(Version.Major == newVersion.Major && Version.Minor == newVersion.Minor && Version.Build == newVersion.Build) && !(asfUpdate || forced)) {
			ASF.ArchiLogger.LogGenericInfo(string.Format(CultureInfo.CurrentCulture, "New {0} plugin version {1} is only compatible with latest ASF version", Name, newVersion));
			return null;
		}


		if (Version >= newVersion & !forced) {
			ASF.ArchiLogger.LogGenericInfo(string.Format(CultureInfo.CurrentCulture, Strings.PluginUpdateNotFound, Name, Version, newVersion));

			return null;
		}

		if (releaseResponse.Assets.Count == 0) {
			ASF.ArchiLogger.LogGenericWarning(string.Format(CultureInfo.CurrentCulture, Strings.PluginUpdateNoAssetFound, Name, Version, newVersion));

			return null;
		}

		ReleaseAsset? asset = await ((IGitHubPluginUpdates) this).GetTargetReleaseAsset(asfVersion, asfVariant, newVersion, releaseResponse.Assets).ConfigureAwait(false);

		if ((asset == null) || !releaseResponse.Assets.Contains(asset)) {
			ASF.ArchiLogger.LogGenericWarning(string.Format(CultureInfo.CurrentCulture, Strings.PluginUpdateNoAssetFound, Name, Version, newVersion));

			return null;
		}

		ASF.ArchiLogger.LogGenericInfo(string.Format(CultureInfo.CurrentCulture, Strings.PluginUpdateFound, Name, Version, newVersion));

		return asset.DownloadURL;
	}

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

		// If we're receiving something in return, and donations are not accepted - ignore
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
