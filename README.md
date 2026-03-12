# Inventory Cloud API

A containerized ASP.NET Core Web API for managing product inventory.  
The application runs with SQL Server in Docker using Docker Compose and Entity Framework Core for database access.

## Architecture

Client  
↓  
ASP.NET Core API (Docker Container)  
↓  
SQL Server Database (Docker Container)

## Tech Stack

- ASP.NET Core Web API (.NET 8)
- Entity Framework Core
- SQL Server
- Docker
- Docker Compose
- Swagger / OpenAPI

## Features

- RESTful API for product inventory
- CRUD operations for products
- Containerized API and database
- Automatic database migrations
- API testing with Swagger

## API Documentation

Swagger UI:

http://localhost:8080/swagger

## Author

Usman 
Cloud Engineering Portfolio Project
