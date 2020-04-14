using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using System;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;

namespace csharpbot.Commands
{
    
/* Create our class and extend from IModule */
    public class BasicCommandsModule : IModule
    {
        /* Commands in DSharpPlus.CommandsNext are identified by supplying a Command attribute to a method in any class you've loaded into it. */
        /* The description is just a string supplied when you use the help command included in CommandsNext. */
        [Command("alive")]
        [Description("Simple command to test if the bot is running!")]
        public async Task Alive(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await ctx.RespondAsync("I'm alive!");
        }
        
        [Command("mood")]
        [Description("Simple command to test interaction!")]
        public async Task Mood(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await ctx.RespondAsync("How are you today?");

            var intr = ctx.Client.GetInteractivityModule(); // Grab the interactivity module
            var reminderContent = await intr.WaitForMessageAsync(
                c => c.Author.Id == ctx.Message.Author.Id, // Make sure the response is from the same person who sent the command
                TimeSpan.FromSeconds(60) // Wait 60 seconds for a response instead of the default 30 we set earlier!
            );
            
            if(reminderContent == null)
            {
                await ctx.RespondAsync("Sorry, I didn't get a response!");
                return;
            }
            
            
            var good_words = new string[] {"good", "great", "awesome", "well", "dobre", "vyborne", "skvelo"};
            var bad_words = new string[] {"bad", "horrible", "terrible", "better", "zle", "hrozne", "otrasne"};

            if (good_words.Any(word => reminderContent.Message.Content.Contains(word)))
            {
                if (reminderContent.Message.Content.Contains("not"))
                {
                    await ctx.RespondAsync("What is wrong my dear?");
                    return;
                }
                await ctx.RespondAsync("That's great! THis is such a nice day then!");
            }
            else if (bad_words.Any(word => reminderContent.Message.Content.Contains(word)))
            {
                if (reminderContent.Message.Content.Contains("not"))
                {
                    await ctx.RespondAsync(Formatter.Bold("That's great! THis is such a nice day then!"));
                    return;
                }
                await ctx.RespondAsync("What is wrong my dear?");
            }
            else
            {
                await ctx.RespondAsync("Just answer the god damn question!");
            }
            
        }

        
        //! sending embed message doesnt work
        [Command("poll")]
        [Description("Creates simple poll, params: time and options")]
        public async Task Poll(CommandContext ctx, string time, params string[] options)
        {
            
            var interaction = ctx.Client.GetInteractivityModule();
            var duration = TimeSpan.FromMinutes(double.Parse(time));
            await ctx.TriggerTypingAsync();
            var optionReactions = new[] {DiscordEmoji.FromName(ctx.Client, ":one:"), 
                DiscordEmoji.FromName(ctx.Client, ":two:"), 
                DiscordEmoji.FromName(ctx.Client, ":three:"), 
                DiscordEmoji.FromName(ctx.Client, ":four:"), 
                DiscordEmoji.FromName(ctx.Client, ":five:")
            };
            var printer = "";
            for (var j = 0; j < optionReactions.Length; j++)
            {
                printer += $"\n{optionReactions[j].Name} {options[j]}";
            }
            //TODO
            /*var embed = new DiscordEmbedBuilder
            {
                Title = "Poll",
                Color = DiscordColor.Blurple,
                ThumbnailUrl = ctx.Client.GatewayUrl,
                Description = printer
            }.Build();
            */
            var pollMessage = await ctx.Channel.SendMessageAsync(printer + "\nVoting starts in 5 seconds, please wait.");
            foreach (var t in optionReactions)
            {
                await pollMessage.CreateReactionAsync(t).ConfigureAwait(false);
            }

            Thread.Sleep(5000);
            var result = await interaction.CollectReactionsAsync(pollMessage, duration).ConfigureAwait(false);
            var results = result.Reactions;

            var printableResults = results.Aggregate("", (rest, emoji) => rest + $"\n{emoji.Key}: {emoji.Value}");
            await ctx.RespondAsync(printableResults).ConfigureAwait(false);
            
        }
        
    }
}