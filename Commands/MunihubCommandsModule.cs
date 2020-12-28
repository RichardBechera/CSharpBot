using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using csharpbot.Commands.Munhub_models;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Text.Json;

namespace csharpbot.Commands
{
    public class MunihubCommandsModule : IModule
    {
        [Command("rate-subject")]
        [Description("Leave reating of subject")]
        public async Task RateSubject(CommandContext ctx, string code, byte rating, params string[] content)
        {
            var positives = new List<string>();
            var negatives = new List<string>();
            var comment = new List<string>();
            ParseContent(content, positives, negatives, comment);
            
            var builder = new DiscordEmbedBuilder()
            {
                Color = DiscordColor.Green,
                Description = printableContent(positives, negatives, comment, rating),
                Timestamp = DateTimeOffset.Now,
                Title = $"Raing for {code}"
            };
            
            
            await ctx.RespondAsync(embed: builder.Build());

            RatingDTO ratingObj = new RatingDTO(string.Join(' ', comment), string.Join(';', positives),
                string.Join(';', negatives), rating);
            await ctx.RespondAsync(JsonSerializer.Serialize<RatingDTO>(ratingObj));
            //ctx.Message.DeleteAsync("shouldnt be there");
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