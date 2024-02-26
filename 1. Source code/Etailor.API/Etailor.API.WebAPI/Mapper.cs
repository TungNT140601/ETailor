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
            CreateMap<Customer, CustomerAllVM>().ReverseMap();

            CreateMap<MaterialType, MaterialTypeVM>().ReverseMap();
            CreateMap<MaterialType, MaterialTypeAllVM>().ReverseMap();

            CreateMap<Category, CategoryVM>().ReverseMap();
            CreateMap<Category, CategoryAllVM>().ReverseMap();
            CreateMap<Category, CategoryAllTemplateVM>().ReverseMap();

            CreateMap<ComponentType, ComponentTypeVM>().ReverseMap();
            CreateMap<ComponentType, ComponentTypeFormVM>().ReverseMap();

            CreateMap<Component, ComponentVM>().ReverseMap();

            CreateMap<ComponentStage, ComponentStageVM>().ReverseMap();

            CreateMap<Discount, DiscountVM>().ReverseMap();
            CreateMap<Discount, DiscountCreateVM>().ReverseMap();

            CreateMap<Order, OrderVM>().ReverseMap();
            CreateMap<Order, GetOrderVM>().ReverseMap();
            CreateMap<Order, OrderCreateVM>().ReverseMap();
            CreateMap<Order, OrderByCustomerVM>().ReverseMap();


            CreateMap<ProductTemplate, ProductTemplateALLVM>().ReverseMap();
            CreateMap<ProductTemplate, ProductTemplateCreateVM>().ReverseMap();

            CreateMap<Product, ProductVM>().ReverseMap();
            CreateMap<Product, ProductOrderVM>().ReverseMap();

            CreateMap<ProductComponent, ProductComponentOrderVM>().ReverseMap();

            CreateMap<ProductComponentMaterial, ProductComponentMaterialOrderVM>().ReverseMap();

            CreateMap<BodySize, BodySizeVM>().ReverseMap();
            CreateMap<BodySize, CreateUpdateBodySizeVM>().ReverseMap();

            CreateMap<ProfileBody, ProfileBodyVM>().ReverseMap();
            CreateMap<ProfileBody, CreateProfileBodyByStaffVM>().ReverseMap();
            CreateMap<ProfileBody, CreateProfileBodyByCustomerVM>().ReverseMap();
            CreateMap<ProfileBody, UpdateProfileBodyVM>().ReverseMap();

            CreateMap<TemplateStage, TemplateStageCreateVM>().ReverseMap();
            CreateMap<TemplateStage, TemplateStageAllVM>().ReverseMap();

            CreateMap<BodyAttribute, BodyAttributeVM>().ReverseMap();
            CreateMap<BodyAttribute, CreateBodyAttributeVM>().ReverseMap();

            CreateMap<Blog, BlogVM>().ReverseMap();
            CreateMap<Blog, ListOfBlogVM>().ReverseMap();
            CreateMap<Blog, CreateBlogVM>().ReverseMap();
            CreateMap<Blog, UpdateBlogVM>().ReverseMap();

            CreateMap<Material, MaterialVM>().ReverseMap();
        }
    }
}
