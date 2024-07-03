USE master;
GO

IF DB_ID('DentalClinicPlatform') IS NOT NULL
DROP DATABASE [DentalClinicPlatform];
GO

IF DB_ID('DentalClinicPlatform') IS NULL
CREATE DATABASE DentalClinicPlatform;
GO

USE DentalClinicPlatform;
GO

create table Appointment (id uniqueidentifier default (NEWID()) not null, appointment_type nvarchar(10) default 'checkup' not null check(appointment_type in ('checkup', 'treatment')), [date] date not null, slot_id uniqueidentifier not null, customer_id int not null, dentist_id int not null, clinic_id int not null, dentist_note nvarchar(1000) default '' not null, status nvarchar(20) default 'booked' not null check(status IN ('booked', 'pending',  'finished', 'canceled', 'no show')), cycle_count int default 0 not null, original_appointment uniqueidentifier null, price_final int default 0 not null, creation_time datetime default (GETDATE()) not null, primary key (id));
create table BookedService (appointment_id uniqueidentifier not null, service_id uniqueidentifier not null, price int not null, primary key (appointment_id));
create table Clinic (clinic_id int identity not null, name nvarchar(128) not null, address nvarchar(128) not null, description text default '' not null, phone nvarchar(10) not null, email nvarchar(64) not null, open_hour time(7) not null, close_hour time(7) not null, working bit default 1 not null, status nvarchar(255) default 'unverified' not null check(status IN ('unverified', 'verified', 'removed')), owner_id int not null, primary key (clinic_id));
create table ClinicService (id uniqueidentifier default (NEWID()) not null, custom_name nvarchar(80) null, description nvarchar(500) not null, price int not null, clinic_id int not null, category_id int not null, available bit default 1 not null, removed bit not null, primary key (id));
create table ClinicSlot (slot_id uniqueidentifier default (NEWID()) not null, max_checkup int not null, max_treatment int not null, weekday tinyint not null check(0 <= weekday AND  weekday < 7), clinic_id int not null, time_id int not null, status bit default 1 not null, primary key (slot_id));
exec sp_addextendedproperty @name = N'MS_Description', @value = N'0: Sunday 
1: Monday 
2: Tuesday 
3: Wednesday 
4: Thursday 
5: Friday 
6: Saturday', @level0type = N'Schema', @level0name = N'dbo', @level1type = N'Table', @level1name = N'ClinicSlot', @level2type = N'Column', @level2name = N'weekday';
create table Customer (id int identity(1, 1) not null, insurance nvarchar(20) null, birthdate date null, sex nvarchar(16) null, user_id int not null unique, primary key (id));
create table Dentist (id int identity(1, 1) not null, is_owner bit default 0 not null, user_id int not null unique, clinic_id int null, primary key (id));
create table Payment (id int identity(1, 1) not null, transaction_id nvarchar(255) not null, info nvarchar(255) not null, amount decimal(19, 0) not null, creation_time datetime default (GETDATE()) not null, expiration_time datetime not null, provider nvarchar(20) not null, appointment_id uniqueidentifier not null, status nvarchar(20) not null check( status IN ('canceled', 'completed', 'pending')), primary key (id));
create table ServiceCategory (id int identity(1, 1) not null, name nvarchar(32) not null, primary key (id));
create table Slot (id int identity(1, 1) not null, [start] time(7) not null, [end] time(7) not null, primary key (id));
create table Token (ID uniqueidentifier not null, token_value nvarchar(255) not null, reason nvarchar(80) not null check(reason IN ('pasword_reset', 'account_create')), creation_time datetime default (GETDATE()) not null, used bit default 0 not null, expiration datetime default (GETDATE()) not null, [user] int not null, primary key (ID));
create table [User] (id int identity(1, 1) not null, username nvarchar(20) not null unique, password_hash nvarchar(128) not null, salt nvarchar(128) not null, email nvarchar(64) not null, fullname nvarchar(255) null, phone nvarchar(10) null, creation_time datetime default (GETDATE()) not null, role nvarchar(20) not null check(role IN ('Customer', 'Dentist', 'Admin')), active bit default 1 not null, removed bit not null, primary key (id));
alter table Customer add constraint FKCustomer336289 foreign key (user_id) references [User] (id);
alter table Clinic add constraint FKClinic40491 foreign key (owner_id) references [User] (id);
alter table ClinicService add constraint FKClinicServ128006 foreign key (clinic_id) references Clinic (clinic_id);
alter table ClinicService add constraint FKClinicServ913410 foreign key (category_id) references ServiceCategory (id);
alter table ClinicSlot add constraint FKClinicSlot657646 foreign key (clinic_id) references Clinic (clinic_id);
alter table ClinicSlot add constraint FKClinicSlot285803 foreign key (time_id) references Slot (id);
alter table Token add constraint FKToken237377 foreign key ([user]) references [User] (id);
alter table BookedService add constraint FKBookedServ419526 foreign key (service_id) references ClinicService (id);
alter table Payment add constraint FKPayment457035 foreign key (appointment_id) references Appointment (id);
alter table Appointment add constraint FKAppointmen41115 foreign key (original_appointment) references Appointment (id);
alter table BookedService add constraint FKBookedServ274862 foreign key (appointment_id) references Appointment (id);
alter table Appointment add constraint FKAppointmen366296 foreign key (customer_id) references Customer (id);
alter table Appointment add constraint FKAppointmen998789 foreign key (slot_id) references ClinicSlot (slot_id);
alter table Appointment add constraint FKAppointmen99798 foreign key (clinic_id) references Clinic (clinic_id);
alter table Appointment add constraint FKAppointmen157913 foreign key (dentist_id) references Dentist (id);
alter table Dentist add constraint FKDentist52014 foreign key (user_id) references [User] (id);
alter table Dentist add constraint FKDentist429950 foreign key (clinic_id) references Clinic (clinic_id);
GO


-- This is for creating fixed 30 minute time slot.
DECLARE @StartTime TIME = '00:00:00';
DECLARE @EndTime TIME;

WHILE @StartTime < '23:30:00'
BEGIN
    SET @EndTime = DATEADD(MINUTE, 30, @StartTime);
    
    INSERT INTO Slot ([start], [end]) VALUES (@StartTime, @EndTime);

    SET @StartTime = @EndTime;
END
GO

-- Because SQL Server has some problem saving 24:00:00 as time (idk ? maybe 24:00:00 IS 00:00:00) so we have to do this manually
INSERT INTO Slot ([start], [end]) VALUES ('23:30:00', '00:00:00');
GO





