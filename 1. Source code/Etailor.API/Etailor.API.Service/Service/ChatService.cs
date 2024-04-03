using Etailor.API.Repository.EntityModels;
using Etailor.API.Repository.Interface;
using Etailor.API.Repository.StoreProcModels;
using Etailor.API.Service.Interface;
using Etailor.API.Ultity;
using Etailor.API.Ultity.CommonValue;
using Etailor.API.Ultity.CustomException;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Service
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository chatRepository;
        private readonly IChatListRepository chatListRepository;
        private readonly IStaffRepository staffRepository;
        private readonly ICustomerRepository customerRepository;
        private readonly IOrderRepository orderRepository;
        private readonly ISignalRService signalRService;

        public ChatService(IChatRepository chatRepository, IChatListRepository chatListRepository, IStaffRepository staffRepository,
            ICustomerRepository customerRepository, ISignalRService signalRService, IOrderRepository orderRepository)
        {
            this.chatRepository = chatRepository;
            this.chatListRepository = chatListRepository;
            this.staffRepository = staffRepository;
            this.customerRepository = customerRepository;
            this.signalRService = signalRService;
            this.orderRepository = orderRepository;
        }

        public async Task SendChat(string wwwrootPath, string orderId, string? staffId, string? customerId, string? message, List<IFormFile>? images)
        {
            if (string.IsNullOrWhiteSpace(message) && (images == null || images.Count == 0))
            {
                return;
            }

            var order = orderRepository.Get(orderId);
            if (order != null && order.IsActive == true)
            {
                if (!string.IsNullOrEmpty(customerId))
                {
                    var customer = customerRepository.Get(customerId);
                    if (customer != null && customer.IsActive == true)
                    {
                        if (order.CustomerId != customer.Id)
                        {
                            throw new UserException("Khách hàng không phù hợp");
                        }
                    }
                    else
                    {
                        throw new UserException("Khách hàng không tồn tại");
                    }
                }

                if (!string.IsNullOrEmpty(staffId))
                {
                    var staff = staffRepository.Get(staffId);
                    if (staff == null || staff.IsActive == false)
                    {
                        throw new UserException("Nhân viên không tồn tại");
                    }
                }

                if (order.Status >= 1 && order.Status < 7)
                {
                    var orderChats = chatRepository.GetAll(x => x.OrderId == orderId && x.IsActive == true);
                    var orderChat = new Chat();
                    if (orderChats == null || !orderChats.Any())
                    {
                        orderChat = new Chat()
                        {
                            CreatedTime = DateTime.UtcNow.AddHours(7),
                            Id = Ultils.GenGuidString(),
                            IsActive = true,
                            OrderId = orderId,
                            InactiveTime = null
                        };

                        chatRepository.Create(orderChat);
                    }
                    else
                    {
                        orderChat = orderChats.First();
                    }

                    if (images != null && images.Count > 0)
                    {
                        var uploadImageTasks = new List<Task>();
                        var listImages = new List<string>();
                        foreach (var image in images)
                        {
                            uploadImageTasks.Add(Task.Run(async () =>
                            {
                                listImages.Add(await Ultils.UploadImage(wwwrootPath, $"ChatImages/{order.Id}", image, null));
                            }));
                        }
                        await Task.WhenAll(uploadImageTasks);

                        var chatDetailImages = new ChatList()
                        {
                            ChatId = orderChat.Id,
                            FromCus = !string.IsNullOrEmpty(customerId),
                            Id = Ultils.GenGuidString(),
                            InactiveTime = null,
                            IsActive = true,
                            IsRead = false,
                            Message = null,
                            Images = JsonConvert.SerializeObject(listImages),
                            ReadTime = null,
                            ReplierId = !string.IsNullOrEmpty(staffId) ? staffId : null,
                            SendTime = DateTime.UtcNow.AddHours(7)
                        };

                        if (chatListRepository.Create(chatDetailImages))
                        {
                            if (chatDetailImages.FromCus.Value)
                            {
                                await signalRService.CheckMessage(null);
                            }
                            else
                            {
                                await signalRService.CheckMessage(order.CustomerId);
                            }
                        }
                        else
                        {
                            throw new SystemsException("Error When Create Chat List", nameof(ChatService));
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        var chatDetail = new ChatList()
                        {
                            ChatId = orderChat.Id,
                            FromCus = !string.IsNullOrEmpty(customerId),
                            Id = Ultils.GenGuidString(),
                            InactiveTime = null,
                            IsActive = true,
                            IsRead = false,
                            Message = message,
                            ReadTime = null,
                            ReplierId = !string.IsNullOrEmpty(staffId) ? staffId : null,
                            SendTime = DateTime.UtcNow.AddHours(7)
                        };

                        if (chatListRepository.Create(chatDetail))
                        {
                            if (chatDetail.FromCus.Value)
                            {
                                await signalRService.CheckMessage(null);
                            }
                            else
                            {
                                await signalRService.CheckMessage(order.CustomerId);
                            }
                        }
                        else
                        {
                            throw new SystemsException("Error When Create Chat List", nameof(ChatService));
                        }

                    }
                }
                else
                {
                    throw new UserException("Hóa đơn đã hoàn thành.");
                }
            }
            else
            {
                throw new UserException("Không tìm thấy hóa đơn");
            }
        }

        public async Task<Chat> GetChatDetail(string orderId, string role, string id)
        {
            var order = orderRepository.Get(orderId);
            if (order != null && order.IsActive == true)
            {
                if (role == RoleName.CUSTOMER && order.CustomerId != id)
                {
                    return null;
                }

                var orderParam = new SqlParameter("@OrderId", System.Data.SqlDbType.NVarChar);
                orderParam.Value = orderId;
                var roleParam = new SqlParameter("@Role", System.Data.SqlDbType.Int);
                if (role == RoleName.CUSTOMER)
                {
                    roleParam.Value = 3;
                }
                else
                {
                    roleParam.Value = 2;
                }
                var chats = chatRepository.GetAll(x => x.OrderId == orderId && x.IsActive == true);

                if (chats != null && chats.Any())
                {
                    var chat = chats.First();

                    var chatLists = chatListRepository.GetStoreProcedure(StoreProcName.Get_Order_Chat_List, orderParam, roleParam);

                    var tasks = new List<Task>();
                    foreach (var chatList in chatLists)
                    {
                        tasks.Add(Task.Run(async () =>
                        {
                            if (!string.IsNullOrEmpty(chatList.Images))
                            {
                                var listUrl = new List<string>();
                                var listImage = JsonConvert.DeserializeObject<List<string>>(chatList.Images);

                                if (listImage != null && listImage.Any())
                                {
                                    var getImageUrlTasks = new List<Task>();
                                    foreach (var image in listImage)
                                    {
                                        getImageUrlTasks.Add(Task.Run(async () =>
                                        {
                                            listUrl.Add(Ultils.GetUrlImage(image));
                                        }));
                                    }
                                    await Task.WhenAll(getImageUrlTasks);
                                }

                                chatList.Images = JsonConvert.SerializeObject(listUrl);
                            }
                        }));
                    }

                    await Task.WhenAll(tasks);

                    chat.ChatLists = chatLists.ToList();

                    return chat;
                }
            }
            return null;
        }
    }
}
