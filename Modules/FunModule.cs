using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Abyss.Extensions;
using Abyss.Interactions;
using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Disqord.Gateway;
using Disqord.Rest;
using Newtonsoft.Json.Linq;
using Qmmands;

namespace Abyss.Modules;

[Name("Fun")]
public partial class FunModule : AbyssModuleBase
{
    [Command("cat")]
    [Description("Meow.")]
    [Cooldown(1, 5, CooldownMeasure.Seconds, CooldownBucketType.User)]
    public async Task<DiscordCommandResult> Command_GetCatPictureAsync()
    {
        return Pages(new CatPageProvider(this));
    }
        
    public class DiceRollView : ViewBase
    {
        private string _dice;

        private static string Evaluate(string data)
        {
            return $"I rolled {DiceExpression.Evaluate(data)} on a **{data}** die.";
        }
            
        public DiceRollView(string dice) : base(new LocalMessage().WithContent(Evaluate(dice)))
        {
            _dice = dice;
        }
            
        [Button(Label="Re-roll", Style = LocalButtonComponentStyle.Success)]
        public ValueTask Reroll(ButtonEventArgs e)
        {
            TemplateMessage.Content = Evaluate(_dice);
            ReportChanges();
            return default;
        }

        [Button(Label = "Double", Style = LocalButtonComponentStyle.Secondary)]
        public ValueTask Double(ButtonEventArgs e)
        {
            _dice += "+" + _dice;
            TemplateMessage.Content = Evaluate(_dice);
            ReportChanges();
            return default;
        }

        [Button(Label = "d20", Style = LocalButtonComponentStyle.Danger)]
        public ValueTask D20(ButtonEventArgs e)
        {
            _dice = "d20";
            TemplateMessage.Content = Evaluate(_dice);
            RemoveComponent(e.Button);
            ReportChanges();
            return default;
        }
    }

    [Command("owo")]
    [Cooldown(1, 5, CooldownMeasure.Seconds, CooldownBucketType.User)]
    public async Task OwoAsync()
    {
        var random = new Random();
        var n = 1;
        while (random.NextDouble() < (double)1 / (double)n)
        {
            await Response($"owo (1/{n})");
            n *= 2;
        }
    }

    [Command("losers", "league")]
    [Description("Exposes players in this server playing League of Legends.")]
    public async Task<DiscordCommandResult> LosersAsync()
    {
        var leaguePlayers = Context.Guild.GetMembers().Values
            .Where(e => e.GetPresence()?.Activities.Any(e => e.Name.Contains("League of Legends")) ?? false).ToList();
        if (leaguePlayers.Count == 0)
        {
            return Response("Nobody in this server is playing League of Legends.");
        }
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("**These losers are playing League:**").AppendLine();
        foreach (var player in leaguePlayers)
        {
            stringBuilder.AppendLine($" - **{player.Nick ?? player.Name}** {(player.Nick != null ? $"({player.Name}) " : "")}(for {(int) (DateTimeOffset.Now - player.GetPresence().Activities.First(e => e.Name.Contains("League of Legends")).CreatedAt).TotalMinutes} minutes)");
        }

        return Response(stringBuilder.ToString());
    }
        
    [Command("roll", "dice")]
    [Remarks("This command also supports complex dice types, like `d20+d18+4`.")]
    [Description("Rolls a dice of the supplied size.")]
    public async Task<DiscordCommandResult> Command_DiceRollAsync(
        [Name("Dice")]
        [Description("The dice configuration to use. It can be simple, like `6`, or complex, like `d20+d18+4`.")]
        string dice)
    {
        if (!dice.Contains("d") && int.TryParse(dice, out var diceParsed))
        {
            if (diceParsed < 1)
            {
                return Reply("Your dice roll must be 1 or above!");
            }

            return View(new DiceRollView("d" + dice));
        } 

        try
        {
            return View(new DiceRollView(dice));
        }
        catch (ArgumentException)
        {
            return Reply("Invalid dice!");
        }
    }
}

public class CatPageProvider : InfinitePageProvider
{
    private readonly AbyssModuleBase _caller;
    public CatPageProvider(AbyssModuleBase caller)
    {
        _caller = caller;
    }
    public override async ValueTask<Page> GetPageAsync(PagedViewBase view)
    {
        var url = _pages[view.CurrentPageIndex];
        if (url == null)
        {
            using var response = await new HttpClient().GetAsync("https://api.thecatapi.com/v1/images/search?size=full");
            url = JToken.Parse(await response.Content.ReadAsStringAsync().ConfigureAwait(false))[0]
                .Value<string>("url");
            _pages[view.CurrentPageIndex] = url;
        }
        return new Page().WithEmbeds(new LocalEmbed()
            .WithTitle("Enjoy your random cat.")
            .WithColor(_caller.Color)
            .WithImageUrl(url)
        ); 
    }

    public string[] _pages = new string[99];
    public override int PageCount { get; } = 99;
}