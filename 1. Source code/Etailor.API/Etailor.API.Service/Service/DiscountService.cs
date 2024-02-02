using Etailor.API.Repository.EntityModels;
using Etailor.API.Repository.Interface;
using Etailor.API.Repository.Repository;
using Etailor.API.Service.Interface;
using Etailor.API.Ultity.CustomException;
using Etailor.API.Ultity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Service
{
    public class DiscountService : IDiscountService
    {
        private readonly IDiscountRepository discountRepository;
        public DiscountService(IDiscountRepository discountRepository)
        {
            this.discountRepository = discountRepository;
        }

        public bool CreateDiscount(Discount discount)
        {
            discount.Id = Ultils.GenGuidString();
            discount.LastestUpdatedTime = DateTime.Now;
            discount.CreatedTime = DateTime.Now;
            discount.InactiveTime = null;
            discount.IsActive = true;
            return discountRepository.Create(discount);
        }

        public bool UpdateDiscount(Discount discount)
        {
            var existDiscount = discountRepository.Get(discount.Id);
            if (existDiscount != null)
            {
                existDiscount.Name = discount.Name;
                existDiscount.LastestUpdatedTime = DateTime.Now;
                existDiscount.InactiveTime = null;
                existDiscount.IsActive = true;

                return discountRepository.Update(existDiscount.Id, existDiscount);
            }
            else
            {
                throw new UserException("Không tìm thấy loại giảm giá.");
            }
        }

        public bool DeleteDiscount(string id)
        {
            var existDiscount = discountRepository.Get(id);
            if (existDiscount != null)
            {
                existDiscount.LastestUpdatedTime = DateTime.Now;
                existDiscount.InactiveTime = DateTime.Now;
                existDiscount.IsActive = false;
                return discountRepository.Update(existDiscount.Id, existDiscount);
            }
            else
            {
                throw new UserException("Không tìm thấy loại giảm giá.");
            }
        }

        public Discount GetDiscount(string id)
        {
            var discount = discountRepository.Get(id);
            return discount == null ? null : discount.IsActive == true ? discount : null;
        }

        public IEnumerable<Discount> GetDiscounts(string? search)
        {
            return discountRepository.GetAll(x => (search == null || (search != null && x.Name.Trim().ToLower().Contains(search.Trim().ToLower()))) && x.IsActive == true);
        }
    }
}
