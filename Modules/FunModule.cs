using System;
using System.Net.Http;
using System.Threading.Tasks;
using Abyss.Extensions;
using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Newtonsoft.Json.Linq;
using Qmmands;

namespace Abyss.Modules
{
    public class FunModule : AbyssModuleBase
    {
        [Command("cat")]
        [Description("Meow.")]
        [Cooldown(1, 5, CooldownMeasure.Seconds, CooldownBucketType.User)]
        public async Task<DiscordCommandResult> Command_GetCatPictureAsync()
        {
            return Pages(new CatPageProvider());
        }
    }

    public class CatPageProvider : InfinitePageProvider
    {
        public override async ValueTask<Page> GetPageAsync(PagedViewBase view)
        {
            var url = _pages[view.CurrentPageIndex];
            if (url == null)
            {
                using var response = await new HttpClient().GetAsync("http://aws.random.cat/meow");
                url = JToken.Parse(await response.Content.ReadAsStringAsync().ConfigureAwait(false))
                    .Value<string>("file");
                _pages[view.CurrentPageIndex] = url;
            }
            return new Page().WithEmbeds(new LocalEmbed()
                .WithTitle("Enjoy your random cat.")
                .WithColor(Constants.Theme)
                .WithImageUrl(url)
            ); 
        }

        public string[] _pages = new string[99];
        public override int PageCount { get; } = 99;
    }
}