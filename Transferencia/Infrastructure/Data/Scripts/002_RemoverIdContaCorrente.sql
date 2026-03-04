-- Migration para remover colunas idcontacorrenteorigem e idcontacorrentedestino
-- SQLite nao suporta DROP COLUMN diretamente, entao recriamos a tabela

-- Criar tabela temporaria com nova estrutura
CREATE TABLE transferencia_new (
    idtransferencia INTEGER PRIMARY KEY AUTOINCREMENT,
    idrequisicao TEXT NOT NULL,
    numerocontaorigem TEXT NOT NULL,
    numerocontadestino TEXT NOT NULL,
    datamovimento DATETIME NOT NULL,
    valor REAL NOT NULL,
    status TEXT NOT NULL DEFAULT 'PENDENTE',
    mensagemerro TEXT
);

-- Copiar dados da tabela antiga para a nova
INSERT INTO transferencia_new (idtransferencia, idrequisicao, numerocontaorigem, numerocontadestino, datamovimento, valor, status, mensagemerro)
SELECT idtransferencia, idrequisicao, numerocontaorigem, numerocontadestino, datamovimento, valor, status, mensagemerro
FROM transferencia;

-- Remover tabela antiga
DROP TABLE transferencia;

-- Renomear tabela nova
ALTER TABLE transferencia_new RENAME TO transferencia;
