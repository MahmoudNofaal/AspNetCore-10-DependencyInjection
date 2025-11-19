# ASP.NET Core MVC - Services and Dependency Injection

## Table of Contents
1. [Introduction to Services](#introduction-to-services)
2. [Dependency Inversion Principle](#dependency-inversion-principle)
3. [Inversion of Control](#inversion-of-control)
4. [Dependency Injection](#dependency-injection)
5. [Service Lifetimes](#service-lifetimes)
6. [Dependency Injection Techniques](#dependency-injection-techniques)
7. [Best Practices](#best-practices)
8. [Autofac Integration](#autofac-integration)
9. [Service Scopes](#service-scopes)
10. [Interview Preparation](#interview-preparation)

---

## Introduction to Services

### Definition

In ASP.NET Core MVC, **services** are classes responsible for implementing the core business logic of your application. They are designed to be reusable, self-contained, and independent of specific controllers or views. Services form the backbone of your application, handling tasks such as data access, calculations, communication with external systems, and other operations that constitute the core functionality of your application.

### Key Purposes

| Purpose | Description |
|---------|-------------|
| **Encapsulation of Business Logic** | Provides a clean separation of complex operations from the presentation layer (controllers and views) |
| **Reusability** | A single service can be utilized by multiple controllers, promoting DRY (Don't Repeat Yourself) principles |
| **Testability** | Services can be easily unit tested in isolation, allowing verification of business logic without running the entire application |
| **Dependency Injection** | Services are typically registered in the DI container, making them easily accessible throughout the application |

### Typical Responsibilities

Services in ASP.NET Core MVC typically handle the following responsibilities:

- **Data Access**: Communicating with databases or other data sources to perform CRUD operations (Create, Read, Update, Delete)
- **Business Rules**: Implementing the rules that govern application behavior, including validation, calculations, and data transformations
- **Integration**: Interacting with external systems, APIs, or third-party services
- **Notifications**: Sending emails, SMS messages, push notifications, or other communication mechanisms
- **Logging**: Recording events, errors, and diagnostic information for troubleshooting and analysis

### Basic Implementation Example

#### CitiesService.cs (Service Implementation)

```csharp
namespace Services
{
    public class CitiesService
    {
        private List<string> _cities;
        
        public CitiesService() 
        {
            _cities = new List<string>() 
            { 
                "London", 
                "Paris", 
                "New York", 
                "Tokyo", 
                "Rome" 
            };
        }
        
        public List<string> GetCities()
        {
            return _cities;
        }
    }
}
```

#### HomeController.cs (Controller)

```csharp
public class HomeController : Controller
{
    private readonly CitiesService _citiesService;
    
    public HomeController() 
    {
        _citiesService = new CitiesService();
    }
    
    [Route("/")]
    public IActionResult Index()
    {
        List<string> cities = _citiesService.GetCities();
        return View(cities);
    }
}
```

**Note**: This example demonstrates basic service usage without dependency injection. In production applications, services should be registered with the DI container and injected through constructor parameters rather than being instantiated directly.

### Key Characteristics of Well-Designed Services

- **Single Responsibility**: Each service should have a single, well-defined purpose
- **Stateless Design**: Services should generally avoid maintaining state between calls
- **Interface-Based**: Services should implement interfaces to facilitate testing and loose coupling
- **Dependency-Free**: Services should not directly instantiate their own dependencies
- **Testable**: Services should be designed with unit testing in mind

---

## Dependency Inversion Principle

### Definition

The Dependency Inversion Principle (DIP) is a fundamental design principle in object-oriented programming that promotes loosely coupled software architecture. It consists of two key statements:

1. **High-level modules should not depend on low-level modules. Both should depend on abstractions.**
2. **Abstractions should not depend on details. Details should depend on abstractions.**

### Explanation

In simpler terms, instead of tightly coupling classes by having them depend on concrete implementations, classes should depend on abstractions such as interfaces or abstract classes. This approach allows for easy substitution of implementations without modifying the higher-level code that depends on them.

### Benefits of DIP

| Benefit | Description |
|---------|-------------|
| **Loose Coupling** | Reduces direct dependencies between classes, making them easier to change independently |
| **Flexibility** | Enables easy substitution of different implementations without affecting dependent code |
| **Testability** | Facilitates unit testing by allowing mock implementations of dependencies |
| **Maintainability** | Creates more modular code that is easier to understand and less prone to ripple effects from changes |

### Implementation Example

#### Without DIP (Tightly Coupled)

```csharp
public class OrderProcessor
{
    private SqlServerRepository _repository;
    
    public OrderProcessor()
    {
        _repository = new SqlServerRepository(); // Direct dependency
    }
    
    public void ProcessOrder(Order order)
    {
        _repository.Save(order);
    }
}
```

**Problems with this approach:**
- `OrderProcessor` is tightly coupled to `SqlServerRepository`
- Changing the data store requires modifying `OrderProcessor`
- Testing `OrderProcessor` requires an actual database connection
- Cannot easily substitute a different repository implementation

#### With DIP (Loosely Coupled)

```csharp
// Abstraction
public interface IOrderRepository
{
    void Save(Order order);
}

// Implementation
public class SqlServerRepository : IOrderRepository
{
    public void Save(Order order)
    {
        // SQL Server-specific implementation
    }
}

// High-level module depending on abstraction
public class OrderProcessor
{
    private readonly IOrderRepository _repository;
    
    public OrderProcessor(IOrderRepository repository)
    {
        _repository = repository; // Depends on abstraction
    }
    
    public void ProcessOrder(Order order)
    {
        _repository.Save(order);
    }
}
```

**Advantages of this approach:**
- `OrderProcessor` depends on the `IOrderRepository` interface, not a concrete implementation
- Easy to substitute different repository implementations
- Testing is simplified using mock implementations
- Changes to the repository implementation do not affect `OrderProcessor`

### Practical Application in ASP.NET Core

#### ServiceContracts (Interface Definition)

```csharp
namespace ServiceContracts
{
    public interface ICitiesService
    {
        List<string> GetCities();
    }
}
```

#### Services (Concrete Implementation)

```csharp
namespace Services
{
    public class CitiesService : ICitiesService
    {
        private List<string> _cities;
        
        public CitiesService()
        {
            _cities = new List<string>() 
            { 
                "London", 
                "Paris", 
                "New York", 
                "Tokyo", 
                "Rome" 
            };
        }
        
        public List<string> GetCities()
        {
            return _cities;
        }
    }
}
```

#### Service Registration (Program.cs)

```csharp
builder.Services.Add(new ServiceDescriptor(
    typeof(ICitiesService),
    typeof(CitiesService),
    ServiceLifetime.Transient
));
```

#### Controller Implementation

```csharp
public class HomeController : Controller
{
    private readonly ICitiesService _citiesService;
    
    public HomeController(ICitiesService citiesService) 
    {
        _citiesService = citiesService;
    }
    
    [Route("/")]
    public IActionResult Index()
    {
        List<string> cities = _citiesService.GetCities();
        return View(cities);
    }
}
```

### Real-World Analogy: Electrical Outlets

#### Without DIP (Hardwired Connection)

Consider a light switch directly wired to a specific light bulb. If you want to change the bulb to a different type, you might need to rewire the switch. This represents tightly coupled code where classes depend directly on specific implementations.

```
Light Switch -----> Specific Light Bulb 
(Tightly Coupled - Modification Required)
```

#### With DIP (Standard Interface)

A standard electrical outlet and plug represent the DIP approach. The outlet provides an interface (abstraction) that any compatible device can use. You can plug any device (light bulb, fan, phone charger) into the outlet without modifying the outlet itself.

```
Electrical Outlet (Interface)
       |
       |--- Light Bulb (Implementation 1)
       |--- Fan (Implementation 2)
       |--- Phone Charger (Implementation 3)
```

#### Code Representation

```csharp
// Without DIP
public class LightSwitch
{
    private SpecificLightBulb _bulb = new SpecificLightBulb();
    
    public void TurnOn()
    {
        _bulb.Illuminate();
    }
}

// With DIP
public interface ILight
{
    void Illuminate();
}

public class LightBulb : ILight 
{ 
    public void Illuminate() 
    {
        // LED bulb implementation
    }
}

public class SmartBulb : ILight
{
    public void Illuminate()
    {
        // Smart bulb implementation with color control
    }
}

public class LightSwitch
{
    private readonly ILight _light;
    
    public LightSwitch(ILight light)
    {
        _light = light;
    }
    
    public void TurnOn()
    {
        _light.Illuminate();
    }
}
```

---

## Inversion of Control

### Definition

Inversion of Control (IoC) is a broad software design principle that involves transferring the control of object creation and management from application code to a framework or container. Instead of classes explicitly creating their dependencies, they receive them from an external source.

### Core Concept

In traditional programming, your code controls the flow of the program and creates all necessary objects. With IoC, this control is "inverted"—a framework or container manages object creation and lifecycle, and your code simply declares what it needs.

### Traditional Control Flow vs. IoC

#### Traditional Approach (Direct Control)

```csharp
public class EmailService
{
    private SmtpClient _smtpClient;
    
    public EmailService()
    {
        _smtpClient = new SmtpClient("smtp.server.com"); // Direct creation
    }
    
    public void SendEmail(string to, string message)
    {
        _smtpClient.Send(to, message);
    }
}
```

In this approach, `EmailService` controls the creation of `SmtpClient`.

#### IoC Approach (Inverted Control)

```csharp
public class EmailService
{
    private readonly IEmailClient _emailClient;
    
    public EmailService(IEmailClient emailClient)
    {
        _emailClient = emailClient; // Control inverted - dependency provided externally
    }
    
    public void SendEmail(string to, string message)
    {
        _emailClient.Send(to, message);
    }
}
```

In this approach, the control of creating `IEmailClient` is inverted to an external container.

### Benefits of IoC

| Benefit | Description |
|---------|-------------|
| **Decoupling** | Classes are no longer responsible for creating their dependencies |
| **Configuration Management** | Centralized configuration of object creation and dependencies |
| **Lifecycle Management** | Framework handles object lifecycle (creation, disposal, lifetime) |
| **Testing Facilitation** | Easy to provide test doubles or mock objects |

### IoC Container

An IoC container (also called a DI container) is a framework component that:

1. Maintains a registry of types and their dependencies
2. Creates instances of types as needed
3. Injects dependencies into created instances
4. Manages the lifetime of created objects
5. Handles disposal of objects when their lifetime ends

ASP.NET Core includes a built-in IoC container accessible through `IServiceCollection` and `IServiceProvider`.

---

## Dependency Injection

### Definition

Dependency Injection (DI) is a specific implementation technique of the Inversion of Control principle. It involves supplying dependencies to a class from an external source rather than having the class create them internally.

### Relationship Between DIP, IoC, and DI

```
Dependency Inversion Principle (DIP)
         ↓
    (Design Principle)
         ↓
Inversion of Control (IoC)
         ↓
    (Broad Concept)
         ↓
Dependency Injection (DI)
         ↓
   (Implementation Technique)
```

### How DI Works in ASP.NET Core

The DI process in ASP.NET Core follows these steps:

1. **Registration**: Services are registered with the DI container during application startup
2. **Resolution**: When a component needs a service, the container resolves (creates or retrieves) an instance
3. **Injection**: The container injects the service instance into the component
4. **Usage**: The component uses the injected service
5. **Disposal**: The container disposes of services when their lifetime ends

### Complete Implementation Example

#### Step 1: Define Interface (Abstraction)

```csharp
namespace ServiceContracts
{
    public interface IProductService
    {
        List<Product> GetProducts();
        Product GetProductById(int id);
        void AddProduct(Product product);
    }
}
```

#### Step 2: Implement Service

```csharp
namespace Services
{
    public class ProductService : IProductService
    {
        private readonly List<Product> _products;
        
        public ProductService()
        {
            _products = new List<Product>()
            {
                new Product { Id = 1, Name = "Laptop", Price = 999.99m },
                new Product { Id = 2, Name = "Mouse", Price = 29.99m },
                new Product { Id = 3, Name = "Keyboard", Price = 79.99m }
            };
        }
        
        public List<Product> GetProducts()
        {
            return _products;
        }
        
        public Product GetProductById(int id)
        {
            return _products.FirstOrDefault(p => p.Id == id);
        }
        
        public void AddProduct(Product product)
        {
            _products.Add(product);
        }
    }
}
```

#### Step 3: Register Service (Program.cs)

```csharp
var builder = WebApplication.CreateBuilder(args);

// Register services with DI container
builder.Services.AddControllersWithViews();
builder.Services.AddTransient<IProductService, ProductService>();

var app = builder.Build();
// ... rest of configuration
```

#### Step 4: Inject and Use Service

```csharp
public class ProductsController : Controller
{
    private readonly IProductService _productService;
    
    // Constructor injection
    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }
    
    public IActionResult Index()
    {
        var products = _productService.GetProducts();
        return View(products);
    }
    
    public IActionResult Details(int id)
    {
        var product = _productService.GetProductById(id);
        if (product == null)
        {
            return NotFound();
        }
        return View(product);
    }
}
```

### Benefits of Dependency Injection

| Benefit | Description | Example |
|---------|-------------|---------|
| **Loose Coupling** | Classes depend on abstractions, not concrete types | Controller depends on `IProductService`, not `ProductService` |
| **Testability** | Easy to substitute mock implementations for testing | Inject mock service in unit tests |
| **Maintainability** | Changes to implementation don't affect dependent code | Change `ProductService` implementation without modifying controllers |
| **Flexibility** | Easy to swap implementations at runtime or configuration | Switch between SQL and NoSQL repositories |
| **Single Responsibility** | Classes focus on their core responsibility | Controller handles HTTP, service handles business logic |

---

## Service Lifetimes

### Overview

When registering a service with the DI container, you must specify its lifetime. The lifetime determines how the container creates, reuses, and disposes of service instances throughout the application's execution.

### The Three Service Lifetimes

#### 1. Transient

**Characteristics:**
- A new instance is created each time the service is requested
- Instance lifetime: Only as long as needed for the current operation
- Memory: New allocation for each request
- Thread safety: Not a concern (each instance is independent)

**Registration:**
```csharp
builder.Services.AddTransient<ITransientService, TransientService>();
```

**When to Use:**
- Lightweight, stateless services
- Services where each operation requires fresh state
- Helper classes and utilities
- Services that shouldn't be shared

**Examples:**
```csharp
// Logging service
builder.Services.AddTransient<ILogger<MyClass>, Logger<MyClass>>();

// Data validator
builder.Services.AddTransient<IValidator, DataValidator>();

// Email sender
builder.Services.AddTransient<IEmailSender, EmailSender>();
```

#### 2. Scoped

**Characteristics:**
- A single instance is created per scope (typically per HTTP request)
- Instance lifetime: Duration of the scope
- Memory: One allocation per scope
- Thread safety: Generally not a concern (each request has its own thread)

**Registration:**
```csharp
builder.Services.AddScoped<IScopedService, ScopedService>();
```

**When to Use:**
- Services that should maintain state during a single request
- Database contexts (Entity Framework Core)
- User-specific operations within a request
- Transaction management

**Examples:**
```csharp
// Database context
builder.Services.AddScoped<ApplicationDbContext>();

// Unit of work pattern
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Shopping cart (for current request)
builder.Services.AddScoped<IShoppingCart, ShoppingCart>();
```

#### 3. Singleton

**Characteristics:**
- A single instance is created for the entire application lifetime
- Instance lifetime: Application lifetime
- Memory: One allocation for entire application
- Thread safety: Must be thread-safe (accessed by multiple threads)

**Registration:**
```csharp
builder.Services.AddSingleton<ISingletonService, SingletonService>();
```

**When to Use:**
- Stateless services used throughout the application
- Application-wide configuration
- Caching services
- Expensive-to-create objects that can be reused

**Examples:**
```csharp
// Application settings
builder.Services.AddSingleton<IConfiguration>(configuration);

// Memory cache
builder.Services.AddSingleton<IMemoryCache, MemoryCache>();

// Background service
builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
```

### Service Lifetime Comparison Table

| Aspect | Transient | Scoped | Singleton |
|--------|-----------|--------|-----------|
| **Instance Creation** | Every request | Once per HTTP request/scope | Once per application |
| **Instance Reuse** | Never | Within same request/scope | Across entire application |
| **Memory Usage** | Higher (many instances) | Moderate | Lower (one instance) |
| **State Management** | No shared state | Request-specific state | Application-wide state |
| **Thread Safety** | Not required | Usually not required | Required |
| **Disposal** | After immediate use | End of request/scope | Application shutdown |
| **Best For** | Stateless operations | Request-bound operations | Shared resources |
| **Performance** | Lower (frequent allocation) | Balanced | Higher (single allocation) |

### Choosing the Right Lifetime

```
Decision Flow:
┌─────────────────────────────────────┐
│ Does the service maintain state?    │
└──────────────┬──────────────────────┘
               │
       ┌───────┴───────┐
       │               │
      Yes             No
       │               │
       ↓               ↓
┌──────────────┐  ┌───────────────┐
│ State scope? │  │ Resource cost?│
└──────┬───────┘  └───────┬───────┘
       │                  │
   ┌───┴───┐          ┌───┴───┐
   │       │          │       │
Request  App-wide  Low    High
   │       │          │       │
   ↓       ↓          ↓       ↓
SCOPED  SINGLETON  TRANSIENT SINGLETON
```

### Registration Examples with All Three Lifetimes

```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Transient: New instance each time
        services.AddTransient<IGuidGenerator, GuidGenerator>();
        services.AddTransient<IEmailService, EmailService>();
        services.AddTransient<IDateTimeProvider, DateTimeProvider>();
        
        // Scoped: One instance per request
        services.AddScoped<IUserContext, UserContext>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<ApplicationDbContext>();
        
        // Singleton: One instance for application lifetime
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton<IMemoryCache, MemoryCache>();
        services.AddSingleton<ILoggerFactory, LoggerFactory>();
    }
}
```

### Important Considerations

#### Captive Dependencies

A **captive dependency** occurs when a service with a longer lifetime captures a service with a shorter lifetime. This is a critical error that can lead to unexpected behavior and memory leaks.

**Examples of Captive Dependencies:**

```csharp
// ❌ BAD: Singleton capturing Scoped
public class SingletonService // Registered as Singleton
{
    private readonly ScopedService _scopedService; // Scoped service
    
    public SingletonService(ScopedService scopedService)
    {
        _scopedService = scopedService; // PROBLEM: Scoped service captured
    }
}

// ❌ BAD: Singleton capturing Transient
public class SingletonService // Registered as Singleton
{
    private readonly TransientService _transientService; // Transient service
    
    public SingletonService(TransientService transientService)
    {
        _transientService = transientService; // PROBLEM: Transient service captured
    }
}

// ✅ GOOD: Scoped can capture Transient
public class ScopedService // Registered as Scoped
{
    private readonly TransientService _transientService;
    
    public ScopedService(TransientService transientService)
    {
        _transientService = transientService; // OK: Shorter lifetime
    }
}
```

#### Valid Dependency Relationships

| Consuming Service | Can Depend On |
|-------------------|---------------|
| **Transient** | Transient, Scoped, Singleton |
| **Scoped** | Transient, Scoped, Singleton |
| **Singleton** | Singleton only (or use IServiceProvider) |

#### Thread Safety for Singleton Services

Singleton services must be thread-safe because they are accessed by multiple requests concurrently.

```csharp
// ❌ BAD: Not thread-safe
public class CounterService
{
    private int _count = 0;
    
    public void Increment()
    {
        _count++; // Race condition!
    }
}

// ✅ GOOD: Thread-safe
public class CounterService
{
    private int _count = 0;
    private readonly object _lock = new object();
    
    public void Increment()
    {
        lock (_lock)
        {
            _count++;
        }
    }
}

// ✅ BETTER: Using Interlocked
public class CounterService
{
    private int _count = 0;
    
    public void Increment()
    {
        Interlocked.Increment(ref _count);
    }
}
```

---

## Dependency Injection Techniques

### Overview

ASP.NET Core supports multiple methods for injecting dependencies into classes. Each technique has specific use cases and advantages.

### 1. Constructor Injection (Recommended)

#### Description

Dependencies are passed as parameters to the class constructor. This is the most common and recommended approach for dependency injection in ASP.NET Core.

#### Advantages

| Advantage | Description |
|-----------|-------------|
| **Explicit Dependencies** | All required dependencies are visible in the constructor signature |
| **Immutability** | Dependencies can be stored in readonly fields |
| **Compile-Time Safety** | Missing dependencies cause compilation errors |
| **Testability** | Easy to provide mock dependencies in unit tests |
| **Object Validity** | Ensures the object has all dependencies before use |

#### Implementation

```csharp
public class ProductsController : Controller
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;
    private readonly IMapper _mapper;
    
    public ProductsController(
        IProductService productService,
        ILogger<ProductsController> logger,
        IMapper mapper)
    {
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }
    
    public IActionResult Index()
    {
        _logger.LogInformation("Retrieving products");
        var products = _productService.GetAll();
        return View(products);
    }
}
```

#### Best Practices for Constructor Injection

```csharp
// ✅ GOOD: Dependency on interface
public class OrderService
{
    private readonly IOrderRepository _repository;
    
    public OrderService(IOrderRepository repository)
    {
        _repository = repository;
    }
}

// ❌ BAD: Dependency on concrete class
public class OrderService
{
    private readonly SqlOrderRepository _repository;
    
    public OrderService(SqlOrderRepository repository)
    {
        _repository = repository;
    }
}

// ✅ GOOD: Reasonable number of dependencies (3-5)
public class CheckoutService
{
    private readonly IOrderService _orderService;
    private readonly IPaymentService _paymentService;
    private readonly IInventoryService _inventoryService;
    
    public CheckoutService(
        IOrderService orderService,
        IPaymentService paymentService,
        IInventoryService inventoryService)
    {
        _orderService = orderService;
        _paymentService = paymentService;
        _inventoryService = inventoryService;
    }
}

// ⚠️ WARNING: Too many dependencies (indicates design issue)
public class GodService
{
    // 10+ constructor parameters indicates Single Responsibility Principle violation
    public GodService(
        IDep1 dep1, IDep2 dep2, IDep3 dep3, IDep4 dep4,
        IDep5 dep5, IDep6 dep6, IDep7 dep7, IDep8 dep8,
        IDep9 dep9, IDep10 dep10) // Too many dependencies!
    {
        // Consider breaking this into smaller services
    }
}
```

### 2. Property Injection

#### Description

Dependencies are assigned to public properties decorated with the `[FromServices]` attribute.

#### When to Use

- Optional dependencies
- Avoiding constructor parameter clutter
- Backward compatibility scenarios
- When you need a dependency in only one or two methods

#### Implementation

```csharp
public class MyMiddleware
{
    [FromServices]
    public ILogger<MyMiddleware> Logger { get; set; }
    
    [FromServices]
    public IConfiguration Configuration { get; set; }
    
    public async Task InvokeAsync(HttpContext context)
    {
        Logger?.LogInformation("Processing request");
        var setting = Configuration?["MySetting"];
        // ... rest of middleware logic
    }
}
```

#### Advantages and Disadvantages

**Advantages:**
- Reduces constructor parameter count
- Useful for optional dependencies
- Can set dependencies after object creation

**Disadvantages:**
- Dependencies are mutable (can be changed after construction)
- Less explicit than constructor injection
- Harder to identify required vs. optional dependencies
- Requires null-checking before use

### 3. Method Injection

#### Description

Dependencies are passed as parameters to individual methods, typically action methods in controllers.

#### When to Use

- Dependency needed in only one or few methods
- Reducing constructor complexity
- Method-specific dependencies

#### Implementation

```csharp
public class HomeController : Controller
{
    [Route("/")]
    public IActionResult Index([FromServices] ICitiesService citiesService)
    {
        var cities = citiesService.GetCities();
        return View(cities);
    }
    
    [Route("/about")]
    public IActionResult About([FromServices] ICompanyInfoService companyService)
    {
        var info = companyService.GetInfo();
        return View(info);
    }
}
```

#### Use Cases

```csharp
public class ReportsController : Controller
{
    // This service is only needed in one action
    public IActionResult Generate(
        int reportId,
        [FromServices] IReportGenerator generator,
        [FromServices] ILogger<ReportsController> logger)
    {
        logger.LogInformation("Generating report {ReportId}", reportId);
        var report = generator.Generate(reportId);
        return File(report, "application/pdf");
    }
    
    // Other actions don't need the report generator
    public IActionResult Index()
    {
        return View();
    }
}
```

### 4. Action Filter Injection

#### Description

Dependencies can be injected into action filters using the `[ServiceFilter]` or `[TypeFilter]` attributes.

#### Implementation

```csharp
// Custom action filter
public class LogActionFilter : IActionFilter
{
    private readonly ILogger<LogActionFilter> _logger;
    
    public LogActionFilter(ILogger<LogActionFilter> logger)
    {
        _logger = logger;
    }
    
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogInformation($"Executing action: {context.ActionDescriptor.DisplayName}");
    }
    
    public void OnActionExecuted(ActionExecutedContext context)
    {
        _logger.LogInformation($"Executed action: {context.ActionDescriptor.DisplayName}");
    }
}

// Register the filter
builder.Services.AddScoped<LogActionFilter>();

// Use with ServiceFilter
[ServiceFilter(typeof(LogActionFilter))]
public class ProductsController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
```

### Comparison of Injection Techniques

| Technique | Use Case | Visibility | Testability | Recommended |
|-----------|----------|------------|-------------|-------------|
| **Constructor** | Required dependencies | High | Excellent | ✅ Yes (primary) |
| **Property** | Optional dependencies | Medium | Good | ⚠️ Sparingly |
| **Method** | Method-specific dependencies | Low | Good | ⚠️ Specific cases |
| **Action Filter** | Cross-cutting concerns | Medium | Good | ✅ Yes (filters) |

---

## Best Practices

### 1. Use Constructor Injection as Default

Constructor injection should be your primary dependency injection technique. It makes dependencies explicit, supports immutability, and ensures object validity.

**Implementation:**
```csharp
public class OrderService : IOrderService
{
    private readonly IOrderRepository _repository;
    private readonly ILogger<OrderService> _logger;
    private readonly IEmailService _emailService;
    
    public OrderService(
        IOrderRepository repository,
        ILogger<OrderService> logger,
        IEmailService emailService)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
    }
}
```

### 2. Always Use Interfaces for Dependencies

Program to interfaces, not implementations. This fundamental principle enables loose coupling, testability, and flexibility.

**Implementation:**
```csharp
// Define interface
public interface IProductRepository
{
    Task<Product> GetByIdAsync(int id);
    Task<IEnumerable<Product>> GetAllAsync();
    Task AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(int id);
}

// Concrete implementation
public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;
    
    public ProductRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<Product> GetByIdAsync(int id)
    {
        return await _context.Products.FindAsync(id);
    }
    
    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _context.Products.ToListAsync();
    }
    
    // ... other implementations
}

// Service depends on interface
public class ProductService : IProductService
{
    private readonly IProductRepository _repository; // Interface, not concrete class
    
    public ProductService(IProductRepository repository)
    {
        _repository = repository;
    }
}
```

### 3. Avoid the Service Locator Anti-Pattern

The Service Locator pattern involves directly accessing the DI container from within your classes. This creates hidden dependencies and tightly couples your code to the DI container.

**Anti-Pattern (Avoid):**
```csharp
// ❌ BAD: Service Locator anti-pattern
public class OrderProcessor
{
    private readonly IServiceProvider _serviceProvider;
    
    public OrderProcessor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public void ProcessOrder(Order order)
    {
        // Hidden dependency - not visible in constructor
        var repository = _serviceProvider.GetService<IOrderRepository>();
        repository.Save(order);
    }
}
```

**Correct Approach:**
```csharp
// ✅ GOOD: Explicit dependency injection
public class OrderProcessor
{
    private readonly IOrderRepository _repository;
    
    public OrderProcessor(IOrderRepository repository)
    {
        _repository = repository;
    }
    
    public void ProcessOrder(Order order)
    {
        _repository.Save(order);
    }
}
```

**Note:** There are rare legitimate cases for using `IServiceProvider`, such as in factory patterns or when resolving services dynamically at runtime, but these should be exceptional cases.

### 4. Register Dependencies at the Composition Root

All service registrations should occur in a central location—the composition root—typically the `Program.cs` or `Startup.cs` file. This centralizes configuration and makes it easier to understand and manage your application's dependencies.

**Implementation:**
```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Register all services in one place
RegisterServices(builder.Services);

var app = builder.Build();

void RegisterServices(IServiceCollection services)
{
    // Infrastructure services
    services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
    
    // Application services
    services.AddScoped<IProductService, ProductService>();
    services.AddScoped<IOrderService, OrderService>();
    services.AddScoped<ICustomerService, CustomerService>();
    
    // Repositories
    services.AddScoped<IProductRepository, ProductRepository>();
    services.AddScoped<IOrderRepository, OrderRepository>();
    services.AddScoped<ICustomerRepository, CustomerRepository>();
    
    // Infrastructure
    services.AddScoped<IEmailService, EmailService>();
    services.AddSingleton<IMemoryCache, MemoryCache>();
    services.AddSingleton<IConfiguration>(builder.Configuration);
}
```

### 5. Choose the Appropriate Service Lifetime

Carefully consider the nature of each service and its usage patterns when selecting a lifetime.

**Decision Guidelines:**

| Service Characteristic | Recommended Lifetime |
|------------------------|---------------------|
| Holds user-specific state | Scoped |
| Accesses database | Scoped |
| Stateless utility | Transient or Singleton |
| Expensive to create | Singleton |
| Lightweight operations | Transient |
| Configuration data | Singleton |
| Caching | Singleton |

**Example:**
```csharp
// Stateless utility - Transient
services.AddTransient<IDateTimeProvider, DateTimeProvider>();

// Database access - Scoped
services.AddScoped<ApplicationDbContext>();
services.AddScoped<IUnitOfWork, UnitOfWork>();

// Configuration - Singleton
services.AddSingleton<IApplicationSettings>(
    builder.Configuration.GetSection("AppSettings").Get<ApplicationSettings>());

// Cache - Singleton
services.AddSingleton<IMemoryCache, MemoryCache>();
```

### 6. Avoid Captive Dependencies

Never inject a service with a shorter lifetime into a service with a longer lifetime. This violates the lifetime hierarchy and can cause serious issues.

**Lifetime Hierarchy (Longest to Shortest):**
```
Singleton → Scoped → Transient
```

**Valid Dependency Rules:**
```csharp
// ✅ GOOD: Longer lifetime can depend on same or longer lifetime
Singleton → Singleton ✓
Scoped → Scoped ✓
Scoped → Singleton ✓
Transient → Transient ✓
Transient → Scoped ✓
Transient → Singleton ✓

// ❌ BAD: Shorter lifetime captured by longer lifetime
Singleton → Scoped ✗
Singleton → Transient ✗
Scoped → Transient ✗ (usually not a problem, but avoid if possible)
```

**Example of Captive Dependency Problem:**
```csharp
// ❌ BAD: Singleton capturing Scoped
public class CachingService // Registered as Singleton
{
    private readonly ApplicationDbContext _context; // Scoped
    
    public CachingService(ApplicationDbContext context)
    {
        _context = context; // PROBLEM: DbContext captured for application lifetime
    }
    
    // This will cause issues because DbContext should be scoped to requests
}
```

**Solution:**
```csharp
// ✅ GOOD: Use IServiceProvider to resolve scoped dependencies
public class CachingService
{
    private readonly IServiceProvider _serviceProvider;
    
    public CachingService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public async Task<Data> GetDataAsync()
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // Use context within this scope
            return await context.Data.ToListAsync();
        }
    }
}
```

### 7. Use the Decorator Pattern for Cross-Cutting Concerns

The Decorator pattern allows you to add behavior to existing services without modifying them directly. This is useful for logging, caching, validation, and other cross-cutting concerns.

**Implementation:**
```csharp
// Base interface
public interface IOrderService
{
    Task<Order> GetOrderAsync(int orderId);
    Task CreateOrderAsync(Order order);
}

// Concrete implementation
public class OrderService : IOrderService
{
    private readonly IOrderRepository _repository;
    
    public OrderService(IOrderRepository repository)
    {
        _repository = repository;
    }
    
    public async Task<Order> GetOrderAsync(int orderId)
    {
        return await _repository.GetByIdAsync(orderId);
    }
    
    public async Task CreateOrderAsync(Order order)
    {
        await _repository.AddAsync(order);
    }
}

// Logging decorator
public class LoggingOrderService : IOrderService
{
    private readonly IOrderService _innerService;
    private readonly ILogger<LoggingOrderService> _logger;
    
    public LoggingOrderService(IOrderService innerService, ILogger<LoggingOrderService> logger)
    {
        _innerService = innerService;
        _logger = logger;
    }
    
    public async Task<Order> GetOrderAsync(int orderId)
    {
        _logger.LogInformation("Getting order {OrderId}", orderId);
        try
        {
            var result = await _innerService.GetOrderAsync(orderId);
            _logger.LogInformation("Successfully retrieved order {OrderId}", orderId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order {OrderId}", orderId);
            throw;
        }
    }
    
    public async Task CreateOrderAsync(Order order)
    {
        _logger.LogInformation("Creating order");
        await _innerService.CreateOrderAsync(order);
        _logger.LogInformation("Order created successfully");
    }
}

// Registration
services.AddScoped<OrderService>();
services.AddScoped<IOrderService>(provider =>
{
    var orderService = provider.GetRequiredService<OrderService>();
    var logger = provider.GetRequiredService<ILogger<LoggingOrderService>>();
    return new LoggingOrderService(orderService, logger);
});
```

### 8. Leverage the Options Pattern for Configuration

The Options pattern provides a strongly-typed way to access configuration settings in your services.

**Implementation:**
```csharp
// Configuration class
public class EmailSettings
{
    public string SmtpServer { get; set; }
    public int Port { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public bool EnableSsl { get; set; }
}

// appsettings.json
{
    "EmailSettings": {
        "SmtpServer": "smtp.example.com",
        "Port": 587,
        "Username": "user@example.com",
        "Password": "password",
        "EnableSsl": true
    }
}

// Registration in Program.cs
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

// Usage in service
public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    
    public EmailService(IOptions<EmailSettings> options)
    {
        _settings = options.Value;
    }
    
    public async Task SendEmailAsync(string to, string subject, string body)
    {
        using var client = new SmtpClient(_settings.SmtpServer, _settings.Port);
        client.EnableSsl = _settings.EnableSsl;
        client.Credentials = new NetworkCredential(_settings.Username, _settings.Password);
        
        await client.SendMailAsync(new MailMessage("from@example.com", to, subject, body));
    }
}
```

### 9. Keep Services Focused and Cohesive

Follow the Single Responsibility Principle. Each service should have one clear purpose and handle a specific area of functionality.

**Anti-Pattern (God Service):**
```csharp
// ❌ BAD: Service doing too many things
public class ApplicationService
{
    public void ProcessOrder() { }
    public void SendEmail() { }
    public void GenerateReport() { }
    public void UpdateInventory() { }
    public void ProcessPayment() { }
    // ... 20 more methods
}
```

**Correct Approach:**
```csharp
// ✅ GOOD: Separate, focused services
public class OrderService
{
    public void ProcessOrder() { }
}

public class EmailService
{
    public void SendEmail() { }
}

public class ReportService
{
    public void GenerateReport() { }
}

public class InventoryService
{
    public void UpdateInventory() { }
}

public class PaymentService
{
    public void ProcessPayment() { }
}
```

### 10. Write Tests for Your Services

Services should be designed with testability in mind. Use interfaces and dependency injection to facilitate unit testing.

**Example Unit Test:**
```csharp
public class OrderServiceTests
{
    [Fact]
    public async Task CreateOrder_ValidOrder_SavesSuccessfully()
    {
        // Arrange
        var mockRepository = new Mock<IOrderRepository>();
        var mockLogger = new Mock<ILogger<OrderService>>();
        var service = new OrderService(mockRepository.Object, mockLogger.Object);
        
        var order = new Order { CustomerId = 1, TotalAmount = 100.00m };
        
        mockRepository
            .Setup(r => r.AddAsync(It.IsAny<Order>()))
            .Returns(Task.CompletedTask);
        
        // Act
        await service.CreateOrderAsync(order);
        
        // Assert
        mockRepository.Verify(r => r.AddAsync(order), Times.Once);
    }
}
```

---

## Autofac Integration

### Overview

While ASP.NET Core includes a built-in dependency injection container, Autofac is a popular third-party IoC container that offers additional features and flexibility. Autofac provides advanced capabilities such as module-based registration, property injection, assembly scanning, and interception.

### Key Advantages of Autofac

| Advantage | Description |
|-----------|-------------|
| **Flexibility** | Wider range of component lifetime scopes and registration options |
| **Modules** | Organize registrations into logical modules |
| **Assembly Scanning** | Automatically register types from assemblies |
| **Interception** | Add cross-cutting concerns through interceptors |
| **Advanced Lifetime Management** | More granular control over object lifecycles |
| **Named and Keyed Services** | Register multiple implementations with identifiers |

### Installation

Install the Autofac integration package via NuGet:

```bash
dotnet add package Autofac.Extensions.DependencyInjection
```

### Basic Integration

#### Program.cs Configuration

```csharp
using Autofac;
using Autofac.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Replace default service provider with Autofac
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

// Register ASP.NET Core services
builder.Services.AddControllersWithViews();

// Configure Autofac container
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    // Register services using Autofac syntax
    containerBuilder.RegisterType<CitiesService>()
        .As<ICitiesService>()
        .InstancePerLifetimeScope();
    
    containerBuilder.RegisterType<ProductService>()
        .As<IProductService>()
        .InstancePerLifetimeScope();
});

var app = builder.Build();
// ... rest of configuration
```

### Autofac Registration Methods

#### Basic Registration

```csharp
// Register concrete type
containerBuilder.RegisterType<CitiesService>();

// Register with interface
containerBuilder.RegisterType<CitiesService>()
    .As<ICitiesService>();

// Register with multiple interfaces
containerBuilder.RegisterType<UserService>()
    .As<IUserService>()
    .As<IAuthenticationService>();
```

#### Lifetime Scopes in Autofac

| Autofac Method | ASP.NET Core Equivalent | Description |
|----------------|------------------------|-------------|
| `InstancePerDependency()` | Transient | New instance each time |
| `InstancePerLifetimeScope()` | Scoped | One instance per scope |
| `SingleInstance()` | Singleton | One instance for application |
| `InstancePerRequest()` | Scoped (web apps) | One instance per HTTP request |
| `InstancePerMatchingLifetimeScope()` | Custom | Instance per named scope |

**Examples:**
```csharp
// Transient
containerBuilder.RegisterType<EmailService>()
    .As<IEmailService>()
    .InstancePerDependency();

// Scoped
containerBuilder.RegisterType<OrderService>()
    .As<IOrderService>()
    .InstancePerLifetimeScope();

// Singleton
containerBuilder.RegisterType<CacheService>()
    .As<ICacheService>()
    .SingleInstance();
```

### Advanced Autofac Features

#### 1. Module-Based Registration

Modules allow you to organize related service registrations into separate classes.

```csharp
// Define a module
public class ServiceModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        // Register services
        builder.RegisterType<ProductService>()
            .As<IProductService>()
            .InstancePerLifetimeScope();
        
        builder.RegisterType<OrderService>()
            .As<IOrderService>()
            .InstancePerLifetimeScope();
        
        builder.RegisterType<CustomerService>()
            .As<ICustomerService>()
            .InstancePerLifetimeScope();
    }
}

public class RepositoryModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        // Register repositories
        builder.RegisterType<ProductRepository>()
            .As<IProductRepository>()
            .InstancePerLifetimeScope();
        
        builder.RegisterType<OrderRepository>()
            .As<IOrderRepository>()
            .InstancePerLifetimeScope();
    }
}

// Register modules in Program.cs
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    containerBuilder.RegisterModule<ServiceModule>();
    containerBuilder.RegisterModule<RepositoryModule>();
});
```

#### 2. Assembly Scanning

Automatically register types from assemblies based on conventions.

```csharp
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    // Register all types ending with "Service" from the current assembly
    containerBuilder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
        .Where(t => t.Name.EndsWith("Service"))
        .AsImplementedInterfaces()
        .InstancePerLifetimeScope();
    
    // Register all types ending with "Repository"
    containerBuilder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
        .Where(t => t.Name.EndsWith("Repository"))
        .AsImplementedInterfaces()
        .InstancePerLifetimeScope();
});
```

#### 3. Property Injection

```csharp
public class MyService
{
    public ILogger Logger { get; set; } // Will be injected
}

// Registration
containerBuilder.RegisterType<MyService>()
    .PropertiesAutowired();
```

#### 4. Named and Keyed Services

Register multiple implementations of the same interface.

```csharp
// Registration with names
containerBuilder.RegisterType<SqlProductRepository>()
    .Named<IProductRepository>("sql")
    .InstancePerLifetimeScope();

containerBuilder.RegisterType<MongoProductRepository>()
    .Named<IProductRepository>("mongo")
    .InstancePerLifetimeScope();

// Usage
public class ProductService
{
    private readonly IProductRepository _repository;
    
    public ProductService(
        [KeyFilter("sql")] IProductRepository repository)
    {
        _repository = repository;
    }
}
```

#### 5. Lambda Registration

Register services using lambda expressions for complex initialization.

```csharp
containerBuilder.Register(c =>
{
    var config = c.Resolve<IConfiguration>();
    var connectionString = config.GetConnectionString("DefaultConnection");
    return new ProductRepository(connectionString);
})
.As<IProductRepository>()
.InstancePerLifetimeScope();
```

### Complete Autofac Example

```csharp
using Autofac;
using Autofac.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Use Autofac as service provider
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

builder.Services.AddControllersWithViews();

// Configure Autofac
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    // Simple registrations
    containerBuilder.RegisterType<CitiesService>()
        .As<ICitiesService>()
        .InstancePerLifetimeScope();
    
    // Register with constructor parameters
    containerBuilder.RegisterType<ProductService>()
        .As<IProductService>()
        .WithParameter("connectionString", builder.Configuration.GetConnectionString("DefaultConnection"))
        .InstancePerLifetimeScope();
    
    // Register singleton
    containerBuilder.RegisterType<CacheService>()
        .As<ICacheService>()
        .SingleInstance();
    
    // Assembly scanning
    containerBuilder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
        .Where(t => t.Name.EndsWith("Repository"))
        .AsImplementedInterfaces()
        .InstancePerLifetimeScope();
    
    // Register modules
    containerBuilder.RegisterModule<ApplicationModule>();
});

var app = builder.Build();
app.MapControllers();
app.Run();
```

### Autofac vs. Built-in Container

| Feature | Built-in Container | Autofac |
|---------|-------------------|---------|
| **Basic DI** | ✅ Yes | ✅ Yes |
| **Constructor Injection** | ✅ Yes | ✅ Yes |
| **Property Injection** | Limited | ✅ Full support |
| **Method Injection** | ✅ Yes | ✅ Yes |
| **Modules** | ❌ No | ✅ Yes |
| **Assembly Scanning** | ❌ No | ✅ Yes |
| **Named/Keyed Services** | ❌ No | ✅ Yes |
| **Interception** | ❌ No | ✅ Yes |
| **Decorators** | Manual | ✅ Built-in support |
| **Performance** | Excellent | Very Good |
| **Learning Curve** | Low | Medium |

---

## Service Scopes

### Definition

A service scope is a logical boundary that defines the lifetime of services registered as Scoped. When a scope is created, the DI container instantiates any scoped services required within that scope. These service instances are shared across all components within the scope and are disposed of when the scope ends.

### How Service Scopes Work

#### Default Request Scope

In ASP.NET Core web applications, a new scope is automatically created at the beginning of each HTTP request. All scoped services are resolved from this request scope and remain alive throughout the entire request-response cycle. When the request completes, the scope is disposed, and all scoped services are disposed as well.

```
HTTP Request → Create Scope → Resolve Services → Process Request → Dispose Scope → HTTP Response
```

#### Explicitly Creating Scopes

You can create custom scopes manually for scenarios that don't correspond to HTTP requests, such as background tasks or unit testing.

**Basic Scope Creation:**
```csharp
public class BackgroundTaskService
{
    private readonly IServiceProvider _serviceProvider;
    
    public BackgroundTaskService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public async Task ProcessTaskAsync()
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
            
            // Use scoped services within this scope
            await orderService.ProcessPendingOrdersAsync();
            await dbContext.SaveChangesAsync();
        }
        // Scope disposed here, services cleaned up
    }
}
```

### Lifetime of Scoped Services

| Phase | Description |
|-------|-------------|
| **Creation** | New instance created on first request within scope |
| **Sharing** | Same instance returned for subsequent requests within scope |
| **Disposal** | Instance disposed when scope is disposed |

### Benefits of Service Scopes

**State Management:**
- Scoped services maintain state throughout a request
- State is isolated between different requests
- Prevents state leaking across user sessions

**Resource Efficiency:**
- Avoids creating multiple instances of the same service during a request
- Reduces memory allocations
- Improves performance through instance reuse

**Consistency:**
- All components in a request work with the same service instances
- Ensures data consistency within the request scope
- Simplifies transaction management

### Common Use Cases

#### 1. Database Contexts (Entity Framework Core)

```csharp
// Registration
builder.Services.AddScoped<ApplicationDbContext>();

// Usage - same context instance throughout request
public class OrderController : Controller
{
    private readonly ApplicationDbContext _context;
    
    public OrderController(ApplicationDbContext context)
    {
        _context = context; // Same instance as injected elsewhere in request
    }
    
    public async Task<IActionResult> Create(Order order)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        return RedirectToAction("Index");
    }
}
```

#### 2. User Context Services

```csharp
public class UserContextService
{
    private readonly HttpContext _httpContext;
    private User _currentUser;
    
    public UserContextService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContext = httpContextAccessor.HttpContext;
    }
    
    public async Task<User> GetCurrentUserAsync()
    {
        if (_currentUser == null)
        {
            var userId = _httpContext.User.GetUserId();
            _currentUser = await LoadUserFromDatabase(userId);
        }
        return _currentUser; // Cached for this request
    }
}

// Registration
builder.Services.AddScoped<UserContextService>();
```

#### 3. Transaction Management

```csharp
public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction _transaction;
    
    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }
    
    public async Task CommitAsync()
    {
        await _context.SaveChangesAsync();
        await _transaction.CommitAsync();
    }
    
    public void Dispose()
    {
        _transaction?.Dispose();
        _context?.Dispose();
    }
}

// Registration as Scoped ensures transaction spans the entire request
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
```

### Important Considerations

#### Avoiding Captive Dependencies

**Problem:**
Injecting a scoped service into a singleton creates a captive dependency. The scoped service will be held for the application lifetime instead of being disposed at the end of each scope.

```csharp
// ❌ BAD: Captive dependency
public class CachingService // Registered as Singleton
{
    private readonly ApplicationDbContext _context; // Scoped service
    
    public CachingService(ApplicationDbContext context)
    {
        _context = context; // PROBLEM: DbContext captured for app lifetime
    }
}
```

**Solution:**
Use `IServiceProvider` to resolve scoped services when needed.

```csharp
// ✅ GOOD: Resolve scoped service within a scope
public class CachingService
{
    private readonly IServiceProvider _serviceProvider;
    
    public CachingService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public async Task<List<Product>> GetProductsAsync()
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            return await context.Products.ToListAsync();
        }
    }
}
```

#### Proper Scope Disposal

Always dispose of manually created scopes to ensure resources are cleaned up properly.

```csharp
// ✅ GOOD: Using statement ensures disposal
public async Task ProcessDataAsync()
{
    using (var scope = _serviceProvider.CreateScope())
    {
        var service = scope.ServiceProvider.GetRequiredService<IDataService>();
        await service.ProcessAsync();
    } // Scope automatically disposed here
}

// ✅ GOOD: Try-finally pattern
public async Task ProcessDataAsync()
{
    var scope = _serviceProvider.CreateScope();
    try
    {
        var service = scope.ServiceProvider.GetRequiredService<IDataService>();
        await service.ProcessAsync();
    }
    finally
    {
        scope.Dispose();
    }
}
```

### Scope Validation

ASP.NET Core can validate scopes to catch captive dependency issues during development.

```csharp
var builder = WebApplication.CreateBuilder(args);

// Enable scope validation in development
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = builder.Environment.IsDevelopment();
    options.ValidateOnBuild = builder.Environment.IsDevelopment();
});
```

When enabled, scope validation will throw an exception if a scoped service is resolved from the root provider or if a captive dependency is detected.

---

## Interview Preparation

### Conceptual Questions

#### Q1: Explain the relationship between DIP, IoC, and DI

**Answer:**

The Dependency Inversion Principle (DIP) is a design principle stating that high-level modules should depend on abstractions rather than concrete implementations. Inversion of Control (IoC) is a broader programming principle where the framework controls object creation and lifecycle management instead of the application code. Dependency Injection (DI) is a specific implementation technique of IoC where dependencies are provided to classes from an external source, typically through constructor, property, or method injection.

**Relationship:**
- DIP provides the design principle (depend on abstractions)
- IoC describes the approach (invert control of object creation)
- DI implements the approach (inject dependencies)

#### Q2: What are the three service lifetimes in ASP.NET Core and when would you use each?

**Answer:**

**Transient:** A new instance is created each time the service is requested. Use for lightweight, stateless services where each operation requires a fresh instance (e.g., utilities, helpers, email senders).

**Scoped:** A single instance is created per scope, typically per HTTP request in web applications. Use for services that should maintain state during a request but not across requests (e.g., database contexts, user-specific data, transaction management).

**Singleton:** A single instance is created for the application lifetime and shared across all requests. Use for stateless services, configuration data, or expensive-to-create objects that can be safely shared (e.g., caching services, application settings, logging factories).

#### Q3: What is a captive dependency and why is it problematic?

**Answer:**

A captive dependency occurs when a service with a longer lifetime (e.g., Singleton) captures a service with a shorter lifetime (e.g., Scoped or Transient) as a dependency. This is problematic because:

1. The shorter-lived service is held in memory for the longer lifetime
2. Can cause memory leaks
3. May lead to stale or incorrect data
4. Violates the intended lifecycle of the captured service

For example, if a Singleton service injects a Scoped database context, that context will be held for the application lifetime instead of being disposed after each request, leading to potential data corruption and memory issues.

**Solution:** Use `IServiceProvider` to create scopes and resolve shorter-lived dependencies when needed, or redesign the service architecture to respect lifetime hierarchies.

### Practical Scenarios

#### Scenario 1: Implementing a Repository Pattern with DI

**Question:** How would you implement a repository pattern with proper dependency injection?

**Answer:**

```csharp
// 1. Define interface
public interface IProductRepository
{
    Task<Product> GetByIdAsync(int id);
    Task<IEnumerable<Product>> GetAllAsync();
    Task AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(int id);
}

// 2. Implement repository
public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;
    
    public ProductRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<Product> GetByIdAsync(int id)
    {
        return await _context.Products.FindAsync(id);
    }
    
    // ... other implementations
}

// 3. Register with DI container
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ApplicationDbContext>();

// 4. Inject into service or controller
public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    
    public ProductService(IProductRepository repository)
    {
        _repository = repository;
    }
    
    public async Task<Product> GetProductAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }
}
```

#### Scenario 2: Handling Multiple Implementations

**Question:** You have multiple implementations of an interface. How would you register and use them?

**Answer:**

```csharp
// Multiple implementations
public interface INotificationService
{
    Task SendNotificationAsync(string message);
}

public class EmailNotificationService : INotificationService
{
    public async Task SendNotificationAsync(string message)
    {
        // Send email
    }
}

public class SmsNotificationService : INotificationService
{
    public async Task SendNotificationAsync(string message)
    {
        // Send SMS
    }
}

public class PushNotificationService : INotificationService
{
    public async Task SendNotificationAsync(string message)
    {
        // Send push notification
    }
}

// Approach 1: Register all implementations and inject IEnumerable
builder.Services.AddScoped<INotificationService, EmailNotificationService>();
builder.Services.AddScoped<INotificationService, SmsNotificationService>();
builder.Services.AddScoped<INotificationService, PushNotificationService>();

// Usage - inject all implementations
public class NotificationManager
{
    private readonly IEnumerable<INotificationService> _notificationServices;
    
    public NotificationManager(IEnumerable<INotificationService> notificationServices)
    {
        _notificationServices = notificationServices;
    }
    
    public async Task SendToAllAsync(string message)
    {
        foreach (var service in _notificationServices)
        {
            await service.SendNotificationAsync(message);
        }
    }
}

// Approach 2: Use factory pattern
public interface INotificationServiceFactory
{
    INotificationService GetService(NotificationType type);
}

public class NotificationServiceFactory : INotificationServiceFactory
{
    private readonly IServiceProvider _serviceProvider;
    
    public NotificationServiceFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public INotificationService GetService(NotificationType type)
    {
        return type switch
        {
            NotificationType.Email => _serviceProvider.GetRequiredService<EmailNotificationService>(),
            NotificationType.Sms => _serviceProvider.GetRequiredService<SmsNotificationService>(),
            NotificationType.Push => _serviceProvider.GetRequiredService<PushNotificationService>(),
            _ => throw new ArgumentException("Invalid notification type")
        };
    }
}
```

#### Scenario 3: Testing Services with Dependencies

**Question:** How do you unit test a service that has dependencies?

**Answer:**

```csharp
// Service to test
public class OrderService : IOrderService
{
    private readonly IOrderRepository _repository;
    private readonly IEmailService _emailService;
    private readonly ILogger<OrderService> _logger;
    
    public OrderService(
        IOrderRepository repository,
        IEmailService emailService,
        ILogger<OrderService> logger)
    {
        _repository = repository;
        _emailService = emailService;
        _logger = logger;
    }
    
    public async Task<bool> CreateOrderAsync(Order order)
    {
        try
        {
            await _repository.AddAsync(order);
            await _emailService.SendOrderConfirmationAsync(order);
            _logger.LogInformation("Order {OrderId} created successfully", order.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return false;
        }
    }
}

// Unit test using Moq
public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _mockRepository;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly Mock<ILogger<OrderService>> _mockLogger;
    private readonly OrderService _service;
    
    public OrderServiceTests()
    {
        _mockRepository = new Mock<IOrderRepository>();
        _mockEmailService = new Mock<IEmailService>();
        _mockLogger = new Mock<ILogger<OrderService>>();
        
        _service = new OrderService(
            _mockRepository.Object,
            _mockEmailService.Object,
            _mockLogger.Object);
    }
    
    [Fact]
    public async Task CreateOrderAsync_ValidOrder_ReturnsTrue()
    {
        // Arrange
        var order = new Order { Id = 1, CustomerId = 100, TotalAmount = 500.00m };
        
        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<Order>()))
            .Returns(Task.CompletedTask);
        
        _mockEmailService
            .Setup(e => e.SendOrderConfirmationAsync(It.IsAny<Order>()))
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _service.CreateOrderAsync(order);
        
        // Assert
        Assert.True(result);
        _mockRepository.Verify(r => r.AddAsync(order), Times.Once);
        _mockEmailService.Verify(e => e.SendOrderConfirmationAsync(order), Times.Once);
    }
    
    [Fact]
    public async Task CreateOrderAsync_RepositoryThrowsException_ReturnsFalse()
    {
        // Arrange
        var order = new Order { Id = 1 };
        
        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<Order>()))
            .ThrowsAsync(new Exception("Database error"));
        
        // Act
        var result = await _service.CreateOrderAsync(order);
        
        // Assert
        Assert.False(result);
        _mockEmailService.Verify(
            e => e.SendOrderConfirmationAsync(It.IsAny<Order>()), 
            Times.Never);
    }
}
```

### Troubleshooting Common Issues

#### Issue 1: Service Not Registered

**Error:**
```
InvalidOperationException: Unable to resolve service for type 'IProductService' 
while attempting to activate 'ProductsController'.
```

**Cause:** The service was not registered in the DI container.

**Solution:**
```csharp
// Add missing registration in Program.cs
builder.Services.AddScoped<IProductService, ProductService>();
```

#### Issue 2: Circular Dependencies

**Error:**
```
InvalidOperationException: A circular dependency was detected for the service 
of type 'IServiceA'.
```

**Example:**
```csharp
// ❌ BAD: Circular dependency
public class ServiceA
{
    public ServiceA(IServiceB serviceB) { }
}

public class ServiceB
{
    public ServiceB(IServiceA serviceA) { } // Circular reference
}
```

**Solutions:**

**Option 1: Refactor to remove circular dependency**
```csharp
// Create a shared service
public class SharedService
{
    // Common functionality
}

public class ServiceA
{
    public ServiceA(SharedService shared) { }
}

public class ServiceB
{
    public ServiceB(SharedService shared) { }
}
```

**Option 2: Use lazy injection**
```csharp
public class ServiceA
{
    private readonly Lazy<IServiceB> _serviceB;
    
    public ServiceA(Lazy<IServiceB> serviceB)
    {
        _serviceB = serviceB;
    }
    
    public void DoWork()
    {
        _serviceB.Value.PerformAction();
    }
}
```

#### Issue 3: Captive Dependency Detected

**Error:**
```
InvalidOperationException: Cannot resolve scoped service 'ApplicationDbContext' 
from root provider.
```

**Cause:** Attempting to resolve a scoped service from a singleton or root provider.

**Solution:**
```csharp
// ✅ GOOD: Create scope when needed
public class SingletonService
{
    private readonly IServiceProvider _serviceProvider;
    
    public SingletonService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public async Task ProcessDataAsync()
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();
            // Use context
        }
    }
}
```

### Performance Considerations

#### 1. Service Resolution Overhead

**Issue:** Frequent service resolution can impact performance.

**Optimization:**
```csharp
// ❌ AVOID: Resolving in tight loops
for (int i = 0; i < 1000; i++)
{
    var service = serviceProvider.GetRequiredService<IMyService>();
    service.DoWork();
}

// ✅ BETTER: Resolve once, reuse
var service = serviceProvider.GetRequiredService<IMyService>();
for (int i = 0; i < 1000; i++)
{
    service.DoWork();
}
```

#### 2. Singleton Memory Management

**Issue:** Singleton services live for the application lifetime and can accumulate memory.

**Best Practice:**
```csharp
// Implement IDisposable for cleanup
public class CacheService : ICacheService, IDisposable
{
    private readonly ConcurrentDictionary<string, object> _cache = new();
    
    public void Clear()
    {
        _cache.Clear();
    }
    
    public void Dispose()
    {
        _cache.Clear();
        GC.SuppressFinalize(this);
    }
}
```

#### 3. Avoiding Over-Injection

**Issue:** Too many dependencies indicate design problems.

**Guideline:**
```csharp
// ⚠️ WARNING: Too many dependencies (>5-7 is a code smell)
public class OrderProcessor
{
    public OrderProcessor(
        IOrderService orderService,
        IPaymentService paymentService,
        IInventoryService inventoryService,
        IShippingService shippingService,
        IEmailService emailService,
        INotificationService notificationService,
        ILoggingService loggingService,
        IAuditService auditService) // Too many!
    {
        // Consider breaking this into smaller classes
    }
}

// ✅ BETTER: Use facade or mediator pattern
public class OrderProcessor
{
    private readonly IOrderFacade _orderFacade;
    
    public OrderProcessor(IOrderFacade orderFacade)
    {
        _orderFacade = orderFacade;
    }
}
```

### Design Patterns with DI

#### 1. Factory Pattern

```csharp
public interface IReportFactory
{
    IReport CreateReport(ReportType type);
}

public class ReportFactory : IReportFactory
{
    private readonly IServiceProvider _serviceProvider;
    
    public ReportFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public IReport CreateReport(ReportType type)
    {
        return type switch
        {
            ReportType.Sales => _serviceProvider.GetRequiredService<SalesReport>(),
            ReportType.Inventory => _serviceProvider.GetRequiredService<InventoryReport>(),
            ReportType.Customer => _serviceProvider.GetRequiredService<CustomerReport>(),
            _ => throw new ArgumentException("Invalid report type")
        };
    }
}
```

#### 2. Decorator Pattern

```csharp
public interface IDataService
{
    Task<Data> GetDataAsync(int id);
}

public class DataService : IDataService
{
    private readonly IRepository _repository;
    
    public DataService(IRepository repository)
    {
        _repository = repository;
    }
    
    public async Task<Data> GetDataAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }
}

// Caching decorator
public class CachedDataService : IDataService
{
    private readonly IDataService _innerService;
    private readonly IMemoryCache _cache;
    
    public CachedDataService(IDataService innerService, IMemoryCache cache)
    {
        _innerService = innerService;
        _cache = cache;
    }
    
    public async Task<Data> GetDataAsync(int id)
    {
        var cacheKey = $"data_{id}";
        
        if (_cache.TryGetValue(cacheKey, out Data cachedData))
        {
            return cachedData;
        }
        
        var data = await _innerService.GetDataAsync(id);
        _cache.Set(cacheKey, data, TimeSpan.FromMinutes(10));
        return data;
    }
}

// Registration
builder.Services.AddScoped<DataService>();
builder.Services.AddScoped<IDataService>(provider =>
{
    var dataService = provider.GetRequiredService<DataService>();
    var cache = provider.GetRequiredService<IMemoryCache>();
    return new CachedDataService(dataService, cache);
});
```

#### 3. Strategy Pattern

```csharp
public interface IPaymentStrategy
{
    Task<PaymentResult> ProcessPaymentAsync(decimal amount);
}

public class CreditCardPayment : IPaymentStrategy
{
    public async Task<PaymentResult> ProcessPaymentAsync(decimal amount)
    {
        // Credit card processing logic
        return await Task.FromResult(new PaymentResult { Success = true });
    }
}

public class PayPalPayment : IPaymentStrategy
{
    public async Task<PaymentResult> ProcessPaymentAsync(decimal amount)
    {
        // PayPal processing logic
        return await Task.FromResult(new PaymentResult { Success = true });
    }
}

public class PaymentContext
{
    private readonly IServiceProvider _serviceProvider;
    
    public PaymentContext(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public async Task<PaymentResult> ProcessAsync(PaymentType type, decimal amount)
    {
        var strategy = type switch
        {
            PaymentType.CreditCard => _serviceProvider.GetRequiredService<CreditCardPayment>(),
            PaymentType.PayPal => _serviceProvider.GetRequiredService<PayPalPayment>(),
            _ => throw new ArgumentException("Invalid payment type")
        };
        
        return await strategy.ProcessPaymentAsync(amount);
    }
}
```

### Advanced Topics

#### 1. Generic Service Registration

```csharp
// Register open generic types
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Usage
public class ProductService
{
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<Category> _categoryRepository;
    
    public ProductService(
        IRepository<Product> productRepository,
        IRepository<Category> categoryRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
    }
}
```

#### 2. Conditional Service Registration

```csharp
// Register different implementations based on environment
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddScoped<IEmailService, MockEmailService>();
}
else
{
    builder.Services.AddScoped<IEmailService, SmtpEmailService>();
}

// Register based on configuration
var useCache = builder.Configuration.GetValue<bool>("Features:UseCache");
if (useCache)
{
    builder.Services.AddSingleton<ICacheService, RedisCacheService>();
}
else
{
    builder.Services.AddSingleton<ICacheService, MemoryCacheService>();
}
```

#### 3. Service Replacement

```csharp
// Replace built-in service
builder.Services.AddControllers();

// Replace the default model binder
builder.Services.Replace(ServiceDescriptor.Transient<IModelBinderProvider, CustomModelBinderProvider>());

// Remove and re-register
var descriptor = builder.Services.FirstOrDefault(d => d.ServiceType == typeof(IMyService));
if (descriptor != null)
{
    builder.Services.Remove(descriptor);
}
builder.Services.AddScoped<IMyService, NewImplementation>();
```

---

## Summary and Key Takeaways

### Core Principles

| Principle | Description | Implementation |
|-----------|-------------|----------------|
| **Dependency Inversion** | Depend on abstractions, not concrete types | Use interfaces for dependencies |
| **Inversion of Control** | Framework controls object creation | Register services with DI container |
| **Dependency Injection** | Supply dependencies externally | Use constructor injection |
| **Single Responsibility** | Each service has one clear purpose | Keep services focused and cohesive |
| **Loose Coupling** | Minimize dependencies between components | Use interfaces and abstraction |

### Service Lifetime Quick Reference

```
Transient  → New instance every time
Scoped     → One instance per request/scope
Singleton  → One instance for application lifetime

Valid Dependencies:
Singleton  → Singleton only
Scoped     → Scoped, Singleton
Transient  → Transient, Scoped, Singleton
```

### Best Practices Checklist

- ✅ Use constructor injection as the default method
- ✅ Always depend on interfaces, not concrete implementations
- ✅ Register all services at the composition root (Program.cs)
- ✅ Choose appropriate service lifetimes based on state and resource usage
- ✅ Avoid captive dependencies (respect lifetime hierarchy)
- ✅ Keep services focused with single responsibility
- ✅ Write unit tests using mock dependencies
- ✅ Avoid the Service Locator anti-pattern
- ✅ Use the Options pattern for configuration
- ✅ Implement IDisposable for proper resource cleanup
- ✅ Validate scopes in development environment
- ✅ Keep constructor parameter count reasonable (3-5 max)

### Common Interview Topics

**Conceptual Understanding:**
- Explain DIP, IoC, and DI relationships
- Describe service lifetimes and their use cases
- Discuss captive dependencies and their problems

**Practical Skills:**
- Demonstrate service registration and injection
- Show how to test services with dependencies
- Explain when to use different injection techniques

**Architecture:**
- Discuss repository and unit of work patterns
- Explain decorator and factory patterns with DI
- Demonstrate proper scope management

**Troubleshooting:**
- Identify and resolve circular dependencies
- Debug service registration issues
- Fix captive dependency problems

### Additional Resources

**Official Documentation:**
- [ASP.NET Core Dependency Injection](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection)
- [Service Lifetimes](https://docs.microsoft.com/en-us/dotnet/core/extensions/dependency-injection#service-lifetimes)
- [Autofac Documentation](https://autofac.readthedocs.io/)

**Design Principles:**
- SOLID Principles
- Repository Pattern
- Unit of Work Pattern
- Factory Pattern
- Decorator Pattern

**Testing:**
- Unit testing with Moq
- Integration testing with WebApplicationFactory
- Test-driven development (TDD)

---

## Conclusion

Dependency Injection is a fundamental concept in modern ASP.NET Core development that enables loose coupling, testability, and maintainability. By understanding and properly implementing the Dependency Inversion Principle, Inversion of Control, and Dependency Injection techniques, you can build robust, scalable applications with clean architecture.

**Key Points to Remember:**

1. **Services** encapsulate business logic and should be reusable, testable, and loosely coupled
2. **DI Container** manages service creation, injection, and disposal throughout the application
3. **Service Lifetimes** determine how long service instances live and how they are shared
4. **Constructor Injection** is the preferred method for injecting dependencies
5. **Interfaces** provide abstraction and enable flexible, testable code
6. **Best Practices** include avoiding captive dependencies, choosing appropriate lifetimes, and keeping services focused

By mastering these concepts and following established best practices, you will be well-equipped to design and implement professional ASP.NET Core applications that are maintainable, testable, and scalabl