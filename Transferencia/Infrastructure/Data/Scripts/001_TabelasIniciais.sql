-- Tabela de transferências
CREATE TABLE IF NOT EXISTS transferencia (
    idtransferencia INTEGER PRIMARY KEY AUTOINCREMENT,
    idrequisicao TEXT NOT NULL,
    idcontacorrenteorigem INTEGER NOT NULL,
    numerocontaorigem TEXT NOT NULL,
    idcontacorrentedestino INTEGER,
    numerocontadestino TEXT NOT NULL,
    datamovimento DATETIME NOT NULL,
    valor REAL NOT NULL,
    status TEXT NOT NULL DEFAULT 'PENDENTE',
    mensagemerro TEXT
);

-- Tabela de idempotência para evitar transferências duplicadas
CREATE TABLE IF NOT EXISTS idempotencia (
    chave_idempotencia TEXT PRIMARY KEY,
    requisicao TEXT,
    resultado TEXT
);
