using Etailor.API.Repository.EntityModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Interface
{
    public interface IDiscountService
    {
        bool CreateDiscount(Discount discount);
        bool UpdateDiscount(Discount discount);
        bool DeleteDiscount(string id);
        Discount GetDiscount(string id);
        IEnumerable<Discount> GetDiscounts (string? search);
    }
}
