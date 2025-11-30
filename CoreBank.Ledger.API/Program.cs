using CoreBank.Ledger.API.Entity;
using CoreBank.Ledger.API.Enums;
using CoreBank.Ledger.API.Infrastructure;
using CoreBank.Ledger.API.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// ========================================
// SQL Server (EF Core)
// ========================================
builder.Services.AddDbContext<LedgerDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("LedgerConnection")));

// ========================================
// HealthChecks (usando DbContextCheck)
// ========================================
builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<LedgerDbContext>(
        name: "sqlserver",
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "db", "sql", "sqlserver" }
    );

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// endpoint de health
app.MapHealthChecks("/health");

// ============ ENDPOINT: Criar Transação ============
app.MapPost("/transactions", async (
    [FromBody] CreateTransactionRequest request,
    LedgerDbContext db,
    HttpContext http) =>
{
    if (request.Amount <= 0)
        return Results.BadRequest("Amount must be greater than zero.");

    if (!Enum.TryParse<TransactionType>(ToPascal(request.Type), true, out var type))
        return Results.BadRequest("Invalid transaction type.");

    if (!Enum.TryParse<OperationType>(ToPascal(request.Operation), true, out var operation))
        return Results.BadRequest("Invalid operation type.");

    var correlationId = request.CorrelationId ?? Guid.NewGuid();

    // Idempotência: se já existe transação com esse CorrelationId para a conta, retorna ela
    var existing = await db.Transactions
        .FirstOrDefaultAsync(t => t.AccountId == request.AccountId && t.CorrelationId == correlationId);

    if (existing != null)
    {
        var existingResponse = ToResponse(existing);
        return Results.Ok(existingResponse);
    }

    // Busca/Cria projeção de saldo
    var accountBalance = await db.AccountBalances
        .FirstOrDefaultAsync(x => x.AccountId == request.AccountId);

    if (accountBalance == null)
    {
        accountBalance = new AccountBalance(request.AccountId);
        db.AccountBalances.Add(accountBalance);
    }

    // Aplica transação no saldo
    var isCredit = type == TransactionType.Credit;
    var newBalance = accountBalance.ApplyTransaction(request.Amount, isCredit);

    // Cria transação
    var transaction = new Transaction(
        request.AccountId,
        type,
        operation,
        request.Amount,
        newBalance,
        request.Description,
        correlationId);

    db.Transactions.Add(transaction);

    try
    {
        await db.SaveChangesAsync();
    }
    catch (DbUpdateConcurrencyException)
    {
        // concorrência otimista falhou – poderia implementar retry aqui
        return Results.StatusCode(StatusCodes.Status409Conflict);
    }

    var response = ToResponse(transaction);
    return Results.Created($"/transactions/{transaction.Id}", response);
});

// ============ ENDPOINT: Saldo ============
app.MapGet("/accounts/{accountId:guid}/balance", async (Guid accountId, LedgerDbContext db) =>
{
    var balance = await db.AccountBalances
        .AsNoTracking()
        .FirstOrDefaultAsync(x => x.AccountId == accountId);

    if (balance is null)
        return Results.Ok(new AccountBalanceResponse(accountId, 0m, DateTime.UtcNow));

    var response = new AccountBalanceResponse(balance.AccountId, balance.CurrentBalance, balance.UpdatedAt);
    return Results.Ok(response);
});

// ============ ENDPOINT: Extrato ============
app.MapGet("/accounts/{accountId:guid}/statement", async (
    Guid accountId,
    [FromQuery] DateTime? from,
    [FromQuery] DateTime? to,
    [FromQuery] int page,
    [FromQuery] int pageSize,
    LedgerDbContext db) =>
{
    page = page <= 0 ? 1 : page;
    pageSize = pageSize <= 0 ? 50 : pageSize;

    var query = db.Transactions
        .AsNoTracking()
        .Where(t => t.AccountId == accountId);

    if (from.HasValue)
        query = query.Where(t => t.CreatedAt >= from.Value.ToUniversalTime());

    if (to.HasValue)
        query = query.Where(t => t.CreatedAt <= to.Value.ToUniversalTime());

    var totalItems = await query.CountAsync();

    var items = await query
        .OrderByDescending(t => t.CreatedAt)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    var balance = await db.AccountBalances
        .AsNoTracking()
        .FirstOrDefaultAsync(x => x.AccountId == accountId);

    var resp = new StatementResponse(
        AccountId: accountId,
        CurrentBalance: balance?.CurrentBalance ?? 0m,
        UpdatedAt: balance?.UpdatedAt ?? DateTime.UtcNow,
        Page: page,
        PageSize: pageSize,
        TotalItems: totalItems,
        Items: items.Select(t => new StatementItemResponse(
            t.Id,
            t.Type.ToString(),
            t.Operation.ToString(),
            t.Amount,
            t.BalanceAfter,
            t.Description,
            t.CreatedAt
        ))
    );

    return Results.Ok(resp);
});

app.Run();

// Helpers
static string ToPascal(string value)
{
    if (string.IsNullOrWhiteSpace(value)) return string.Empty;
    value = value.Trim().ToLowerInvariant();
    return value switch
    {
        "credit" => "Credit",
        "debit" => "Debit",
        "deposit" => "Deposit",
        "withdraw" => "Withdraw",
        "pix_in" => "PixIn",
        "pix_out" => "PixOut",
        _ => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(value)
    };
}

static TransactionResponse ToResponse(Transaction t) =>
    new(
        t.Id,
        t.AccountId,
        t.Type.ToString(),
        t.Operation.ToString(),
        t.Amount,
        t.BalanceAfter,
        t.Description,
        t.CorrelationId,
        t.CreatedAt);
