# E-Commerce ASP.NET Core Project

## Overview
A full-featured E-Commerce backend built with ASP.NET Core (.NET 9), Entity Framework Core, and SignalR for real-time stock updates. The project supports user authentication, product and category management, cart and order processing, and secure role-based API endpoints.

## Features
- User registration, login, and JWT authentication
- Role-based authorization (Admin, Customer)
- Product and category CRUD operations (with image upload)
- Cart management and checkout
- Order management
- Real-time stock updates using SignalR
- Global error handling middleware
- Refresh token support

## Technologies Used
- ASP.NET Core (.NET 9)
- Entity Framework Core
- SQL Server
- SignalR
- JWT Authentication
- Mapster (object mapping)

## Getting Started

### Prerequisites
- .NET 9 SDK
- SQL Server (LocalDB or full instance)

### Setup
1. Clone the repository
2. Update the connection string in `appsettings.json` if needed
3. Run database migrations:
   ```sh
   dotnet ef database update
   ```
4. Build and run the project:
   ```sh
   dotnet run
   ```
5. Access the API at `https://localhost:5001` or `http://localhost:5000`

### API Documentation
- Scalar is available in development mode for interactive API documentation and testing.

## Authentication & Authorization
- Register and login to receive a JWT token
- Use the token in the `Authorization: Bearer <token>` header for secured endpoints
- Admin-only endpoints require a user with the `Admin` role

## Real-Time Stock Updates (SignalR)
- SignalR hub is available at `/stockHub`
- Frontend clients can connect and listen for `ReceiveStockUpdate` events
- Example client: `wwwroot/stockUpdates.html`

## API Endpoints (Summary)

### Auth
- `POST /api/Auth/register` — Register new user
- `POST /api/Auth/login` — Login and get JWT token
- `POST /api/Auth/refresh-token` — Refresh JWT token

### Products
- `GET /api/Product` — List products
- `GET /api/Product/{id}` — Get product details
- `POST /api/Product` — Create product (**Admin only**)
- `PUT /api/Product/{id}` — Update product (**Admin only**)
- `DELETE /api/Product/{id}` — Delete product (**Admin only**)
- `PUT /api/Product/{id}/AddStock` — Add stock (**Admin only**)

### Categories
- `GET /api/Category` — List categories
- `GET /api/Category/{id}` — Get category details
- `POST /api/Category` — Create category (**Admin only**)
- `PUT /api/Category/{id}` — Update category (**Admin only**)
- `DELETE /api/Category/{id}` — Delete category (**Admin only**)

### Cart
- All endpoints require authentication

### Orders
- `POST /api/Order/Checkout/{userId}` — Checkout (**Authenticated users**)
- `GET /api/Order/Admin` — List all orders (**Admin only**)

## License
This project is for educational purposes.

---

**Author:** Abdelrahman (role: Admin)
