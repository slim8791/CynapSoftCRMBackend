namespace Cynapharm_Mobile.Services;

public static class ApiRoutes
{
    public static class Auth
    {
        public const string Login          = "api/auth/login";
        public const string ForgotPassword = "api/auth/forgot-password";
        public const string ChangePassword = "api/auth/change-password";
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
        public const string Base       = "api/orders";
        public const string Lines      = "api/lignes";
        public const string Complaints = "api/reclamations";
    }

    public static class Field
    {
        public const string Visites   = "api/visites";
        public const string Rapports  = "api/rapports";
        public const string Plannings = "api/plannings";
        public const string Objectifs = "api/objectifs";
        public const string Kpi       = "api/kpi";
        public const string Regions   = "api/regions";
    }

    public static class Inventory
    {
        public const string Stocks        = "api/stocks-delegue";
        public const string Movements     = "api/stock-movements";
        public const string Distributions = "api/distributions";
        public const string StocksPromo   = "api/stocks-promotionnels";
        public const string Business      = "api/inventory-business";
    }

    public static class Documents
    {
        public const string Factures      = "api/factures";
        public const string BonsCommande  = "api/bons-commandes";
        public const string BonsLivraison = "api/bons-livraison";
    }
}
