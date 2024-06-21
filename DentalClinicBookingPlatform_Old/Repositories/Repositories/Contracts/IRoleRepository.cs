using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Repositories.Contracts
{
    public interface IRoleRepository
    {
        /// <summary>
        ///  <para>Tìm thông tin của một vai trò người dùng trong hệ thống.</para>
        /// </summary>
        /// <param name="roleName">Tên vai trò</param>
        /// <returns><see cref="Nullable">null</see> nếu không tồn tại <see cref="Role"/> nào có tên như input, nếu tồn tại thì trả về thông tin của <see cref="Role"/> đó.</returns>
        public Role? GetRoleByName(string roleName);
    }
}
