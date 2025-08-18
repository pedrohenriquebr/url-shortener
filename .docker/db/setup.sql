USE [UrlShortenerDb];
GO

IF NOT EXISTS (SELECT *
FROM INFORMATION_SCHEMA.TABLES
where TABLE_NAME='ShortenedUrls' AND TABLE_SCHEMA='dbo')
BEGIN

CREATE SEQUENCE [dbo].[ShortenedUrl_HiLoSequence]
    AS BIGINT       -- Define o tipo de dado do contador (BIGINT é bom para escalar)
    START WITH 1    -- O primeiro número que a sequência irá gerar
    INCREMENT BY 10  -- A cada chamada, o valor da sequência aumenta em 1
    MINVALUE 1      -- O valor mínimo que a sequência pode ter
    NO CYCLE        -- Impede que a sequência reinicie após atingir o valor máximo
    NO CACHE;       -- Garante que os valores não sejam perdidos se o servidor reiniciar

    CREATE TABLE  [dbo].ShortenedUrls(
     Id BIGINT NOT NULL PRIMARY KEY,
     LongUrl NVARCHAR(MAX) COLLATE Latin1_General_CS_AS NOT NULL,
     ShortCode NVARCHAR(7) COLLATE Latin1_General_CS_AS NOT NULL,
     AccessCount BIGINT NOT NULL,
     CreatedAt DATETIME NOT NULL,
     UpdatedAt DATETIME NOT NULL
    );

CREATE UNIQUE INDEX IX_ShortenedUrls_ShortCode
    ON ShortenedUrls (ShortCode);
END

GO