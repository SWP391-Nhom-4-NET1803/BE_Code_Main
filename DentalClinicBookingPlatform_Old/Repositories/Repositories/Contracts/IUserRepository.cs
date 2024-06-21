using Microsoft.EntityFrameworkCore;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Repositories.Contracts
{
    public interface IUserRepository
    {
        Customer? GetCustomerInfo(int id);

        ClinicStaff? GetStaffInfo(int id);

        /**
         * <summary>
         *  <para>Kiểm tra thông tin đăng nhập của một người dùng.</para>
         * </summary>
         * <param name="username">Tên tài khoản đăng nhập</param>
         * <param name="password">Mật khẩu của tài khoản</param>
         * <returns>Thông tin chi tiết của <see cref="User">người dùng</see> </returns>
         */
        User? Authenticate(string username, string password);

        /**
         * <summary>
         *   <para>Kiểm tra thông tin về username và email để kiểm tra có thể tạo người dùng mới trong hệ thống hay không.</para>
         *   <para>Trả về một chuỗi nêu lí do tại sao lại thất bại (nếu có).</para>
         * </summary>
         * <param name="username">Tên tài khoản cần kiểm tra</param>
         * <param name="email">Email tài khoản cần kiểm tra</param>
         * <param name="message">Tin nhắn đầu ra</param>
         * <returns><see cref="bool">true</see> nếu có thể tạo một người dùng, <see cref="bool">false</see> nếu email hoặc username đã tồn tại.</returns>
         */
        bool CheckAvailability(string username, string email, out string message);

        /**
         * <summary>
         *  <para>Tìm thông tin của người dùng dựa trên email.</para>
         *  <para>Mỗi người dùng chỉ được liên kết một địa chỉ với một tài khoản nên có thể dễ dàng tìm kiếm thông tin cảu người đó.</para>
         * </summary>
         * <param name="email">Email của người dùng</param>
         * <returns><see cref="Nullable">null</see> nếu người dùng không tồn tại, thông tin <see cref="User"/> nếu có.</returns>
         */
        User? GetUserWithEmail(string email);

        /**
         * <summary>
         *  <para>Tìm thông tin của các nhân viên làm việc tại một phòng khám thông qua id.</para>
         * </summary>
         * <param name="clinic_id">ID của phòng khám</param>
         * <returns>Danh sách các nhân viên làm việc tại phòng khám nói trên, trả về chuỗi rỗng nếu không có nhân viên hoặc phòng khám không tồn tại.</returns>
         */
        IEnumerable<ClinicStaff> GetAllClinicStaff(int clinic_id);

        /**
         * <summary>
         *  <para>Tìm thông tin của các nhân viên làm việc tại một phòng khám thông qua tên.</para>
         * </summary>
         * <param name="clinic_name">tên của phòng khám</param>
         * <returns>Danh sách các nhân viên làm việc tại phòng khám nói trên, trả về chuỗi rỗng nếu không có nhân viên hoặc phòng khám không tồn tại.</returns>
         */
        IEnumerable<ClinicStaff> GetAllClinicStaff(string clinic_name);

        /**
         * <summary>
         *  <para>Kiểm tra xem thông tin của một người dùng có tồn tại hay không.</para>
         * </summary>
         * <param name="id">Id của người dùng</param>
         * <returns><see cref="true"/> nếu người dùng tồn tại, <see cref="false"/> nếu không tìm thấy.</returns>
         */
        bool ExistUser(int id, out User? info);
    }
}
