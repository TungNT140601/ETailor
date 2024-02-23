using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Ultity.CommonValue
{
    public static class RoleName
    {
        public static string ADMIN = "Admin";
        public static string MANAGER = "Manager";
        public static string STAFF = "Staff";
        public static string CUSTOMER = "Customer";
    }
    public static class AppValue
    {
        public static string SALT_STRING = "";
        public static string BUCKET_NAME = "etailor-21a50.appspot.com";
        public static string FIREBASE_KEY = "etailor-21a50-firebase-adminsdk-badmt-5e743a2d8e.json";
    }

    public static class SystemMessage
    {
        //format: table_function_(Field)_status
        public static string STAFF_LOGIN_SUCCESS = "Đăng nhập thành công";
        public static string STAFF_LOGIN_PASSWORD_FAIL= "Sai mật khẩu";
        public static string STAFF_LOGIN_USERNAME_FAIL = "Sai tài khoản";
        public static string FOR_BID_ERROR = "";

        public static string PRODUCTTEMPLATE_CREATE_SUCCESS = "Thêm mẫu sản phẩm thành công";
        public static string PRODUCTTEMPLATE_CREATE_NAME_FAIL = "Mẫu sản phẩm đã có trong cửa hàng";

    }

    public static class PlatformName
    {
        public static string OFFLINE = "Offline";
        public static string VN_PAY = "VN Pay";
        public static string MOMO = "MoMo";
        public static string ZALO_PAY = "Zalo Pay";
        public static string VIETQR = "VietQR";
    }
}
