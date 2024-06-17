using ClinicPlatformBusinessObject;
using ClinicPlatformDTOs.UserModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformRepositories.Contracts
{
    internal interface IUserRepository: IDisposable
    {
        IEnumerable<UserInfoModel> GetAll();

        UserInfoModel? GetUser(int userId);

        UserInfoModel? AddUser(UserInfoModel userInfo);

        UserInfoModel? UpdateUser(UserInfoModel userInfo);

        void DeleteUser(int userId);

        void SaveChanges();

        // More specific items
        CustomerInfoModel? GetCustomerInfo(int customerId);

        ClinicStaffInfoModel? GetStaffInfo(int staffId);
    }
}
