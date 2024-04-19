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
using Etailor.API.Repository.StoreProcModels;

namespace Etailor.API.Service.Service
{
    public class DiscountService : IDiscountService
    {
        private readonly IDiscountRepository discountRepository;
        public DiscountService(IDiscountRepository discountRepository)
        {
            this.discountRepository = discountRepository;
        }

        public async Task<bool> CreateDiscount(Discount discount)
        {
            var tasks = new List<Task>();

            tasks.Add(Task.Run(() =>
            {
                discount.Id = Ultils.GenGuidString();
                discount.LastestUpdatedTime = DateTime.UtcNow.AddHours(7);
                discount.CreatedTime = DateTime.UtcNow.AddHours(7);
                discount.InactiveTime = null;
                discount.IsActive = true;
            }));

            tasks.Add(Task.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(discount.Name))
                {
                    throw new UserException("Vui lòng nhập tên mã giảm giá");
                }
            }));

            tasks.Add(Task.Run(() =>
            {
                if (!discount.StartDate.HasValue)
                {
                    throw new UserException("Vui lòng chọn ngày bắt đầu giảm giá");
                }
                else if (!discount.EndDate.HasValue)
                {
                    discount.EndDate = discount.StartDate.Value.AddMonths(1);
                }
                else if (discount.EndDate.Value < discount.StartDate.Value)
                {
                    throw new UserException("Ngày kết thúc không được trước ngày bắt đầu giảm giá");
                }
                discount.StartDate = discount.StartDate.Value.Date;
                discount.EndDate = discount.EndDate.Value.AddDays(1).AddSeconds(-1);

                var intimeDiscounts = discountRepository.GetAll(x => x.EndDate < discount.StartDate && x.StartDate > discount.EndDate && x.IsActive == true);
                if (intimeDiscounts != null && intimeDiscounts.Any())
                {
                    intimeDiscounts = null;
                    throw new UserException("Đã có chương trình giảm giá trong khung thời gian này");
                }
            }));

            tasks.Add(Task.Run(() =>
            {
                if (!discount.ConditionProductMin.HasValue && !discount.ConditionPriceMin.HasValue)
                {
                    throw new UserException("Vui lòng chọn điều kiện giảm giá");
                }
                else if (discount.ConditionProductMin.HasValue && !discount.ConditionPriceMin.HasValue)
                {
                    if (discount.ConditionProductMin <= 0)
                    {
                        throw new UserException("Điều kiện giảm giá không hợp lệ: Số lượng sản phẩm tối thiểu phải lớn hơn 0");
                    }
                }
                else if (!discount.ConditionProductMin.HasValue && discount.ConditionPriceMin.HasValue)
                {
                    if (!discount.ConditionPriceMin.HasValue || discount.ConditionPriceMin < 0)
                    {
                        throw new UserException("Điều kiện giảm giá không hợp lệ: Tổng tiền hóa đơn tối thiểu không hợp lệ");
                    }
                }
            }));

            tasks.Add(Task.Run(() =>
            {
                if (!discount.DiscountPrice.HasValue && !discount.DiscountPercent.HasValue)
                {
                    throw new UserException("Vui lòng chọn số tiền giảm giá");
                }
                else if (discount.DiscountPrice.HasValue && discount.DiscountPercent.HasValue)
                {
                    throw new UserException("Vui lòng chọn 1 phương thức giảm giá");
                }
                else if (discount.DiscountPrice.HasValue && !discount.DiscountPercent.HasValue)
                {
                    if (discount.DiscountPrice <= 0)
                    {
                        throw new UserException("Số tiền giảm giá không hợp lệ: Số tiền giảm giá tối thiểu phải lớn hơn 0");
                    }
                }
                else if (!discount.DiscountPrice.HasValue && discount.DiscountPercent.HasValue)
                {
                    if (discount.DiscountPercent < 0.1 && discount.DiscountPercent > 0.5)
                    {
                        throw new UserException("Số tiền giảm giá không hợp lệ: Số % tiền giảm giá phải từ 10 - 50%");
                    }
                    else if (!discount.ConditionPriceMax.HasValue || discount.ConditionPriceMax <= 0)
                    {
                        throw new UserException("Điều kiện giảm giá không hợp lệ: Số tiền giảm tối đa không hợp lệ");
                    }
                }
            }));

            await Task.WhenAll(tasks);

            return discountRepository.Create(discount);
        }

        public async Task<bool> UpdateDiscount(Discount discount)
        {
            var existDiscount = discountRepository.Get(discount.Id);
            if (existDiscount != null)
            {
                var tasks = new List<Task>();

                tasks.Add(Task.Run(() =>
                {
                    existDiscount.LastestUpdatedTime = DateTime.UtcNow.AddHours(7);
                    existDiscount.InactiveTime = null;
                    existDiscount.IsActive = true;
                }));

                tasks.Add(Task.Run(() =>
                {
                    if (string.IsNullOrWhiteSpace(discount.Name))
                    {
                        throw new UserException("Vui lòng nhập tên mã giảm giá");
                    }
                    else
                    {
                        existDiscount.Name = discount.Name;
                    }
                }));

                tasks.Add(Task.Run(() =>
                {
                    if (!discount.StartDate.HasValue)
                    {
                        throw new UserException("Vui lòng chọn ngày bắt đầu giảm giá");
                    }
                    else if (!discount.EndDate.HasValue)
                    {
                        discount.EndDate = discount.StartDate.Value.AddMonths(1);
                    }
                    else if (discount.EndDate.Value < discount.StartDate.Value)
                    {
                        throw new UserException("Ngày kết thúc không được trước ngày bắt đầu giảm giá");
                    }
                    else
                    {
                        existDiscount.StartDate = discount.StartDate.Value.Date;
                        existDiscount.EndDate = discount.EndDate.Value.AddDays(1).AddSeconds(-1);
                    }

                    var intimeDiscounts = discountRepository.GetAll(x => x.Id != existDiscount.Id && x.EndDate < existDiscount.StartDate && x.StartDate > existDiscount.EndDate && x.IsActive == true);
                    if (intimeDiscounts != null && intimeDiscounts.Any())
                    {
                        intimeDiscounts = null;
                        throw new UserException("Đã có chương trình giảm giá trong khung thời gian này");
                    }
                }));

                tasks.Add(Task.Run(() =>
                {
                    if (!discount.ConditionProductMin.HasValue && !discount.ConditionPriceMin.HasValue)
                    {
                        throw new UserException("Vui lòng chọn điều kiện giảm giá");
                    }
                    else if (discount.ConditionProductMin.HasValue && !discount.ConditionPriceMin.HasValue)
                    {
                        if (discount.ConditionProductMin <= 0)
                        {
                            throw new UserException("Điều kiện giảm giá không hợp lệ: Số lượng sản phẩm tối thiểu phải lớn hơn 0");
                        }
                    }
                    else if (!discount.ConditionProductMin.HasValue && discount.ConditionPriceMin.HasValue)
                    {
                        if (!discount.ConditionPriceMin.HasValue || discount.ConditionPriceMin < 0)
                        {
                            throw new UserException("Điều kiện giảm giá không hợp lệ: Tổng tiền hóa đơn tối thiểu không hợp lệ");
                        }
                    }
                    existDiscount.ConditionPriceMin = discount.ConditionPriceMin;
                    existDiscount.ConditionProductMin = discount.ConditionProductMin;
                }));

                tasks.Add(Task.Run(() =>
                {
                    if (!discount.DiscountPrice.HasValue && !discount.DiscountPercent.HasValue)
                    {
                        throw new UserException("Vui lòng chọn số tiền giảm giá");
                    }
                    else if (discount.DiscountPrice.HasValue && discount.DiscountPercent.HasValue)
                    {
                        throw new UserException("Vui lòng chọn 1 phương thức giảm giá");
                    }
                    else if (discount.DiscountPrice.HasValue && !discount.DiscountPercent.HasValue)
                    {
                        if (discount.DiscountPrice <= 0)
                        {
                            throw new UserException("Số tiền giảm giá không hợp lệ: Số tiền giảm giá tối thiểu phải lớn hơn 0");
                        }
                    }
                    else if (!discount.DiscountPrice.HasValue && discount.DiscountPercent.HasValue)
                    {
                        if (discount.DiscountPercent < 0.1 && discount.DiscountPercent > 0.5)
                        {
                            throw new UserException("Số tiền giảm giá không hợp lệ: Số % tiền giảm giá phải từ 10 - 50%");
                        }
                        else if (!discount.ConditionPriceMax.HasValue || discount.ConditionPriceMax <= 0)
                        {
                            throw new UserException("Điều kiện giảm giá không hợp lệ: Số tiền giảm tối đa không hợp lệ");
                        }
                    }
                    existDiscount.ConditionPriceMax = discount.ConditionPriceMax;
                    existDiscount.DiscountPercent = discount.DiscountPercent;
                    existDiscount.DiscountPrice = discount.DiscountPrice;
                }));

                await Task.WhenAll(tasks);

                return discountRepository.Update(existDiscount.Id, existDiscount);
            }
            else
            {
                throw new UserException("Không tìm thấy giảm giá.");
            }
        }

        public bool DeleteDiscount(string id)
        {
            var existDiscount = discountRepository.Get(id);
            if (existDiscount != null && existDiscount.IsActive == true)
            {
                existDiscount.LastestUpdatedTime = DateTime.UtcNow.AddHours(7);
                existDiscount.InactiveTime = DateTime.UtcNow.AddHours(7);
                existDiscount.IsActive = false;
                return discountRepository.Update(existDiscount.Id, existDiscount);
            }
            else
            {
                throw new UserException("Không tìm thấy giảm giá.");
            }
        }

        public Discount GetDiscount(string id)
        {
            var discount = discountRepository.Get(id);
            return discount == null ? null : discount.IsActive == true ? discount : null;
        }

        public IEnumerable<Discount> GetDiscounts(string? search)
        {
            return discountRepository.GetAll(x => (search == null || (search != null && (x.Name.Trim().ToLower().Contains(search.Trim().ToLower()))) || x.Code.Trim().ToLower().Contains(search.Trim().ToLower())) && x.IsActive == true);
        }

        public IEnumerable<Discount> GetSuitableDiscounts(string orderId)
        {
            var discounts = discountRepository.GetStoreProcedure(StoreProcName.Get_Suitable_Discout_For_Order, new Microsoft.Data.SqlClient.SqlParameter
            {
                DbType = System.Data.DbType.String,
                Value = orderId,
                ParameterName = "@OrderId"
            });
            return discounts;
        }
    }
}
