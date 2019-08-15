using System;
using System.Composition;
using System.Threading.Tasks;
using ArchiSteamFarm;
using ArchiSteamFarm.Json;
using ArchiSteamFarm.Plugins;
using JetBrains.Annotations;

namespace ItemDispenser
{
	[Export(typeof(IPlugin))]
	public class ItemDispenser : IBotTradeOffer {
		public string Name => throw new NotImplementedException();

		public Version Version => throw new NotImplementedException();

		public Task<bool> OnBotTradeOffer([NotNull] Bot bot, [NotNull] Steam.TradeOffer tradeOffer) => throw new NotImplementedException();
		public void OnLoaded() => throw new NotImplementedException();
	}
}
