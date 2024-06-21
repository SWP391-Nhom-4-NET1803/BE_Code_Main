using Microsoft.EntityFrameworkCore;
using PlatformRepository.Repositories;
using Repositories.Models;
using Repositories.Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Repositories
{
    public class BookingRepository : GenericRepository<Booking, Guid>, IBookingRepository
    {

        public BookingRepository(DentalClinicPlatformContext context) : base(context) { }

        public IEnumerable<Booking> GetBookingForUser(int userId)
        {
            var bookings = (from bookItem in this.context.Bookings
                            join customer in this.context.Customers on bookItem.CustomerId equals customer.CustomerId
                            join dentist in this.context.ClinicStaffs on bookItem.DentistId equals dentist.StaffId
                            where customer.UserId == userId || dentist.UserId == userId
                            select bookItem);

            return bookings
                .Include(x => x.Clinic)
                .Include(x => x.Customer)
                    .ThenInclude(y => y.User)
                .Include(x => x.Dentist)
                    .ThenInclude(y => y.User)
                .Include(x => x.ScheduleSlot)
                    .ThenInclude(y => y.Slot)
                .ToList();
        }

        public IEnumerable<Booking> getBookingForCustomer(int customerId)
        {
            var bookings = (from bookItem in this.context.Bookings
                            join customer in this.context.Customers on bookItem.CustomerId equals customer.CustomerId
                            where customer.CustomerId == customerId
                            select bookItem);

            return bookings
                .Include(x=>x.Clinic)
                .Include(x => x.Customer)
                    .ThenInclude(y => y.User)
                .Include(x=>x.Dentist)
                    .ThenInclude(y=>y.User)
                .Include(x=>x.ScheduleSlot)
                    .ThenInclude(y=>y.Slot)
                .ToList() ;
        }

        public IEnumerable<Booking> getBookingForClinicStaff(int staffId)
        {
            var bookings = (from bookItem in this.context.Bookings
                            join dentist in this.context.ClinicStaffs on bookItem.DentistId equals dentist.StaffId
                            where dentist.StaffId == staffId
                            select bookItem);

            return bookings
                .Include(x => x.Clinic)
                .Include(x => x.Customer)
                    .ThenInclude(y => y.User)
                .Include(x => x.Dentist)
                    .ThenInclude(y => y.User)
                .Include(x => x.ScheduleSlot)
                    .ThenInclude(y => y.Slot)
                .ToList();
        }

        public IEnumerable<Booking> getClinicBooking(int clinicId, bool futureOnly)
        {
            var bookings = (from bookItem in this.context.Bookings
                            join clinic in this.context.Clinics on bookItem.ClinicId equals clinic.ClinicId 
                            select bookItem);

            if (futureOnly)
            {
                return bookings.Where(x => x.AppointmentDate > DateOnly.FromDateTime(DateTime.Now)).ToList();
            }

            return bookings.ToList();
        }

        public IEnumerable<Booking> getClinicBookingInDateRange(int clinicId, DateOnly startDate, DateOnly endDate)
        {
            if (startDate > endDate)
            {
                throw new Exception("Invalid agurments, endDate can not occured before startDate.");
            }

            var bookings = (from bookItem in this.context.Bookings
                            join clinic in this.context.Clinics on bookItem.ClinicId equals clinic.ClinicId
                            where (clinic.ClinicId == clinicId) && (startDate <= bookItem.AppointmentDate && bookItem.AppointmentDate <= endDate)
                            select bookItem);

            return bookings.ToList();
        }

        public IEnumerable<Booking> getBookingInDateRange(int userId, DateOnly startDate, DateOnly endDate)
        {
            if (startDate > endDate)
            {
                throw new Exception("Invalid agurments, endDate can not occured before startDate.");
            }

            var bookings = (from bookItem in this.context.Bookings
                            join customer in this.context.Customers on bookItem.CustomerId equals customer.CustomerId
                            join dentist in this.context.ClinicStaffs on bookItem.DentistId equals dentist.StaffId
                            where (customer.UserId == userId || dentist.UserId == userId) && (startDate <= bookItem.AppointmentDate && bookItem.AppointmentDate <= endDate)
                            select bookItem);

            return bookings;
        }

        public IEnumerable<Booking> getFutureBooking(int userId)
        {
            var books = (from book in this.context.Bookings
                   join customer in this.context.Customers on book.CustomerId equals customer.CustomerId
                   join user in this.context.Users on customer.UserId equals user.UserId
                   where book.AppointmentDate > DateOnly.FromDateTime(DateTime.Now) && user.UserId == userId
                   select book);

            return books.ToList();
        }

        /// <summary>
        ///  I'm sorry
        /// </summary>
        /// <param name="bookId"></param>
        /// <returns></returns>
        public Booking? getFullBookingInfo(Guid bookId)
        {
            return (Booking?) dbSet.Include(x => x.Customer).Include(x => x.Dentist).Include(x => x.Clinic).Include(x => x.ScheduleSlot).Include(x => x.ScheduleSlot.Slot).Where(x => x.BookId == bookId).ToList().FirstOrDefault();
        }
    }
}
