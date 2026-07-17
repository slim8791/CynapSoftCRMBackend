# 🐳 Fiche de Maîtrise — Docker & Docker Compose

> **Projet :** CynapPharm CRM — Déploiement Microservices  
> **Objectif :** Comprendre pourquoi et comment dockeriser votre backend

---

## Table des matières

1. [Pourquoi Docker ? Le problème concret](#1--pourquoi-docker)
2. [Les 4 concepts en pratique](#2--les-4-concepts-en-pratique)
3. [Comment lire un Dockerfile (le vôtre)](#3--comment-lire-un-dockerfile)
4. [Docker Compose : l'orchestre complet](#4--docker-compose)
5. [Mise en pratique sur votre projet](#5--mise-en-pratique)
6. [Défendre devant le jury](#6--défendre-devant-le-jury)

---

## 1. 📌 Pourquoi Docker ?

### Le problème sans Docker

Vous avez **7 projets .NET** à lancer pour que votre application fonctionne :
- AuthAPI, OrderAPI, InventoryAPI, FieldAPI, ProductAPI, DocAPI, Gateway

**Sans Docker, pour faire tourner votre projet chez un collègue ou en production :**

| Problème | Conséquence |
|---|---|
| Il faut installer .NET 9 SDK sur chaque machine | ❌ "Ça marche pas, j'ai .NET 8" |
| Il faut installer SQL Server | ❌ "J'ai pas SQL Server" |
| Il faut configurer RabbitMQ | ❌ "C'est quoi CloudAMQP ?" |
| Il faut ouvrir 7 terminaux et lancer 7 `dotnet run` | ❌ "J'ai oublié de lancer InventoryAPI" |

> 🍳 **Analogie :** C'est comme donner une recette de cuisine à quelqu'un sans lui donner le four, les casseroles, ni les ingrédients. Il ne peut rien faire !

### La solution avec Docker

Docker **emballe votre application avec tout ce qu'il faut** dans une boîte hermétique (un conteneur) :

| Avantage | Résultat |
|---|---|
| Tout est emballé (code + .NET + config) | ✅ Fonctionne sur n'importe quelle machine |
| Une seule commande lance tout | ✅ `docker compose up` → 7 services démarrent |
| Chaque service est isolé | ✅ Pas de conflit entre versions ou ports |
| Reproductible à 100% | ✅ "Ça marche chez moi" = "Ça marche partout" |

> 🍳 **Analogie :** Docker, c'est comme un **kit repas livré à domicile** : la boîte contient les ingrédients pré-dosés, la recette et même le petit four jetable. Le client n'a besoin de rien d'autre !

---

## 2. 📖 Les 4 concepts en pratique

### Vue d'ensemble rapide

```
  Dockerfile ──► docker build ──► Image ──► docker run ──► Conteneur
  (La recette)    (Cuisiner)      (Le plat    (Servir)      (L'assiette
                                   surgelé)                  servie !)
```

| Concept | C'est quoi ? | Exemple concret |
|---|---|---|
| **Dockerfile** | La **recette** de cuisine pour construire votre application | "Prends .NET 9, copie mon code, compile-le" |
| **Image** | Le **plat surgelé** prêt à l'emploi (résultat de la recette) | `cynapharm-orderapi:latest` (fichier ~80 Mo) |
| **Conteneur** | Le plat **servi dans l'assiette**, en train de fonctionner | OrderAPI qui tourne sur le port 7004 |
| **Volume** | Un **tiroir partagé** entre la machine et le conteneur | Les données SQL Server qui survivent au redémarrage |

### Et Docker Compose ?

| Sans Compose | Avec Compose |
|---|---|
| `docker run orderapi` | **`docker compose up`** |
| `docker run inventoryapi` | *(une seule commande fait tout)* |
| `docker run authapi` | |
| `docker run fieldapi` | |
| `docker run gateway` | |
| `docker run rabbitmq` | |
| `docker run sqlserver` | |

> **Docker Compose** est le **chef d'orchestre** qui lit un fichier `docker-compose.yml` et lance tous vos conteneurs **ensemble**, avec le bon réseau, les bons ports et les bonnes connexions.

---

## 3. 🔍 Comment lire un Dockerfile (le vôtre)

Votre projet a déjà des Dockerfiles (générés par Visual Studio). Voici comment lire celui d'OrderAPI **ligne par ligne** :

```dockerfile
# ─── ÉTAPE 1 : L'image de base (le système d'exploitation minimal) ───
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
#    ↑ "Prends une mini-machine Linux avec .NET 9 installé"
WORKDIR /app
#    ↑ "Crée un dossier /app et travaille dedans"
EXPOSE 8080
#    ↑ "Ce conteneur écoute sur le port 8080"

# ─── ÉTAPE 2 : Compiler le code source ───
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
#    ↑ "Prends une machine avec le SDK .NET 9 (pour compiler)"
WORKDIR /src
COPY ["CynapCRM.Services.OrderAPI/CynapCRM.Services.OrderAPI.csproj", "CynapCRM.Services.OrderAPI/"]
#    ↑ "Copie le fichier .csproj pour restaurer les packages NuGet"
RUN dotnet restore "./CynapCRM.Services.OrderAPI/CynapCRM.Services.OrderAPI.csproj"
#    ↑ "Télécharge les dépendances (MassTransit, EF Core, etc.)"
COPY . .
#    ↑ "Copie tout le code source"
RUN dotnet build "./CynapCRM.Services.OrderAPI.csproj" -c Release -o /app/build
#    ↑ "Compile le projet en mode Release"

# ─── ÉTAPE 3 : Publier (version optimisée pour la production) ───
FROM build AS publish
RUN dotnet publish "./CynapCRM.Services.OrderAPI.csproj" -c Release -o /app/publish
#    ↑ "Crée la version finale allégée (sans les fichiers de développement)"

# ─── ÉTAPE 4 : L'image finale (légère, ~80 Mo au lieu de ~800 Mo) ───
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
#    ↑ "Copie uniquement le résultat publié dans l'image finale"
ENTRYPOINT ["dotnet", "CynapCRM.Services.OrderAPI.dll"]
#    ↑ "Au démarrage du conteneur, lance cette commande"
```

> 💡 **Pourquoi 4 étapes (multi-stage build) ?**  
> L'étape de compilation utilise le SDK complet (~800 Mo). L'image finale ne contient que le runtime (~80 Mo). Résultat : une image **10 fois plus légère** pour la production !

---

## 4. 🎼 Docker Compose : l'orchestre complet

### Le fichier `docker-compose.yml` — Ce qu'il contient

Ce fichier décrit **tous vos services** et comment ils sont connectés :

```yaml
services:
  # ── Vos 7 microservices .NET ──
  authapi:         # Conteneur 1
  orderapi:        # Conteneur 2
  inventoryapi:    # Conteneur 3
  fieldapi:        # Conteneur 4
  productapi:      # Conteneur 5
  docapi:          # Conteneur 6
  gateway:         # Conteneur 7 (point d'entrée unique)

  # ── Les services d'infrastructure ──
  rabbitmq:        # Conteneur 8 (le bus de messages)
  sqlserver:       # Conteneur 9 (la base de données)
```

### Comment les services se parlent dans Docker ?

**Sans Docker :** chaque service utilise `localhost:7000`, `localhost:7001`, etc.

**Avec Docker :** chaque service a un **nom réseau** (son nom dans le docker-compose). Donc au lieu de `localhost:7004`, OrderAPI devient juste `orderapi:8080`.

```
Avant (localhost)                    Après (Docker)
───────────────                      ──────────────
https://localhost:7000 (AuthAPI)  →  http://authapi:8080
https://localhost:7004 (OrderAPI) →  http://orderapi:8080
Server=ASUS\SQLEXPRESS            →  Server=sqlserver
```

> Docker crée automatiquement un **réseau privé virtuel** entre tous les conteneurs. Ils se voient par leur nom de service !

---

## 5. 🛠️ Mise en pratique sur votre projet CynapPharm

### Les 3 commandes à connaître

| Commande | Ce qu'elle fait |
|---|---|
| `docker compose up --build` | Construit toutes les images et lance tous les conteneurs |
| `docker compose down` | Arrête et supprime tous les conteneurs |
| `docker compose logs orderapi` | Affiche les logs d'un seul service |

### Votre arborescence après dockerisation

```
CynapSoftCRMBackend/
├── docker-compose.yml              ← Le chef d'orchestre (NOUVEAU)
├── docker-compose.override.yml     ← Variables d'environnement (NOUVEAU)
├── CynapCRM.Services.AuthAPI/
│   └── Dockerfile                  ← Existe déjà ✅
├── CynapCRM.Services.OrderAPI/
│   └── Dockerfile                  ← Existe déjà ✅
├── CynapCRM.Services.InventoryAPI/
│   └── Dockerfile                  ← Existe déjà ✅
├── CynapCRM.Services.FieldAPI/
│   └── Dockerfile                  ← Existe déjà ✅
├── CynapCRM.Services.ProductAPI/
│   └── Dockerfile                  ← Existe déjà ✅
├── CynapCRM.Services.DocAPI/
│   └── Dockerfile                  ← Existe déjà ✅
└── CynapCRM.Gateway/
    └── Dockerfile                  ← Existe déjà ✅
```

### Ce qui change dans la configuration

Quand vos services tournent dans Docker, les adresses changent :

| Paramètre | Valeur locale (sans Docker) | Valeur Docker (avec Docker) |
|---|---|---|
| SQL Server | `Server=ASUS\SQLEXPRESS` | `Server=sqlserver,1433` |
| RabbitMQ | `Host=fuji.lmq.cloudamqp.com` | `Host=rabbitmq` (local Docker) |
| Gateway → AuthAPI | `https://localhost:7000` | `http://authapi:8080` |
| Gateway → OrderAPI | `https://localhost:7004` | `http://orderapi:8080` |

> 💡 Ces changements se font via les **variables d'environnement** dans le `docker-compose.yml`, sans modifier vos fichiers `appsettings.json` !

---

## 6. 🎓 Défendre devant le jury

### Question probable :
> *"Pourquoi avez-vous dockerisé votre application ?"*

### Réponse structurée :

**1. Portabilité :**  
*"Un seul `docker compose up` déploie nos 7 microservices + RabbitMQ + SQL Server sur n'importe quelle machine, sans installer quoi que ce soit d'autre que Docker."*

**2. Isolation :**  
*"Chaque microservice tourne dans son propre conteneur isolé. Un crash de OrderAPI ne fait pas tomber les autres services."*

**3. Reproductibilité :**  
*"L'environnement de production est identique à l'environnement de développement. Plus jamais le fameux 'ça marche sur ma machine'."*

**4. Scalabilité :**  
*"Si OrderAPI est surchargé, on peut lancer 3 conteneurs OrderAPI en parallèle avec `docker compose up --scale orderapi=3`."*

### Phrase de conclusion :
> *"Docker nous permet de livrer notre plateforme CRM pharmaceutique comme un produit industriel : emballé, testé et reproductible. C'est le standard de l'industrie pour le déploiement de microservices."*
