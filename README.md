# E-Commerce Backend API

A .NET Core Web API for managing an e-commerce system with user authentication, product management, and inventory control.

## Features

### User Management
- User registration and authentication using JWT
- Role-based authorization (Administrator and Visitor roles)
- User CRUD operations
- Password hashing using BCrypt

### Product Management
- Product Categories
- Product Inventory
- Product Discounts
- CRUD operations for all product-related entities

## Technologies Used

- .NET 6.0
- Entity Framework Core
- SQL Server
- JWT Authentication
- BCrypt.Net-Next
- Swagger/OpenAPI

## Database Schema

### Users
- User management with roles
- Soft delete functionality
- Timestamp tracking (created_at, modified_at, deleted_at)

### Products
- Categories
- Inventory tracking
- Discount management

## API Endpoints

### Authentication



## Security Features

- Password hashing using BCrypt
- JWT token authentication
- Role-based authorization
- Soft delete implementation
- Input validation and model binding

## Error Handling

- Global exception handling
- Detailed logging using ILogger
- Custom error responses
- Transaction management for critical operations

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details
