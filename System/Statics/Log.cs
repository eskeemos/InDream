using Binance.Net.Objects.Spot.SpotData;
using Scheme.Services.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Templates;
using System.Text;
using System.Threading.Tasks;

namespace Scheme.Statics
{
    public static class Log
    {
        #region General

        public static string GetPrice(Strategy strategy, decimal price, decimal balance)
            => $"Current price for {strategy.Symbol} is {price}, to trade : {balance}";
        public static string GetAverage(Strategy strategy, decimal average)
            => $"Average price for {strategy.Symbol} is {average}";
        public static string WarnStrategy 
            => $"Provided `strategy.Id` is incorrect";
        public static string WarnSymbol(string symbol)
            => $"Provided symbol `{symbol}` is incorrect";
        public static string WarnKeys 
            => $"Provided keys are incorrect";
        public static string GetMinNationalInfo(string asset, decimal balance, decimal minNotional)
            => $"Not enought {asset} ({balance}), needed at least {minNotional}";

        #endregion

        #region Buy

        public static string GetBuyInfo(MarketResponse response)
            => $"BUY : {response.ReadyToTrade}, change : {response.Change}%";
        public static string GetBuyReady(decimal price, MarketResponse response, Strategy strategy)
            => $"Price ({price}) dropped ({response.Change}%) > ({strategy.Drop}%), buying {strategy.Symbol}";
        public static string GetTestBuyInfo 
            => "Buy in test mode";
        public static string BuyStart(long orderId)
            => $"Start buying order : {orderId}";
        public static string BuyEnd(long orderId)
            => $"End buying order : {orderId}";
        public static string BuyResult(BinanceOrderTrade item)
            => $"Order filled : Quantity ({item.Quantity}), Price ({item.Price})";

        #endregion

        #region Sell

        public static string GetSellInfo(MarketResponse response)
            => $"SELL : {response.ReadyToTrade}, change : {response.Change}%";
        public static string GetSellReady(decimal price, MarketResponse response, Strategy strategy)
            => $"Price ({price}) increased ({response.Change}%) > ({strategy.Rise}%), selling {strategy.Symbol}";
        public static string GetTestSellInfo 
            => "Sell in test mode";
        public static string SellStart(long orderId)
            => $"Start selling order : {orderId}";
        public static string SellEnd(long orderId)
            => $"End selling order : {orderId}";
        public static string SellResult(BinanceOrderTrade item)
            => $"Order filled : Quantity ({item.Quantity}), Price ({item.Price})%";
        public static string GetBaseBalanceError
            => "You do not own any balance to sell";
        public static string GetStopLossInfo(MarketResponse response)
            => $"STOP LOSE : {response.ReadyToTrade}, change : {response.Change}%";
        public static string GetStopLoseReady(decimal price, MarketResponse response, Strategy strategy)
            => $"Price ({price}) dropped ({response.Change}%) > ({strategy.StopLoss}%), selling all {strategy.Symbol}";
        public static string GetTestStopLoseInfo
            => "Sell in test mode";
        public static string SellStopLoseStart(long orderId)
            => $"Start (stop lose) selling order : {orderId}";
        public static string SellStopLoseResult(BinanceOrderTrade item)
            => $"Order (stop lose) filled : Quantity ({item.Quantity}), Price ({item.Price})";
        public static string SellStopLoseEnd(long orderId)
            => $"End (stop lose) selling order : {orderId}";

        #endregion
    }
}
