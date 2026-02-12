# Security Guidelines (C# 14 / .NET 10)

> **Context7**:
>
> - **Context**: Enterprise application development using .NET 10 and C# 14.
> - **Intent**: Establish mandatory code-level security standards, moving from "defensive coding" to "secure by design".
> - **Method**: Sequential Security Flow (Supply Chain -> Input -> Process -> Storage -> Output).

This document outlines the security standards for all C# development. In .NET 10, we prioritize **compile-time safety** (C# 14 Extensions/Roles) and **zero-trust architecture**.

## 0. Supply Chain & Governance (Pre-Build)

Before code is even written, the environment must be secured.

- **NuGet Auditing**: MANDATORY. .NET 10 enables strict auditing by default.
  ```xml
  <PropertyGroup>
      <NuGetAudit>true</NuGetAudit>
      <NuGetAuditMode>all</NuGetAuditMode> <!-- Audits transitive dependencies -->
      <NuGetAuditLevel>low</NuGetAuditLevel> <!-- Fail on any known vulnerability -->
      <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  </PropertyGroup>
  ```
- **Secret Management**:
  - **Dev**: Use `dotnet user-secrets`. NEVER commit `appsettings.Development.json` with real keys.
  - **Prod**: Use Azure Key Vault or Environment Variables via `IConfiguration`.
  - **Detection**: Pre-commit hooks must scan for high-entropy strings.

## 1. Input Security (The Gate)

Secure the edge. Assume all input is malicious until proven valid.

### 1.1 Strong Typing with C# 14 Extensions

Stop using `string` for domain concepts. Use C# 14 **Extensions** (Roles) to enforce validation at the type system level with zero allocation.

```csharp
// BAD: Passing naked strings
public void ProcessEmail(string email) { ... }

// GOOD: C# 14 Extension Type
public explicit extension EmailAddress for string
{
    public static bool TryParse(string s, out EmailAddress result)
    {
        if (!s.Contains("@"))
        {
            result = default;
            return false;
        }
        result = (EmailAddress)s; // Zero-cost cast
        return true;
    }
}

// Usage enforces validation before the method is even called
public void ProcessEmail(EmailAddress email) { ... }
```

### 1.2 Rate Limiting (DoS Protection)

Reference `Microsoft.AspNetCore.RateLimiting`. Apply globally.

```csharp
// Program.cs (.NET 10)
app.UseRateLimiter(new RateLimiterOptions
{
    GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            });
    }),
    RejectionStatusCode = StatusCodes.Status429TooManyRequests
});
```

## 2. Processing Security (The Logic)

### 2.1 Cryptography (.NET 10)

Legacy algorithms (SHA1, MD5, RSA < 2048) are removed or deprecated.

- **Post-Quantum Readiness**: Use `System.Security.Cryptography` algorithms marked as CNSA 2.0 compliant.
- **Randomness**: NEVER use `System.Random` for security. Use `RandomNumberGenerator.GetInt32()`.

### 2.2 Broken Access Control

Avoid "check-then-act" race conditions. Use **Policy-Based Authorization** on Route Groups.

```csharp
// BAD: Manual check inside handler
app.MapGet("/documents/{id}", (int id, ClaimsPrincipal user) => {
   if (!user.HasClaim("admin")) return Results.Forbid();
   ...
});

// GOOD: Declarative Policy on Group
var adminGroup = app.MapGroup("/admin")
    .RequireAuthorization(policy => policy.RequireRole("Admin"));

adminGroup.MapGet("/documents/{id}", GetDocumentHandler);
```

### 2.3 Open Redirects

Unvalidated redirects are phishing vectors.

- **Rule**: Use `LocalRedirect` whenever possible.
- **Rule**: If redirecting to external URL, validate it against an allowlist.

```csharp
// GOOD: Throws if URL is not local
return Results.LocalRedirect(returnUrl);
```

## 3. Data Security (The State)

### 3.1 EF Core 10 & SQL Injection

- **Rule**: NEVER use `FromSqlRaw`.
- **Rule**: ALWAYS use `FromSqlInterpolated` or LINQ.

```csharp
// BAD: Vulnerable to Injection
ctx.Users.FromSqlRaw($"SELECT * FROM Users WHERE Name = '{name}'");

// GOOD: Parameterized by default
ctx.Users.FromSqlInterpolated($"SELECT * FROM Users WHERE Name = {name}");
```

### 3.2 Sensitive Data Redaction

Use .NET 10 compliance redaction for logs.

```csharp
public class UserProfile
{
    public string Username { get; set; }

    [LogProperties(OmitReferenceName = true)]
    [Redact(TraceLoggingRedactor.Validation)] // Redacts in telemetry
    public string SocialSecurityNumber { get; set; }
}
```

## 4. Output & Infrastructure (The Result)

### 4.1 HTTP Headers (CSP & CORS)

Security headers are the first line of defense in the browser.

- **CSP (Content-Security-Policy)**: Prevent XSS and Injection.
- **CORS**: Never use `AllowAnyOrigin` with credentials.

```csharp
// Program.cs: Middleware Order Matters!
app.Use(async (context, next) =>
{
    // Strict CSP: Default to nothing, only allow self.
    context.Response.Headers.Append("Content-Security-Policy",
        "default-src 'self'; object-src 'none'; frame-ancestors 'none'; upgrade-insecure-requests;");
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    await next();
});

// Secure CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("StrictPolicy", policy =>
    {
        policy.WithOrigins("https://trusted.com") // Specific origins only
              .AllowCredentials()                 // Safe only with specific origins
              .WithMethods("GET", "POST");
    });
});
```

### 4.2 Native AOT Structure

.NET 10 defaults to Native AOT for cloud workloads.

- **Security Benefit**: Removes JIT definitions, reducing code injection surface area.
- **Traceability**: Ensure `OpenTelemetry` is configured to forward security events to SIEM.

### 4.2 AI Safety (LLM Integration)

When using `Microsoft.Extensions.AI`:

- **Prompt Injection**: Never concatenate user input directly into system prompts. Use parameterized templates.
- **Output Handling**: Treat LLM output as "Untrusted Input". Sanitize Markdown/HTML before rendering.

```csharp
// Secure AI Pattern
var prompt = new ChatMessage(ChatRole.User, "Summarize this text");
// Do not append user text blindly.
```

## Reference

- **OWASP Top 10 (2025)**: [owasp.org/www-project-top-ten/](https://owasp.org/www-project-top-ten/)
- **Microsoft Security Best Practices**: [learn.microsoft.com/security/](https://learn.microsoft.com/en-us/security/)
- **ASP.NET Core Security**: [learn.microsoft.com/aspnet/core/security/](https://learn.microsoft.com/en-us/aspnet/core/security/)
- **NuGet Auditing**: [learn.microsoft.com/nuget/concepts/auditing-packages](https://learn.microsoft.com/en-us/nuget/concepts/auditing-packages)
