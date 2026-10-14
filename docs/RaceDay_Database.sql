/* =========================================================
   RaceDay Database Script - Part 1
   ========================================================= */
 
-- Drop tables in reverse dependency order so re-running this script never fails
IF OBJECT_ID('Results', 'U') IS NOT NULL DROP TABLE Results;
IF OBJECT_ID('Enrolments', 'U') IS NOT NULL DROP TABLE Enrolments;
IF OBJECT_ID('Categories', 'U') IS NOT NULL DROP TABLE Categories;
IF OBJECT_ID('Events', 'U') IS NOT NULL DROP TABLE Events;
IF OBJECT_ID('Users', 'U') IS NOT NULL DROP TABLE Users;
IF OBJECT_ID('Roles', 'U') IS NOT NULL DROP TABLE Roles;
GO
 
-- =========================================================
-- 1. Roles - lookup table for the two user roles
-- =========================================================
CREATE TABLE Roles (
    RoleID INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(20) NOT NULL UNIQUE
);
GO
 
-- =========================================================
-- 2. Users - both Organisers and Participants live here,
--    distinguished by RoleID
-- =========================================================
CREATE TABLE Users (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    RoleID INT NOT NULL,
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(150) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(256) NOT NULL,
    ProfilePictureUrl NVARCHAR(300) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleID) REFERENCES Roles(RoleID)
);
GO
 
-- =========================================================
-- 3. Events - created and managed by an Organiser
-- =========================================================
CREATE TABLE Events (
    EventID INT IDENTITY(1,1) PRIMARY KEY,
    OrganiserID INT NOT NULL,
    Name NVARCHAR(150) NOT NULL,
    Description NVARCHAR(1000) NULL,
    EventDate DATE NOT NULL,
    Location NVARCHAR(150) NOT NULL,
    DistanceKm DECIMAL(6,2) NOT NULL,
    EventType NVARCHAR(20) NOT NULL CHECK (EventType IN ('run', 'walk', 'cycle')),
    BannerImageUrl NVARCHAR(300) NULL,
    CONSTRAINT FK_Events_Users FOREIGN KEY (OrganiserID) REFERENCES Users(UserID)
);
GO
 
-- =========================================================
-- 4. Categories - age or distance categories per event
-- =========================================================
CREATE TABLE Categories (
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    EventID INT NOT NULL,
    CategoryName NVARCHAR(50) NOT NULL,
    MinAge INT NULL,
    MaxAge INT NULL,
    DistanceKm DECIMAL(6,2) NULL,
    CONSTRAINT FK_Categories_Events FOREIGN KEY (EventID) REFERENCES Events(EventID)
);
GO
 
-- =========================================================
-- 5. Enrolments - links a Participant to an Event + Category
-- =========================================================
CREATE TABLE Enrolments (
    EnrolmentID INT IDENTITY(1,1) PRIMARY KEY,
    ParticipantID INT NOT NULL,
    EventID INT NOT NULL,
    CategoryID INT NOT NULL,
    EnrolmentDate DATETIME NOT NULL DEFAULT GETDATE(),
    Status NVARCHAR(20) NOT NULL DEFAULT 'Confirmed' CHECK (Status IN ('Confirmed', 'Pending', 'Cancelled')),
    CONSTRAINT FK_Enrolments_Users FOREIGN KEY (ParticipantID) REFERENCES Users(UserID),
    CONSTRAINT FK_Enrolments_Events FOREIGN KEY (EventID) REFERENCES Events(EventID),
    CONSTRAINT FK_Enrolments_Categories FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID),
    CONSTRAINT UQ_Enrolment UNIQUE (ParticipantID, EventID) -- a participant can't enrol in the same event twice
);
GO
 
-- =========================================================
-- 6. Results - one result per completed enrolment
-- =========================================================
CREATE TABLE Results (
    ResultID INT IDENTITY(1,1) PRIMARY KEY,
    EnrolmentID INT NOT NULL UNIQUE, -- unique enforces the 1-to-1 relationship shown in the ERD
    FinishTime TIME NOT NULL,
    FinishPosition INT NOT NULL,
    RecordedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Results_Enrolments FOREIGN KEY (EnrolmentID) REFERENCES Enrolments(EnrolmentID)
);
GO
 
/* =========================================================
   SEED DATA
   2 Organisers, 2 Participants, 3 Events, categories for
   each event, and sample enrolments - as required by the brief
   ========================================================= */
 
-- Roles
INSERT INTO Roles (RoleName) VALUES ('Organiser'), ('Participant');
GO
 
-- Users: 2 Organisers, 2 Participants
INSERT INTO Users (RoleID, FullName, Email, PasswordHash, ProfilePictureUrl) VALUES
(1, 'Eli Basson',    'eli.basson@raceday.co.za',    'hashed_password_1', NULL),
(1, 'Lindsay Dube',   'lindsay.dube@raceday.co.za',   'hashed_password_2', NULL),
(2, 'Sarah van Wyk',  'sarah.vanwyk@raceday.co.za',   'hashed_password_3', NULL),
(2, 'Michael Petersen','michael.petersen@raceday.co.za','hashed_password_4', NULL);
GO
 
-- Events: 3 events, created by the 2 Organisers (UserID 1 and 2)
INSERT INTO Events (OrganiserID, Name, Description, EventDate, Location, DistanceKm, EventType, BannerImageUrl) VALUES
(1, 'Cape Town Beachfront 10K',   'A scenic 10km run along the Cape Town beachfront.', '2026-11-14', 'Cape Town', 10.0, 'run',   NULL),
(1, 'Joburg Community Fun Walk',  'A relaxed 5km community fun walk.',                  '2026-10-05', 'Johannesburg', 5.0, 'walk', NULL),
(2, 'Durban Cycle Classic',       'A challenging 40km road cycling event.',             '2026-12-02', 'Durban', 40.0, 'cycle', NULL);
GO
 
-- Categories: at least one category per event
INSERT INTO Categories (EventID, CategoryName, MinAge, MaxAge, DistanceKm) VALUES
(1, 'Under 20',   NULL, 19, 10.0),
(1, 'Senior',     20,   59, 10.0),
(2, 'Family',     NULL, NULL, 5.0),
(3, '21km Category', 18, NULL, 21.0),
(3, '40km Category', 18, NULL, 40.0);
GO
 
-- Enrolments: sample participants enrolling in events
INSERT INTO Enrolments (ParticipantID, EventID, CategoryID, Status) VALUES
(3, 1, 2, 'Confirmed'),  -- Sarah enrols in the Cape Town 10K, Senior category
(4, 1, 2, 'Confirmed'),  -- Michael enrols in the Cape Town 10K, Senior category
(3, 2, 3, 'Confirmed'),  -- Sarah enrols in the Joburg Fun Walk, Family category
(4, 3, 5, 'Confirmed');  -- Michael enrols in the Durban Cycle Classic, 40km category
GO
 
-- Results: sample finish times for completed enrolments
INSERT INTO Results (EnrolmentID, FinishTime, FinishPosition) VALUES
(1, '00:52:30', 47),
(2, '00:55:10', 63);
GO
   
 Select * From Roles
 Select * From Users
 Select * From Events
 Select * From Categories
 Select * From Enrolments
 Select * From Results
