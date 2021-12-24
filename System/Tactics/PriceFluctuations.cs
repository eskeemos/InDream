using Binance.Net;
using Binance.Net.Enums;
using Binance.Net.Objects.Spot.SpotData;
using CryptoExchange.Net.Objects;
using NLog;
using Quartz;
using Scheme.Actions;
using Scheme.Extensions;
using Scheme.Services.Interfaces;
using Scheme.Statics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Templates;
using System.Threading.Tasks;

namespace System.Tactics
{
    [DisallowConcurrentExecution]
    public class PriceFluctuations : IJob
    {
        #region Variables

        private static readonly Logger logger = LogManager.GetLogger("INDREAM");
        private readonly IStorage storage;
        private readonly IMarket market;

        #endregion

        #region Constructor

        public PriceFluctuations(IStorage _storage, IMarket _market)
        {
            storage = _storage;
            market = _market;
        }

        #endregion

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                Main main = context.JobDetail.JobDataMap["Main"] as Main;
                Strategy strategy = main.Strategies.FirstOrDefault(strategy => strategy.Id == main.StrategyId);

                if (!(strategy is null))
                {
                    storage.SetPath(Path.Combine(strategy.StoragePath, $"{strategy.Symbol}.txt"));

                    using (var client = new BinanceClient())
                    {
                        var priceType = new PriceType(client);
                        var accountInfo = await client.General.GetAccountInfoAsync();

                        if (accountInfo.Success)
                        {
                            var exchangeInfo = await client.Spot.System.GetExchangeInfoAsync();

                            if (exchangeInfo.Success)
                            {
                                var symbol = exchangeInfo.Data.Symbols.FirstOrDefault(symbol => symbol.Name == strategy.Symbol);

                                if (!(symbol is null) && symbol.Status == SymbolStatus.Trading)
                                {
                                    var baseAsset = symbol.BaseAsset;
                                    var quoteAsset = symbol.QuoteAsset;
                                    var priceInfo = await priceType.GetPrice(strategy);

                                    if (priceInfo.Success)
                                    {
                                        var price = priceInfo.Price;

                                        storage.SaveValue(price);

                                        var average = Average.GetAverage(storage.GetValues(), strategy.Average);
                                        var baseBalance = accountInfo.Data.Balances.FirstOrDefault(symbol => symbol.Asset == baseAsset).Free;
                                        baseBalance = market.GetFunds(strategy.Funds, baseBalance).AvailFunds;
                                        var quoteBalance = accountInfo.Data.Balances.FirstOrDefault(symbol => symbol.Asset == quoteAsset).Free;

                                        logger.Info(Log.GetPrice(strategy, price, quoteBalance));
                                        logger.Info(Log.GetAverage(strategy, average));

                                        if (baseBalance > 0.0m && baseBalance > symbol.MinNotionalFilter.MinNotional)
                                        {
                                            // STOP LOSS
                                            var stopLoss = market.StopLossReached(strategy.StopLoss, average, price);

                                            logger.Info(Log.GetStopLossInfo(stopLoss));

                                            if (stopLoss.ReadyToTrade)
                                            {
                                                logger.Info(Log.GetStopLoseReady(price, stopLoss, strategy));

                                                if (!main.Test)
                                                {
                                                    WebCallResult<BinancePlacedOrder> order = null;
                                                    var quantity = BinanceHelpers.ClampQuantity(symbol.LotSizeFilter.MinQuantity, symbol.LotSizeFilter.MaxQuantity, symbol.LotSizeFilter.StepSize, baseBalance);

                                                    if (strategy.SellType == 0)
                                                    {
                                                        var minNational = quantity * price;

                                                        if (minNational > symbol.MinNotionalFilter.MinNotional)
                                                        {
                                                            order = await client.Spot.Order.PlaceOrderAsync(
                                                                strategy.Symbol,
                                                                OrderSide.Sell,
                                                                OrderType.Market,
                                                                quantity: quantity);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        var stopPrice = BinanceHelpers.ClampPrice(symbol.PriceFilter.MinPrice, symbol.PriceFilter.MaxPrice, price);
                                                        var minNational = quantity * stopPrice;

                                                        if (minNational > symbol.MinNotionalFilter.MinNotional)
                                                        {
                                                            order = await client.Spot.Order.PlaceOrderAsync(
                                                                strategy.Symbol,
                                                                OrderSide.Sell,
                                                                OrderType.StopLossLimit,
                                                                quantity: quantity,
                                                                stopPrice: BinanceHelpers.FloorPrice(symbol.PriceFilter.TickSize, stopPrice),
                                                                price: BinanceHelpers.FloorPrice(symbol.PriceFilter.TickSize, stopPrice),
                                                                timeInForce: TimeInForce.GoodTillCancel);
                                                        }
                                                    }

                                                    if (!(order is null))
                                                    {
                                                        if (order.Success)
                                                        {
                                                            logger.Info(Log.SellStopLoseStart(order.Data.OrderId));

                                                            if (order.Data.Fills.AnyAndNotNull())
                                                            {
                                                                foreach (var item in order.Data.Fills)
                                                                {
                                                                    logger.Info(Log.SellStopLoseResult(item));
                                                                }
                                                            }

                                                            logger.Info(Log.SellStopLoseEnd(order.Data.OrderId));
                                                        }
                                                        else
                                                        {
                                                            logger.Warn(order.Error.Message);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    logger.Info(Log.GetTestStopLoseInfo);
                                                }
                                            }

                                            // SELL
                                            var sell = market.SellCondition(strategy.Rise, average, price);

                                            logger.Info(Log.GetSellInfo(sell));

                                            if (sell.ReadyToTrade)
                                            {
                                                logger.Info(Log.GetSellReady(price, sell, strategy));

                                                if (!main.Test)
                                                {
                                                    var quantity = BinanceHelpers.ClampQuantity(symbol.LotSizeFilter.MinQuantity, symbol.LotSizeFilter.MaxQuantity, symbol.LotSizeFilter.StepSize, baseBalance);

                                                    var order = await client.Spot.Order.PlaceOrderAsync(
                                                        strategy.Symbol,
                                                        OrderSide.Sell,
                                                        OrderType.Market,
                                                        quantity: quantity);

                                                    if (order.Success)
                                                    {
                                                        logger.Info(Log.SellStart(order.Data.OrderId));

                                                        if (order.Data.Fills.AnyAndNotNull())
                                                        {
                                                            foreach (var item in order.Data.Fills)
                                                            {
                                                                logger.Info(Log.SellResult(item));
                                                            }
                                                        }

                                                        logger.Info(Log.SellEnd(order.Data.OrderId));
                                                    }
                                                    else
                                                    {
                                                        logger.Info(order.Error.Message);
                                                    }
                                                }
                                                else
                                                {
                                                    logger.Info(Log.GetTestSellInfo);
                                                }
                                            }

                                        }
                                        else
                                        {
                                            logger.Warn(Log.GetBaseBalanceError);
                                        }

                                        if (quoteBalance > 0.0m && quoteBalance > symbol.MinNotionalFilter.MinNotional)
                                        {
                                            // BUY
                                            var buy = market.BuyCondition(strategy.Drop, average, price);

                                            logger.Info(Log.GetBuyInfo(buy));

                                            if (buy.ReadyToTrade)
                                            {
                                                logger.Info(Log.GetBuyReady(price, buy, strategy));

                                                if (!main.Test)
                                                {
                                                    var quantity = BinanceHelpers.ClampQuantity(symbol.LotSizeFilter.MinQuantity, symbol.LotSizeFilter.MaxQuantity, symbol.LotSizeFilter.StepSize, quoteBalance);

                                                    var order = await client.Spot.Order.PlaceOrderAsync(
                                                        strategy.Symbol,
                                                        OrderSide.Buy,
                                                        OrderType.Market,
                                                        quoteOrderQuantity: quoteBalance);

                                                    if (order.Success)
                                                    {
                                                        logger.Info(Log.BuyStart(order.Data.OrderId));

                                                        if (order.Data.Fills.AnyAndNotNull())
                                                        {
                                                            foreach (var item in order.Data.Fills)
                                                            {
                                                                logger.Info(Log.BuyResult(item));
                                                            }
                                                        }

                                                        logger.Info(Log.BuyEnd(order.Data.OrderId));
                                                    }
                                                    else
                                                    {
                                                        logger.Warn(order.Error.Message);
                                                    }
                                                }
                                                else
                                                {
                                                    logger.Info(Log.GetTestBuyInfo);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            logger.Warn(Log.GetMinNationalInfo(quoteAsset, quoteBalance, symbol.MinNotionalFilter.MinNotional));
                                        }
                                    }
                                    else
                                    {
                                        logger.Warn(priceInfo.Message);
                                    }
                                }
                                else
                                {
                                    logger.Warn(Log.WarnSymbol(strategy.Symbol));
                                }
                            }
                            else
                            {
                                logger.Warn(exchangeInfo.Error.Message);
                            }
                        }
                        else
                        {
                            logger.Warn(accountInfo.Error.Message);
                        }
                    }
                }
                else
                {
                    logger.Warn(Log.WarnStrategy);
                }
            }
            catch (Exception e)
            {
                logger.Fatal($"{e.Message}");
            }
        }
    }
}
