using AutoMapper;
using Etailor.API.Repository.EntityModels;
using Etailor.API.WebAPI.ViewModels;

namespace Etailor.API.WebAPI
{
    public class Mapper : Profile
    {
        public Mapper()
        {
            CreateMap<Staff, StaffVM>().ReverseMap();
            CreateMap<Customer, CusRegis>().ReverseMap();
        }
    }
}
