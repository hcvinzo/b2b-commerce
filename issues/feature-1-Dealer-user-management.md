### Title
Implementing backend multi-user feature for dealer

### Description
Each dealer (Customer) has an admin user when customer is created. Dealer can add other users and assign customer specific roles to them. This issue's scope is only backend development and admin development. Frontend features will be  

### User Story
As a dealer
I want to add users to b2b system
So that i can delegate my employee to take actions on b2b system withinb given permissions

### Requirements
-[] A dealer can have multiple users 
-[] Each dealer user has pre-defined User roles for Customer user type
-[] Default dealer user (the one created while creating dealer) is customer's admin user. 
-[] Customer admin user has pre-defined role "Bayi Yöneticisi" which has all customer permissions.
-[] Customer can add new user's to system and assign pre-defined customer role(s) to that user
-[] Customer can only add customer specific user roles
-[] There should be a user - dealer relation. Dealer's users are permitted to their dealer account but in the feature we'll have "satış temsilcisi" users which are system users and they can able to place order behalf of the dealer. Consider this need.

### Sample Customer User Permissions
- Place order
- View Order
- Edit Order
- View Account Balance
- Manage dealer account

## Sample Pre-defined Customer User Roles
- Bayi Yöneticisi (Default dealer user(admin))
- Satınalma (full order management)
- Finans / Muhasebe (payment and accountant management)

### NOTES
- Create entity classes, repository classes and migration files for the new tables
- Follow the project's database standarts