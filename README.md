<h1 align="center">InventoryManagement</h1>

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


