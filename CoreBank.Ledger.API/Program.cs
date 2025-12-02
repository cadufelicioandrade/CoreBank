using CoreBank.Ledger.API.Application.DTO;
using CoreBank.Ledger.API.Application.Services;
using CoreBank.Ledger.API.Infrastructure;
using CoreBank.Ledger.API.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLedgerInfrastructure(builder.Configuration);

builder.Services.AddScoped<ILedgerService, LedgerService>();

builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<LedgerDbContext>(
        name: "sqlserver",
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "db", "sql", "ledger" });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("/health");

// ===========================
// Endpoints LIMPOS
// ===========================
app.MapPost("/api/ledger/transactions", async (
    [FromBody] CreateTransactionRequest request,
    ILedgerService service,
    CancellationToken ct) =>
{
    // Chama o service que retorna Result<TransactionResponse>
    var result = await service.CreateAsync(request, ct);

    // Se deu erro de regra, validação, etc
    if (result.IsFailure)
    {
        // 400 com a mensagem de erro do Result
        return Results.BadRequest(new { error = result.Error });
    }

    // Aqui está o TransactionResponse de verdade
    var response = result.Value!;

    // Usa o Id do TransactionResponse
    return Results.Created($"/api/ledger/transactions/{response.Id}", response);
});


app.MapGet("/api/ledger/transactions/{id:guid}", async (
    Guid id,
    ILedgerService service,
    CancellationToken ct) =>
{
    var result = await service.GetByIdAsync(id, ct);
    return result is null ? Results.NotFound() : Results.Ok(result);
});

app.MapGet("/api/ledger/accounts/{accountNumber}/transactions", async (
    string accountNumber,
    ILedgerService service,
    CancellationToken ct) =>
{
    var result = await service.GetByAccountAsync(accountNumber, ct);
    return Results.Ok(result);
});

app.Run();
