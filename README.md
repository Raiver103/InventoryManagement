<h1 align="center">InventoryManagement</h1>


Presentationt with <3 for Innowise group
https://github.com/user-attachments/assets/e7e9ebe0-b742-429e-8e40-270127c130bb
(but bad quality, If you need good quality write me in telegram @raiver103)


# Technical tools 
* Backend: ASP.NET Web API
* Frontend: Blazor
* Database: Entity Framework Core, MS SQL Server
* Testing: Unit and integration tests
* Infrastructure: Docker
* Logging & Monitoring: Serilog, Exception Middleware

# Architecture
* Clean Architecture
* Services instead Use cases

# Advantages 
* Authentication and authorization via Auth0 role-based access control
* WebSocket (SignalR) for real-time updates on inventory changes(Items and Transactions)
* Exporting reports about Transactions(CSV, Excel) 
* GitHub Actions (CI/CD)

# ⚙️ CI/CD Pipeline (Workflow Overview)
* On every push/PR to master:
    * Starts a SQL Server container
    * Restores dependencies and builds the project
    * Runs unit & integration tests
* If tests pass:
    * Logs into Docker Hub
    * Builds and pushes the Docker image


# Prerequisites
✅ Download Docker - https://www.docker.com/products/docker-desktop/

✅ Docker Compose (included with Docker Desktop)

✅ Download Git - https://git-scm.com/downloads

# Running the solution
1️⃣ Clone Repository:  
* git clone https://github.com/Raiver103/InventoryManagement.git
* cd InventoryManagement

2️⃣ Start Docker-containers: 
* docker-compose up --build -d
* (Migration of database setup automatically)
  
3️⃣ Checking:
* Open - http://localhost:5000/
* Open - http://localhost:5000/swagger 

4️⃣ Stop Docker-containers: 
* docker-compose down

# Auth0
Post-Login Actions in Auth0

What Happens Post-Login?

* Adding User Metadata to ID Token

    * Retrieves first_name and last_name from user_metadata.

    * Adds them as custom claims to the ID token.

* Injecting User Roles into Tokens

    * Checks for user roles from event.authorization.roles.

    * Adds them to both ID Token and Access Token under a custom namespace.

* Assigning Default Role for Users 

    * If the user has no assigned role, sets "Admin" in app_metadata.

Why Use This?

✅ Personalizing tokens with user-specific claims.

✅ Enforcing role-based access control (RBAC).

✅ Storing default roles for new users.


<h2>Post-User Registration in Auth0</h3>

What Happens Post-User Registration? 

* Updates user_metadata

    * Adds first_name, last_name, and assigns the "Admin"(for test) role.

* Updates app_metadata

    * Explicitly sets the "Admin"(for test) role in app_metadata.

Why is this useful?

✅ Automatically assigns roles to new users.

✅ Enhances user profiles with additional metadata.

✅ Simplifies access management through app_metadata.
