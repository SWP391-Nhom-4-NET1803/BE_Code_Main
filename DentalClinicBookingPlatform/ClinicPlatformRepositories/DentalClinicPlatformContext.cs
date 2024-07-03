using System;
using System.Collections.Generic;
using ClinicPlatformDatabaseObject;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ClinicPlatformRepositories;

public partial class DentalClinicPlatformContext : DbContext
{
    public DentalClinicPlatformContext()
    {
    }

    public DentalClinicPlatformContext(DbContextOptions<DentalClinicPlatformContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Appointment> Appointments { get; set; }

    public virtual DbSet<BookedService> BookedServices { get; set; }

    public virtual DbSet<Clinic> Clinics { get; set; }

    public virtual DbSet<ClinicService> ClinicServices { get; set; }

    public virtual DbSet<ClinicSlot> ClinicSlots { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Dentist> Dentists { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<ServiceCategory> ServiceCategories { get; set; }

    public virtual DbSet<Slot> Slots { get; set; }

    public virtual DbSet<Token> Tokens { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(GetConnectionString());
    }

    private string GetConnectionString()
    {
        IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", true, true)
            .Build();

        var connectionString = config.GetConnectionString("Database");

        if (connectionString == null)
        {
            throw new Exception("Connection string not found! Please check your configuration files.");
        }

        return connectionString;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Appointm__3213E83FB2DE0F1D");

            entity.ToTable("Appointment");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.AppointmentType)
                .HasMaxLength(10)
                .HasDefaultValue("checkup")
                .HasColumnName("appointment_type");
            entity.Property(e => e.ClinicId).HasColumnName("clinic_id");
            entity.Property(e => e.CreationTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("creation_time");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.CycleCount).HasColumnName("cycle_count");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.DentistId).HasColumnName("dentist_id");
            entity.Property(e => e.DentistNote)
                .HasMaxLength(1000)
                .HasDefaultValue("")
                .HasColumnName("dentist_note");
            entity.Property(e => e.OriginalAppointment).HasColumnName("original_appointment");
            entity.Property(e => e.PriceFinal).HasColumnName("price_final");
            entity.Property(e => e.SlotId).HasColumnName("slot_id");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("booked")
                .HasColumnName("status");

            entity.HasOne(d => d.Clinic).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.ClinicId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FKAppointmen99798");

            entity.HasOne(d => d.Customer).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FKAppointmen366296");

            entity.HasOne(d => d.Dentist).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.DentistId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FKAppointmen157913");

            entity.HasOne(d => d.OriginalAppointmentNavigation).WithMany(p => p.InverseOriginalAppointmentNavigation)
                .HasForeignKey(d => d.OriginalAppointment)
                .HasConstraintName("FKAppointmen41115");

            entity.HasOne(d => d.Slot).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.SlotId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FKAppointmen998789");
        });

        modelBuilder.Entity<BookedService>(entity =>
        {
            entity.HasKey(e => e.AppointmentId).HasName("PK__BookedSe__A50828FC7A4A9073");

            entity.ToTable("BookedService");

            entity.Property(e => e.AppointmentId)
                .ValueGeneratedNever()
                .HasColumnName("appointment_id");
            entity.Property(e => e.Price).HasColumnName("price");
            entity.Property(e => e.ServiceId).HasColumnName("service_id");

            entity.HasOne(d => d.Appointment).WithOne(p => p.BookedService)
                .HasForeignKey<BookedService>(d => d.AppointmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FKBookedServ274862");

            entity.HasOne(d => d.Service).WithMany(p => p.BookedServices)
                .HasForeignKey(d => d.ServiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FKBookedServ419526");
        });

        modelBuilder.Entity<Clinic>(entity =>
        {
            entity.HasKey(e => e.ClinicId).HasName("PK__Clinic__A0C8D19BA0B1C1F5");

            entity.ToTable("Clinic");

            entity.Property(e => e.ClinicId).HasColumnName("clinic_id");
            entity.Property(e => e.Address)
                .HasMaxLength(128)
                .HasColumnName("address");
            entity.Property(e => e.CloseHour).HasColumnName("close_hour");
            entity.Property(e => e.Description)
                .HasDefaultValue("")
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.Email)
                .HasMaxLength(64)
                .HasColumnName("email");
            entity.Property(e => e.Name)
                .HasMaxLength(128)
                .HasColumnName("name");
            entity.Property(e => e.OpenHour).HasColumnName("open_hour");
            entity.Property(e => e.OwnerId).HasColumnName("owner_id");
            entity.Property(e => e.Phone)
                .HasMaxLength(10)
                .HasColumnName("phone");
            entity.Property(e => e.Status)
                .HasMaxLength(255)
                .HasDefaultValue("unverified")
                .HasColumnName("status");
            entity.Property(e => e.Working)
                .HasDefaultValue(true)
                .HasColumnName("working");

            entity.HasOne(d => d.Owner).WithMany(p => p.Clinics)
                .HasForeignKey(d => d.OwnerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FKClinic40491");
        });

        modelBuilder.Entity<ClinicService>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ClinicSe__3213E83F343E07AD");

            entity.ToTable("ClinicService");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Available)
                .HasDefaultValue(true)
                .HasColumnName("available");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.ClinicId).HasColumnName("clinic_id");
            entity.Property(e => e.CustomName)
                .HasMaxLength(80)
                .HasColumnName("custom_name");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.Price).HasColumnName("price");
            entity.Property(e => e.Removed).HasColumnName("removed");

            entity.HasOne(d => d.Category).WithMany(p => p.ClinicServices)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FKClinicServ913410");

            entity.HasOne(d => d.Clinic).WithMany(p => p.ClinicServices)
                .HasForeignKey(d => d.ClinicId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FKClinicServ128006");
        });

        modelBuilder.Entity<ClinicSlot>(entity =>
        {
            entity.HasKey(e => e.SlotId).HasName("PK__ClinicSl__971A01BB3992DA7A");

            entity.ToTable("ClinicSlot");

            entity.Property(e => e.SlotId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("slot_id");
            entity.Property(e => e.ClinicId).HasColumnName("clinic_id");
            entity.Property(e => e.MaxCheckup).HasColumnName("max_checkup");
            entity.Property(e => e.MaxTreatment).HasColumnName("max_treatment");
            entity.Property(e => e.Status)
                .HasDefaultValue(true)
                .HasColumnName("status");
            entity.Property(e => e.TimeId).HasColumnName("time_id");
            entity.Property(e => e.Weekday)
                .HasComment("0: Sunday \r\n1: Monday \r\n2: Tuesday \r\n3: Wednesday \r\n4: Thursday \r\n5: Friday \r\n6: Saturday")
                .HasColumnName("weekday");

            entity.HasOne(d => d.Clinic).WithMany(p => p.ClinicSlots)
                .HasForeignKey(d => d.ClinicId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FKClinicSlot657646");

            entity.HasOne(d => d.Time).WithMany(p => p.ClinicSlots)
                .HasForeignKey(d => d.TimeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FKClinicSlot285803");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Customer__3213E83F8685F410");

            entity.ToTable("Customer");

            entity.HasIndex(e => e.UserId, "UQ__Customer__B9BE370EA108E792").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Birthdate).HasColumnName("birthdate");
            entity.Property(e => e.Insurance)
                .HasMaxLength(20)
                .HasColumnName("insurance");
            entity.Property(e => e.Sex)
                .HasMaxLength(16)
                .HasColumnName("sex");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithOne(p => p.Customer)
                .HasForeignKey<Customer>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FKCustomer336289");
        });

        modelBuilder.Entity<Dentist>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Dentist__3213E83F6301879C");

            entity.ToTable("Dentist");

            entity.HasIndex(e => e.UserId, "UQ__Dentist__B9BE370E8D2ED2D5").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ClinicId).HasColumnName("clinic_id");
            entity.Property(e => e.IsOwner).HasColumnName("is_owner");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Clinic).WithMany(p => p.Dentists)
                .HasForeignKey(d => d.ClinicId)
                .HasConstraintName("FKDentist429950");

            entity.HasOne(d => d.User).WithOne(p => p.Dentist)
                .HasForeignKey<Dentist>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FKDentist52014");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Payment__3213E83FBA74383E");

            entity.ToTable("Payment");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(19, 0)")
                .HasColumnName("amount");
            entity.Property(e => e.AppointmentId).HasColumnName("appointment_id");
            entity.Property(e => e.CreationTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("creation_time");
            entity.Property(e => e.ExpirationTime)
                .HasColumnType("datetime")
                .HasColumnName("expiration_time");
            entity.Property(e => e.Info)
                .HasMaxLength(255)
                .HasColumnName("info");
            entity.Property(e => e.Provider)
                .HasMaxLength(20)
                .HasColumnName("Provider");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasColumnName("status");
            entity.Property(e => e.TransactionId)
                .HasMaxLength(255)
                .HasColumnName("transaction_id");

            entity.HasOne(d => d.Appointment).WithMany(p => p.Payments)
                .HasForeignKey(d => d.AppointmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FKPayment457035");
        });

        modelBuilder.Entity<ServiceCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ServiceC__3213E83FD2FDA383");

            entity.ToTable("ServiceCategory");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(32)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Slot>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Slot__3213E83F49AEDC66");

            entity.ToTable("Slot");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.End).HasColumnName("end");
            entity.Property(e => e.Start).HasColumnName("start");
        });

        modelBuilder.Entity<Token>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Token__3214EC27F17EAFD0");

            entity.ToTable("Token");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.CreationTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("creation_time");
            entity.Property(e => e.Expiration)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("expiration");
            entity.Property(e => e.Reason)
                .HasMaxLength(80)
                .HasColumnName("reason");
            entity.Property(e => e.TokenValue)
                .HasMaxLength(255)
                .HasColumnName("token_value");
            entity.Property(e => e.Used).HasColumnName("used");
            entity.Property(e => e.User).HasColumnName("user");

            entity.HasOne(d => d.UserNavigation).WithMany(p => p.Tokens)
                .HasForeignKey(d => d.User)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FKToken237377");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__User__3213E83FE58E2328");

            entity.ToTable("User");

            entity.HasIndex(e => e.Username, "UQ__User__F3DBC57293375DD0").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Active)
                .HasDefaultValue(true)
                .HasColumnName("active");
            entity.Property(e => e.CreationTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("creation_time");
            entity.Property(e => e.Email)
                .HasMaxLength(64)
                .HasColumnName("email");
            entity.Property(e => e.Fullname)
                .HasMaxLength(255)
                .HasColumnName("fullname");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(128)
                .HasColumnName("password_hash");
            entity.Property(e => e.Phone)
                .HasMaxLength(10)
                .HasColumnName("phone");
            entity.Property(e => e.Removed).HasColumnName("removed");
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .HasColumnName("role");
            entity.Property(e => e.Salt)
                .HasMaxLength(128)
                .HasColumnName("salt");
            entity.Property(e => e.Username)
                .HasMaxLength(20)
                .HasColumnName("username");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
