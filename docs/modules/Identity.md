# Identity Module

## Overview

The Identity module is responsible for authentication, authorization, user accounts and the basic identity information of every user in JobWize.

This module is the owner of the `User` aggregate.

No other module may modify or directly access the Identity database. Cross-module communication must occur through application contracts and integration events.

---

# Responsibilities

The Identity module is responsible for:

-   Authentication
-   Authorization
-   User accounts
-   User roles
-   Account status
-   Password management
-   Refresh tokens
-   Basic identity information

The Identity module is **not** responsible for:

-   Candidate CVs
-   Skills
-   Experiences
-   Education
-   Job applications
-   Interviews
-   Company information
-   Any business-specific profile information

---

# Actors

## Anonymous User

Can:

-   Register as Candidate
-   Login

---

## Candidate

Can:

-   Login
-   Logout
-   Refresh authentication
-   Change password
-   Update basic identity information

Cannot:

-   Access administration pages

---

## Admin

Can:

-   Login
-   View users
-   Suspend candidates
-   Reactivate candidates

Cannot:

-   Create admins
-   Modify admin accounts
-   Change user roles

---

## Super Admin

Has every Admin permission plus:

-   Create admin accounts
-   Update admin accounts
-   Suspend admin accounts
-   Reactivate admin accounts

The Super Admin account is automatically seeded and cannot be modified by another administrator.

---

# User Aggregate

The Identity module owns the `User` aggregate.

Example properties:

-   Id
-   Email
-   PasswordHash
-   FirstName
-   LastName
-   AvatarUrl (future)
-   Role
-   Status
-   RefreshTokens
-   RequirePasswordChange
-   CreatedAt
-   UpdatedAt
-   DeletedAt

## Aggregate Behaviors

The `User` aggregate encapsulates all business operations related to a user account.

Examples include:

-   ChangePassword
-   Suspend
-   Reactivate
-   UpdateBasicInformation
-   AddRefreshToken
-   RemoveRefreshToken
-   RevokeRefreshTokens

---

# Roles

Current roles:

-   Candidate
-   Admin
-   SuperAdmin

Roles are mutually exclusive.

---

# Account Status

Current statuses:

-   Active
-   Suspended

Suspended users cannot authenticate.

Authentication failures always return a generic error message to avoid revealing account existence.

---

# Authentication

Authentication uses:

-   JWT Access Tokens
-   Refresh Tokens

Each authenticated session owns its own refresh token, allowing multiple concurrent logins across different devices.

Future identity providers (Google, Microsoft, etc.) may be added without changing the module boundaries.

---

# Commands

## Anonymous

-   RegisterCandidate
-   Login
-   Logout
-   RefreshToken

---

## Candidate

-   UpdatePersonalInformation
-   ChangePassword

---

## Admin

-   SuspendUser
-   ReactivateUser

---

## Super Admin

-   CreateAdmin
-   UpdateAdmin
-   SuspendAdmin
-   ReactivateAdmin

---

# Queries

## Authenticated User

-   GetCurrentUser

---

## Administration

-   GetUsers
-   GetUserDetails

---

# Registration

Only Candidates may self-register.

Admins cannot register themselves.

Admin accounts are created exclusively by the Super Admin.

---

# Admin Creation Workflow

The Super Admin provides:

-   First Name
-   Last Name
-   Email

The system:

1. Creates the account.
2. Generates a strong temporary password.
3. Stores the password hash.
4. Sets `RequirePasswordChange = true`.
5. Displays the temporary password once.

The temporary password must be communicated securely to the new administrator.

At the first successful login, the administrator must change the password before accessing the application.

Future versions will replace the temporary password with an email-based password setup flow.

---

# Password Management

Current version supports:

-   Change password

Future versions may add:

-   Forgot password
-   Password reset by email

---

# Basic Identity Information

The Identity module owns information that every user possesses regardless of business role.

Examples:

-   First Name
-   Last Name
-   Avatar (future)
-   Preferred Language (future)
-   Time Zone (future)

Business-specific information belongs to the owning module.

---

# Seeding

Production:

-   Seed the Super Admin account.

Debug builds:

-   Seed:

    -   Super Admin
    -   Admin
    -   Candidate

These additional accounts exist only to simplify local development.

---

# Audit

The aggregate stores:

-   CreatedAt
-   UpdatedAt
-   DeletedAt

Detailed auditing is handled by the infrastructure audit/event history system.

---

# Business Rules

-   Email addresses are unique.
-   Email is the login identifier.
-   Email addresses cannot be changed after account creation.
-   Soft-deleted users continue reserving their email address.
-   Soft-deleted users cannot authenticate.
-   Users may only change their own password.
-   Admins cannot modify other admin accounts.
-   Only the Super Admin may manage administrator accounts.
-   Roles cannot be changed after account creation.
-   Authentication must not reveal whether an account exists.
-   Modules must never reference the Identity database through foreign keys.

---

# Integration Rules

Other modules reference users by `UserId`.

Foreign keys across modules are forbidden.

Example:

Candidate Module

-   UserId

No foreign key exists to `Identity.Users`.

The Identity module remains the sole owner of the `User` aggregate.

---

# Future Enhancements

Planned but intentionally excluded from V1:

-   Email verification
-   Forgot password
-   Password reset emails
-   External identity providers
-   Login attempt throttling
-   Account lockout
-   Two-factor authentication
-   Avatar upload
-   User preferences
