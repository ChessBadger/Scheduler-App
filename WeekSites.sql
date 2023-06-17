USE master
GO

IF DB_ID('WeekSites') IS NOT NULL 
DROP DATABASE WeekSites;
GO

CREATE DATABASE WeekSites;
GO

USE WeekSites
GO

CREATE TABLE SundaySites (
    SiteID INT PRIMARY KEY IDENTITY(1,1),
    Location VARCHAR(50),
    StartTime TIME,
    PeopleRequired INT,
    Priority VARCHAR(10),
	Supervisor VARCHAR(50)
);

CREATE TABLE MondaySites (
    SiteID INT PRIMARY KEY IDENTITY(1,1),
    Location VARCHAR(50),
    StartTime TIME,
    PeopleRequired INT,
    Priority VARCHAR(10),
	Supervisor VARCHAR(50)
);

CREATE TABLE TuesdaySites (
    SiteID INT PRIMARY KEY IDENTITY(1,1),
    Location VARCHAR(50),
    StartTime TIME,
    PeopleRequired INT,
    Priority VARCHAR(10),
	Supervisor VARCHAR(50)
);

CREATE TABLE WednesdaySites (
    SiteID INT PRIMARY KEY IDENTITY(1,1),
    Location VARCHAR(50),
    StartTime TIME,
    PeopleRequired INT,
    Priority VARCHAR(10),
	Supervisor VARCHAR(50)
);

CREATE TABLE ThursdaySites (
    SiteID INT PRIMARY KEY IDENTITY(1,1),
    Location VARCHAR(50),
    StartTime TIME,
    PeopleRequired INT,
    Priority VARCHAR(10),
	Supervisor VARCHAR(50)
);

CREATE TABLE FridaySites (
    SiteID INT PRIMARY KEY IDENTITY(1,1),
    Location VARCHAR(50),
    StartTime TIME,
    PeopleRequired INT,
    Priority VARCHAR(10),
	Supervisor VARCHAR(50)
);

CREATE TABLE SaturdaySites (
    SiteID INT PRIMARY KEY IDENTITY(1,1),
    Location VARCHAR(50),
    StartTime TIME,
    PeopleRequired INT,
    Priority VARCHAR(10),
	Supervisor VARCHAR(50)
);
