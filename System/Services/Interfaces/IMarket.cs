using Scheme.Services.Responses;

namespace Scheme.Services.Interfaces
{
    public interface IMarket
    {
        MarketResponse BuyCondition(int drop, decimal average, decimal price);
        MarketResponse SellCondition(int rise, decimal average, decimal price);
        MarketResponse StopLossReached(int stopLose, decimal average, decimal price);
        FundsResponse GetFunds(int funds, decimal balance);
    }
}
