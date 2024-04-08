using AutoMapper;
using Etailor.API.Repository.EntityModels;
using Etailor.API.Service.Interface;
using Etailor.API.Service.Service;
using Etailor.API.Ultity.CommonValue;
using Etailor.API.Ultity.CustomException;
using Etailor.API.WebAPI.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Etailor.API.WebAPI.Controllers
{
    [Route("api/notification")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService notificationService;
        private readonly IStaffService staffService;
        private readonly ICustomerService customerService;
        private readonly IMapper mapper;

        public NotificationsController(INotificationService notificationService, IStaffService staffService
            , ICustomerService customerService, IMapper mapper)
        {
            this.notificationService = notificationService;
            this.staffService = staffService;
            this.customerService = customerService;
            this.mapper = mapper;
        }

        [HttpGet("get-notification")]
        public async Task<IActionResult> GetNotifications()
        {
            try
            {
                var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                if (role == null)
                {
                    return Unauthorized("Chưa đăng nhập");
                }
                else
                {
                    var id = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                    var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                    if ((role == RoleName.CUSTOMER && !customerService.CheckSecerctKey(id, secrectKey)) || (role != RoleName.CUSTOMER && !staffService.CheckSecrectKey(id, secrectKey)))
                    {
                        return Unauthorized("Chưa đăng nhập");
                    }
                    else
                    {
                        var notifications = notificationService.GetNotifications(id, role);

                        return Ok(new
                        {
                            Data = mapper.Map<NotificationVM>(notifications),
                            Unread = notifications.Count(x => x.IsRead == false)
                        });
                    }
                }
            }
            catch (UserException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (SystemsException ex)
            {
                return StatusCode(500, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpGet("get-notification/{id}")]
        public async Task<IActionResult> GetNotification(string id)
        {
            try
            {
                var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                if (role == null)
                {
                    return Unauthorized("Chưa đăng nhập");
                }
                else
                {
                    var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                    var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                    if ((role == RoleName.CUSTOMER && !customerService.CheckSecerctKey(id, secrectKey)) || (role != RoleName.CUSTOMER && !staffService.CheckSecrectKey(id, secrectKey)))
                    {
                        return Unauthorized("Chưa đăng nhập");
                    }
                    else
                    {
                        var notifications = notificationService.GetNotification(id, userId, role);

                        return Ok(mapper.Map<NotificationVM>(notifications));
                    }
                }
            }
            catch (UserException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (SystemsException ex)
            {
                return StatusCode(500, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
