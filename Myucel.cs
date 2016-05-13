using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace MyucelEngine
{
    public static class Myucel
    {
        public static int ScoreWeightEpisode = 3;
        public static int ScoreWeightDiscussKeyword = 1;
        public static int ScoreWeightAnimeTitle = 1;
        public static int ScoreWeightSpoilerKeyword = 1;

        /// <summary>
        /// Search Reddit's /r/anime for an anime discussion thread.
        /// </summary>
        /// <param name="animeTitle">Title of the anime to look for</param>
        /// <param name="episode">Episode title of the anime to look for, eg. "OVA"</param>
        /// <returns>
        /// A list of FindSubmissionResult object containining 
        /// the title of the submission, the URL of the submission,
        /// and a certainty value between 0.0 and 1.0. 
        /// Number of submissions returned depends on Reddit's search API default,
        /// which as of July 2015 is 25 submissions.
        /// </returns>
        public static List<FindSubmissionResult> FindSubmission(string animeTitle, string episode)
        {
            if (string.IsNullOrWhiteSpace(animeTitle)) throw new ArgumentException("Anime title cannot be empty.");
            var queryString = QueryString(animeTitle, episode);
            var response = QueryReddit(queryString);
            var result = ParseJsonResponse(response);
            var results = CalcuateScoreAndSort(result, animeTitle, episode);
            return results;
        }

        /// <summary>
        /// Search Reddit's /r/anime for an anime discussion thread.
        /// </summary>
        /// <param name="animeTitle">Title of the anime to look for</param>
        /// <param name="episodeNumber">Episode number to look for</param>
        /// <returns>
        /// A list of FindSubmissionResult object containining 
        /// the title of the submission, the URL of the submission,
        /// and a certainty value between 0.0 and 1.0. 
        /// Number of submissions returned depends on Reddit's search API default,
        /// which as of July 2015 is 25 submissions.
        /// </returns>
        public static List<FindSubmissionResult> FindSubmission(string animeTitle, int episodeNumber)
        {
            if (string.IsNullOrWhiteSpace(animeTitle)) throw new ArgumentException("Anime title cannot be empty.");
            var queryString = QueryString(animeTitle, episodeNumber);
            var response = QueryReddit(queryString);
            var result = ParseJsonResponse(response);
            var results = CalcuateScoreAndSort(result, animeTitle, episodeNumber);
            return results;
        }

        private static string QueryString(string animeTitle, string episode)
        {
            return string.Format("{0} {1} Discussion", animeTitle, episode);
        }

        private static string QueryString(string animeTitle, int episodeNumber)
        {
            return QueryString(animeTitle, episodeNumber.ToString());
        }

        private static Uri QueryEndpoint
        {
            get
            {
                return new Uri("http://www.reddit.com/r/anime/search.json");
            }
        }

        private static string QueryUrlParameters(string queryString)
        {
            return string.Format("?q=\"{0}\"&restrict_sr=true", queryString);
        }

        private static string QueryReddit(string queryString)
        {
            var client = new HttpClient { BaseAddress = QueryEndpoint };
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.UserAgent.ParseAdd("u-gunt3001-myucel-library/1.0");

            var response = client.GetAsync(QueryUrlParameters(queryString)).Result;
            return response.IsSuccessStatusCode ? response.Content.ReadAsStringAsync().Result : null;
        }

        private static JObject ReadJsonResponse(string response)
        {
            return JObject.Parse(response);
        }

        private static IEnumerable<FindSubmissionResult> ParseJsonResponse(string response)
        {
            var responseObject = ReadJsonResponse(response);
            return responseObject["data"]["children"].Select(thread => new FindSubmissionResult
            {
                Title = (string) thread["data"]["title"], 
                Link = new Uri((string) thread["data"]["url"])
            });
        }

        private static List<FindSubmissionResult> CalcuateScoreAndSort(IEnumerable<FindSubmissionResult> results, string animeTitle, string episode)
        {
            var sorted = results.ToList();
            foreach (var findSubmissionResult in sorted)
            {
                findSubmissionResult.Certainty = CalculateScore(findSubmissionResult.Title, animeTitle, episode);
            }
            return sorted.OrderByDescending(x => x.Certainty).ToList();
        }

        private static List<FindSubmissionResult> CalcuateScoreAndSort(IEnumerable<FindSubmissionResult> results, string animeTitle, int episodeNumber)
        {
            var sorted = results.ToList();
            foreach (var findSubmissionResult in sorted)
            {
                findSubmissionResult.Certainty = CalculateScore(findSubmissionResult.Title, animeTitle, episodeNumber);
            }
            return sorted.OrderByDescending(x => x.Certainty).ToList();
        }

        private static float CalculateScore(string threadTitle, string animeTitle, string episode)
        {
            var score = 0;

            score += FindTitle(threadTitle, animeTitle) ? ScoreWeightAnimeTitle : 0;
            score += FindEpisode(threadTitle, episode) ? ScoreWeightEpisode : 0;
            score += FindDiscussionKeyword(threadTitle) ? ScoreWeightDiscussKeyword : 0;
            score += FindSpoilerKeyword(threadTitle) ? ScoreWeightSpoilerKeyword : 0;

            var certainty = (float)score / (ScoreWeightEpisode + ScoreWeightDiscussKeyword + ScoreWeightAnimeTitle + ScoreWeightSpoilerKeyword);

            return certainty;
        }

        private static float CalculateScore(string threadTitle, string animeTitle, int episodeNumber)
        {
            var score = 0;

            score += FindTitle(threadTitle, animeTitle) ? ScoreWeightAnimeTitle : 0;
            score += FindEpisode(threadTitle, episodeNumber) ? ScoreWeightEpisode : 0;
            score += FindDiscussionKeyword(threadTitle) ? ScoreWeightDiscussKeyword : 0;
            score += FindSpoilerKeyword(threadTitle) ? ScoreWeightSpoilerKeyword : 0;

            var certainty = (float)score / (ScoreWeightEpisode + ScoreWeightDiscussKeyword + ScoreWeightAnimeTitle + ScoreWeightSpoilerKeyword);

            return certainty;
        }

        private static bool FindEpisode(string threadTitle, string episode)
        {
            return threadTitle.Contains(episode, StringComparison.OrdinalIgnoreCase);
        }

        private static bool FindEpisode(string threadTitle, int episodeNumber)
        {
            var epNumWithLeadingSpace = " " + episodeNumber;
            var epNumWithLeadingZero = " 0" + episodeNumber;
            return threadTitle.Contains(epNumWithLeadingSpace) || threadTitle.Contains(epNumWithLeadingZero);
        }

        private static bool FindTitle(string threadTitle, string animeTitle)
        {
            return threadTitle.Contains(animeTitle, StringComparison.OrdinalIgnoreCase);
        }

        private static bool FindDiscussionKeyword(string threadTitle)
        {
            return threadTitle.Contains("discussion", StringComparison.OrdinalIgnoreCase);
        }

        private static bool FindSpoilerKeyword(string threadTitle)
        {
            return threadTitle.Contains("spoiler", StringComparison.OrdinalIgnoreCase);
        }
    }
}
