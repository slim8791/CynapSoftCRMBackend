1. Geolocation Integration (Field Validation)
Objective: Proof of presence at the medical facility.

Service (VisiteService): Integrate Microsoft.Maui.Devices.Sensors.Geolocator.

Action: When a Delegate submits a Rapport, the app must capture Latitude and Longitude.

Logic:

Add Lat and Long fields to the Rapport Model.

Implement a "Check-in" feature or silent capture upon "Submit Report".

Requirement: Handle permission requests for Android and Windows location services.

2. Advanced Offline Strategy (SQLite)
Objective: Maintain usability in "Dead Zones" (Hospitals, clinics).

Library: Add sqlite-net-pcl.

Implementation:

Replace simple JSON caching with a local SQLite database (CynapharmLocal.db).

Tables: Product_Cache, Stock_Local, Pending_Rapports.

Sync Logic: On Dashboard load, "Seed" or "Update" the local database with the latest product catalog and the delegate's stock levels.

Search: Update ProductListViewModel to query the local SQLite DB if Connectivity.NetworkAccess != Internet.

3. Inventory Quota Enforcement (Client-Side)
Objective: Prevent data inconsistency and "over-distribution" of samples.

Logic in InventoryService:

Maintain a local counter of StockDelegue.

Rule: The "Distribute Sample" button must be disabled (IsEnabled = false) if LocalStockQty <= 0.

Feedback: Show a specific UI warning: "Stock insuffisant pour ce lot".

4. Input Validation (ObservableValidator)
Objective: Ensure high-quality data entry and UI feedback.

Framework: Use CommunityToolkit.Mvvm.ComponentModel.ObservableValidator.

Implementation in RapportViewModel & CreateOrderViewModel:

Rules: * ContenuRapport: Required, Min Length 20 chars.

OrderQuantity: Range [1 - MaxAvailable].

UI: Use ErrorsContainer to display validation messages in red below Entry fields. Disable Submit buttons until HasErrors is false.

5. Automatic Promotion Engine (Order Flow)
Objective: Real-time price calculation for clients.

Logic in CreateOrderViewModel:

As soon as a product is added to the cart, the ViewModel must query the local Promotion_Cache.

Calculation: * BasePrice = Product.PrixUnitaire.

DiscountedPrice = BasePrice * (1 - Promo.RemisePourcentage / 100).

UI: Display the crossed-out original price and the new "Prix Promo" in bold green to emphasize the saving.