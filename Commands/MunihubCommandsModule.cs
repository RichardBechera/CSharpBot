using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using csharpbot.Commands.Munhub_models;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Text.Json;
using System.Threading;
using DSharpPlus.Interactivity;
using RestSharp;

namespace csharpbot.Commands
{
    public class MunihubCommandsModule : IModule
    {
        [Command("rate-subject")]
        [Description("Leave rating of subject")]
        public async Task RateSubject(CommandContext ctx, string code, byte rating, params string[] content)
            => await Rate(ctx, 0, code, rating, content);

        [Command("rate-teacher")]
        [Description("Leave rating of teacher")]
        public async Task RateTeacher(CommandContext ctx, int uco, byte rating, params string[] content)
            => await Rate(ctx, uco, "", rating, content);
        
        public async Task Rate(CommandContext ctx, int uco, string code, byte rating, params string[] content)
        {
            var positives = new List<string>();
            var negatives = new List<string>();
            var comment = new List<string>();
            ParseContent(content, positives, negatives, comment);

            var whome = uco == 0 ? code : uco.ToString();
            
            var builder = new DiscordEmbedBuilder()
            {
                Color = DiscordColor.Green,
                Description = printableContent(positives, negatives, comment, rating),
                Timestamp = DateTimeOffset.Now,
                Title = $"Raing for {whome}"
            };
            
            
            var ratingMessage = await ctx.RespondAsync(embed: builder.Build());

            RatingDTO ratingObj = new RatingDTO(string.Join(' ', comment), string.Join(';', positives),
                string.Join(';', negatives), rating, uco, code);
            var jsonRating = JsonSerializer.Serialize<RatingDTO>(ratingObj);
            await ctx.RespondAsync(jsonRating);
            
            var optionReactions = new[] {DiscordEmoji.FromName(ctx.Client, ":thumbsup:"), 
                DiscordEmoji.FromName(ctx.Client, ":thumbsdown:")
            };
            foreach (var t in optionReactions)
            {
                await ratingMessage.CreateReactionAsync(t).ConfigureAwait(false);
            }
            var interaction = ctx.Client.GetInteractivityModule();
            Thread.Sleep(5000);
            var result = await interaction
                .WaitForMessageReactionAsync(ratingMessage, ctx.Member, new TimeSpan(0,0,20));
            var results = result.Emoji;
            
            if (results.GetDiscordName() == ":+1:")
            {
                var response = PostRating(ratingObj);
                await ctx.RespondAsync(response.StatusCode + response.Content + response.ErrorMessage);
                await ratingMessage.DeleteAsync();
            }
            else if (results.GetDiscordName() == ":-1:")
            {
                //aaaaaaaaa why the fuck u wont delete ???
                await ctx.Channel.DeleteMessageAsync(ratingMessage);
            }
            //Thread.Sleep(5000);
            //await ctx.Message.DeleteAsync();


            //ctx.Message.DeleteAsync("shouldnt be there");
        }

        private IRestResponse PostRating(RatingDTO rating)
        {
            string json = JsonSerializer.Serialize<RatingDTO>(rating);
            var client = new RestClient("https://localhost:5001/api/rating");
            client.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", json,  ParameterType.RequestBody);
            return client.Execute(request);
            
            /*using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
            }*/
        }

        private string printableContent(List<string> positives, List<string> negatives, List<string> comment, byte rating)
        {
            var builder = new StringBuilder();
            builder.AppendJoin('\n', comment);
            builder.Append($"\n**Positives:**\n");
            foreach (var pos in positives)
            {
                builder.AppendLine($":thumbsup:  {pos}");
            }
            builder.Append($"\n**Negatives:**\n");
            foreach (var neg in negatives)
            {
                builder.AppendLine($":thumbsdown:  {neg}");
            }

            builder.AppendLine($"\n{string.Concat(Enumerable.Repeat(":green_circle:", rating))}" +
                               $"{string.Concat(Enumerable.Repeat(":red_circle:" ,10 - rating))}");

            return builder.ToString();
        }

        private void ParseContent(string[] content, List<string> positives, List<string> negatives, List<string> comment)
        {
            byte current = 0;
            
            var builder = new StringBuilder("");
            foreach (string s in content)
            {
                if (s.StartsWith("+") && current != 1)
                {
                    assignString(builder, current, positives, negatives, comment);
                    current = 1;
                    builder.Append(s.Substring(1));
                }
                else if (s.StartsWith("-") && current != 2)
                {
                    assignString(builder, current, positives, negatives, comment);
                    current = 2;
                    builder.Append(s.Substring(1));
                }
                else if (s.StartsWith("^") && current != 0)
                {
                    assignString(builder, current, positives, negatives, comment);
                    current = 0;
                    builder.Append(s.Substring(1));
                }
                else
                {
                    builder.Append(s);
                }

                builder.Append(' ');
            }
        }

        private void assignString(StringBuilder s, byte current, List<string> pos, List<string> neg, List<string> com)
        {
            switch (current)
            {
                case 0:
                    com.Add(s.ToString());
                    break;
                case 1:
                    pos.Add(s.ToString());
                    break;
                case 2:
                    neg.Add(s.ToString());
                    break;
            }

            s.Clear();
        }
    } 
}