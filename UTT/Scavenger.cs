using Microsoft.AspNetCore.SignalR;

namespace UTT;

public class Scavenger : BackgroundService
{
    private readonly IHubContext<UTTHub> _hubContext;
    private readonly ILogger<Scavenger> _logger;
    public Scavenger(IHubContext<UTTHub> hubContext, ILogger<Scavenger> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Mark games that haven't played in a while as completed
            var changed = Game.MarkExpiredGames();

            // Mark completed games as removed
            var removed = Game.RemoveCompletedGames();

            if (removed > 0)
            {
                _logger.LogInformation("Removed {GameCount} games.", removed);
            }

            if (removed > 0 || changed)
            {
                await _hubContext.Clients.All.SendAsync("GameUpdated", Game.GetGames());
            }

            await Task.Delay(5000);
        }
    }
}
