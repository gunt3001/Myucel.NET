using System;

namespace MyucelEngine
{
    public class MyucelSettings
    {
        public int ScoreWeightEpisode { get; set; } = 3;
        public int ScoreWeightDiscussKeyword { get; set; } = 1;
        public int ScoreWeightAnimeTitle { get; set; } = 1;
        public int ScoreWeightSpoilerKeyword { get; set; } = 1;

        public Uri SearchEndpoint { get; set; } = new Uri("http://www.reddit.com/r/anime/search.json");
    }
}