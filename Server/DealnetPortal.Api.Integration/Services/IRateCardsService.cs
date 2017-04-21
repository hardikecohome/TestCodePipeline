using System.Security.Cryptography.X509Certificates;
using DealnetPortal.Api.Models.Contract;

namespace DealnetPortal.Api.Integration.Services
{
    public interface IRateCardsService
    {
        TierDTO GetRateCardsByDealerId(string id);
    }
}