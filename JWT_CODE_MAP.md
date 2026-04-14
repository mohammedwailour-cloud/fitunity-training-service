# JWT dans ton projet `Training-Service`

Ce document explique **où le JWT est utilisé dans ton code actuel**, avec les **fichiers, classes et points précis** à relire.

## 1. Où le JWT est valide

### Fichier exact
- `D:\Training-Service\Training.Api\Program.cs`

### Zone de configuration importante
Dans `Program.cs`, le JWT est configuré ici :
- lignes `74` à `91` pour les options + la validation
- lignes `151` à `152` pour l'exécution du middleware d'authentification/autorisation

Extrait exact :

```csharp
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContext, JwtUserContext>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();
```

Et dans le pipeline HTTP :

```csharp
app.UseAuthentication();
app.UseAuthorization();
```

### D'où viennent `Issuer`, `Audience`, `Key`
- `D:\Training-Service\Training.Api\appsettings.json`, lignes `9` à `15`
- `D:\Training-Service\Training.Api\Security\JwtOptions.cs`, lignes `3` à `12`

Extrait `appsettings.json` :

```json
"Jwt": {
  "Issuer": "Training.Api",
  "Audience": "Training.Client",
  "Key": "TrainingApiDevKey-PleaseReplaceWithAStrongSecret-2026",
  "DefaultUserId": "11111111-1111-1111-1111-111111111111",
  "DefaultRole": "User",
  "EnableDevelopmentFallback": true
}
```

### Comment ASP.NET récupère le token dans ton projet
Dans ton code, rien ne parse manuellement le header dans les controllers.
Le token est récupéré par le middleware JWT branché via :

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(...)
```

Puis exécuté par :

```csharp
app.UseAuthentication();
```

Concrètement, pour les endpoints protégés, ASP.NET lit le header HTTP `Authorization: Bearer <token>`, valide le token avec les paramètres définis dans `Program.cs`, puis remplit `HttpContext.User`.

## 2. Où le JWT est lu (claims)

### Classe exacte
- `D:\Training-Service\Training.Api\Security\JwtUserContext.cs`
- classe : `JwtUserContext`

### Interface utilisée par l'application
- `D:\Training-Service\Training.Application\Common\Interfaces\IUserContext.cs`
- interface : `IUserContext`

Extrait exact de l'interface :

```csharp
public interface IUserContext
{
    Guid UserId { get; }
    string? Role { get; }
    bool IsAuthenticated { get; }
}
```

### Comment `UserId` est récupéré
Dans `JwtUserContext.cs`, lignes `19` à `37` :

```csharp
public Guid UserId
{
    get
    {
        var claimValue = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        if (Guid.TryParse(claimValue, out var userId))
        {
            return userId;
        }

        if (_jwtOptions.EnableDevelopmentFallback && Guid.TryParse(_jwtOptions.DefaultUserId, out var defaultUserId))
        {
            return defaultUserId;
        }

        throw new UserContextUnavailableException();
    }
}
```

Ici, ton code lit **exactement** le claim :
- `ClaimTypes.NameIdentifier`

### Comment `Role` est récupéré
Dans `JwtUserContext.cs`, lignes `39` à `57` :

```csharp
public string? Role
{
    get
    {
        var role = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role);

        if (!string.IsNullOrWhiteSpace(role))
        {
            return role;
        }

        if (_jwtOptions.EnableDevelopmentFallback)
        {
            return _jwtOptions.DefaultRole;
        }

        throw new UserContextUnavailableException();
    }
}
```

Ici, ton code lit **exactement** le claim :
- `ClaimTypes.Role`

### Fallback local
Toujours dans `JwtUserContext.cs` :
- si le claim `NameIdentifier` est absent/invalide et que `EnableDevelopmentFallback = true`, alors `UserId` retourne `DefaultUserId`
- si le claim `Role` est absent et que `EnableDevelopmentFallback = true`, alors `Role` retourne `DefaultRole`
- sinon ton code lève `UserContextUnavailableException`

## 3. Où le JWT est généré dans ton projet

### Réponse courte
Dans le **code applicatif** (`Training.Api`, `Training.Application`, `Training.Infrastructure`, `Training.Domain`) :
- je n'ai trouvé **aucune classe qui génère un JWT**

### Vérification dans le code
La recherche dans le repo retourne :
- `D:\Training-Service\Training.Api\Program.cs:78` -> configuration `.AddJwtBearer(...)`
- `D:\Training-Service\Training.Tests\Integration\SessionPipelineTests.cs:229` -> création d'un `JwtSecurityToken`
- `D:\Training-Service\Training.Tests\Integration\SessionPipelineTests.cs:237` -> `WriteToken(token)`

### Conclusion
Le token de dev que tu utilises dans Scalar n'est **pas généré par une classe de production de ton projet**.
Il est **externe** au runtime applicatif.

Autrement dit :
- ton projet **valide** des JWT
- ton projet **lit** les claims du JWT
- ton projet **ne fournit pas** d'endpoint ni de service interne pour générer le JWT côté production

## 4. Dans les tests

### Est-ce que les tests génèrent un JWT ?
Oui.

### Fichier exact
- `D:\Training-Service\Training.Tests\Integration\SessionPipelineTests.cs`

### Où exactement
- constantes JWT : lignes `17` à `19`
- ajout du token sur le client HTTP : lignes `99`, `125`, `150`
- méthode de génération : lignes `219` à `238`

### Code exact utilisé pour générer le JWT

```csharp
private static string CreateJwt(Guid userId, string role = "User")
{
    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
        new Claim(ClaimTypes.Role, role)
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    var token = new JwtSecurityToken(
        issuer: JwtIssuer,
        audience: JwtAudience,
        claims: claims,
        notBefore: DateTime.UtcNow.AddMinutes(-1),
        expires: DateTime.UtcNow.AddHours(1),
        signingCredentials: credentials);

    return new JwtSecurityTokenHandler().WriteToken(token);
}
```

### Comment les tests envoient le JWT
Exemple exact :

```csharp
client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateJwt(userId));
```

Donc dans les tests :
- le JWT est bien généré localement dans `SessionPipelineTests.cs`
- puis envoyé dans le header `Authorization`
- ensuite validé par le vrai pipeline ASP.NET de `Training.Api`

## 5. Flow complet dans ton projet

### Flow HTTP réel pour un endpoint protégé
Exemple concret avec `POST /api/reservations` :

- `D:\Training-Service\Training.Api\Controllers\ReservationsController.cs`, lignes `7` à `9`

```csharp
[ApiController]
[Authorize(Roles = "Admin,User,Coach")]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
```

### Chaîne complète

```text
HTTP Request
  -> app.UseAuthentication() dans Training.Api/Program.cs
  -> handler JWT configuré par AddJwtBearer(...) dans Training.Api/Program.cs
  -> HttpContext.User rempli par ASP.NET
  -> JwtUserContext lit HttpContext.User
  -> IUserContext injecté dans le UseCase
  -> UseCase consomme _userContext.UserId
```

### Point d'entrée controller
- `D:\Training-Service\Training.Api\Controllers\ReservationsController.cs`, lignes `32` à `37`

```csharp
[HttpPost]
public async Task<IActionResult> Reserve(CreateReservationRequest request)
{
    var result = await _reserveSessionUseCase.ExecuteAsync(request);

    return Ok(result);
}
```

Le controller ne lit pas les claims lui-même.
Il délègue directement au use case.

### Lecture côté UseCase
- `D:\Training-Service\Training.Application\Reservations\UseCases\ReserveSessionUseCase.cs`, lignes `29` à `31`

```csharp
public async Task<ReservationResponse> ExecuteAsync(CreateReservationRequest request)
{
    var userId = _userContext.UserId;
```

Donc, pour `POST /api/reservations`, le `userId` utilisé métier vient de :
- `HttpContext.User`
- lu par `JwtUserContext`
- exposé via `IUserContext`
- consommé dans `ReserveSessionUseCase`

### Même logique pour `GET /api/reservations/me`
- `D:\Training-Service\Training.Api\Controllers\ReservationsController.cs`, lignes `54` à `58`
- `D:\Training-Service\Training.Application\Reservations\UseCases\GetReservationsByUserUseCase.cs`, lignes `21` à `24`

Code exact dans le use case :

```csharp
public async Task<IEnumerable<Reservation>> ExecuteForCurrentUserAsync()
{
    return await _reservationRepository.GetByUserIdAsync(_userContext.UserId);
}
```

## 6. Où chercher rapidement dans ton code

Si tu veux relire le JWT dans ton projet sans te perdre, pars dans cet ordre :

1. `D:\Training-Service\Training.Api\Program.cs`
   - config JWT
   - exécution `UseAuthentication()` / `UseAuthorization()`

2. `D:\Training-Service\Training.Api\Security\JwtUserContext.cs`
   - lecture de `NameIdentifier`
   - lecture de `Role`
   - fallback de développement

3. `D:\Training-Service\Training.Application\Common\Interfaces\IUserContext.cs`
   - contrat consommé par l'application

4. `D:\Training-Service\Training.Application\Reservations\UseCases\ReserveSessionUseCase.cs`
   - usage métier concret de `_userContext.UserId`

5. `D:\Training-Service\Training.Application\Reservations\UseCases\GetReservationsByUserUseCase.cs`
   - usage métier concret pour `/api/reservations/me`

6. `D:\Training-Service\Training.Tests\Integration\SessionPipelineTests.cs`
   - génération de JWT en test
   - injection dans `Authorization: Bearer ...`

## 7. Réponse directe à ta question "où chercher pour JWT ?"

Dans ton code actuel :
- **validation JWT** -> `Training.Api/Program.cs`
- **lecture des claims** -> `Training.Api/Security/JwtUserContext.cs`
- **abstraction utilisée par l'application** -> `Training.Application/Common/Interfaces/IUserContext.cs`
- **usage métier du UserId** -> `ReserveSessionUseCase.cs` et `GetReservationsByUserUseCase.cs`
- **génération de JWT en test** -> `Training.Tests/Integration/SessionPipelineTests.cs`
- **génération de JWT en production** -> **absente** dans ton projet actuel
