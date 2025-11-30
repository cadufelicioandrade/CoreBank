CREATE TABLE [dbo].[AccountBalances] (
    [AccountId] UNIQUEIDENTIFIER NOT NULL,
    [CurrentBalance] DECIMAL(18,2) NOT NULL CONSTRAINT DF_AccountBalances_CurrentBalance DEFAULT (0),
    [UpdatedAt] DATETIME2(3) NOT NULL,
    [RowVersion] ROWVERSION NOT NULL,
    CONSTRAINT PK_AccountBalances PRIMARY KEY CLUSTERED ([AccountId] ASC)
);
CREATE TABLE [dbo].[Transactions] (
    [Id] UNIQUEIDENTIFIER NOT NULL,
    [AccountId] UNIQUEIDENTIFIER NOT NULL,
    [Type] TINYINT NOT NULL,
    [Operation] TINYINT NOT NULL,
    [Amount] DECIMAL(18,2) NOT NULL,
    [BalanceAfter] DECIMAL(18,2) NOT NULL,
    [Description] NVARCHAR(200) NULL,
    [CorrelationId] UNIQUEIDENTIFIER NULL,
    [CreatedAt] DATETIME2(3) NOT NULL,
    CONSTRAINT PK_Transactions PRIMARY KEY CLUSTERED ([Id] ASC)
);

-- Índice para extrato (por conta e data)
CREATE NONCLUSTERED INDEX IX_Transactions_AccountId_CreatedAt
    ON [dbo].[Transactions] ([AccountId] ASC, [CreatedAt] DESC);

-- Idempotência: uma transação por (AccountId, CorrelationId)
CREATE UNIQUE NONCLUSTERED INDEX IX_Transactions_AccountId_CorrelationId
    ON [dbo].[Transactions] ([AccountId] ASC, [CorrelationId] ASC)
    WHERE [CorrelationId] IS NOT NULL;
