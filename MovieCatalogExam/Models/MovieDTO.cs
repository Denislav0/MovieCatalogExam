using System.Text.Json.Serialization;

namespace ExamPrepIdeaCenter.Models
{
    internal class MovieDTO
    {
        [JsonPropertyName("id")]

        public string? Id { get; set; }

        [JsonPropertyName("title")]

        public string? Title { get; set; }

        [JsonPropertyName("description")]

        public string? Description { get; set; }

    }
}