namespace csharpbot.Commands.Munhub_models
{
    public class RatingDTO
    {
        public string Comment { get; set; }

        public string Positives { get; set; }

        public string Negatives { get; set; }

        public byte Rating { get; set; }
        
        public int Teacher { get; set; }

        public string Subject { get; set; }

        public RatingDTO(string comment, string positives, string negatives, byte rating, int teacher, string subject)
        {
            Comment = comment;
            Positives = positives;
            Negatives = negatives;
            Rating = rating;
            Teacher = teacher;
            Subject = subject;
        }
    }
}