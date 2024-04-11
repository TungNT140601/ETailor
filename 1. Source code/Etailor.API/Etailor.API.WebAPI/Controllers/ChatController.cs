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
    [Route("api/chat")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IStaffService staffService;
        private readonly ICustomerService customerService;
        private readonly IChatService chatService;
        private readonly IMapper mapper;
        private readonly string _wwwrootPath;

        public ChatController(IStaffService staffService, ICustomerService customerService, IChatService chatService, IWebHostEnvironment webHost, IMapper mapper)
        {
            this.staffService = staffService;
            this.customerService = customerService;
            this.chatService = chatService;
            _wwwrootPath = webHost.WebRootPath;
            this.mapper = mapper;
        }

        [HttpGet("order/{orderId}")]
        public async Task<IActionResult> GetChatDetail(string orderId)
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
                        var chat = await chatService.GetChatDetail(orderId, role, id);

                        if (chat != null)
                        {
                            return Ok(mapper.Map<ChatAllVM>(chat));
                        }
                        else
                        {
                            return Ok(new ChatAllVM()
                            {
                                ChatLists = new List<ChatListVM>(),
                                CreatedTime = DateTime.Now,
                                OrderId = orderId
                            });
                        }
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

        [HttpPost("order/{orderId}/send")]
        public async Task<IActionResult> SendChat(string orderId, [FromForm] ChatVM chat)
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
                        await chatService.SendChat(_wwwrootPath, orderId, role != RoleName.CUSTOMER ? id : null, role == RoleName.CUSTOMER ? id : null, chat.Message, chat.MessageImages);

                        return Ok();
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
