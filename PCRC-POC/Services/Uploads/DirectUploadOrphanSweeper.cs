using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PCRC.ServicesInterface.Configuration;
using PCRC.ServicesInterface.Uploads;

namespace PCRC.Services.Uploads;

/// Timer-driven background worker that periodically asks IUploadService to clean up direct uploads
/// whose clients walked away after Phase 1 (BeginDirect) without ever calling Phase 3 (Finalize).
/// See KodiakMultiSelectContractUploadSequence.puml — "Orphan sweep" section.
public sealed class DirectUploadOrphanSweeper : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptionsMonitor<DirectUploadOrphanSweeperOptions> _optionsMonitor;
    private readonly ILogger<DirectUploadOrphanSweeper> _logger;

    public DirectUploadOrphanSweeper(
        IServiceScopeFactory scopeFactory,
        IOptionsMonitor<DirectUploadOrphanSweeperOptions> optionsMonitor,
        ILogger<DirectUploadOrphanSweeper> logger)
    {
        _scopeFactory = scopeFactory;
        _optionsMonitor = optionsMonitor;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var options = _optionsMonitor.CurrentValue;
            if (!options.Enabled)
            {
                await Task.Delay(options.Interval, stoppingToken);
                continue;
            }

            try
            {
                var cutoff = DateTime.UtcNow - options.MaxAge;
                await using var scope = _scopeFactory.CreateAsyncScope();
                var uploadService = scope.ServiceProvider.GetRequiredService<IUploadService>();
                await uploadService.SweepDirectUploadOrphansAsync(cutoff, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Direct-upload orphan sweep pass failed; retrying after Interval.");
            }

            try
            {
                await Task.Delay(options.Interval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }
}