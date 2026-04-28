# Angular Architecture Plan for Cynapharm Project

**Date:** April 28, 2026  
**Project:** Cynapharm  

## 1. Folder Structure

The project will follow the standard Angular folder structure:

```
src/
├── app/
│   ├── core/                # Core module for app-wide services, guards, and interceptors
│   │   ├── services/        # Core services (e.g., AuthService, ApiService)
│   │   ├── guards/          # Route guards
│   │   ├── interceptors/    # HTTP interceptors
│   │   └── core.module.ts   # Core module definition
│   ├── shared/              # Shared module for reusable components, directives, and pipes
│   │   ├── components/      # Shared UI components
│   │   ├── directives/      # Shared directives
│   │   ├── pipes/           # Shared pipes
│   │   └── shared.module.ts # Shared module definition
│   ├── features/            # Feature modules for specific app functionality
│   │   ├── dashboard/       # Dashboard feature module
│   │   ├── products/        # Products feature module
│   │   ├── orders/          # Orders feature module
│   │   ├── users/           # Users feature module
│   │   └── ...              # Additional feature modules
│   ├── app-routing.module.ts # Root routing module
│   ├── app.module.ts        # Root app module
│   └── app.component.*      # Root app component (HTML, CSS, TS)
├── assets/                  # Static assets (images, styles, etc.)
├── environments/            # Environment-specific configuration files
```

---

## 2. Modules and Lazy Loading

### Core Module
- **Purpose:** Contains app-wide services, guards, and interceptors.
- **Location:** `src/app/core/`
- **Contents:**
  - `AuthService`: Handles authentication logic.
  - `ApiService`: Centralized HTTP client for API calls.
  - `AuthGuard`: Protects routes requiring authentication.
  - `ErrorInterceptor`: Intercepts HTTP errors and handles them globally.
  - `TokenInterceptor`: Attaches authentication tokens to outgoing requests.

### Shared Module
- **Purpose:** Contains reusable components, directives, and pipes.
- **Location:** `src/app/shared/`
- **Contents:**
  - Components: `ButtonComponent`, `CardComponent`, `TableComponent`.
  - Directives: `HighlightDirective`, `DebounceClickDirective`.
  - Pipes: `DateFormatPipe`, `CurrencyPipe`.

### Feature Modules
Each feature module will be lazily loaded and contain its own routing module. Below are the planned feature modules:

#### Dashboard Module
- **Purpose:** Displays an overview of key metrics and data.
- **Location:** `src/app/features/dashboard/`
- **Components:**
  - `DashboardComponent`: Main dashboard view.
- **Services:**
  - `DashboardService`: Fetches dashboard data from the backend.

#### Products Module
- **Purpose:** Manages product-related functionality.
- **Location:** `src/app/features/products/`
- **Components:**
  - `ProductListComponent`: Displays a list of products.
  - `ProductDetailComponent`: Displays product details.
  - `ProductFormComponent`: Handles product creation/editing.
- **Services:**
  - `ProductService`: Handles product-related API calls.

#### Orders Module
- **Purpose:** Manages order-related functionality.
- **Location:** `src/app/features/orders/`
- **Components:**
  - `OrderListComponent`: Displays a list of orders.
  - `OrderDetailComponent`: Displays order details.
- **Services:**
  - `OrderService`: Handles order-related API calls.

#### Users Module
- **Purpose:** Manages user-related functionality.
- **Location:** `src/app/features/users/`
- **Components:**
  - `UserListComponent`: Displays a list of users.
  - `UserDetailComponent`: Displays user details.
  - `UserFormComponent`: Handles user creation/editing.
- **Services:**
  - `UserService`: Handles user-related API calls.

---

## 3. Routing Structure

The app will use lazy loading for feature modules. Below is the routing structure:

```typescript
const routes: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  { path: 'dashboard', loadChildren: () => import('./features/dashboard/dashboard.module').then(m => m.DashboardModule) },
  { path: 'products', loadChildren: () => import('./features/products/products.module').then(m => m.ProductsModule) },
  { path: 'orders', loadChildren: () => import('./features/orders/orders.module').then(m => m.OrdersModule) },
  { path: 'users', loadChildren: () => import('./features/users/users.module').then(m => m.UsersModule) },
  { path: '**', redirectTo: 'dashboard' }
];
```

---

## 4. Backend API Integration

### API Endpoints
The following backend API endpoints will be integrated into the Angular app:

- **Authentication:**
  - `POST /api/auth/login`: User login.
  - `POST /api/auth/register`: User registration.
  - `GET /api/auth/profile`: Fetch user profile.

- **Products:**
  - `GET /api/products`: Fetch all products.
  - `GET /api/products/:id`: Fetch product details.
  - `POST /api/products`: Create a new product.
  - `PUT /api/products/:id`: Update a product.
  - `DELETE /api/products/:id`: Delete a product.

- **Orders:**
  - `GET /api/orders`: Fetch all orders.
  - `GET /api/orders/:id`: Fetch order details.
  - `POST /api/orders`: Create a new order.
  - `PUT /api/orders/:id`: Update an order.
  - `DELETE /api/orders/:id`: Delete an order.

- **Inventory:**
  - `GET /api/inventory`: Fetch inventory data.
  - `GET /api/inventory/:id`: Fetch specific inventory details.
  - `POST /api/inventory`: Add new inventory.
  - `PUT /api/inventory/:id`: Update inventory details.
  - `DELETE /api/inventory/:id`: Remove inventory.

- **Field:**
  - `GET /api/field`: Fetch all field data.
  - `GET /api/field/:id`: Fetch specific field details.
  - `POST /api/field`: Add new field data.
  - `PUT /api/field/:id`: Update field data.
  - `DELETE /api/field/:id`: Remove field data.

- **Documents:**
  - `GET /api/documents`: Fetch all documents.
  - `GET /api/documents/:id`: Fetch specific document details.
  - `POST /api/documents`: Upload a new document.
  - `PUT /api/documents/:id`: Update document metadata.
  - `DELETE /api/documents/:id`: Delete a document.

### API Integration Services
Each feature module will have its own service to handle API calls. These services will use `HttpClient` and will be located in the respective feature module folders.

---

## 5. Reusable Components

The following reusable components will be part of the `SharedModule`:

- **ButtonComponent:** A customizable button component.
- **CardComponent:** A card layout component.
- **TableComponent:** A table component for displaying data.

---

## 6. Interceptors

The following interceptors will be implemented in the `CoreModule`:

- **TokenInterceptor:** Attaches the authentication token to outgoing HTTP requests.
- **ErrorInterceptor:** Handles HTTP errors globally and displays appropriate messages.

---

This plan provides a clear and modular structure for the Cynapharm Angular project, ensuring scalability, maintainability, and adherence to Angular best practices.