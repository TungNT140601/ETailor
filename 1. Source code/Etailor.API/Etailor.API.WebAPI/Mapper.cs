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

            CreateMap<SkillOfStaff, SkillOfStaffListVM>().ReverseMap();
            
            CreateMap<Skill, SkillListVM>().ReverseMap();
            CreateMap<Skill, SkillCreateVM>().ReverseMap();
            CreateMap<Skill, SkillUpdateVM>().ReverseMap();
            CreateMap<Skill, SkillVM>().ReverseMap();

            CreateMap<Customer, CusRegis>().ReverseMap();
            CreateMap<Customer, CustomerVM>().ReverseMap();

            CreateMap<MaterialType, MaterialTypeVM>().ReverseMap();
            CreateMap<MaterialType, MaterialTypeAllVM>().ReverseMap();


            CreateMap<Category, CategoryVM>().ReverseMap();

            CreateMap<ComponentType, ComponentTypeVM>().ReverseMap();
            CreateMap<ComponentType, ComponentTypeFormVM>().ReverseMap();
        }
    }
}
