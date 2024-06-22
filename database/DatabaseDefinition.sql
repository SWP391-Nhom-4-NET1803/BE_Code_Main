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

CREATE TABLE Appointment (
  id                   uniqueidentifier DEFAULT (NEWID()) NOT NULL, 
  appointment_type     nvarchar(10) DEFAULT 'checkup' NOT NULL CHECK(appointment_type in ('checkup', 'treatment')), 
  number               int NOT NULL, 
  [date]               date NOT NULL, 
  slot_id              uniqueidentifier NOT NULL, 
  customer_id          int NOT NULL, 
  dentist_id           int NOT NULL, 
  clinic_id            int NOT NULL, 
  cycle_count          int DEFAULT 0 NOT NULL, 
  dentist_note         nvarchar(1000) DEFAULT '' NOT NULL, 
  status               nvarchar(20) DEFAULT 'booked' NOT NULL CHECK(status IN ('booked', 'finished', 'canceled', 'no show')), 
  original_appointment uniqueidentifier NULL, 
  price_final          int DEFAULT 0 NOT NULL, 
  PRIMARY KEY (id));
CREATE TABLE BookedService (
  appointment_id uniqueidentifier NOT NULL, 
  service_id     uniqueidentifier NOT NULL, 
  price          int NOT NULL, 
  PRIMARY KEY (appointment_id));
CREATE TABLE Clinic (
  clinic_id   int IDENTITY NOT NULL, 
  name        nvarchar(128) NOT NULL, 
  address     nvarchar(128) NOT NULL, 
  description text DEFAULT '' NOT NULL, 
  phone       nvarchar(10) NOT NULL, 
  email       nvarchar(64) NOT NULL, 
  open_hour   time(7) NOT NULL, 
  close_hour  time(7) NOT NULL, 
  working     bit DEFAULT 1 NOT NULL, 
  status      nvarchar(255) DEFAULT 'unverified' NOT NULL CHECK(status IN ('unverified', 'verified', 'removed')), 
  owner_id    int NOT NULL, 
  PRIMARY KEY (clinic_id));
CREATE TABLE ClinicService (
  id                   uniqueidentifier DEFAULT (NEWID()) NOT NULL, 
  custom_name          nvarchar(80) NULL, 
  description          nvarchar(500) NOT NULL, 
  price                int NOT NULL, 
  clinic_id            int NOT NULL, 
  category_id          int NOT NULL, 
  available            bit DEFAULT 1 NOT NULL, 
  removed              bit NOT NULL, 
  first_slot_treatment bit NULL, 
  PRIMARY KEY (id));
CREATE TABLE ClinicSlot (
  slot_id       uniqueidentifier DEFAULT (NEWID()) NOT NULL, 
  max_checkup   int NOT NULL, 
  max_treatment int NOT NULL, 
  weekday       tinyint NOT NULL CHECK(0 <= weekday AND  weekday < 7), 
  clinic_id     int NOT NULL, 
  time_id       int NOT NULL, 
  status        bit DEFAULT 1 NOT NULL, 
  PRIMARY KEY (slot_id));
EXEC sp_addextendedproperty 
  @NAME = N'MS_Description', @VALUE = N'0: Sunday 
1: Monday 
2: Tuesday 
3: Wednesday 
4: Thursday 
5: Friday 
6: Saturday', 
  @LEVEL0TYPE = N'Schema', @LEVEL0NAME = N'dbo', 
  @LEVEL1TYPE = N'Table', @LEVEL1NAME = N'ClinicSlot', 
  @LEVEL2TYPE = N'Column', @LEVEL2NAME = N'weekday';
CREATE TABLE Customer (
  id        int IDENTITY(1, 1) NOT NULL, 
  insurance nvarchar(20) NULL, 
  birthdate date NULL, 
  sex       nvarchar(16) NULL, 
  user_id   int NOT NULL UNIQUE, 
  PRIMARY KEY (id));
CREATE TABLE Dentist (
  id        int IDENTITY(1, 1) NOT NULL, 
  is_owner  bit DEFAULT 0 NOT NULL, 
  user_id   int NOT NULL UNIQUE, 
  clinic_id int NULL, 
  PRIMARY KEY (id));
CREATE TABLE Payment (
  id             int IDENTITY NOT NULL, 
  transaction_id nvarchar(255) NOT NULL, 
  amount         decimal(19, 0) NOT NULL, 
  title          int NULL, 
  expiration     date NOT NULL, 
  creation_time  datetime DEFAULT (GETDATE()) NOT NULL, 
  status         bit DEFAULT 0 NOT NULL CHECK(status IN ('waiting', 'canceled', 'completed')), 
  creator        int NOT NULL, 
  appointment_id uniqueidentifier NOT NULL, 
  type_id        int NOT NULL, 
  provider       nvarchar(20) NOT NULL, 
  PRIMARY KEY (id));
CREATE TABLE ServiceCategory (
  id   int IDENTITY(1, 1) NOT NULL, 
  name nvarchar(32) NOT NULL, 
  PRIMARY KEY (id));
CREATE TABLE Slot (
  id      int IDENTITY(1, 1) NOT NULL, 
  [start] time(7) NOT NULL, 
  [end]   time(7) NOT NULL, 
  PRIMARY KEY (id));
CREATE TABLE Token (
  ID            uniqueidentifier NOT NULL, 
  token_value   nvarchar(255) NOT NULL, 
  reason        nvarchar(80) NOT NULL CHECK(reason IN ('password_reset', '')), 
  creation_time datetime DEFAULT (GETDATE()) NOT NULL, 
  used          bit DEFAULT 0 NOT NULL, 
  expiration    datetime DEFAULT (GETDATE()) NOT NULL, 
  [user]        int NOT NULL, 
  PRIMARY KEY (ID));
CREATE TABLE [User] (
  id            int IDENTITY(1, 1) NOT NULL, 
  username      nvarchar(20) NOT NULL UNIQUE, 
  password_hash nvarchar(128) NOT NULL, 
  salt          nvarchar(128) NOT NULL, 
  email         nvarchar(64) NOT NULL, 
  fullname      nvarchar(255) NULL, 
  phone         nvarchar(10) NULL, 
  creation_time datetime DEFAULT (GETDATE()) NOT NULL, 
  role          nvarchar(20) NOT NULL CHECK(role IN ('Patient', 'Staff', 'Admin')), 
  active        bit DEFAULT 1 NOT NULL, 
  removed       bit NOT NULL, 
  PRIMARY KEY (id));
ALTER TABLE Customer ADD CONSTRAINT FKCustomer336289 FOREIGN KEY (user_id) REFERENCES [User] (id);
ALTER TABLE Dentist ADD CONSTRAINT FKDentist52014 FOREIGN KEY (user_id) REFERENCES [User] (id);
ALTER TABLE Clinic ADD CONSTRAINT FKClinic40491 FOREIGN KEY (owner_id) REFERENCES [User] (id);
ALTER TABLE Dentist ADD CONSTRAINT FKDentist429950 FOREIGN KEY (clinic_id) REFERENCES Clinic (clinic_id);
ALTER TABLE ClinicService ADD CONSTRAINT FKClinicServ128006 FOREIGN KEY (clinic_id) REFERENCES Clinic (clinic_id);
ALTER TABLE ClinicService ADD CONSTRAINT FKClinicServ913410 FOREIGN KEY (category_id) REFERENCES ServiceCategory (id);
ALTER TABLE Payment ADD CONSTRAINT FKPayment177997 FOREIGN KEY (creator) REFERENCES [User] (id);
ALTER TABLE ClinicSlot ADD CONSTRAINT FKClinicSlot657646 FOREIGN KEY (clinic_id) REFERENCES Clinic (clinic_id);
ALTER TABLE ClinicSlot ADD CONSTRAINT FKClinicSlot285803 FOREIGN KEY (time_id) REFERENCES Slot (id);
ALTER TABLE Token ADD CONSTRAINT FKToken237377 FOREIGN KEY ([user]) REFERENCES [User] (id);
ALTER TABLE BookedService ADD CONSTRAINT FKBookedServ419526 FOREIGN KEY (service_id) REFERENCES ClinicService (id);
ALTER TABLE Payment ADD CONSTRAINT FKPayment457035 FOREIGN KEY (appointment_id) REFERENCES Appointment (id);
ALTER TABLE Appointment ADD CONSTRAINT FKAppointmen41115 FOREIGN KEY (original_appointment) REFERENCES Appointment (id);
ALTER TABLE BookedService ADD CONSTRAINT FKBookedServ274862 FOREIGN KEY (appointment_id) REFERENCES Appointment (id);
ALTER TABLE Appointment ADD CONSTRAINT FKAppointmen366296 FOREIGN KEY (customer_id) REFERENCES Customer (id);
ALTER TABLE Appointment ADD CONSTRAINT FKAppointmen157913 FOREIGN KEY (dentist_id) REFERENCES Dentist (id);
ALTER TABLE Appointment ADD CONSTRAINT FKAppointmen998789 FOREIGN KEY (slot_id) REFERENCES ClinicSlot (slot_id);
ALTER TABLE Appointment ADD CONSTRAINT FKAppointmen99798 FOREIGN KEY (clinic_id) REFERENCES Clinic (clinic_id);
