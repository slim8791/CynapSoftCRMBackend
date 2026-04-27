# Angular Frontend Architecture Plan for Cynapharm

## 1. Analysis of Backend APIs and Project Requirements
- **Backend APIs**:
  - Analyze the endpoints provided by the backend for CRUD operations, authentication, and other functionalities.
  - Identify the data models and relationships exposed by the backend.
  - Ensure proper error handling and response structure for seamless integration.

- **Project Requirements**:
  - Define the core features of the application (e.g., user management, product catalog, order processing).
  - Ensure the frontend aligns with the backend's capabilities and business logic.

## 2. Page and Navigation Structure
- **Pages**:
  - **Authentication**:
    - Login
    - Register
    - Forgot Password
  - **Dashboard**:
    - Overview of key metrics and notifications.
  - **User Management**:
    - User List
    - User Details
    - Add/Edit User
  - **Product Management**:
    - Product List
    - Product Details
    - Add/Edit Product
  - **Order Management**:
    - Order List
    - Order Details
    - Add/Edit Order
  - **Settings**:
    - Profile Settings
    - Application Settings

- **Navigation**:
  - Use a side navigation bar for primary sections (Dashboard, User Management, Product Management, etc.).
  - Include a top navigation bar for user profile and quick actions.

## 3. Required Services and Interceptors
- **Services**:
  - `AuthService`: Handle authentication and token management.
  - `UserService`: Manage user-related API calls.
  - `ProductService`: Manage product-related API calls.
  - `OrderService`: Manage order-related API calls.
  - `SettingsService`: Handle application settings.

- **Interceptors**:
  - `AuthInterceptor`: Attach JWT tokens to outgoing requests.
  - `ErrorInterceptor`: Handle API errors globally.

## 4. API Integration and Data Flow
- **State Management**:
  - Use NgRx or Akita for managing application state.
  - Define actions, reducers, and selectors for each feature module.

- **Data Flow**:
  - Fetch data from APIs using services.
  - Store data in the state for shared access across components.
  - Use resolvers for preloading data in routes.

- **Error Handling**:
  - Display user-friendly error messages for API failures.
  - Log errors for debugging purposes.

## 5. Folder Structure
```
src/
├── app/
│   ├── core/
│   │   ├── services/
│   │   ├── interceptors/
│   │   ├── guards/
│   │   └── models/
│   ├── features/
│   │   ├── auth/
│   │   ├── dashboard/
│   │   ├── users/
│   │   ├── products/
│   │   ├── orders/
│   │   └── settings/
│   ├── shared/
│   │   ├── components/
│   │   ├── directives/
│   │   └── pipes/
│   └── app.module.ts
```

## 6. Testing
- Write unit tests for services, components, and interceptors.
- Use end-to-end testing for critical user flows.

## 7. Mapping Backend API Endpoints to Angular Pages and Services

### Authentication
- **Login Page**:
  - Endpoint: `POST /login`
  - Service: `AuthService`
- **Register Page**:
  - Endpoint: `POST /register`
  - Service: `AuthService`

### User Management
- **User List Page**:
  - Endpoint: `GET /users`
  - Service: `UserService`
- **User Details Page**:
  - Endpoint: `GET /users/{id}`
  - Service: `UserService`
- **Add/Edit User Page**:
  - Endpoint: `POST /users`
  - Service: `UserService`

### Product Management
- **Product List Page**:
  - Endpoint: `GET /products`
  - Service: `ProductService`
- **Product Details Page**:
  - Endpoint: `GET /products/{id}`
  - Service: `ProductService`
- **Add/Edit Product Page**:
  - Endpoint: `POST /products`
  - Service: `ProductService`

### Order Management
- **Order List Page**:
  - Endpoint: `GET /orders`
  - Service: `OrderService`
- **Order Details Page**:
  - Endpoint: `GET /orders/{id}`
  - Service: `OrderService`
- **Add/Edit Order Page**:
  - Endpoint: `POST /orders`
  - Service: `OrderService`

### Reports
- **Reports List Page**:
  - Endpoint: `GET /rapports/by-visite/{idVisite}`
  - Service: `RapportService`
- **Report Details Page**:
  - Endpoint: `GET /rapports/{id}`
  - Service: `RapportService`
- **Add/Edit Report Page**:
  - Endpoint: `POST /rapports/createUpdate`
  - Service: `RapportService`

### Inventory Management
- **Inventory List Page**:
  - Endpoint: `GET /lots/{id}/lots`
  - Service: `LotService`
- **Adjust Stock Page**:
  - Endpoint: `PUT /product/{productId}/adjust-stock`
  - Service: `LotService`

### Promotions
- **Promotions List Page**:
  - Endpoint: `GET /promotions`
  - Service: `PromoService`
- **Add/Edit Promotion Page**:
  - Endpoint: `POST /promotions`
  - Service: `PromoService`

### Regions
- **Regions List Page**:
  - Endpoint: `GET /regions/all`
  - Service: `RegionService`
- **Add/Edit Region Page**:
  - Endpoint: `POST /regions`
  - Service: `RegionService`

### FieldAPI Mapping

#### KPI Management
- **KPI Dashboard Page**:
  - Endpoint: `GET /api/kpi/visites-count`
  - Service: `KPIService`
- **KPI History Page**:
  - Endpoint: `GET /api/kpi/historique/{idDelegue}`
  - Service: `KPIService`
- **Client Loyalty Page**:
  - Endpoint: `GET /api/kpi/client-fidelite/{idClient}`
  - Service: `KPIService`

#### Objectives Management
- **Objectives List Page**:
  - Endpoint: `GET /api/objectifs`
  - Service: `ObjectifService`
- **Objective Details Page**:
  - Endpoint: `GET /api/objectifs/{idObjectif}`
  - Service: `ObjectifService`
- **Add/Edit Objective Page**:
  - Endpoint: `POST /api/objectifs`
  - Service: `ObjectifService`

#### Planning Management
- **Planning List Page**:
  - Endpoint: `GET /api/plannings/by-delegue/{idDelegue}`
  - Service: `PlanningService`
- **Planning Details Page**:
  - Endpoint: `GET /api/plannings/{idPlanning}`
  - Service: `PlanningService`
- **Add/Edit Planning Page**:
  - Endpoint: `POST /api/plannings`
  - Service: `PlanningService`

#### Reports Management
- **Reports List Page**:
  - Endpoint: `GET /api/rapports/by-visite/{idVisite}`
  - Service: `RapportService`
- **Report Details Page**:
  - Endpoint: `GET /api/rapports/{id}`
  - Service: `RapportService`
- **Add/Edit Report Page**:
  - Endpoint: `POST /api/rapports/createUpdate`
  - Service: `RapportService`

#### Regions Management
- **Regions List Page**:
  - Endpoint: `GET /api/regions/all`
  - Service: `RegionService`
- **Region Details Page**:
  - Endpoint: `GET /api/regions/{idRegion}`
  - Service: `RegionService`
- **Add/Edit Region Page**:
  - Endpoint: `POST /api/regions`
  - Service: `RegionService`

#### Visits Management
- **Visits List Page**:
  - Endpoint: `GET /api/visites/by-delegue/{idDelegue}`
  - Service: `VisiteService`
- **Visit Details Page**:
  - Endpoint: `GET /api/visites/{idVisite}`
  - Service: `VisiteService`
- **Add/Edit Visit Page**:
  - Endpoint: `POST /api/visites`
  - Service: `VisiteService`