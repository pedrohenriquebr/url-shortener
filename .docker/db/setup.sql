USE [UrlShortenerDb];
GO

IF NOT EXISTS (SELECT *
FROM INFORMATION_SCHEMA.TABLES
where TABLE_NAME='ShortenedUrls' AND TABLE_SCHEMA='dbo')
BEGIN

CREATE SEQUENCE [dbo].[ShortenedUrl_HiLoSequence]
    AS BIGINT       
    START WITH 1    
    INCREMENT BY 2000000  
    MINVALUE 1      
    NO CYCLE        
    NO CACHE;       

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