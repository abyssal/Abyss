using Disqord.Bot;
using Qmmands;
using System.Threading.Tasks;
using Disqord;
using HumanDateParser;
using System.Text;
using System;
using System.Linq;
using Abyss.Persistence.Relational;
using Abyss.Services;
using Disqord.Gateway;

namespace Abyss.Modules
{
    [Name("Scheduling")]
    public class ReminderModule : DiscordGuildModuleBase
    {
        [Group("remindme", "remind", "reminders")]
        [Description("Reminder control.")]
        public class RemindSubmodule : DiscordGuildModuleBase
        {
            public RemindSubmodule(ReminderService reminder, AbyssPersistenceContext dbContext)
            {
                _reminders = reminder;
                _dbContext = dbContext;
            }
            private readonly ReminderService _reminders;
            private readonly AbyssPersistenceContext _dbContext;

            /*[Command("", "list", "all")]
            public async Task RemindersAsync()
            {
                string Map(Reminder r)
                {
                    return new StringBuilder().AppendLine($"**{r.Text}** (ID {r.Id})").AppendLine($"- Due {r.DueAt}").AppendLine($"- Created {r.CreatedAt}").AppendLine($"- In {(Context.Guild.GetChannel(r.ChannelId) as CachedTextChannel)?.Mention}").ToString();
                }
                var gsr = await _reminders.ListRemindersInGuildFromUserAsync(Context.Guild.Id, Context.User.Id);
                await ReplyAsync(embed: 
                    new LocalEmbedBuilder()
                        .WithColor(Color.LightPink)
                        .WithAuthor("Your reminders in " + Context.Guild.Name, Context.Guild.GetIconUrl())
                        .WithDescription(string.Join("\n", gsr.Select(Map)))
                        .WithFooter("Start a reminder with 'remindme 14h {message}'.")
                        .Build()
                );
            }*/

            [Command]
            [Description("Starts a reminder.")]
            public async Task<DiscordCommandResult> AddPrefixAsync([Name("Time")] [Description("The time to remind you.")] string time,
                [Name("Message")] [Description("What to remind you of.")] [Remainder] string description)
            {
                try
                {
                    var offset = HumanDateParser.HumanDateParser.Parse(time);
                    var diff = offset - DateTime.Now;
                    if (diff.TotalSeconds <= 0 || diff.TotalMilliseconds > 2147400000)
                    {
                        return Reply(
                            "Invalid time. I can only take times in the future, and not past 3.5 weeks from today.");
                    }

                    var reminder = await _reminders.CreateReminderAsync(Context.Guild.Id, Context.Channel.Id,
                        Context.Message.Id, Context.Author.Id, description, offset);
                    return Reply(
                        new LocalEmbed()
                            .WithColor(Constants.Theme)
                            .WithTitle("Reminder created")
                            .WithDescription(
                                $"I'll remind you at {Markdown.Timestamp(offset, Markdown.TimestampFormat.ShortDateTime)} of \"{description}\" in this channel.")
                    );
                }
                catch (ParseException)
                {
                    return Reply(
                        "Invalid time. I can only take times in the future, and not past 3.5 weeks from today.");
                }
            }

            [Command("delete")]
            [Description("Deletes a reminder.")]
            public async Task<DiscordCommandResult> DeletePrefixAsync([Name("Reminder")] [Description("The ID of the reminder to delete.")] int reminderId)
            {
                var reminder = _dbContext.Reminders.SingleOrDefault(r => r.Id == reminderId);
                if (reminder == null)
                {
                    return Reply("There's no reminder with that ID.");
                }
                if (reminder.CreatorId != Context.Author.Id && Context.Author.GetPermissions().Has(Permission.ManageGuild))
                {
                    return Reply("You can't delete reminders that aren't yours.");
                }
                _dbContext.Reminders.Remove(reminder);
                await _dbContext.SaveChangesAsync();
                return Reply($"Deleted reminder {reminderId}.");
            }

            [Command("inspect")]
            [Description("Provides information about a relative date.")]
            public async Task<DiscordCommandResult> InspectTimeAsync(string time)
            {
                try
                {
                    var offset = HumanDateParser.HumanDateParser.ParseDetailed(time);
                    return Reply($"**{offset.Result}** ({(offset.Result-DateTimeOffset.Now)} from now)\n{string.Join(", ", offset.Tokens.Select(d => "`" + d.ToString() + "`"))}");
                } catch (ParseException)
                {
                    return Reply("Invalid time. Try a time like `14h`, or `3d`.");
                }
            }
        }
    }
}