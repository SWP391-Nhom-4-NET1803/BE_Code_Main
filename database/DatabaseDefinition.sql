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

CREATE TABLE Booking (
  book_id            uniqueidentifier DEFAULT (NEWID()) NOT NULL, 
  appointment_date   date NOT NULL, 
  customer_id        int NOT NULL, 
  clinic_id          int NOT NULL, 
  dentist_id         int NOT NULL, 
  schedule_slot_id   uniqueidentifier NOT NULL, 
  creation_date      datetime NOT NULL, 
  status             bit NOT NULL, 
  booking_type       nvarchar(50) NOT NULL CHECK(booking_type in ('Khám tổng quát', 'Khám theo dịch vụ', 'Khám định kì')), 
  booking_service_id uniqueidentifier NULL, 
  PRIMARY KEY (book_id));
CREATE TABLE Certification (
  certification_id  uniqueidentifier DEFAULT (NEWID()) NOT NULL, 
  name              nvarchar(50) NULL, 
  certification_url varchar(50) NULL, 
  clinic_id         int NOT NULL, 
  media_id          uniqueidentifier NOT NULL, 
  PRIMARY KEY (certification_id));
CREATE TABLE Clinic (
  clinic_id   int IDENTITY(1, 1) NOT NULL, 
  name        nvarchar(50) NOT NULL UNIQUE, 
  address     nvarchar(50) NOT NULL UNIQUE, 
  open_hour   time(7) NOT NULL, 
  close_hour  time(7) NOT NULL, 
  description varchar(500) NULL, 
  email       nvarchar(80) NOT NULL UNIQUE, 
  phone       nvarchar(10) NOT NULL UNIQUE, 
  status      bit NOT NULL, 
  owner_id    int NOT NULL, 
  PRIMARY KEY (clinic_id));
CREATE TABLE ClinicServices (
  clinic_service_id uniqueidentifier DEFAULT (NEWID()) NOT NULL, 
  price             bigint NULL, 
  description       nvarchar(255) NULL, 
  clinic_id         int NOT NULL, 
  service_id        int NOT NULL, 
  PRIMARY KEY (clinic_service_id));
CREATE TABLE ClinicStaff (
  staff_id  int IDENTITY(1, 1) NOT NULL, 
  is_owner  bit NOT NULL, 
  user_id   int NOT NULL UNIQUE, 
  clinic_id int NULL, 
  PRIMARY KEY (staff_id));
CREATE TABLE Customer (
  customer_id int IDENTITY(1, 1) NOT NULL, 
  sex         nvarchar(10) NULL, 
  birth_date  date NULL, 
  insurance   nvarchar(20) NULL, 
  user_id     int NOT NULL UNIQUE, 
  PRIMARY KEY (customer_id));
CREATE TABLE Media (
  media_id     uniqueidentifier DEFAULT (NEWID()) NOT NULL, 
  media_path   int NULL, 
  created_date datetime DEFAULT (GETDATE()) NOT NULL, 
  type_id      int NOT NULL, 
  creator_id   int NOT NULL, 
  PRIMARY KEY (media_id));
CREATE TABLE MediaType (
  type_id   int IDENTITY(1, 1) NOT NULL, 
  type_name nvarchar(50) NOT NULL, 
  PRIMARY KEY (type_id));
CREATE TABLE Messages (
  message_id    uniqueidentifier DEFAULT (NEWID()) NOT NULL, 
  content       nvarchar(1000) NOT NULL, 
  creation_date datetime DEFAULT (GETDATE()) NOT NULL, 
  sender        int NOT NULL UNIQUE, 
  receiver      int NOT NULL, 
  PRIMARY KEY (message_id));
CREATE TABLE Payment (
  payment_id      uniqueidentifier DEFAULT (NEWID()) NOT NULL, 
  status          bit NOT NULL, 
  made_on         datetime NOT NULL, 
  amount          bigint NOT NULL, 
  booking_id      uniqueidentifier NOT NULL, 
  payment_type_id int NULL, 
  PRIMARY KEY (payment_id));
CREATE TABLE PaymentType (
  type_id              int IDENTITY(1, 1) NOT NULL, 
  type_provider        nvarchar(50) NOT NULL, 
  type_provider_secret nvarchar(255) NOT NULL, 
  type_provider_token  nvarchar(255) NOT NULL, 
  PRIMARY KEY (type_id));
CREATE TABLE Role (
  role_id          int IDENTITY(1, 1) NOT NULL, 
  role_name        nvarchar(50) NOT NULL UNIQUE, 
  role_description nvarchar(255) NULL, 
  PRIMARY KEY (role_id));
CREATE TABLE ScheduledSlot (
  schedule_slot_id uniqueidentifier DEFAULT (NEWID()) NOT NULL, 
  max_appointments int NOT NULL, 
  date_of_week     tinyint NOT NULL CHECK((date_of_week >= 0 AND date_of_week <= 7) ), 
  clinic_id        int NOT NULL, 
  slot_id          int NOT NULL, 
  PRIMARY KEY (schedule_slot_id));
CREATE TABLE Service (
  service_id   int IDENTITY(1, 1) NOT NULL, 
  service_name nvarchar(50) NOT NULL UNIQUE, 
  PRIMARY KEY (service_id));
CREATE TABLE Slot (
  slot_id    int IDENTITY(1, 1) NOT NULL, 
  start_time time(7) NOT NULL, 
  end_time   time(7) NOT NULL, 
  PRIMARY KEY (slot_id));
CREATE TABLE [User] (
  user_id       int IDENTITY(1, 1) NOT NULL, 
  username      varchar(50) NOT NULL UNIQUE, 
  password      varchar(50) NOT NULL, 
  email         varchar(50) NULL UNIQUE, 
  status        bit NOT NULL, 
  phone_number  varchar(50) NULL, 
  fullname      nvarchar(50) NULL, 
  creation_date datetime DEFAULT (GETDATE()) NULL, 
  role_id       int NOT NULL, 
  PRIMARY KEY (user_id));
ALTER TABLE Certification ADD CONSTRAINT FKCertificat536417 FOREIGN KEY (clinic_id) REFERENCES Clinic (clinic_id);
ALTER TABLE Payment ADD CONSTRAINT FKPayment428514 FOREIGN KEY (booking_id) REFERENCES Booking (book_id);
ALTER TABLE Booking ADD CONSTRAINT FKBooking367844 FOREIGN KEY (clinic_id) REFERENCES Clinic (clinic_id);
ALTER TABLE Booking ADD CONSTRAINT FKBooking965192 FOREIGN KEY (customer_id) REFERENCES Customer (customer_id);
ALTER TABLE ClinicServices ADD CONSTRAINT FKClinicServ622868 FOREIGN KEY (clinic_id) REFERENCES Clinic (clinic_id);
ALTER TABLE ClinicServices ADD CONSTRAINT FKClinicServ95478 FOREIGN KEY (service_id) REFERENCES Service (service_id);
ALTER TABLE Customer ADD CONSTRAINT FKCustomer199874 FOREIGN KEY (user_id) REFERENCES [User] (user_id);
ALTER TABLE ClinicStaff ADD CONSTRAINT FKClinicStaf352227 FOREIGN KEY (user_id) REFERENCES [User] (user_id);
ALTER TABLE Booking ADD CONSTRAINT FKBooking760309 FOREIGN KEY (schedule_slot_id) REFERENCES ScheduledSlot (schedule_slot_id);
ALTER TABLE ScheduledSlot ADD CONSTRAINT FKScheduledS501966 FOREIGN KEY (slot_id) REFERENCES Slot (slot_id);
ALTER TABLE ScheduledSlot ADD CONSTRAINT FKScheduledS448005 FOREIGN KEY (clinic_id) REFERENCES Clinic (clinic_id);
ALTER TABLE [User] ADD CONSTRAINT FKUser1655 FOREIGN KEY (role_id) REFERENCES Role (role_id);
ALTER TABLE Payment ADD CONSTRAINT FKPayment702148 FOREIGN KEY (payment_type_id) REFERENCES PaymentType (type_id);
ALTER TABLE Messages ADD CONSTRAINT FKMessages901196 FOREIGN KEY (sender) REFERENCES [User] (user_id);
ALTER TABLE Messages ADD CONSTRAINT FKMessages658033 FOREIGN KEY (receiver) REFERENCES [User] (user_id);
ALTER TABLE Booking ADD CONSTRAINT FKBooking214257 FOREIGN KEY (dentist_id) REFERENCES ClinicStaff (staff_id);
ALTER TABLE ClinicStaff ADD CONSTRAINT FKClinicStaf705438 FOREIGN KEY (clinic_id) REFERENCES Clinic (clinic_id);
ALTER TABLE Clinic ADD CONSTRAINT FKClinic176906 FOREIGN KEY (owner_id) REFERENCES [User] (user_id);
ALTER TABLE Media ADD CONSTRAINT FKMedia498708 FOREIGN KEY (type_id) REFERENCES MediaType (type_id);
ALTER TABLE Media ADD CONSTRAINT FKMedia66473 FOREIGN KEY (creator_id) REFERENCES [User] (user_id);
ALTER TABLE Certification ADD CONSTRAINT FKCertificat893466 FOREIGN KEY (media_id) REFERENCES Media (media_id);
ALTER TABLE Booking ADD CONSTRAINT FKBooking137674 FOREIGN KEY (booking_service_id) REFERENCES ClinicServices (clinic_service_id);
