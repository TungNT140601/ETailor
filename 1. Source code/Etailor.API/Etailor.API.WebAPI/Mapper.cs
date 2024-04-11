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
            CreateMap<Staff, StaffTaskVM>().ReverseMap();

            CreateMap<Customer, CusRegis>().ReverseMap();
            CreateMap<Customer, CustomerVM>().ReverseMap();
            CreateMap<Customer, CustomerAllVM>().ReverseMap();
            CreateMap<Customer, CustomerCreateVM>().ReverseMap();

            CreateMap<MaterialType, MaterialTypeVM>().ReverseMap();
            CreateMap<MaterialType, MaterialTypeAllVM>().ReverseMap();
            CreateMap<MaterialType, MaterialTypeTaskDetailVM>().ReverseMap();

            CreateMap<Category, CategoryVM>().ReverseMap();
            CreateMap<Category, CategoryAllVM>().ReverseMap();
            CreateMap<Category, CategoryAllTemplateVM>().ReverseMap();
            CreateMap<Category, CategoryAllTaskVM>().ReverseMap();

            CreateMap<ComponentType, ComponentTypeVM>().ReverseMap();
            CreateMap<ComponentType, ComponentTypeFormVM>().ReverseMap();
            CreateMap<ComponentType, ComponentTypeOrderVM>().ReverseMap();

            CreateMap<Component, ComponentVM>().ReverseMap();
            CreateMap<Component, ComponentOrderVM>().ReverseMap();
            //CreateMap<Component, ComponentDetailVM>().ReverseMap();

            CreateMap<Chat, ChatAllVM>().ReverseMap();

            CreateMap<ChatList, ChatListVM>().ReverseMap();

            CreateMap<ComponentStage, ComponentStageVM>().ReverseMap();

            CreateMap<Discount, DiscountVM>().ReverseMap();
            CreateMap<Discount, DiscountCreateVM>().ReverseMap();
            CreateMap<Discount, DiscountOrderDetailVM>().ReverseMap();

            CreateMap<Order, OrderVM>().ReverseMap();
            CreateMap<Order, GetOrderVM>().ReverseMap();
            CreateMap<Order, OrderCreateVM>().ReverseMap();
            CreateMap<Order, OrderByCustomerVM>().ReverseMap();
            CreateMap<Order, OrderDetailVM>().ReverseMap();

            CreateMap<OrderMaterial, OrderMaterialVM>().ReverseMap();
            CreateMap<OrderMaterial, OrderMaterialUpdateVM>().ReverseMap();


            CreateMap<ProductTemplate, ProductTemplateALLVM>();
            CreateMap<ProductTemplate, ProductTemplateAllTaskVM>();
            CreateMap<ProductTemplateALLVM, ProductTemplate>();
            CreateMap<ProductTemplate, ProductTemplateCreateVM>().ReverseMap();
            CreateMap<ProductTemplate, ProductTemplateUpdateVM>().ReverseMap();
            CreateMap<ProductTemplate, ProductTemplateTaskDetailVM>().ReverseMap();

            CreateMap<Product, ProductVM>().ReverseMap();
            CreateMap<Product, ProductAllTaskVM>().ReverseMap();
            CreateMap<Product, ProductOrderVM>().ReverseMap();
            CreateMap<Product, ProductDetailOrderVM>().ReverseMap();
            CreateMap<Product, ProductListOrderDetailVM>().ReverseMap();

            CreateMap<ProductComponent, ProductComponentOrderVM>().ReverseMap();
            CreateMap<ProductComponent, ProductComponentTaskDetailVM>().ReverseMap();

            CreateMap<ProductBodySize, ProductBodySizeTaskDetailVM>().ReverseMap();

            CreateMap<ProductComponentMaterial, ProductComponentMaterialOrderVM>().ReverseMap();

            CreateMap<BodySize, BodySizeVM>().ReverseMap();
            CreateMap<BodySize, CreateUpdateBodySizeVM>().ReverseMap();
            CreateMap<BodySize, CreateBodySizeVM>().ReverseMap();
            CreateMap<BodySize, BodySizeTaskDetailVM>().ReverseMap();

            CreateMap<ProfileBody, ProfileBodyVM>().ReverseMap();
            CreateMap<ProfileBody, CreateProfileBodyVM>().ReverseMap();
            //CreateMap<ProfileBody, CreateProfileBodyByCustomerVM>().ReverseMap();
            CreateMap<ProfileBody, UpdateProfileBodyByStaffVM>().ReverseMap();
            CreateMap<ProfileBody, UpdateProfileBodyVM>().ReverseMap();
            CreateMap<ProfileBody, GetDetailProfileBodyVM>().ReverseMap();
            CreateMap<ProfileBody, GetAllProfileBodyOfCustomerVM>().ReverseMap();

            CreateMap<BodyAttribute, DetailProfileBody>().ReverseMap();

            CreateMap<TemplateStage, TemplateStageCreateVM>().ReverseMap();
            CreateMap<TemplateStage, TemplateStageAllVM>().ReverseMap();
            CreateMap<TemplateStage, TemplateStageAllTaskVM>().ReverseMap();

            CreateMap<BodyAttribute, BodyAttributeVM>().ReverseMap();
            CreateMap<BodyAttribute, CreateBodyAttributeVM>().ReverseMap();

            CreateMap<Blog, BlogVM>().ReverseMap();
            CreateMap<Blog, ListOfBlogVM>().ReverseMap();
            CreateMap<Blog, CreateBlogVM>().ReverseMap();
            CreateMap<Blog, UpdateBlogVM>().ReverseMap();

            CreateMap<Material, MaterialVM>().ReverseMap();
            CreateMap<Material, MaterialFormVM>().ReverseMap();
            CreateMap<Material, FabricMaterialVM>().ReverseMap();
            CreateMap<Material, FabricMaterialTaskVM>().ReverseMap();

            CreateMap<MaterialCategory, MaterialCategoryVM>().ReverseMap();
            CreateMap<MaterialCategory, CreateMaterialCategoryVM>().ReverseMap();
            CreateMap<MaterialCategory, UpdateMaterialCategoryVM>().ReverseMap();
            CreateMap<MaterialCategory, MaterialCaterogyTaskDetailVM>().ReverseMap();

            CreateMap<Product, TaskListVM>().ReverseMap();
            CreateMap<Product, TaskListByStaffVM>().ReverseMap();
            CreateMap<Product, TaskDetailByStaffVM>().ReverseMap();

            CreateMap<ProductStage, ProductStagesNeedForTask>().ReverseMap();
            CreateMap<ProductStage, ProductStageTaskDetailVM>().ReverseMap();

            CreateMap<Notification, NotificationVM>().ReverseMap();
        }
    }
}
