using Scheme.Services.Interfaces;
using Scheme.Services.Responses;
using System;

namespace Scheme.Services.Implementations
{
    public class Market : IMarket
    {
        #region Implemented functions

        public MarketResponse BuyCondition(int drop, decimal average, decimal price)
        {
            var change = GetChange(price, average);

            return new MarketResponse
            {
                ReadyToTrade = average > price
                ? -change >= drop
                : false,
                Change = decimal.Round(change, 2)
            };
        }

        public FundsResponse GetFunds(int funds, decimal balance)
        {
            var result = new FundsResponse();

            if(funds >= 0 && funds <= 100)
            {
                result.AvailFunds = funds * balance / 100;
            }
            else
            {
                result.AvailFunds = balance;
            }

            return result;
        }

        public MarketResponse SellCondition(int rise, decimal average, decimal price)
        {
            var change = GetChange(price, average);

            return new MarketResponse
            {
                ReadyToTrade = price > average
                ? change >= rise
                : false,
                Change = decimal.Round(change, 2)
            };
        }

        public MarketResponse StopLossReached(int stopLose, decimal average, decimal price)
        {
            var change = GetChange(price, average);

            return new MarketResponse
            {
                ReadyToTrade = average > price
                ? -change >= stopLose
                : false,
                Change = decimal.Round(change, 2)
            };
        }

        #endregion

        #region Private

        private decimal GetChange(decimal price, decimal average) 
        {
            return (price / average * 100) - 100;
        }

        #endregion
    }
}
