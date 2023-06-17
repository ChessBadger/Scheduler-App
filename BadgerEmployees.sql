USE master
GO

IF DB_ID('BadgerEmployees') IS NOT NULL 
DROP DATABASE BadgerEmployees;
GO

CREATE DATABASE BadgerEmployees;
GO

USE BadgerEmployees
GO

CREATE TABLE Employees (
    EmployeeID INT PRIMARY KEY ,
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    PerformanceRating INT NOT NULL,
    AttendanceRating INT NOT NULL,
    IsSupervisor BIT NOT NULL
)
