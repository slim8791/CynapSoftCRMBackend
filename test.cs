using System;
using System.Linq;
using CynapCRM.Services.OrderAPI.Data;
using Microsoft.EntityFrameworkCore;

class Program
{
    static void Main()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(""Server=.;Database=Cynapharm_OrderDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"")
            .Options;
            
        using var db = new AppDbContext(options);
        var recs = db.Reclamations.ToList();
        Console.WriteLine($""Total reclamations: {recs.Count}"");
        foreach(var r in recs) {
            Console.WriteLine($""ID: {r.Id_Rec}, Client: {r.Id_Client}, Commande: {r.Id_Commande}, Message: {r.Message}"");
        }
    }
}
