-- Tabela de Conta Corrente
CREATE TABLE IF NOT EXISTS contacorrente (
    id SERIAL PRIMARY KEY,
    numero TEXT UNIQUE NOT NULL,
    nome TEXT NOT NULL,
    cpf TEXT UNIQUE NOT NULL,
    ativo INTEGER NOT NULL DEFAULT 1
);

-- Tabela de Movimentacoes
CREATE TABLE IF NOT EXISTS movimento (
    id SERIAL PRIMARY KEY,
    contacorrente TEXT NOT NULL,
    datamovimento TIMESTAMP NOT NULL,
    tipomovimento TEXT NOT NULL, -- 'C' ou 'D'
    valor DECIMAL(18,2) NOT NULL,
    FOREIGN KEY (contacorrente) REFERENCES contacorrente (numero)
);

-- Tabela de Idempotência
CREATE TABLE IF NOT EXISTS idempotencia (
    chave_idempotencia TEXT PRIMARY KEY,
    requisicao TEXT,
    resultado TEXT
);
