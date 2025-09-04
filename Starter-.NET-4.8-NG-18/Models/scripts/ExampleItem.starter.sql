CREATE DATABASE example1;

USE example1;

CREATE TABLE [dbo].[ExampleItem] (
    [ExampleItemID] INT IDENTITY(1,1) PRIMARY KEY,
    [Name] NVARCHAR(150) NULL,
    [Thing] NVARCHAR(150) NULL,
);

insert into [dbo].[ExampleItem] ([Name], [Thing])
values ('rabi nachman', 'tzadik yessod olam'), ('Bresleveloper Digital', 'Best int the Galaxy'), ('ariel', 'steak')