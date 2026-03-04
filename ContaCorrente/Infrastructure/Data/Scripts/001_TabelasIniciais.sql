-- Tabela de Conta Corrente 
CREATE TABLE IF NOT EXISTS contacorrente (
    idcontacorrente INTEGER PRIMARY KEY AUTOINCREMENT,
    numero TEXT NOT NULL,
    nome TEXT NOT NULL,
    cpf TEXT UNIQUE NOT NULL,
    ativo INTEGER NOT NULL DEFAULT 1,
    senha TEXT NOT NULL,
    salt TEXT
);

-- Tabela de Movimentaçõe
CREATE TABLE IF NOT EXISTS movimento (
    idmovimento INTEGER PRIMARY KEY AUTOINCREMENT,
    idcontacorrente INTEGER NOT NULL,
    datamovimento DATETIME NOT NULL,
    tipomovimento TEXT NOT NULL, -- 'C' ou 'D'
    valor REAL NOT NULL,
    FOREIGN KEY (idcontacorrente) REFERENCES contacorrente (idcontacorrente)
);

-- Tabela de Idempotência
CREATE TABLE IF NOT EXISTS idempotencia (
    chave_idempotencia TEXT PRIMARY KEY,
    requisicao TEXT,
    resultado TEXT
);