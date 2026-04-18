using System.Text.Json.Serialization;

namespace ExamPrepIdeaCenter.Models
{
    internal class ApiResponseDTO
    {
        [JsonPropertyName("msg")]

        public string? Msg { get; set; }

        [JsonPropertyName("movie")]
        public MovieDTO Movie { get; set; }
    }
}