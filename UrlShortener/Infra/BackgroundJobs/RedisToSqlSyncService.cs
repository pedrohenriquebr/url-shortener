using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using UrlShortener.Infra.Context;

namespace UrlShortener.Infra.BackgroundJobs;

public class RedisToSqlSyncService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisToSqlSyncService> _logger;
    private const string AccessCountKeyPrefix = "access_count:";

    public RedisToSqlSyncService(
        IServiceProvider serviceProvider, 
        IConnectionMultiplexer redis,
        ILogger<RedisToSqlSyncService> logger)
    {
        _serviceProvider = serviceProvider;
        _redis = redis;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Serviço de Sincronização Redis -> SQL iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); // Espera 30 segundos entre cada ciclo
                
                var redisDb = _redis.GetDatabase();
                var server = _redis.GetServer(_redis.GetEndPoints().First());

                // Usa SCAN para buscar as chaves sem bloquear o servidor Redis
                var keys = server.KeysAsync(pattern: $"{AccessCountKeyPrefix}*");
                
                var keysToProcess = new List<RedisKey>();
                await foreach (var key in keys)
                {
                    keysToProcess.Add(key);
                }

                if (!keysToProcess.Any())
                {
                    continue; // Pula o ciclo se não houver nada para processar
                }

                _logger.LogInformation($"Encontradas {keysToProcess.Count} chaves de contagem para sincronizar.");

                // Pega os valores de todas as chaves de uma vez
                var values = await redisDb.StringGetAsync(keysToProcess.ToArray());

                // Cria um dicionário para facilitar o acesso: ShortCode -> Contagem
                var updateTasks = new Dictionary<string, long>();
                for (int i = 0; i < keysToProcess.Count; i++)
                {
                    string shortCode = keysToProcess[i].ToString().Replace(AccessCountKeyPrefix, "");
                    if (long.TryParse(values[i], out long count))
                    {
                        updateTasks[shortCode] = count;
                    }
                }
                
                // --- Atualização em Lote no SQL Server ---
                // É crucial criar um escopo para o DbContext em um BackgroundService
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<UrlShortenerDbContext>();
                    
                    var transaction = await dbContext.Database.BeginTransactionAsync(stoppingToken);

                    foreach (var task in updateTasks)
                    {
                        // Usa ExecuteUpdateAsync para uma atualização performática sem carregar a entidade
                        await dbContext.ShortenedUrls
                            .Where(u => u.ShortCode == task.Key)
                            .ExecuteUpdateAsync(s => s.SetProperty(
                                u => u.AccessCount, 
                                u => u.AccessCount + task.Value), // Incrementa o valor existente
                                cancellationToken: stoppingToken) ;
                    }

                    await transaction.CommitAsync(stoppingToken);
                }

                // Se tudo deu certo no SQL, remove as chaves do Redis
                await redisDb.KeyDeleteAsync(keysToProcess.ToArray());
                _logger.LogInformation("Sincronização concluída com sucesso.");
            }
            catch (OperationCanceledException)
            {
                // Comportamento normal ao parar a aplicação
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro no serviço de sincronização Redis -> SQL.");
                // Em um cenário real, você adicionaria uma lógica de retry aqui.
            }
        }
        
        _logger.LogInformation("Serviço de Sincronização Redis -> SQL finalizado.");
    }
}