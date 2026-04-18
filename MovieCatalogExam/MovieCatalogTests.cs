using ExamPrepIdeaCenter.Models;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Net;
using System.Text.Json;



namespace MovieCatalogExam

{
    [TestFixture]
    public class Tests
    {
        private RestClient client;
        private static string lastCreatedMovieId;

        private const string BaseUrl = "http://144.91.123.158:5000/api";
        private const string StaticToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJKd3RTZXJ2aWNlQWNjZXNzVG9rZW4iLCJqdGkiOiI1MDBiZDBkZi05MDhiLTQ5MTctOTA4Yi1jMGNiNTI4ZTcyNmYiLCJpYXQiOiIwNC8xOC8yMDI2IDA2OjA4OjAxIiwiVXNlcklkIjoiMzFjYWQ1MDEtOTkyOC00ZGU0LTYyMjMtMDhkZTc2OTcxYWI5IiwiRW1haWwiOiJ1c2VyMTIzMTIzQGV4YW1wbGUuY29tIiwiVXNlck5hbWUiOiJzdHJpbmcxMjMxMjMiLCJleHAiOjE3NzY1MTQwODEsImlzcyI6Ik1vdmllQ2F0YWxvZ19BcHBfU29mdFVuaSIsImF1ZCI6Ik1vdmllQ2F0YWxvZ19XZWJBUElfU29mdFVuaSJ9.tWmhfvGFfxuKFYLoTqx9yyq6bzuCxeOk1Qa0jcRWhU4";

        private const string LoginEmail = "user123123@example.com";
        private const string LoginPassword = "string321321";

        [OneTimeSetUp]
        public void Setup()
        {
            string jwtToken;

            if (!string.IsNullOrWhiteSpace(StaticToken))
            {
                jwtToken = StaticToken;
            }
            else
            {
                jwtToken = GetJwtToken(LoginEmail, LoginPassword);
            }

            var options = new RestClientOptions(BaseUrl)
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };

            this.client = new RestClient(options);
        }

        private string GetJwtToken(string email, string password)
        {
            var tempClient = new RestClient(BaseUrl);
            var request = new RestRequest("/User/Authentication", Method.Post);
            request.AddJsonBody(new { email, password });

            var response = tempClient.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(response.Content);
                var token = content.GetProperty("token").GetString();

                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new InvalidOperationException("Token not found in the response.");
                }
                return token;
            }
            else
            {
                throw new InvalidOperationException($"Failed to authenticate. Status code: {response.StatusCode}, Response: {response.Content}");
            }
        }

        [Order(1)]
        [Test]
        public void Test_CreateMovieWithRequiredFields()
        {
            var newMovie = new MovieDTO
            {
                Title = "The Matrix",
                Description = "A computer hacker learns from mysterious rebels about the true nature of his reality."
            };

            var request = new RestRequest("/Movie/Create", Method.Post);
            request.AddJsonBody(newMovie);

            var response = client.Execute(request);
            var content = JsonConvert.DeserializeObject<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(content.Movie, Is.Not.Null);
            Assert.That(content.Movie.Id, Is.Not.Null.And.Not.Empty);
            Assert.That(content.Msg, Is.EqualTo("Movie created successfully!"));

            lastCreatedMovieId = content.Movie.Id; 

        }

        [Order(2)]
        [Test]
        public void Test_EditMovie()
        {
            var request = new RestRequest("/Movie/Edit", Method.Put);
            request.AddQueryParameter("movieId", lastCreatedMovieId); 

            var updatedMovie = new MovieDTO
            {
                Title = "The Matrix Reloaded",
                Description = "Updated description for the sequel."
            };
            request.AddJsonBody(updatedMovie);

            var response = client.Execute(request);
            var content = JsonConvert.DeserializeObject<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(content.Msg, Is.EqualTo("Movie edited successfully!"));
        }

        [Order(3)]
        [Test]

        public void Test_GetAllMovies()
        {
            var request = new RestRequest("/Catalog/All", Method.Get);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var movies = JsonConvert.DeserializeObject<List<MovieDTO>>(response.Content);
            Assert.That(movies, Is.Not.Empty);
        }


        [Order(4)]
        [Test]

        public void Test_DeleteMovie()
        {
            var request = new RestRequest("/Movie/Delete", Method.Delete);
            request.AddQueryParameter("movieId", lastCreatedMovieId); 

            var response = client.Execute(request);
            var content = JsonConvert.DeserializeObject<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(content.Msg, Is.EqualTo("Movie deleted successfully!"));
        }

        [Order(5)]
        [Test]
        public void Test_CreateMovie_MissingFields_ReturnsBadRequest()
        {
            var request = new RestRequest("/Movie/Create", Method.Post);
            request.AddJsonBody(new { posterUrl = "http://image.jpg" }); 

            var response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Order(6)]
        [Test]

        public void Test_EditNonExistingMovie_ReturnsBadRequest()
        {
            var request = new RestRequest("/Movie/Edit", Method.Put);
            request.AddQueryParameter("movieId", "invalid_id_123");
            request.AddJsonBody(new { title = "None", description = "None" });

            var response = client.Execute(request);
            var content = JsonConvert.DeserializeObject<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(content.Msg, Is.EqualTo("Unable to edit the movie! Check the movieId parameter or user verification!"));
        }

        [Order(7)]
        [Test]

        public void Test_DeleteNonExistingMovie_ReturnsBadRequest()
        {
            var request = new RestRequest("/Movie/Delete", Method.Delete);
            request.AddQueryParameter("movieId", "invalid_id_123");

            var response = client.Execute(request);
            var content = JsonConvert.DeserializeObject<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(content.Msg, Is.EqualTo("Unable to delete the movie! Check the movieId parameter or user verification!"));
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            this.client?.Dispose();
        }
    }
}