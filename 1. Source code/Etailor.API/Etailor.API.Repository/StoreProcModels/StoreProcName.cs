using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Repository.StoreProcModels
{
    public static class StoreProcName
    {
        public static string Get_Order_Dashboard = "GetOrderDashboard";
        public static string Get_Staff_With_Total_Task = "GetStaffWithTotalTask";
        public static string Get_Order_Chat = "GetOrderChat";
        public static string Get_Order_Chat_List = "GetOrderChatList";
        public static string Get_Total_Fabric_Material_Common_Used = "GetTotalFabricMaterialCommonUsed";
        public static string Get_Template_Dashboard = "GetTemplateDashboard";
        public static string Get_Active_Orders = "GetActiveOrders";
        public static string Get_Active_Orders_Products = "GetActiveOrdersProducts";
        public static string Insert_Chat_List = "InsertChatList";
        public static string Check_Order_Paid = "CheckOrderPaid";
        public static string Check_Order_Discount = "CheckOrderDiscount";
        public static string Get_Suitable_Discout_For_Order = "GetSuitableDiscoutForOrder";
        public static string Get_Template_Components = "GetTemplateComponents";
        public static string Get_Template_Component_Types = "GetTemplateComponentTypes";
        public static string Create_Manager_Notification = "CreateManagerNotification";
        public static string Customer_Regis = "CustomerRegis";
        public static string Set_Material_For_Task = "SetMaterialForTask";
        public static string Delete_Product = "DeleteProduct";
        public static string Start_Task = "StartTask";
        public static string Finish_Task = "FinishTask";
        public static string Cancel_Order = "CancelOrder";
        public static string Read_All_Notification = "ReadAllNotification";
        public static string Auto_Assign_Task_For_Staff = "AutoAssignTaskForStaff";
        public static string Get_Staff_Task = "GetStaffTask";
        public static string Get_Order_Materials = "GetOrderMaterials";
        public static string Get_Materials = "GetMaterials";
        public static string Get_Order_Products = "GetOrderProducts";
        public static string Get_Order_Product_Templates = "GetOrderProductTemplates";
    }
    public class SpResult
    {
        public int ReturnValue { get; set; }
    }
}
