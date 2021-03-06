USE [master]
GO
/****** Object:  Database [MovieRatingCalculator]    Script Date: 01/22/2013 23:39:42 ******/
CREATE DATABASE [MovieRatingCalculator] ON  PRIMARY 
( NAME = N'MovieRatingCalculator', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL10.SQLEXPRESS\MSSQL\DATA\MovieRatingCalculator.mdf' , SIZE = 3072KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
 LOG ON 
( NAME = N'MovieRatingCalculator_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL10.SQLEXPRESS\MSSQL\DATA\MovieRatingCalculator_log.ldf' , SIZE = 1024KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)
GO
ALTER DATABASE [MovieRatingCalculator] SET COMPATIBILITY_LEVEL = 100
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [MovieRatingCalculator].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [MovieRatingCalculator] SET ANSI_NULL_DEFAULT OFF
GO
ALTER DATABASE [MovieRatingCalculator] SET ANSI_NULLS OFF
GO
ALTER DATABASE [MovieRatingCalculator] SET ANSI_PADDING OFF
GO
ALTER DATABASE [MovieRatingCalculator] SET ANSI_WARNINGS OFF
GO
ALTER DATABASE [MovieRatingCalculator] SET ARITHABORT OFF
GO
ALTER DATABASE [MovieRatingCalculator] SET AUTO_CLOSE OFF
GO
ALTER DATABASE [MovieRatingCalculator] SET AUTO_CREATE_STATISTICS ON
GO
ALTER DATABASE [MovieRatingCalculator] SET AUTO_SHRINK OFF
GO
ALTER DATABASE [MovieRatingCalculator] SET AUTO_UPDATE_STATISTICS ON
GO
ALTER DATABASE [MovieRatingCalculator] SET CURSOR_CLOSE_ON_COMMIT OFF
GO
ALTER DATABASE [MovieRatingCalculator] SET CURSOR_DEFAULT  GLOBAL
GO
ALTER DATABASE [MovieRatingCalculator] SET CONCAT_NULL_YIELDS_NULL OFF
GO
ALTER DATABASE [MovieRatingCalculator] SET NUMERIC_ROUNDABORT OFF
GO
ALTER DATABASE [MovieRatingCalculator] SET QUOTED_IDENTIFIER OFF
GO
ALTER DATABASE [MovieRatingCalculator] SET RECURSIVE_TRIGGERS OFF
GO
ALTER DATABASE [MovieRatingCalculator] SET  DISABLE_BROKER
GO
ALTER DATABASE [MovieRatingCalculator] SET AUTO_UPDATE_STATISTICS_ASYNC OFF
GO
ALTER DATABASE [MovieRatingCalculator] SET DATE_CORRELATION_OPTIMIZATION OFF
GO
ALTER DATABASE [MovieRatingCalculator] SET TRUSTWORTHY OFF
GO
ALTER DATABASE [MovieRatingCalculator] SET ALLOW_SNAPSHOT_ISOLATION OFF
GO
ALTER DATABASE [MovieRatingCalculator] SET PARAMETERIZATION SIMPLE
GO
ALTER DATABASE [MovieRatingCalculator] SET READ_COMMITTED_SNAPSHOT OFF
GO
ALTER DATABASE [MovieRatingCalculator] SET HONOR_BROKER_PRIORITY OFF
GO
ALTER DATABASE [MovieRatingCalculator] SET  READ_WRITE
GO
ALTER DATABASE [MovieRatingCalculator] SET RECOVERY SIMPLE
GO
ALTER DATABASE [MovieRatingCalculator] SET  MULTI_USER
GO
ALTER DATABASE [MovieRatingCalculator] SET PAGE_VERIFY CHECKSUM
GO
ALTER DATABASE [MovieRatingCalculator] SET DB_CHAINING OFF
GO
USE [MovieRatingCalculator]
GO
/****** Object:  Table [dbo].[ParticipantTypes]    Script Date: 01/22/2013 23:39:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ParticipantTypes](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_ParticipantTypes] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY],
 CONSTRAINT [UniqueParticipantTypesConstraint] UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[ParticipantTypes] ON
INSERT [dbo].[ParticipantTypes] ([Id], [Name]) VALUES (1, N'Director')
INSERT [dbo].[ParticipantTypes] ([Id], [Name]) VALUES (2, N'Actor')
SET IDENTITY_INSERT [dbo].[ParticipantTypes] OFF
/****** Object:  Table [dbo].[Movies]    Script Date: 01/22/2013 23:39:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Movies](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
	[OriginalName] [nvarchar](max) NULL,
	[ReleaseYear] [smallint] NOT NULL,
	[Duration] [smallint] NULL,
	[KinopoiskMovieId] [nvarchar](max) NOT NULL,
	[KinopoiskMovieRating] [float] NULL,
	[KinopoiskNumberUsersRate] [int] NULL,
 CONSTRAINT [PK_Movies] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Genres]    Script Date: 01/22/2013 23:39:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Genres](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_Genres] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY],
 CONSTRAINT [UniqueGenreConstraint] UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_Genres] ON [dbo].[Genres] 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Countries]    Script Date: 01/22/2013 23:39:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Countries](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_Countries] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY],
 CONSTRAINT [UniqueCountriesConstraint] UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MovieParticipants]    Script Date: 01/22/2013 23:39:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MovieParticipants](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
 CONSTRAINT [PK_MovieParticipants] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY],
 CONSTRAINT [UniqueMovieParticipantsConstraint] UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Users]    Script Date: 01/22/2013 23:39:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Email] [nvarchar](max) NOT NULL,
	[FirstName] [nvarchar](max) NULL,
	[LastName] [nvarchar](max) NULL,
 CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserLoginHistory]    Script Date: 01/22/2013 23:39:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserLoginHistory](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[IpAddress] [nvarchar](20) NULL,
	[LoginDate] [datetime] NOT NULL,
 CONSTRAINT [PK_UserLoginHistory] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MovieProductionCountries]    Script Date: 01/22/2013 23:39:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MovieProductionCountries](
	[MovieId] [int] NOT NULL,
	[CountryId] [int] NOT NULL,
 CONSTRAINT [PK_MovieCountry] PRIMARY KEY NONCLUSTERED 
(
	[MovieId] ASC,
	[CountryId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_FK_MovieCountry_Country] ON [dbo].[MovieProductionCountries] 
(
	[CountryId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MovieParticipantTypes]    Script Date: 01/22/2013 23:39:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MovieParticipantTypes](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[MovieParticipantId] [int] NOT NULL,
	[ParticipantTypeId] [int] NOT NULL,
 CONSTRAINT [PK_MovieParticipantTypes] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_FK_MovieParticipantTypeMovieParticipant] ON [dbo].[MovieParticipantTypes] 
(
	[MovieParticipantId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_FK_MovieParticipantTypeParticipantType] ON [dbo].[MovieParticipantTypes] 
(
	[ParticipantTypeId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MovieGenres]    Script Date: 01/22/2013 23:39:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MovieGenres](
	[MovieId] [int] NOT NULL,
	[GenreId] [int] NOT NULL,
 CONSTRAINT [PK_GenreMovie] PRIMARY KEY NONCLUSTERED 
(
	[GenreId] ASC,
	[MovieId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_FK_GenreMovie_Movie] ON [dbo].[MovieGenres] 
(
	[MovieId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MovieRatings]    Script Date: 01/22/2013 23:39:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MovieRatings](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[MovieId] [int] NOT NULL,
	[UserId] [int] NOT NULL,
	[Rating] [smallint] NOT NULL,
	[UpdateDate] [datetime] NOT NULL,
 CONSTRAINT [PK_MovieRatings] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_FK_MovieRatingMovie] ON [dbo].[MovieRatings] 
(
	[MovieId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_FK_MovieRatingUser] ON [dbo].[MovieRatings] 
(
	[UserId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MovieProductionParticipants]    Script Date: 01/22/2013 23:39:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MovieProductionParticipants](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[MovieId] [int] NOT NULL,
	[MovieParticipantTypeId] [int] NOT NULL,
 CONSTRAINT [PK_MovieProductionParticipants] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_FK_MovieProductionParticipantMovieParticipantType] ON [dbo].[MovieProductionParticipants] 
(
	[MovieParticipantTypeId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_FK_MovieProductionParticipantTypeMovie] ON [dbo].[MovieProductionParticipants] 
(
	[MovieId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
/****** Object:  ForeignKey [FK_UserLoginHistory_Users]    Script Date: 01/22/2013 23:39:47 ******/
ALTER TABLE [dbo].[UserLoginHistory]  WITH CHECK ADD  CONSTRAINT [FK_UserLoginHistory_Users] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[UserLoginHistory] CHECK CONSTRAINT [FK_UserLoginHistory_Users]
GO
/****** Object:  ForeignKey [FK_MovieCountry_Country]    Script Date: 01/22/2013 23:39:47 ******/
ALTER TABLE [dbo].[MovieProductionCountries]  WITH CHECK ADD  CONSTRAINT [FK_MovieCountry_Country] FOREIGN KEY([CountryId])
REFERENCES [dbo].[Countries] ([Id])
GO
ALTER TABLE [dbo].[MovieProductionCountries] CHECK CONSTRAINT [FK_MovieCountry_Country]
GO
/****** Object:  ForeignKey [FK_MovieCountry_Movie]    Script Date: 01/22/2013 23:39:47 ******/
ALTER TABLE [dbo].[MovieProductionCountries]  WITH CHECK ADD  CONSTRAINT [FK_MovieCountry_Movie] FOREIGN KEY([MovieId])
REFERENCES [dbo].[Movies] ([Id])
GO
ALTER TABLE [dbo].[MovieProductionCountries] CHECK CONSTRAINT [FK_MovieCountry_Movie]
GO
/****** Object:  ForeignKey [FK_MovieParticipantTypeMovieParticipant]    Script Date: 01/22/2013 23:39:47 ******/
ALTER TABLE [dbo].[MovieParticipantTypes]  WITH CHECK ADD  CONSTRAINT [FK_MovieParticipantTypeMovieParticipant] FOREIGN KEY([MovieParticipantId])
REFERENCES [dbo].[MovieParticipants] ([Id])
GO
ALTER TABLE [dbo].[MovieParticipantTypes] CHECK CONSTRAINT [FK_MovieParticipantTypeMovieParticipant]
GO
/****** Object:  ForeignKey [FK_MovieParticipantTypeParticipantType]    Script Date: 01/22/2013 23:39:47 ******/
ALTER TABLE [dbo].[MovieParticipantTypes]  WITH CHECK ADD  CONSTRAINT [FK_MovieParticipantTypeParticipantType] FOREIGN KEY([ParticipantTypeId])
REFERENCES [dbo].[ParticipantTypes] ([Id])
GO
ALTER TABLE [dbo].[MovieParticipantTypes] CHECK CONSTRAINT [FK_MovieParticipantTypeParticipantType]
GO
/****** Object:  ForeignKey [FK_GenreMovie_Genre]    Script Date: 01/22/2013 23:39:47 ******/
ALTER TABLE [dbo].[MovieGenres]  WITH CHECK ADD  CONSTRAINT [FK_GenreMovie_Genre] FOREIGN KEY([GenreId])
REFERENCES [dbo].[Genres] ([Id])
GO
ALTER TABLE [dbo].[MovieGenres] CHECK CONSTRAINT [FK_GenreMovie_Genre]
GO
/****** Object:  ForeignKey [FK_GenreMovie_Movie]    Script Date: 01/22/2013 23:39:47 ******/
ALTER TABLE [dbo].[MovieGenres]  WITH CHECK ADD  CONSTRAINT [FK_GenreMovie_Movie] FOREIGN KEY([MovieId])
REFERENCES [dbo].[Movies] ([Id])
GO
ALTER TABLE [dbo].[MovieGenres] CHECK CONSTRAINT [FK_GenreMovie_Movie]
GO
/****** Object:  ForeignKey [FK_MovieRatingMovie]    Script Date: 01/22/2013 23:39:47 ******/
ALTER TABLE [dbo].[MovieRatings]  WITH CHECK ADD  CONSTRAINT [FK_MovieRatingMovie] FOREIGN KEY([MovieId])
REFERENCES [dbo].[Movies] ([Id])
GO
ALTER TABLE [dbo].[MovieRatings] CHECK CONSTRAINT [FK_MovieRatingMovie]
GO
/****** Object:  ForeignKey [FK_MovieRatingUser]    Script Date: 01/22/2013 23:39:47 ******/
ALTER TABLE [dbo].[MovieRatings]  WITH CHECK ADD  CONSTRAINT [FK_MovieRatingUser] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[MovieRatings] CHECK CONSTRAINT [FK_MovieRatingUser]
GO
/****** Object:  ForeignKey [FK_MovieProductionParticipantMovieParticipantType]    Script Date: 01/22/2013 23:39:47 ******/
ALTER TABLE [dbo].[MovieProductionParticipants]  WITH CHECK ADD  CONSTRAINT [FK_MovieProductionParticipantMovieParticipantType] FOREIGN KEY([MovieParticipantTypeId])
REFERENCES [dbo].[MovieParticipantTypes] ([Id])
GO
ALTER TABLE [dbo].[MovieProductionParticipants] CHECK CONSTRAINT [FK_MovieProductionParticipantMovieParticipantType]
GO
/****** Object:  ForeignKey [FK_MovieProductionParticipantTypeMovie]    Script Date: 01/22/2013 23:39:47 ******/
ALTER TABLE [dbo].[MovieProductionParticipants]  WITH CHECK ADD  CONSTRAINT [FK_MovieProductionParticipantTypeMovie] FOREIGN KEY([MovieId])
REFERENCES [dbo].[Movies] ([Id])
GO
ALTER TABLE [dbo].[MovieProductionParticipants] CHECK CONSTRAINT [FK_MovieProductionParticipantTypeMovie]
GO
