using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using System;
using System.Linq;
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
            /* Trigger the Typing... in discord */
            await ctx.TriggerTypingAsync();

            /* Send the message "I'm Alive!" to the channel the message was recieved from */
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
            // You can also check for a specific message by doing something like
            // c => c.Content == "something"

            // Null if the user didn't respond before the timeout
            if(reminderContent == null)
            {
                await ctx.RespondAsync("Sorry, I didn't get a response!");
                return;
            }
            
            // Homework: have this change depending on if they say "good" or "bad", etc.
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
    }
}