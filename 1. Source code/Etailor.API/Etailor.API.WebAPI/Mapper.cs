using AutoMapper;
using Etailor.API.Repository.EntityModels;
using Etailor.API.WebAPI.ViewModels;

namespace Etailor.API.WebAPI
{
    public class Mapper : Profile
    {
        public Mapper()
        {
            CreateMap<Staff, StaffCreateVM>().ReverseMap();
            CreateMap<Staff, StaffUpdateVM>().ReverseMap();
            CreateMap<Staff, StaffListVM>().ReverseMap();
            CreateMap<Staff, StaffVM>().ReverseMap();

            CreateMap<Customer, CusRegis>().ReverseMap();
            CreateMap<Customer, CustomerVM>().ReverseMap();

            CreateMap<MaterialType, MaterialTypeVM>().ReverseMap();
            CreateMap<MaterialType, MaterialTypeAllVM>().ReverseMap();


            CreateMap<Category, CategoryVM>().ReverseMap();
            CreateMap<Category, CategoryAllTemplateVM>().ReverseMap();

            CreateMap<ComponentType, ComponentTypeVM>().ReverseMap();
            CreateMap<ComponentType, ComponentTypeFormVM>().ReverseMap();

            CreateMap<Component, ComponentVM>().ReverseMap();

            CreateMap<Discount, DiscountVM>().ReverseMap();
            CreateMap<Order, OrderVM>().ReverseMap();

            CreateMap<ProductTemplate, ProductTemplateALLVM>().ReverseMap();
            CreateMap<ProductTemplate, ProductTemplateCreateVM>().ReverseMap();

            CreateMap<Product, ProductVM>().ReverseMap();

            CreateMap<BodySize, BodySizeVM>().ReverseMap();

            CreateMap<ProfileBody, ProfileBodyVM>().ReverseMap();

            CreateMap<BodyAttribute, BodyAttributeVM>().ReverseMap();

            CreateMap<Blog, BlogVM>().ReverseMap();
            CreateMap<Blog, ListOfBlogVM>().ReverseMap();
        }
    }
}
