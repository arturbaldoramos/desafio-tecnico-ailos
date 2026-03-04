-- Tabela de tarifacoes
CREATE TABLE IF NOT EXISTS tarifacao (
    idtarifacao INTEGER PRIMARY KEY AUTOINCREMENT,
    numerocontacorrente TEXT NOT NULL,
    idrequisicaotransferencia TEXT NOT NULL UNIQUE,
    valor REAL NOT NULL,
    datatarfacao DATETIME NOT NULL
);

-- Tabela de idempotencia para evitar tarifacoes duplicadas
CREATE TABLE IF NOT EXISTS idempotencia (
    chave_idempotencia TEXT PRIMARY KEY,
    requisicao TEXT,
    resultado TEXT
);
