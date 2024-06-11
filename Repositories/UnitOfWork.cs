using Repositories.Models;
using PlatformRepository.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repositories.Repositories;
using Repositories.Repositories.Contracts;

namespace Repositories
{
    public class UnitOfWork
    {
        private readonly DentalClinicPlatformContext _context;

        private IUserRepository? _userRepository;
        private IClinicRepository? _clinicRepository;
        private IBookingRepository? _bookingRepository;
        private IClinicServiceRepository? _clinicServiceRepository;
        private IServiceRepository? _serviceRepository;
        private IRoleRepository? _roleRepository;
        private IClinicStaffRepository? _clinicStaffRepository;

        public UnitOfWork(DentalClinicPlatformContext context)
        {
            _context = context;
        }

        public UserRepository UserRepository
        {
            get
            {
                if (_userRepository == null)
                {
                    this._userRepository = new UserRepository(this._context);
                }

                return (UserRepository) this._userRepository;
            }
        }

        public ClinicRepository ClinicRepository
        {
            get
            {
                if (_clinicRepository == null)
                {
                    this._clinicRepository = new ClinicRepository(this._context);
                }

                return (ClinicRepository) this._clinicRepository;
            }
        }

        public BookingRepository BookingRepository
        {
            get
            {
                if (_bookingRepository == null)
                {
                    this._bookingRepository = new BookingRepository(this._context);
                }

                return (BookingRepository) this._bookingRepository;
            }
        }

        public ClinicServiceRepository ClinicServiceRepository
        {
            get
            {
                if (_clinicServiceRepository == null)
                {
                    this._clinicServiceRepository = new ClinicServiceRepository(this._context);
                }

                return (ClinicServiceRepository)this._clinicServiceRepository;
            }
        }

        public ServiceRepository ServiceRepository
        {
            get
            {
                if (_serviceRepository == null)
                {
                    this._serviceRepository = new ServiceRepository(this._context);
                }

                return (ServiceRepository) this._serviceRepository;
            }
        }

        public RoleRepository RoleRepository
        {
            get
            {
                if (this._roleRepository == null)
                {
                    this._roleRepository = new RoleRepository(this._context);
                }

                return (RoleRepository) this._roleRepository;
            }
        }

        public ClinicStaffRepository clinicStaffRepository
        {
            get
            {
                if (this._clinicStaffRepository == null)
                {
                    this._clinicStaffRepository = new ClinicStaffRepository(this._context);
                }

                return (ClinicStaffRepository)this._clinicStaffRepository;
            }
        }

        /**
         * <summary>
         *  Lưu trạng thái hiện tại của context xuống database. (commit changes) <br/>
         *  Nếu không gọi hàm này khi thay đổi thông tin thì tất cả thay đổi sẽ được đặt trong trạng thái "chờ"
         * </summary>
         */
        public void Save()
        {
            _context.SaveChanges();
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
