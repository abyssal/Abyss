using Abyssal.Common;
using Disqord;
using Disqord.Bot.Commands;
using Disqord.Bot.Commands.Application;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Disqord.Rest;
using Jacksonbot.Interactions;
using Jacksonbot.Modules.Abstract;
using Newtonsoft.Json.Linq;
using Qmmands;

namespace Jacksonbot.Modules;

[Name("Fun")]
public partial class FunModule : BotModuleBase
{
    [SlashCommand("cat")]
    [Description("Meow.")]
    public async Task<DiscordMenuCommandResult> Command_GetCatPictureAsync()
    {
        return View(new InfinitePageView(new CatPageProvider(this), e => { }));
    }

    public class DiceRollView : ViewBase
    {
        private string _dice;

        private static string Evaluate(string data)
        {
            return $"I rolled {DiceExpression.Evaluate(data)} on a **{data}** die.";
        }

        public DiceRollView(string dice) : base(e => e.WithContent(Evaluate(dice)))
        {
            _dice = dice;
        }

        [Button(Label = "Re-roll", Style = LocalButtonComponentStyle.Success)]
        public ValueTask Reroll(ButtonEventArgs e)
        {
            MessageTemplate = e => e.WithContent(Evaluate(_dice));
            ReportChanges();
            return default;
        }

        [Button(Label = "Double", Style = LocalButtonComponentStyle.Secondary)]
        public ValueTask Double(ButtonEventArgs e)
        {
            _dice += "+" + _dice;
            MessageTemplate = e => e.WithContent(Evaluate(_dice));
            ReportChanges();
            return default;
        }

        [Button(Label = "d20", Style = LocalButtonComponentStyle.Danger)]
        public ValueTask D20(ButtonEventArgs e)
        {
            _dice = "d20";
            MessageTemplate = e => e.WithContent(Evaluate(_dice));
            RemoveComponent(e.Button);
            ReportChanges();
            return default;
        }
    }

    // [Command("owo")]
    // [Cooldown(1, 5, CooldownMeasure.Seconds, CooldownBucketType.User)]
    // public async Task OwoAsync()
    // {
    //     var random = new Random();
    //     var n = 1;
    //     while (random.NextDouble() < (double)1 / (double)n)
    //     {
    //         await Response($"owo (1/{n})");
    //         n *= 2;
    //     }
    // }

    // [SlashCommand("losers", "league")]
    // [Description("Exposes players in this server playing League of Legends.")]
    // public async Task<DiscordCommandResult> LosersAsync()
    // {
    //     var leaguePlayers = Context.Guild.GetMembers().Values
    //         .Where(e => e.GetPresence()?.Activities.Any(e => e.Name.Contains("League of Legends")) ?? false).ToList();
    //     if (leaguePlayers.Count == 0)
    //     {
    //         return Response("Nobody in this server is playing League of Legends.");
    //     }
    //     var stringBuilder = new StringBuilder();
    //     stringBuilder.AppendLine("**These losers are playing League:**").AppendLine();
    //     foreach (var player in leaguePlayers)
    //     {
    //         stringBuilder.AppendLine($" - **{player.Nick ?? player.Name}** {(player.Nick != null ? $"({player.Name}) " : "")}(for {(int) (DateTimeOffset.Now - player.GetPresence().Activities.First(e => e.Name.Contains("League of Legends")).CreatedAt).TotalMinutes} minutes)");
    //     }
    //
    //     return Response(stringBuilder.ToString());
    // }

    [SlashCommand("randompin")]
    [Description("Shows a random pin.")]
    public async Task<IDiscordCommandResult> Command_RandomPinAsync([Name("From")] 
        [Description("Limits the selection to pins from a specific user. (optional)")] IMember? member = null)
    {
        if (Context.GuildId == null) return Response();
        await Deferral();
        var channels = await Context.Bot.FetchChannelsAsync(Context.GuildId.Value);

        var messages = 
            (await Task.WhenAll(channels.Where(e => e is ITextChannel).Select(async e =>
            {
                try
                {
                    return await ((ITextChannel)e).FetchPinnedMessagesAsync();
                }
                catch (Exception)
                {
                    return new List<IUserMessage>();
                }
            }))).SelectMany(x => x).Where(e => member == null || e.Author.Id == member.Id).ToArray();

        if (messages.Length == 0) return Response("I couldn't fetch any pins for this server. Does this server have any pins?");
        var m = messages.Random();

        var embed = new LocalEmbed()
            .WithAuthor(m.Author)
            .WithDescription(m.Content)
            .WithTimestamp(m.CreatedAt());
        if (m.Attachments.Any())
        {
            embed.ImageUrl = m.Attachments[0].Url;
        }
        return Response(Discord.MessageJumpLink(Context.GuildId, m.ChannelId, m.Id), embed);
    }

    [SlashCommand("roll")]
    [Description("Rolls a dice of the supplied size.")]
    public async Task<IDiscordCommandResult> Command_DiceRollAsync(
        [Name("Dice")]
        [Description("The dice configuration to use. It can be simple, like `6`, or complex, like `d20+d18+4`.")]
        string dice)
    {
        if (!dice.Contains("d") && int.TryParse(dice, out var diceParsed))
        {
            if (diceParsed < 1)
            {
                return Response("Your dice roll must be 1 or above!");
            }

            return View(new DiceRollView("d" + dice));
        }

        try
        {
            return View(new DiceRollView(dice));
        }
        catch (ArgumentException)
        {
            return Response("Invalid dice!");
        }
    }
}

public class CatPageProvider : InfinitePageProvider
{
    private readonly BotModuleBase _caller;
    public CatPageProvider(BotModuleBase caller)
    {
        _caller = caller;
    }
    public override async ValueTask<Page?> GetPageAsync(PagedViewBase view)
    {
        using var response = await new HttpClient().GetAsync("https://api.thecatapi.com/v1/images/search?size=full");
        var url = JToken.Parse(await response.Content.ReadAsStringAsync().ConfigureAwait(false))[0]
            ?.Value<string>("url");
        if (url == null) return null;

        using var factr = await new HttpClient().GetAsync("https://catfact.ninja/fact?max_length=140");
        var fact = JToken.Parse(await factr.Content.ReadAsStringAsync())?.Value<string>("fact");

        return new Page().WithEmbeds(new LocalEmbed()
            .WithTitle(fact ?? "")
            .WithColor(Constants.Theme)
            .WithImageUrl(url)
        );
    }
}