using Etailor.API.Repository.Interface;
using Etailor.API.Ultity.CommonValue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Service
{
    public class BackgroundService
    {
        private readonly IStaffRepository staffRepository;
        public BackgroundService(IStaffRepository staffRepository)
        {
            this.staffRepository = staffRepository;
        }

        public void CheckAvatarStaff()
        {

        }
        public void CheckAvatarCustomer()
        {

        }

        //private async Task<List<string>> GetImageLinks()
        //{
        //    var storage = Google.Cloud.Storage.V1.StorageClient.Create();

        //    // Specify your Firebase Storage bucket name
        //    string bucketName = AppValue.BUCKET_NAME;

        //    // List all objects in the Firebase Storage bucket
        //    var objects = storage.ListObjects(bucketName);

        //    return objects;
        //}
    }
}
