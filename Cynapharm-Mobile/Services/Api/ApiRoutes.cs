namespace Cynapharm_Mobile.Services;

public static class ApiRoutes
{
    public static class Auth
    {
        public const string Login          = "api/auth/login";
        public const string ForgotPassword = "api/auth/forgot-password";
        public const string ChangePassword  = "api/auth/change-password";
        public const string UpdateProfile  = "api/auth/update-profile";
        public const string Me             = "api/auth/me";
    }

    public static class Products
    {
        public const string Base       = "api/products";
        public const string Search     = "api/products/search";
        public const string Categories = "api/products/categories";
        public const string Lots       = "api/lots";
        public const string Promos     = "api/promos";
        public const string Marketing  = "api/marketting";
    }

    public static class Orders
    {
        public const string Base         = "orders";
        public const string ByStatus     = "orders/by-status";
        public const string ByDate       = "orders/by-date";
        public const string Dashboard    = "orders/dashboard";
        public const string Cancel       = "orders/{0}/cancel";
        public const string UpdateStatus = "orders/status";
        public const string ByClient     = "orders/by-client";
        public const string Lignes       = "orders/lignes";
        public const string Reclamations = "orders/reclamations";
    }

    public static class Field
    {
        public const string Visites          = "api/visites";
        public const string Rapports         = "api/rapports";
        public const string Plannings        = "api/plannings";
        public const string Objectifs        = "api/objectifs";
        public const string Kpi              = "api/kpi";
        public const string Regions          = "api/regions";
        public const string AllVisites       = "fields/visites";
        public const string TauxConversion   = "fields/kpi/taux-conversion";
        public const string AllPlannings     = "fields/plannings";
        public const string RapportsByDelegue = "fields/rapports/by-delegue";
    }

    public static class Inventory
    {
        public const string Stocks             = "api/stocks-delegue";
        public const string Movements          = "api/stock-movements";
        public const string Distributions      = "api/distributions";
        public const string StocksPromo        = "api/stocks-promotionnels";
        public const string Business           = "api/inventory-business";
        public const string StockSummary       = "inventory/inventory-business/summary";
        public const string MovementsByDelegue = "inventory/stock-movements/by-delegue";
        public const string AllDistributions   = "inventory/distributions";
    }

    public static class Documents
    {
        public const string Factures                = "api/factures";
        public const string BonsCommande            = "api/bons-commandes";
        public const string BonsLivraison           = "api/bons-livraison";
        public const string DocumentsByClientAndType = "documents/client/{0}/type/{1}";
        public const string FacturesByCommande      = "documents/factures/commande";
        public const string BCByCommande            = "documents/bons-commandes/commande";
        public const string BLByCommande            = "documents/bons-livraison/commande";
    }
}
