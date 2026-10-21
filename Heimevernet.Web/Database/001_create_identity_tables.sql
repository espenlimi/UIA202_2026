-- ASP.NET Core Identity schema for MariaDB. The application applies an equivalent EF Core migration at startup.
-- This script is useful when setting up a database manually. It is safe to run on an empty database.

CREATE TABLE IF NOT EXISTS AspNetRoles (
    Id varchar(255) NOT NULL,
    Name varchar(256) NULL,
    NormalizedName varchar(256) NULL,
    ConcurrencyStamp longtext NULL,
    CONSTRAINT PK_AspNetRoles PRIMARY KEY (Id)
) CHARACTER SET=utf8mb4;

CREATE UNIQUE INDEX RoleNameIndex ON AspNetRoles (NormalizedName);

CREATE TABLE IF NOT EXISTS AspNetUsers (
    Id varchar(255) NOT NULL,
    UserName varchar(256) NULL,
    NormalizedUserName varchar(256) NULL,
    Email varchar(256) NULL,
    NormalizedEmail varchar(256) NULL,
    EmailConfirmed tinyint(1) NOT NULL,
    PasswordHash longtext NULL,
    SecurityStamp longtext NULL,
    ConcurrencyStamp longtext NULL,
    PhoneNumber longtext NULL,
    PhoneNumberConfirmed tinyint(1) NOT NULL,
    TwoFactorEnabled tinyint(1) NOT NULL,
    LockoutEnd datetime(6) NULL,
    LockoutEnabled tinyint(1) NOT NULL,
    AccessFailedCount int NOT NULL,
    CONSTRAINT PK_AspNetUsers PRIMARY KEY (Id)
) CHARACTER SET=utf8mb4;

CREATE INDEX EmailIndex ON AspNetUsers (NormalizedEmail);
CREATE UNIQUE INDEX UserNameIndex ON AspNetUsers (NormalizedUserName);

CREATE TABLE IF NOT EXISTS AspNetRoleClaims (
    Id int NOT NULL AUTO_INCREMENT,
    RoleId varchar(255) NOT NULL,
    ClaimType longtext NULL,
    ClaimValue longtext NULL,
    CONSTRAINT PK_AspNetRoleClaims PRIMARY KEY (Id),
    CONSTRAINT FK_AspNetRoleClaims_AspNetRoles_RoleId FOREIGN KEY (RoleId) REFERENCES AspNetRoles (Id) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE INDEX IX_AspNetRoleClaims_RoleId ON AspNetRoleClaims (RoleId);

CREATE TABLE IF NOT EXISTS AspNetUserClaims (
    Id int NOT NULL AUTO_INCREMENT,
    UserId varchar(255) NOT NULL,
    ClaimType longtext NULL,
    ClaimValue longtext NULL,
    CONSTRAINT PK_AspNetUserClaims PRIMARY KEY (Id),
    CONSTRAINT FK_AspNetUserClaims_AspNetUsers_UserId FOREIGN KEY (UserId) REFERENCES AspNetUsers (Id) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE INDEX IX_AspNetUserClaims_UserId ON AspNetUserClaims (UserId);

CREATE TABLE IF NOT EXISTS AspNetUserLogins (
    LoginProvider varchar(255) NOT NULL,
    ProviderKey varchar(255) NOT NULL,
    ProviderDisplayName longtext NULL,
    UserId varchar(255) NOT NULL,
    CONSTRAINT PK_AspNetUserLogins PRIMARY KEY (LoginProvider, ProviderKey),
    CONSTRAINT FK_AspNetUserLogins_AspNetUsers_UserId FOREIGN KEY (UserId) REFERENCES AspNetUsers (Id) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE INDEX IX_AspNetUserLogins_UserId ON AspNetUserLogins (UserId);

CREATE TABLE IF NOT EXISTS AspNetUserRoles (
    UserId varchar(255) NOT NULL,
    RoleId varchar(255) NOT NULL,
    CONSTRAINT PK_AspNetUserRoles PRIMARY KEY (UserId, RoleId),
    CONSTRAINT FK_AspNetUserRoles_AspNetRoles_RoleId FOREIGN KEY (RoleId) REFERENCES AspNetRoles (Id) ON DELETE CASCADE,
    CONSTRAINT FK_AspNetUserRoles_AspNetUsers_UserId FOREIGN KEY (UserId) REFERENCES AspNetUsers (Id) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE INDEX IX_AspNetUserRoles_RoleId ON AspNetUserRoles (RoleId);

CREATE TABLE IF NOT EXISTS AspNetUserTokens (
    UserId varchar(255) NOT NULL,
    LoginProvider varchar(255) NOT NULL,
    Name varchar(255) NOT NULL,
    Value longtext NULL,
    CONSTRAINT PK_AspNetUserTokens PRIMARY KEY (UserId, LoginProvider, Name),
    CONSTRAINT FK_AspNetUserTokens_AspNetUsers_UserId FOREIGN KEY (UserId) REFERENCES AspNetUsers (Id) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
SELECT 'f410ba1e-387c-45ba-8b0a-5f0d49df6459', 'Admin', 'ADMIN', 'f06a43e4-ec10-449e-8b9e-1f321bea0ad3'
WHERE NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE NormalizedName = 'ADMIN');
