using Binance.Net.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Templates;
using System.Text;
using System.Threading.Tasks;

namespace Scheme.Actions
{
    public class PriceType
    {
        #region Variables

        private IBinanceClient binanceClient;

        #endregion

        #region MyRegion

        public PriceType(IBinanceClient _binanceClient)
        {
            binanceClient = _binanceClient;
        }

        #endregion

        #region Task

        public async Task<PriceResponse> GetPrice(Strategy strategy)
        {
            var result = new PriceResponse();

            if (strategy.PriceType == 0)
            {
                var response = await binanceClient.Spot.Market.GetPriceAsync(strategy.Symbol);

                if(response.Success)
                {
                    result.SetResult(response.Data.Price);
                }
                else
                {
                    result.Message = response.Error.Message;
                }
            }
            else if (strategy.PriceType == 1)
            {
                var response = await binanceClient.Spot.Market.GetCurrentAvgPriceAsync(strategy.Symbol);

                if (response.Success)
                {
                    result.SetResult(response.Data.Price);
                }
                else
                {
                    result.Message = response.Error.Message;
                }
            }
            else
            {
                result.Message = "Ticker definition is wrong!";
            }

            return result;
        }

        #endregion
    }

    public class PriceResponse
    {
        public decimal Price { get; private set; }
        public void SetResult(decimal price)
        {
            Price = price;
        }
        public string Message { get; set; }
        public bool Success => string.IsNullOrWhiteSpace(Message);
    }
}
