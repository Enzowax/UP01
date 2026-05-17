-- ============================================================
--  UP_ShutIKrol — чистый скрипт для SQL Server 2016-2022
--  Запускать от имени Администратора или через sa-аккаунт
-- ============================================================

USE master;
GO

-- Фикс формата дат для русской локали
SET DATEFORMAT YMD;
GO

-- Пересоздать БД если уже существует
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'UP_ShutIKrol')
BEGIN
    ALTER DATABASE [UP_ShutIKrol] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [UP_ShutIKrol];
END
GO

-- Создать БД без указания путей (SQL Server выберет папку по умолчанию)
CREATE DATABASE [UP_ShutIKrol];
GO

USE [UP_ShutIKrol];
GO

-- ============================================================
--  ТАБЛИЦЫ
-- ============================================================

CREATE TABLE [dbo].[Roles] (
    [Id]   INT          IDENTITY(1,1) NOT NULL,
    [Name] NVARCHAR(50) NOT NULL,
    CONSTRAINT [PK_Roles] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

CREATE TABLE [dbo].[Users] (
    [Id]               INT           IDENTITY(1,1) NOT NULL,
    [Name]             NVARCHAR(50)  NOT NULL,
    [Login]            NVARCHAR(50)  NOT NULL,
    [Password]         NVARCHAR(50)  NOT NULL,
    [Email]            NVARCHAR(50)  NOT NULL,
    [RoleId]           INT           NOT NULL,
    [IsFrozen]         BIT           NOT NULL,
    [RegistrationDate] DATETIME      NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

CREATE TABLE [dbo].[Books] (
    [Id]        INT           IDENTITY(1,1) NOT NULL,
    [Name]      NVARCHAR(50)  NOT NULL,
    [CoverPath] NVARCHAR(50)  NOT NULL,
    [AuthorId]  INT           NOT NULL,
    [IsFrozen]  BIT           NOT NULL,
    CONSTRAINT [PK_Books] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

CREATE TABLE [dbo].[Genres] (
    [Id]          INT           IDENTITY(1,1) NOT NULL,
    [Name]        NVARCHAR(50)  NOT NULL,
    [Description] NVARCHAR(255) NOT NULL,
    CONSTRAINT [PK_Genres] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

CREATE TABLE [dbo].[BookGenres] (
    [BookId]  INT NOT NULL,
    [GenreId] INT NOT NULL,
    CONSTRAINT [PK_BookGenres] PRIMARY KEY CLUSTERED ([BookId] ASC, [GenreId] ASC)
);
GO

CREATE TABLE [dbo].[Chapters] (
    [Id]     INT           IDENTITY(1,1) NOT NULL,
    [BookId] INT           NOT NULL,
    [Number] INT           NOT NULL,
    [Name]   NVARCHAR(50)  NOT NULL,
    [Path]   NVARCHAR(50)  NOT NULL,
    CONSTRAINT [PK_Chapters] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

CREATE TABLE [dbo].[Reviews] (
    [Id]           INT           IDENTITY(1,1) NOT NULL,
    [UserId]       INT           NOT NULL,
    [BookId]       INT           NOT NULL,
    [Text]         NVARCHAR(255) NOT NULL,
    [Rate]         TINYINT       NOT NULL,
    [CreationDate] DATETIME      NULL,
    CONSTRAINT [PK_Reviews]  PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [CK_Reviews]  CHECK ([Rate] >= 1 AND [Rate] <= 10)
);
GO

CREATE TABLE [dbo].[Complaints] (
    [Id]         INT           IDENTITY(1,1) NOT NULL,
    [UserId]     INT           NOT NULL,
    [BookId]     INT           NULL,
    [ReviewId]   INT           NULL,
    [ReasonText] NVARCHAR(255) NOT NULL,
    CONSTRAINT [PK_Complaints] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

CREATE TABLE [dbo].[ReadStatuses] (
    [Id]   INT          IDENTITY(1,1) NOT NULL,
    [Name] NVARCHAR(50) NOT NULL,
    CONSTRAINT [PK_ReadStatuses] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

CREATE TABLE [dbo].[ReadList] (
    [Id]       INT IDENTITY(1,1) NOT NULL,
    [UserId]   INT NOT NULL,
    [BookId]   INT NOT NULL,
    [StatusId] INT NOT NULL,
    CONSTRAINT [PK_ReadList] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

CREATE TABLE [dbo].[RequestTypes] (
    [Id]       INT          IDENTITY(1,1) NOT NULL,
    [TypeName] NVARCHAR(50) NOT NULL,
    CONSTRAINT [PK_RequestTypes] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

CREATE TABLE [dbo].[Requests] (
    [Id]      INT           IDENTITY(1,1) NOT NULL,
    [TypeId]  INT           NOT NULL,
    [UserId]  INT           NOT NULL,
    [Comment] NVARCHAR(255) NOT NULL,
    CONSTRAINT [PK_Requests] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

-- ============================================================
--  ВНЕШНИЕ КЛЮЧИ
-- ============================================================

ALTER TABLE [dbo].[Users]      ADD CONSTRAINT [FK_Users_Roles]              FOREIGN KEY ([RoleId])   REFERENCES [dbo].[Roles]       ([Id]);
ALTER TABLE [dbo].[Books]      ADD CONSTRAINT [FK_Books_Users]              FOREIGN KEY ([AuthorId]) REFERENCES [dbo].[Users]       ([Id]);
ALTER TABLE [dbo].[BookGenres] ADD CONSTRAINT [FK_BookGenres_Books]         FOREIGN KEY ([BookId])   REFERENCES [dbo].[Books]       ([Id]);
ALTER TABLE [dbo].[BookGenres] ADD CONSTRAINT [FK_BookGenres_Genres]        FOREIGN KEY ([GenreId])  REFERENCES [dbo].[Genres]      ([Id]);
ALTER TABLE [dbo].[Chapters]   ADD CONSTRAINT [FK_Chapters_Books]           FOREIGN KEY ([BookId])   REFERENCES [dbo].[Books]       ([Id]);
ALTER TABLE [dbo].[Reviews]    ADD CONSTRAINT [FK_Reviews_Books]            FOREIGN KEY ([BookId])   REFERENCES [dbo].[Books]       ([Id]);
ALTER TABLE [dbo].[Reviews]    ADD CONSTRAINT [FK_Reviews_Users]            FOREIGN KEY ([UserId])   REFERENCES [dbo].[Users]       ([Id]);
ALTER TABLE [dbo].[Complaints] ADD CONSTRAINT [FK_Complaints_Users]         FOREIGN KEY ([UserId])   REFERENCES [dbo].[Users]       ([Id]);
ALTER TABLE [dbo].[Complaints] ADD CONSTRAINT [FK_Complaints_Books]         FOREIGN KEY ([BookId])   REFERENCES [dbo].[Books]       ([Id]);
ALTER TABLE [dbo].[Complaints] ADD CONSTRAINT [FK_Complaints_Reviews]       FOREIGN KEY ([ReviewId]) REFERENCES [dbo].[Reviews]     ([Id]);
ALTER TABLE [dbo].[ReadList]   ADD CONSTRAINT [FK_ReadList_Users]           FOREIGN KEY ([UserId])   REFERENCES [dbo].[Users]       ([Id]);
ALTER TABLE [dbo].[ReadList]   ADD CONSTRAINT [FK_ReadList_Books]           FOREIGN KEY ([BookId])   REFERENCES [dbo].[Books]       ([Id]);
ALTER TABLE [dbo].[ReadList]   ADD CONSTRAINT [FK_ReadList_ReadStatuses]    FOREIGN KEY ([StatusId]) REFERENCES [dbo].[ReadStatuses]([Id]);
ALTER TABLE [dbo].[Requests]   ADD CONSTRAINT [FK_Requests_RequestTypes]    FOREIGN KEY ([TypeId])   REFERENCES [dbo].[RequestTypes]([Id]);
ALTER TABLE [dbo].[Requests]   ADD CONSTRAINT [FK_Requests_Users]           FOREIGN KEY ([UserId])   REFERENCES [dbo].[Users]       ([Id]);
GO

-- ============================================================
--  ДАННЫЕ: Roles
-- ============================================================

SET IDENTITY_INSERT [dbo].[Roles] ON;
INSERT [dbo].[Roles] ([Id],[Name]) VALUES (4, N'Admin');
INSERT [dbo].[Roles] ([Id],[Name]) VALUES (5, N'Author');
INSERT [dbo].[Roles] ([Id],[Name]) VALUES (6, N'Reader');
SET IDENTITY_INSERT [dbo].[Roles] OFF;
GO

-- ============================================================
--  ДАННЫЕ: Users
-- ============================================================

SET IDENTITY_INSERT [dbo].[Users] ON;
INSERT [dbo].[Users] ([Id],[Name],[Login],[Password],[Email],[RoleId],[IsFrozen],[RegistrationDate])
VALUES (3, N'Иван Петров',     N'ivan_p',   N'pass1234', N'ivan@mail.ru',        4, 0, CONVERT(datetime,'2023-01-15 10:00:00',120));
INSERT [dbo].[Users] ([Id],[Name],[Login],[Password],[Email],[RoleId],[IsFrozen],[RegistrationDate])
VALUES (4, N'Мария Сидорова',  N'masha_s',  N'qwerty99', N'masha@gmail.com',     5, 0, CONVERT(datetime,'2023-02-20 09:30:00',120));
INSERT [dbo].[Users] ([Id],[Name],[Login],[Password],[Email],[RoleId],[IsFrozen],[RegistrationDate])
VALUES (5, N'Алексей Волков',  N'alex_v',   N'wolf2023', N'alexv@yandex.ru',     5, 0, CONVERT(datetime,'2023-03-05 14:00:00',120));
INSERT [dbo].[Users] ([Id],[Name],[Login],[Password],[Email],[RoleId],[IsFrozen],[RegistrationDate])
VALUES (6, N'Ольга Кузнецова', N'olga_k',   N'olga5678', N'olga@mail.ru',        6, 0, CONVERT(datetime,'2023-04-10 11:00:00',120));
INSERT [dbo].[Users] ([Id],[Name],[Login],[Password],[Email],[RoleId],[IsFrozen],[RegistrationDate])
VALUES (7, N'Дмитрий Фомин',   N'dima_f',   N'dim12345', N'dima@gmail.com',      6, 0, CONVERT(datetime,'2023-05-01 08:00:00',120));
INSERT [dbo].[Users] ([Id],[Name],[Login],[Password],[Email],[RoleId],[IsFrozen],[RegistrationDate])
VALUES (8, N'Елена Новикова',  N'elena_n',  N'elena999', N'elena@inbox.ru',      6, 1, CONVERT(datetime,'2023-06-18 16:45:00',120));
INSERT [dbo].[Users] ([Id],[Name],[Login],[Password],[Email],[RoleId],[IsFrozen],[RegistrationDate])
VALUES (9, N'Сергей Орлов',    N'sergey_o', N'eagle777', N'sergey@mail.ru',      5, 0, CONVERT(datetime,'2023-07-22 12:00:00',120));
INSERT [dbo].[Users] ([Id],[Name],[Login],[Password],[Email],[RoleId],[IsFrozen],[RegistrationDate])
VALUES (10,N'Анна Беляева',    N'anna_b',   N'white123', N'anna@gmail.com',      6, 0, CONVERT(datetime,'2023-08-30 10:15:00',120));
SET IDENTITY_INSERT [dbo].[Users] OFF;
GO

-- ============================================================
--  ДАННЫЕ: Genres
-- ============================================================

SET IDENTITY_INSERT [dbo].[Genres] ON;
INSERT [dbo].[Genres] ([Id],[Name],[Description]) VALUES (1, N'Fantasy',        N'Magic, mythical creatures and imaginary worlds');
INSERT [dbo].[Genres] ([Id],[Name],[Description]) VALUES (2, N'Science Fiction', N'Future technologies, space and alternate realities');
INSERT [dbo].[Genres] ([Id],[Name],[Description]) VALUES (3, N'Detective',       N'Crime solving and mysteries');
INSERT [dbo].[Genres] ([Id],[Name],[Description]) VALUES (4, N'Romance',         N'Stories focused on love and relationships');
INSERT [dbo].[Genres] ([Id],[Name],[Description]) VALUES (5, N'Horror',          N'Dark and frightening stories meant to scare');
INSERT [dbo].[Genres] ([Id],[Name],[Description]) VALUES (6, N'History',         N'Historical events and figures');
INSERT [dbo].[Genres] ([Id],[Name],[Description]) VALUES (7, N'Thriller',        N'Suspenseful and fast-paced action stories');
INSERT [dbo].[Genres] ([Id],[Name],[Description]) VALUES (8, N'Biography',       N'Real life stories of notable people');
SET IDENTITY_INSERT [dbo].[Genres] OFF;
GO

-- ============================================================
--  ДАННЫЕ: Books
-- ============================================================

SET IDENTITY_INSERT [dbo].[Books] ON;
INSERT [dbo].[Books] ([Id],[Name],[CoverPath],[AuthorId],[IsFrozen])
VALUES (4,  N'Потерянное королевство', N'/covers/lost_kingdom.jpg',  4, 0);
INSERT [dbo].[Books] ([Id],[Name],[CoverPath],[AuthorId],[IsFrozen])
VALUES (5,  N'Звёзды за горизонтом',  N'/covers/stars_beyond.jpg',  5, 0);
INSERT [dbo].[Books] ([Id],[Name],[CoverPath],[AuthorId],[IsFrozen])
VALUES (6,  N'Теневой детектив',      N'/covers/shadow_det.jpg',    9, 0);
INSERT [dbo].[Books] ([Id],[Name],[CoverPath],[AuthorId],[IsFrozen])
VALUES (7,  N'Багряная роза',         N'/covers/crimson_rose.jpg',  4, 0);
INSERT [dbo].[Books] ([Id],[Name],[CoverPath],[AuthorId],[IsFrozen])
VALUES (8,  N'Тёмные коридоры',       N'/covers/dark_corr.jpg',     5, 1);
INSERT [dbo].[Books] ([Id],[Name],[CoverPath],[AuthorId],[IsFrozen])
VALUES (9,  N'Империя пыли',          N'/covers/empire_dust.jpg',   9, 0);
INSERT [dbo].[Books] ([Id],[Name],[CoverPath],[AuthorId],[IsFrozen])
VALUES (10, N'Стальное сердце',       N'/covers/steel_heart.jpg',   4, 0);
INSERT [dbo].[Books] ([Id],[Name],[CoverPath],[AuthorId],[IsFrozen])
VALUES (11, N'Эхо прошлого',          N'/covers/echoes.jpg',        5, 0);
SET IDENTITY_INSERT [dbo].[Books] OFF;
GO

-- ============================================================
--  ДАННЫЕ: BookGenres
-- ============================================================

INSERT [dbo].[BookGenres] ([BookId],[GenreId]) VALUES (4,1),(4,6);
INSERT [dbo].[BookGenres] ([BookId],[GenreId]) VALUES (5,2),(5,7);
INSERT [dbo].[BookGenres] ([BookId],[GenreId]) VALUES (6,3),(6,7);
INSERT [dbo].[BookGenres] ([BookId],[GenreId]) VALUES (7,4);
INSERT [dbo].[BookGenres] ([BookId],[GenreId]) VALUES (8,5);
INSERT [dbo].[BookGenres] ([BookId],[GenreId]) VALUES (9,1),(9,6);
INSERT [dbo].[BookGenres] ([BookId],[GenreId]) VALUES (10,2);
INSERT [dbo].[BookGenres] ([BookId],[GenreId]) VALUES (11,8);
GO

-- ============================================================
--  ДАННЫЕ: Chapters
-- ============================================================

SET IDENTITY_INSERT [dbo].[Chapters] ON;
INSERT [dbo].[Chapters] ([Id],[BookId],[Number],[Name],[Path]) VALUES (5,  4, 1, N'Начало',              N'/chapters/1/ch1.txt');
INSERT [dbo].[Chapters] ([Id],[BookId],[Number],[Name],[Path]) VALUES (6,  4, 2, N'В лес',               N'/chapters/1/ch2.txt');
INSERT [dbo].[Chapters] ([Id],[BookId],[Number],[Name],[Path]) VALUES (7,  4, 3, N'Древние руины',       N'/chapters/1/ch3.txt');
INSERT [dbo].[Chapters] ([Id],[BookId],[Number],[Name],[Path]) VALUES (8,  5, 1, N'Взлёт',               N'/chapters/2/ch1.txt');
INSERT [dbo].[Chapters] ([Id],[BookId],[Number],[Name],[Path]) VALUES (9,  5, 2, N'Далёкий космос',      N'/chapters/2/ch2.txt');
INSERT [dbo].[Chapters] ([Id],[BookId],[Number],[Name],[Path]) VALUES (10, 5, 3, N'Первый контакт',      N'/chapters/2/ch3.txt');
INSERT [dbo].[Chapters] ([Id],[BookId],[Number],[Name],[Path]) VALUES (11, 6, 1, N'Сцена преступления',  N'/chapters/3/ch1.txt');
INSERT [dbo].[Chapters] ([Id],[BookId],[Number],[Name],[Path]) VALUES (12, 6, 2, N'Подозреваемые',       N'/chapters/3/ch2.txt');
INSERT [dbo].[Chapters] ([Id],[BookId],[Number],[Name],[Path]) VALUES (13, 7, 1, N'Шанс встречи',        N'/chapters/4/ch1.txt');
INSERT [dbo].[Chapters] ([Id],[BookId],[Number],[Name],[Path]) VALUES (14, 7, 2, N'Растущие чувства',    N'/chapters/4/ch2.txt');
INSERT [dbo].[Chapters] ([Id],[BookId],[Number],[Name],[Path]) VALUES (15, 8, 1, N'Старый дом',          N'/chapters/5/ch1.txt');
INSERT [dbo].[Chapters] ([Id],[BookId],[Number],[Name],[Path]) VALUES (16, 8, 2, N'Странные звуки',      N'/chapters/5/ch2.txt');
INSERT [dbo].[Chapters] ([Id],[BookId],[Number],[Name],[Path]) VALUES (17, 9, 1, N'Империя встаёт',      N'/chapters/6/ch1.txt');
INSERT [dbo].[Chapters] ([Id],[BookId],[Number],[Name],[Path]) VALUES (18, 9, 2, N'Война и мир',         N'/chapters/6/ch2.txt');
INSERT [dbo].[Chapters] ([Id],[BookId],[Number],[Name],[Path]) VALUES (19,10, 1, N'Прототип',            N'/chapters/7/ch1.txt');
INSERT [dbo].[Chapters] ([Id],[BookId],[Number],[Name],[Path]) VALUES (20,11, 1, N'Ранние годы',         N'/chapters/8/ch1.txt');
INSERT [dbo].[Chapters] ([Id],[BookId],[Number],[Name],[Path]) VALUES (21,11, 2, N'Великое путешествие', N'/chapters/8/ch2.txt');
SET IDENTITY_INSERT [dbo].[Chapters] OFF;
GO

-- ============================================================
--  ДАННЫЕ: ReadStatuses
-- ============================================================

SET IDENTITY_INSERT [dbo].[ReadStatuses] ON;
INSERT [dbo].[ReadStatuses] ([Id],[Name]) VALUES (5, N'В планах');
INSERT [dbo].[ReadStatuses] ([Id],[Name]) VALUES (6, N'Читаю');
INSERT [dbo].[ReadStatuses] ([Id],[Name]) VALUES (7, N'Прочитано');
INSERT [dbo].[ReadStatuses] ([Id],[Name]) VALUES (8, N'Заброшено');
SET IDENTITY_INSERT [dbo].[ReadStatuses] OFF;
GO

-- ============================================================
--  ДАННЫЕ: ReadList
-- ============================================================

SET IDENTITY_INSERT [dbo].[ReadList] ON;
INSERT [dbo].[ReadList] ([Id],[UserId],[BookId],[StatusId]) VALUES (5,  6, 4,  7);
INSERT [dbo].[ReadList] ([Id],[UserId],[BookId],[StatusId]) VALUES (6,  6, 5,  7);
INSERT [dbo].[ReadList] ([Id],[UserId],[BookId],[StatusId]) VALUES (7,  6, 6,  6);
INSERT [dbo].[ReadList] ([Id],[UserId],[BookId],[StatusId]) VALUES (8,  7, 4,  7);
INSERT [dbo].[ReadList] ([Id],[UserId],[BookId],[StatusId]) VALUES (9,  7, 7,  5);
INSERT [dbo].[ReadList] ([Id],[UserId],[BookId],[StatusId]) VALUES (10, 7, 10, 8);
INSERT [dbo].[ReadList] ([Id],[UserId],[BookId],[StatusId]) VALUES (11, 8, 5,  6);
INSERT [dbo].[ReadList] ([Id],[UserId],[BookId],[StatusId]) VALUES (12,10, 8,  5);
INSERT [dbo].[ReadList] ([Id],[UserId],[BookId],[StatusId]) VALUES (13,10, 9,  7);
INSERT [dbo].[ReadList] ([Id],[UserId],[BookId],[StatusId]) VALUES (14,10,11,  6);
SET IDENTITY_INSERT [dbo].[ReadList] OFF;
GO

-- ============================================================
--  ДАННЫЕ: Reviews
-- ============================================================

SET IDENTITY_INSERT [dbo].[Reviews] ON;
INSERT [dbo].[Reviews] ([Id],[UserId],[BookId],[Text],[Rate],[CreationDate])
VALUES (9,  6, 4,  N'По правде насладился этой книгой',           8,  CONVERT(datetime,'2023-05-01 10:00:00',120));
INSERT [dbo].[Reviews] ([Id],[UserId],[BookId],[Text],[Rate],[CreationDate])
VALUES (10, 7, 4,  N'Очень скучно в начале, но развязка взорвала', 6,  CONVERT(datetime,'2023-05-12 14:30:00',120));
INSERT [dbo].[Reviews] ([Id],[UserId],[BookId],[Text],[Rate],[CreationDate])
VALUES (11, 6, 5,  N'Невероятная фантастика, рекомендую',         10,  CONVERT(datetime,'2023-05-20 09:15:00',120));
INSERT [dbo].[Reviews] ([Id],[UserId],[BookId],[Text],[Rate],[CreationDate])
VALUES (12, 8, 6,  N'Ничего нового',                               5,  CONVERT(datetime,'2023-06-01 11:00:00',120));
INSERT [dbo].[Reviews] ([Id],[UserId],[BookId],[Text],[Rate],[CreationDate])
VALUES (13,10, 6,  N'Книга будоражила весь период',                9,  CONVERT(datetime,'2023-06-15 16:45:00',120));
INSERT [dbo].[Reviews] ([Id],[UserId],[BookId],[Text],[Rate],[CreationDate])
VALUES (14, 6, 7,  N'Сладкий романс, чувствуется реальность',     7,  CONVERT(datetime,'2023-07-02 08:30:00',120));
INSERT [dbo].[Reviews] ([Id],[UserId],[BookId],[Text],[Rate],[CreationDate])
VALUES (15, 7, 8,  N'Слишком страшно для меня, но написано хорошо',7, CONVERT(datetime,'2023-07-18 13:00:00',120));
INSERT [dbo].[Reviews] ([Id],[UserId],[BookId],[Text],[Rate],[CreationDate])
VALUES (16,10, 9,  N'Исторически богато',                          8,  CONVERT(datetime,'2023-08-01 09:00:00',120));
INSERT [dbo].[Reviews] ([Id],[UserId],[BookId],[Text],[Rate],[CreationDate])
VALUES (17, 6,10,  N'Интересный концепт но скучный конец',         6,  CONVERT(datetime,'2023-08-22 17:20:00',120));
INSERT [dbo].[Reviews] ([Id],[UserId],[BookId],[Text],[Rate],[CreationDate])
VALUES (18, 7,11,  N'Очень завораживающая биография',              9,  CONVERT(datetime,'2023-09-10 12:00:00',120));
INSERT [dbo].[Reviews] ([Id],[UserId],[BookId],[Text],[Rate],[CreationDate])
VALUES (20, 9, 5,  N'Топ!',                                       10,  CONVERT(datetime,'2023-10-11 15:30:00',120));
SET IDENTITY_INSERT [dbo].[Reviews] OFF;
GO

-- ============================================================
--  ДАННЫЕ: Complaints
-- ============================================================

SET IDENTITY_INSERT [dbo].[Complaints] ON;
INSERT [dbo].[Complaints] ([Id],[UserId],[BookId],[ReviewId],[ReasonText])
VALUES (8,  4, 8,    NULL, N'Эта книга содержит непозволительный контент');
INSERT [dbo].[Complaints] ([Id],[UserId],[BookId],[ReviewId],[ReasonText])
VALUES (9,  5, NULL, 11,   N'Отзыв содержит спойлер без предупреждения');
INSERT [dbo].[Complaints] ([Id],[UserId],[BookId],[ReviewId],[ReasonText])
VALUES (10, 8, 4,    NULL, N'Обложка украдена с другого источника');
INSERT [dbo].[Complaints] ([Id],[UserId],[BookId],[ReviewId],[ReasonText])
VALUES (11, 4, NULL, 14,   N'Проплаченный отзыв');
SET IDENTITY_INSERT [dbo].[Complaints] OFF;
GO

-- ============================================================
--  ДАННЫЕ: RequestTypes
-- ============================================================

SET IDENTITY_INSERT [dbo].[RequestTypes] ON;
INSERT [dbo].[RequestTypes] ([Id],[TypeName]) VALUES (3, N'BookUnfrozing');
INSERT [dbo].[RequestTypes] ([Id],[TypeName]) VALUES (4, N'AccountUnfrozing');
INSERT [dbo].[RequestTypes] ([Id],[TypeName]) VALUES (5, N'AuthorRoleRequest');
SET IDENTITY_INSERT [dbo].[RequestTypes] OFF;
GO

-- ============================================================
--  ДАННЫЕ: Requests
-- ============================================================

SET IDENTITY_INSERT [dbo].[Requests] ON;
INSERT [dbo].[Requests] ([Id],[TypeId],[UserId],[Comment])
VALUES (4,  3, 4, N'Пожалуйста разморозьте мою книгу');
INSERT [dbo].[Requests] ([Id],[TypeId],[UserId],[Comment])
VALUES (5,  3, 5, N'Запрещённые материалы зацензурены');
INSERT [dbo].[Requests] ([Id],[TypeId],[UserId],[Comment])
VALUES (6,  3, 9, N'Я больше так не буду');
INSERT [dbo].[Requests] ([Id],[TypeId],[UserId],[Comment])
VALUES (7,  4, 3, N'Мой аккаунт взломали, я не буду спамить');
INSERT [dbo].[Requests] ([Id],[TypeId],[UserId],[Comment])
VALUES (8,  4, 4, N'Я честно исправился');
SET IDENTITY_INSERT [dbo].[Requests] OFF;
GO

-- ============================================================
PRINT 'База данных UP_ShutIKrol успешно создана!';
PRINT '';
PRINT 'Тестовые аккаунты:';
PRINT '  Admin:  ivan_p   / pass1234';
PRINT '  Author: masha_s  / qwerty99';
PRINT '  Author: alex_v   / wolf2023';
PRINT '  Reader: olga_k   / olga5678';
PRINT '  Frozen: elena_n  / elena999';
GO
