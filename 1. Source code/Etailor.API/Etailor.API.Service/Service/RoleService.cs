using Etailor.API.Repository.EntityModels;
using Etailor.API.Repository.Interface;
using Etailor.API.Service.Interface;
using Etailor.API.Ultity;
using Etailor.API.Ultity.CustomException;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Service
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository roleRepository;

        public RoleService(IRoleRepository roleRepository)
        {
            this.roleRepository = roleRepository;
        }

        public string GetRoleId(string roleName)
        {
            try
            {
                var role = roleRepository.GetAll(x => x.Name.Trim() == roleName.Trim()).FirstOrDefault();
                if (role == null)
                {
                    role = new Role()
                    {
                        Id = Ultils.GenGuidString(),
                        Name = roleName,
                        IsDelete = false
                    };
                    if (roleRepository.Create(role))
                    {
                        return role.Id;
                    }
                    else
                    {
                        throw new SystemsException("Error when add new role");
                    }
                }
                else
                {
                    return role.Id;
                }
            }
            catch (UserException ex)
            {
                throw new UserException(ex.Message);
            }
            catch (SystemsException ex)
            {
                throw new SystemsException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message);
            }
        }

        public IEnumerable<Role> GetRoles()
        {
            try
            {
                return roleRepository.GetAll(x => x.IsDelete == false);
            }
            catch (UserException ex)
            {
                throw new UserException(ex.Message);
            }
            catch (SystemsException ex)
            {
                throw new SystemsException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message);
            }
        }
    }
}
