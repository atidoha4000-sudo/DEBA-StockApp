-- Schema for DEBA.StockApp (MVP)

BEGIN TRANSACTION;

CREATE TABLE IF NOT EXISTS Products (
    ProductId INTEGER PRIMARY KEY AUTOINCREMENT,
    SKU TEXT UNIQUE,
    Name TEXT NOT NULL,
    Description TEXT,
    UnitPrice NUMERIC NOT NULL DEFAULT 0,
    CostPrice NUMERIC NOT NULL DEFAULT 0,
    ReorderLevel INTEGER NOT NULL DEFAULT 0,
    IsActive INTEGER NOT NULL DEFAULT 1,
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),
    UpdatedAt TEXT
);

CREATE TABLE IF NOT EXISTS Sales (
    SaleId INTEGER PRIMARY KEY AUTOINCREMENT,
    Date TEXT NOT NULL DEFAULT (datetime('now')),
    TotalAmount NUMERIC NOT NULL DEFAULT 0,
    Note TEXT
);

CREATE TABLE IF NOT EXISTS SaleLines (
    SaleLineId INTEGER PRIMARY KEY AUTOINCREMENT,
    SaleId INTEGER NOT NULL,
    ProductId INTEGER NOT NULL,
    Quantity INTEGER NOT NULL,
    UnitPrice NUMERIC NOT NULL,
    LineTotal NUMERIC NOT NULL,
    FOREIGN KEY (SaleId) REFERENCES Sales(SaleId) ON DELETE CASCADE,
    FOREIGN KEY (ProductId) REFERENCES Products(ProductId)
);

CREATE TABLE IF NOT EXISTS Purchases (
    PurchaseId INTEGER PRIMARY KEY AUTOINCREMENT,
    Date TEXT NOT NULL DEFAULT (datetime('now')),
    TotalAmount NUMERIC NOT NULL DEFAULT 0,
    Note TEXT
);

CREATE TABLE IF NOT EXISTS PurchaseLines (
    PurchaseLineId INTEGER PRIMARY KEY AUTOINCREMENT,
    PurchaseId INTEGER NOT NULL,
    ProductId INTEGER NOT NULL,
    Quantity INTEGER NOT NULL,
    UnitPrice NUMERIC NOT NULL,
    LineTotal NUMERIC NOT NULL,
    FOREIGN KEY (PurchaseId) REFERENCES Purchases(PurchaseId) ON DELETE CASCADE,
    FOREIGN KEY (ProductId) REFERENCES Products(ProductId)
);

CREATE TABLE IF NOT EXISTS Expenses (
    ExpenseId INTEGER PRIMARY KEY AUTOINCREMENT,
    Date TEXT NOT NULL DEFAULT (datetime('now')),
    Category TEXT,
    Amount NUMERIC NOT NULL,
    Note TEXT
);

CREATE TABLE IF NOT EXISTS StockMovements (
    MovementId INTEGER PRIMARY KEY AUTOINCREMENT,
    ProductId INTEGER NOT NULL,
    QuantityChange INTEGER NOT NULL,
    MovementType TEXT NOT NULL,
    ReferenceId INTEGER,
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),
    Note TEXT,
    FOREIGN KEY (ProductId) REFERENCES Products(ProductId)
);

CREATE TABLE IF NOT EXISTS Stock (
    ProductId INTEGER PRIMARY KEY,
    Quantity INTEGER NOT NULL DEFAULT 0,
    FOREIGN KEY (ProductId) REFERENCES Products(ProductId)
);

CREATE INDEX IF NOT EXISTS IDX_Products_SKU ON Products(SKU);
CREATE INDEX IF NOT EXISTS IDX_StockMovements_ProductDate ON StockMovements(ProductId, CreatedAt);
CREATE INDEX IF NOT EXISTS IDX_SaleLines_SaleId ON SaleLines(SaleId);
CREATE INDEX IF NOT EXISTS IDX_SaleLines_ProductId ON SaleLines(ProductId);
CREATE INDEX IF NOT EXISTS IDX_PurchaseLines_ProductId ON PurchaseLines(ProductId);

COMMIT;
