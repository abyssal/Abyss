﻿// See https://aka.ms/new-console-template for more information

using Abyss;
using Abyss.Interactions.Blackjack;
using Abyss.Interactions.Slots;
using Abyss.Interactions.Trivia;
using Abyss.Persistence.Relational;
using Abyss.Services;
using Abyssal.Common;
using AbyssalSpotify;
using Disqord;
using Disqord.Bot;
using Disqord.Bot.Commands;
using Disqord.Bot.Commands.Application;
using Disqord.Bot.Commands.Application.Default;
using Disqord.Bot.Commands.Interaction;
using Disqord.Bot.Hosting;
using Disqord.Gateway;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Qmmands;
using Qmmands.Default;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var host = new HostBuilder()
    .ConfigureLogging((host, logging) =>
    {
        logging.AddSimpleConsole(options =>
        {
            options.IncludeScopes = true;
        });
        logging.AddConfiguration(host.Configuration.GetSection("Logging"));
    }).ConfigureAppConfiguration(appConfigure =>
    {
        appConfigure.AddJsonFile(Constants.CONFIGURATION_FILENAME);
        appConfigure.AddEnvironmentVariables(Constants.ENVIRONMENT_VARIABLE_PREFIX);
    })
    .ConfigureServices((b, s) =>
    {
        s.AddDbContext<AbyssDatabaseContext>((options, ctx) =>
        {
            ctx.UseNpgsql(b.Configuration.GetConnectionString("Database"));
        });
        s.AddSingleton<IActionScheduler, ActionScheduler>();
        s.AddSingleton<ReminderService>();
        s.AddScoped<TransactionManager>();
        s.AddSingleton(SpotifyClient.FromClientCredentials(
                b.Configuration.GetSection("Secrets").GetSection("Spotify")["ClientId"],
                b.Configuration.GetSection("Secrets").GetSection("Spotify")["ClientSecret"]
            )
        );
        //s.AddSingleton<IApplicationCommandCacheProvider>(new NoopCacheProvider());
    })
    .ConfigureDiscordBot((ctx, bot) =>
    {
        bot.Token = ctx.Configuration.GetSection("Secrets").GetSection("Discord")["Token"];
        bot.Intents = GatewayIntents.Unprivileged;
        bot.Prefixes = new[] { "ad." };
        
    })
    .Build();

using var scope = host.Services.CreateScope();
await using (var ctx = scope.ServiceProvider.GetRequiredService<AbyssDatabaseContext>())
{
    await ctx.Database.MigrateAsync();
    await ctx.Database.EnsureCreatedAsync();
}

var bot = scope.ServiceProvider.GetRequiredService<DiscordBot>();
await host.RunAsync();

public class NoopCache : IApplicationCommandCache
{
    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public IApplicationCommandCacheChanges GetChanges(Snowflake? guildId, IEnumerable<LocalApplicationCommand> commands)
    {
        return new DefaultApplicationCommandCacheProvider.Changes(true);
    }

    public void ApplyChanges(Snowflake? guildId, IApplicationCommandCacheChanges changes, IEnumerable<IApplicationCommand> commands)
    {
    }
}
public class NoopCacheProvider : IApplicationCommandCacheProvider
{
    public async ValueTask<IApplicationCommandCache> GetCacheAsync(CancellationToken cancellationToken)
    {
        return new NoopCache();
    }
}