# Tài Liệu Phát Triển Back-end với .NET Core và ASP.NET Core Web API

## Giới thiệu

Chào mừng bạn đến với tài liệu phát triển Back-end với .NET Core và ASP.NET Core Web API! Bộ tài liệu này được thiết kế để cung cấp một hướng dẫn toàn diện về phát triển ứng dụng web API với ASP.NET Core, từ những kiến thức cơ bản đến các kỹ thuật nâng cao.

Tài liệu này được tổ chức thành các chương logic, mỗi chương tập trung vào một khía cạnh cụ thể của phát triển back-end với .NET Core. Mỗi file đều bao gồm giải thích lý thuyết, code ví dụ thực tế, best practices, và phân tích dựa trên dự án DecorStore.API thực tế.

## Cấu trúc tài liệu

### 1. Fundamentals
- [1.1 ASP.NET Core Introduction](1-fundamentals/1.1-aspnet-core-intro.md)
- [1.2 Project Structure](1-fundamentals/1.2-project-structure.md)
- [1.3 Middleware Pipeline](1-fundamentals/1.3-middleware-pipeline.md)

### 2. Dependency Injection
- [2.1 DI Basics](2-dependency-injection/2.1-di-basics.md)
- [2.2 DI Lifetimes](2-dependency-injection/2.2-di-lifetimes.md)

### 3. Web API Development
- [3.1 Controllers & Actions](3-web-api-development/3.1-controllers-actions.md)
- [3.2 Model Binding](3-web-api-development/3.2-model-binding.md)
- [3.3 Action Results](3-web-api-development/3.3-action-results.md)

### 4. Database Access
- [4.1 EF Core Setup](4-database-access/4.1-ef-core-setup.md)
- [4.2 DbContext & Entities](4-database-access/4.2-dbcontext-entities.md)
- [4.3 Migrations](4-database-access/4.3-migrations.md)
- [4.4 CRUD Operations](4-database-access/4.4-crud-operations.md)

### 5. Design Patterns
- [5.1 Repository Pattern](5-design-patterns/5.1-repository-pattern.md)
- [5.2 Unit of Work](5-design-patterns/5.2-unit-of-work.md)
- [5.3 DTOs & AutoMapper](5-design-patterns/5.3-dtos-automapper.md)
- [5.4 Clean Architecture](5-design-patterns/5.4-clean-architecture.md)

### 6. Error Handling
- [6.1 Exception Middleware](6-error-handling/6.1-exception-middleware.md)
- [6.2 Logging](6-error-handling/6.2-logging.md)
- [6.3 Problem Details](6-error-handling/6.3-problem-details.md)

### 7. Security
- [7.1 Authentication](7-security/7.1-authentication.md)
- [7.2 Authorization](7-security/7.2-authorization.md)
- [7.3 Data Protection](7-security/7.3-data-protection.md)

### 8. Performance
- [8.1 Caching](8-performance/8.1-caching.md)
- [8.2 Async Programming](8-performance/8.2-async-programming.md)
- [8.3 Optimizations](8-performance/8.3-optimizations.md)

### 9. Testing
- [9.1 Unit Testing](9-testing/9.1-unit-testing.md)
- [9.2 Integration Testing](9-testing/9.2-integration-testing.md)
- [9.3 Mocking](9-testing/9.3-mocking.md)

### 10. Deployment
- [10.1 Docker](10-deployment/10.1-docker.md)
- [10.2 CI/CD](10-deployment/10.2-ci-cd.md)
- [10.3 Monitoring](10-deployment/10.3-monitoring.md)

### 11. Advanced Topics
- [11.1 Microservices](11-advanced-topics/11.1-microservices.md)
- [11.2 SignalR](11-advanced-topics/11.2-signalr.md)
- [11.3 Background Services](11-advanced-topics/11.3-background-services.md)
- [11.4 Learning Path](11-advanced-topics/11.4-learning-path.md)

## Cách sử dụng tài liệu

1. **Lộ trình học tập**: Nên đọc các file theo thứ tự được đề xuất để xây dựng kiến thức từ cơ bản đến nâng cao.
2. **Code ví dụ**: Tất cả code ví dụ đều có thể chạy được và được chú thích đầy đủ.
3. **Phân tích dự án thực tế**: Mỗi chủ đề đều kết nối với dự án DecorStore.API thực tế.
4. **Best practices**: Mỗi chủ đề đều bao gồm các best practices và design patterns liên quan.

## Công nghệ và thư viện được đề cập

### Core Technologies
- **.NET Core**: Platform để xây dựng các ứng dụng hiện đại
- **ASP.NET Core**: Framework để xây dựng web applications và APIs
- **Entity Framework Core**: ORM (Object-Relational Mapper) cho .NET

### Libraries & Packages
- **AutoMapper**: Mapping giữa objects
- **FluentValidation**: Validation cho objects
- **Swagger/OpenAPI**: Documentation cho APIs
- **JWT Authentication**: JSON Web Token cho authentication
- **Serilog/NLog**: Logging frameworks
- **xUnit/NUnit**: Testing frameworks
- **Moq/NSubstitute**: Mocking libraries

### Design Patterns
- **Repository Pattern**: Abstraction của data layer
- **Unit of Work**: Quản lý transactions và operations
- **Dependency Injection**: Loose coupling giữa các components
- **Mediator Pattern (MediatR)**: Reduces coupling between components
- **CQRS Pattern**: Command Query Responsibility Segregation

## Cách đóng góp

Nếu bạn muốn đóng góp vào tài liệu này, hãy tạo một pull request với các thay đổi đề xuất của bạn. Mọi đóng góp đều được đánh giá cao!

---

*Tài liệu này được phát triển dựa trên dự án DecorStore.API và best practices từ cộng đồng .NET.* 