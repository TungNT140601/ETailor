using Etailor.API.Repository.EntityModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Interface
{
    public interface ICategoryService
    {
        bool AddCategory(Category category);
        bool UpdateCategory(Category category);
        bool DeleteCategory(string id);
        Category GetCategory(string id);
        IEnumerable<Category> GetCategorys(string? search);
    }
}
