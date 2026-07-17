# 📘 Fiche de Maîtrise — RabbitMQ & MassTransit

> **Projet :** CynapPharm CRM — Architecture Microservices .NET 9  
> **Objectif :** Comprendre, expliquer et implémenter la communication asynchrone entre microservices

---

## Table des matières

1. [Introduction — Le problème et la solution](#1--introduction)
2. [Vocabulaire — Les 5 concepts clés](#2--vocabulaire)
3. [MassTransit — Le rôle du framework](#3--masstransit)
4. [Implémentation — La recette en 4 étapes](#4--implémentation)
5. [Notre architecture — Les 6 scénarios réels](#5--nos-6-scénarios)
6. [Défendre devant le jury](#6--défendre-devant-le-jury)

---

## 1. 📌 Introduction

### Le problème : la communication synchrone (HTTP)

Quand `OrderAPI` appelle `InventoryAPI` directement par HTTP :

| Inconvénient | Explication |
|---|---|
| **Blocage** | `OrderAPI` attend la réponse. Le client voit un chargement long. |
| **Fragilité** | Si `InventoryAPI` est en panne → la commande **échoue et est perdue**. |
| **Couplage** | Pour ajouter `DocAPI`, il faut modifier le code de `OrderAPI`. |

> 🍽️ **Analogie :** Un serveur de restaurant qui reste planté devant le cuisinier au lieu de retourner en salle.

### La solution : la communication asynchrone (RabbitMQ)

| Avantage | Explication |
|---|---|
| **Rapidité** | `OrderAPI` dépose le message et répond au client en 5 ms. |
| **Résilience** | Si `InventoryAPI` est éteint, le message attend dans la file. Zéro perte. |
| **Découplage** | On ajoute 10 récepteurs sans toucher au code de l'émetteur. |

> 🍽️ **Analogie :** Le serveur dépose le ticket sur le **passe-plat** et retourne en salle. Le cuisinier prend le ticket quand il est prêt.

---

## 2. 📖 Vocabulaire

Les 5 mots à connaître absolument :

```
  📢 Publisher ──► 🔀 Exchange ──► 📥 Queue ──► ⚙️ Consumer
  (Émetteur)       (Aiguilleur)    (Boîte)      (Récepteur)
```

| Concept | Rôle | Exemple dans notre projet |
|---|---|---|
| **Message / Event** | L'objet d'information transmis (un `record` C#) | `OrderCreatedEvent` |
| **Publisher** | Le service qui **envoie** le message | `OrderAPI` publie une commande |
| **Exchange** | Le bureau de poste qui **distribue** le message dans les bonnes files | Créé automatiquement par MassTransit |
| **Queue** | La boîte aux lettres qui **stocke** les messages en attendant le récepteur | Une file par consommateur |
| **Consumer** | Le service qui **lit** le message et exécute la logique métier | `InventoryAPI` réserve le stock |

---

## 3. 🔧 MassTransit

### Pourquoi ne pas coder RabbitMQ directement ?

Utiliser `RabbitMQ.Client` en C# brut demande des centaines de lignes : connexions TCP, canaux, sérialisation JSON, retries, accusés de réception (ACK/NACK)...

**MassTransit** simplifie tout :

| Ce que MassTransit fait pour vous | Sans MassTransit |
|---|---|
| Crée les Exchanges et Queues automatiquement | Il faut les déclarer manuellement |
| Gère les reconnexions si le réseau coupe | Il faut coder un retry loop |
| Sérialise/désérialise le JSON | Il faut le faire à la main |
| Publier = 1 ligne de code | ~50 lignes de code |

---

## 4. 🛠️ Implémentation

### Les fichiers concernés dans notre projet

```
CynapSoftCRMBackend/
├── CynapCRM.MessageBus/          ← Projet partagé (Events + Configuration)
│   ├── Events/
│   │   ├── OrderCreatedEvent.cs        ← Étape 1
│   │   ├── OrderStatusChangedEvent.cs
│   │   ├── StockDistributedEvent.cs
│   │   ├── VisiteCompletedEvent.cs
│   │   ├── ProductPriceChangedEvent.cs
│   │   └── UserCreatedEvent.cs
│   └── Extensions/
│       └── MassTransitExtensions.cs    ← Configuration MassTransit
│
├── CynapCRM.Services.OrderAPI/    ← Émetteur + Récepteur
│   ├── Service/OrderService.cs         ← Étape 2 (Publish)
│   ├── Consumers/
│   │   ├── VisiteCompletedConsumer.cs  ← Étape 3
│   │   └── ProductPriceChangedConsumer.cs
│   └── Program.cs                      ← Étape 4
│
├── CynapCRM.Services.InventoryAPI/ ← Récepteur principal
│   ├── Consumers/
│   │   ├── OrderCreatedConsumer.cs
│   │   ├── OrderStatusChangedConsumer.cs
│   │   └── UserCreatedConsumer.cs
│   └── Program.cs
│ (... et de même pour FieldAPI, DocAPI, etc.)
```

---

### Étape 1 — Créer le message (Event)

**Où :** `CynapCRM.MessageBus/Events/`  
**Pourquoi dans un projet partagé ?** L'émetteur et le récepteur doivent connaître la même structure.

```csharp
// Fichier : CynapCRM.MessageBus/Events/OrderCreatedEvent.cs

namespace CynapCRM.MessageBus.Events;

public record OrderCreatedEvent
{
    public int OrderId { get; init; }
    public int ClientId { get; init; }
    public DateTime OrderDate { get; init; }
    public List<OrderLineItem> Lines { get; init; } = new();
}
```

> 💡 On utilise `public record` car c'est **immuable** (personne ne peut modifier le message en transit).

---

### Étape 2 — Publier le message (Publisher)

**Où :** Dans le Service du microservice émetteur (ex: `OrderService.cs`).

```csharp
// Fichier : CynapCRM.Services.OrderAPI/Service/OrderService.cs

public class OrderService
{
    private readonly IPublishEndpoint _publishEndpoint; // ← Injection MassTransit

    public OrderService(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task CreerCommande(CommandeDto dto)
    {
        // 1. Sauvegarder la commande en base de données
        var commande = await _db.Commandes.AddAsync(...);
        await _db.SaveChangesAsync();

        // 2. Publier l'événement sur RabbitMQ (1 seule ligne !)
        await _publishEndpoint.Publish(new OrderCreatedEvent
        {
            OrderId = commande.Id,
            ClientId = dto.ClientId,
            OrderDate = DateTime.UtcNow
        });
    }
}
```

> 💡 `_publishEndpoint.Publish(...)` dépose le message dans RabbitMQ et retourne **immédiatement**. Le service ne bloque pas.

---

### Étape 3 — Créer le consommateur (Consumer)

**Où :** Dossier `Consumers/` du microservice récepteur.

```csharp
// Fichier : CynapCRM.Services.InventoryAPI/Consumers/OrderCreatedConsumer.cs

using CynapCRM.MessageBus.Events;
using MassTransit;

namespace CynapCRM.Services.InventoryAPI.Consumers;

public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly AppDbContext _db;
    private readonly ILogger<OrderCreatedConsumer> _logger;

    public OrderCreatedConsumer(AppDbContext db, ILogger<OrderCreatedConsumer> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var message = context.Message; // ← On récupère l'événement ici !

        _logger.LogInformation("📥 Commande #{Id} reçue", message.OrderId);

        // Logique métier : réserver le stock
        foreach (var line in message.Lines)
        {
            var stock = await _db.StocksDelegues
                .FirstOrDefaultAsync(s => s.Id_Produit == line.ProductId);
            if (stock != null)
            {
                stock.QteReservee += line.Quantity;
            }
        }
        await _db.SaveChangesAsync();
    }
}
```

> 💡 La classe **implémente `IConsumer<T>`** où `T` est le type de message à écouter. MassTransit appelle automatiquement la méthode `Consume()` dès qu'un message arrive.

---

### Étape 4 — Activer le consommateur dans Program.cs

**Où :** `Program.cs` du microservice récepteur.

```csharp
// Fichier : CynapCRM.Services.InventoryAPI/Program.cs

using CynapCRM.MessageBus.Extensions;
using CynapCRM.Services.InventoryAPI.Consumers;

// ... (configuration existante) ...

builder.Services.AddCynapMessageBus(builder.Configuration, x =>
{
    x.AddConsumer<OrderCreatedConsumer>();        // ← Active l'écoute !
    x.AddConsumer<OrderStatusChangedConsumer>();  // ← Peut en avoir plusieurs
});
```

> 💡 N'oubliez pas d'ajouter le bloc `"RabbitMQ"` dans le `appsettings.json` du service pour lui donner l'adresse de connexion au serveur.

---

## 5. 🌐 Nos 6 scénarios

### Vue d'ensemble

```
OrderAPI ──── OrderCreatedEvent ──────► InventoryAPI (réserve le stock)
                                   └──► DocAPI (génère le Bon de Commande)

OrderAPI ──── OrderStatusChangedEvent ─► DocAPI (génère BL + Facture)

InventoryAPI ─ StockDistributedEvent ──► FieldAPI (trace les échantillons)

FieldAPI ──── VisiteCompletedEvent ────► OrderAPI (prépare prise de commande)

ProductAPI ── ProductPriceChangedEvent ► OrderAPI (met à jour les tarifs)

AuthAPI ───── UserCreatedEvent ────────► FieldAPI (crée planning du délégué)
                                   └──► InventoryAPI (ouvre espace stock)
```

### Détail de chaque scénario

| # | Scénario | Qui parle ? | Événement | Qui écoute ? | Ce qui se passe |
|---|---|---|---|---|---|
| 1 | **Prise de commande** | `OrderAPI` | `OrderCreatedEvent` | `InventoryAPI` + `DocAPI` | Stock réservé + BC généré |
| 2 | **Livraison** | `OrderAPI` | `OrderStatusChangedEvent` | `DocAPI` | BL + Facture générés |
| 3 | **Échantillons** | `InventoryAPI` | `StockDistributedEvent` | `FieldAPI` | Distribution tracée |
| 4 | **Visite positive** | `FieldAPI` | `VisiteCompletedEvent` | `OrderAPI` | Alerte commerciale |
| 5 | **Changement de prix** | `ProductAPI` | `ProductPriceChangedEvent` | `OrderAPI` | Tarifs actualisés |
| 6 | **Nouvel employé** | `AuthAPI` | `UserCreatedEvent` | `FieldAPI` + `InventoryAPI` | Profil + stock créés |

### Explication simple de chaque scénario

**1. Le pharmacien commande 10 boîtes d'Aspirine →** le magasinier met 10 boîtes de côté, la secrétaire imprime le bon de commande.

**2. Le livreur dépose le carton chez le pharmacien →** le téléphone du client affiche "livré", la comptabilité génère la facture.

**3. Le délégué offre 2 boîtes d'échantillons au Dr. Dupont →** le suivi note automatiquement que le docteur a reçu ses boîtes.

**4. Le délégué sort du cabinet et note "visite positive" →** les commerciaux sont prévenus : ce médecin va sûrement commander bientôt.

**5. Le directeur baisse le prix de la Vitamine C de 500 DA à 400 DA →** la caisse change son tarif instantanément.

**6. Les RH embauchent Karim comme nouveau délégué →** le système lui prépare son calendrier de visites et son étagère au dépôt.

---

## 6. 🎓 Défendre devant le jury

### Question probable :
> *"Pourquoi RabbitMQ au lieu d'appels HTTP entre vos microservices ?"*

### Réponse structurée :

**1. Tolérance aux pannes :**  
*"Si le service d'inventaire tombe, les messages de commande sont conservés par RabbitMQ. Aucune donnée n'est perdue. Dès le redémarrage, tout est traité."*

**2. Performance :**  
*"L'émetteur n'attend pas le traitement. Il dépose le message et répond au client en quelques millisecondes."*

**3. Extensibilité :**  
*"Pour ajouter un nouveau module (ex: notifications SMS), on ajoute un consommateur sans modifier le code existant."*

### Phrase de conclusion pour la soutenance :
> *"Notre architecture événementielle avec RabbitMQ et MassTransit garantit le découplage, la résilience et la scalabilité de notre plateforme CRM pharmaceutique. C'est la même approche utilisée par les grandes plateformes industrielles."*
