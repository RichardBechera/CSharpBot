using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using Newtonsoft.Json;

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
        public async Task Poll(CommandContext ctx, TimeSpan time, params string[] options)
        {
            
            var interaction = ctx.Client.GetInteractivityModule();
            //var duration = TimeSpan.FromMinutes(double.Parse(time));
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
            var result = await interaction.CollectReactionsAsync(pollMessage, time).ConfigureAwait(false);
            var results = result.Reactions;

            var printableResults = results.Aggregate("", (rest, emoji) => rest + $"\n{emoji.Key}: {emoji.Value}");
            await ctx.RespondAsync(printableResults).ConfigureAwait(false);
            
        }

        [Command("emotevote")]
        [Description("Voting which emote should remain")]
        public async Task Emotevote(CommandContext ctx, string old, string newemote, string name)
        {
            var webClient = new WebClient();
            Stream imageBytes;
            /*try
            {
                imageBytes = webClient.OpenRead(newemote);
            }
            catch (WebException e)
            {
                ctx.RespondAsync("wrong uri");
                return;
            }*/
            var interaction = ctx.Client.GetInteractivityModule();
            var time = new TimeSpan(0, 0, 0, 59);

            var optionReactions = new[] {DiscordEmoji.FromName(ctx.Client, ":thumbsup:"), 
                DiscordEmoji.FromName(ctx.Client, ":thumbsdown:")
            };
            var pollMessage = await ctx.Channel.SendMessageAsync( $"{old} => {newemote}\nVoting starts in 5 seconds, please wait.");
            foreach (var t in optionReactions)
            {
                await pollMessage.CreateReactionAsync(t).ConfigureAwait(false);
            }
            Thread.Sleep(5000);
            var resultReactions = await interaction.CollectReactionsAsync(pollMessage, time).ConfigureAwait(false);
            var resultType = resultReactions.Reactions
                .Where(a => a.Key.Equals(optionReactions[0]) || a.Key.Equals(optionReactions[1]))
                .Select(a => new {emoji = a.Key.Equals(optionReactions[0]), count = a.Value})
                .OrderBy(a=> a.count);
            var isMore = false;
            var isRatio = true;
            if (resultType.Any())
                isMore = resultType.First().emoji;
            if (resultType.Count() == 2)
                isRatio = resultType.First().count / resultType.Last().count > 9 / 6.0;

            var result = isMore && isRatio;
            await ctx.RespondAsync(result ? "The change will come" : "People disagreed");
            //! delete and add doesn't work properly
            var emoji = ctx.Guild.GetEmojisAsync().Result.First(a => a.GetDiscordName().Equals(old.Trim()));
            await ctx.RespondAsync(emoji.GetDiscordName());
            await ctx.Guild.DeleteEmojiAsync(emoji);
            await ctx.RespondAsync(emoji.GetDiscordName());
            await ctx.Guild.CreateEmojiAsync(name, webClient.OpenRead(newemote));
            await ctx.RespondAsync(emoji.Name);

        }

        [Command("memes_t")]
        [Description("Sends random meme")]
        public async Task MemesT(CommandContext ctx)
        {
            var client = new WebClient();
            var content = client.DownloadString("https://api.imgflip.com/get_memes");
            dynamic stuff = JsonConvert.DeserializeObject(content);
            if (stuff == null) return;
            var memes = stuff.data.memes;
            var templates = "";
            for (int i = 0; i < 100; i++)
            {
                templates += $"{memes[i].name}: {memes[i].id}\n";
                if (i % 10 != 9) continue;
                await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                {
                    Description = templates
                });
                templates = "";
            }
        }
        
        [Command("memes_s")]
        [Description("Sends random meme")]
        public async Task MemesS(CommandContext ctx, string id)
        {
            var client = new WebClient();
            var content = client.DownloadString("https://api.imgflip.com/get_memes");
            dynamic stuff = JsonConvert.DeserializeObject(content);
            if (stuff == null) return;
            var memes = stuff.data.memes;
            
            var template = "";
            for (var i = 0; i < 100; i++)
                if (memes[i].id == id)
                {
                    template = memes[i].url;
                    break;
                }

            
            //var index = new Random().Next(0, 99);
            //var name = memes[index].name;
            //var imageUrl = memes[index].url;
            
            await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
            {
                ImageUrl = template
            });
        }
        
        [Command("memes_c")]
        [Description("Sends random meme with custom caption, separate text with |")]
        public async Task MemesC(CommandContext ctx, string id, [RemainingText] string caption)
        {
            var client = new WebClient();
            var captions = caption.Split('|', 2);
            var content = client.DownloadString("https://api.imgflip.com/get_memes");
            dynamic stuff = JsonConvert.DeserializeObject(content);
            if (stuff == null) return;
            var memes = stuff.data.memes;

            var imageResult = AddCaption(captions[0], captions[1], id);
            await ctx.RespondAsync(await imageResult);
        }

        private async Task<string> AddCaption(string caption1, string caption2, string template)
        {
            var client = new HttpClient();
            var values =  new Dictionary<string, string>{
                {"template_id", template},
                {"username", "richardBechera"},
                {"password", "nExv.cH9.KZvH#9"},
                {"text0", caption1},
                {"text1", caption2}
            };

            var content = new FormUrlEncodedContent(values);
            var response = await client.PostAsync("https://api.imgflip.com/caption_image", content);
            var stuff = response.Content;
            var jsonContent = stuff.ReadAsStringAsync().Result;
            dynamic url = JsonConvert.DeserializeObject(jsonContent);
            return url.data.url;
        } 
        
    }
}