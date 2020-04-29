using EasyGiveawayPicker.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;

namespace EasyGiveawayPicker.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TweetController : ControllerBase
    {

        [HttpGet("[action]")]
        public TweetModel SelectWinner(string giveawayLink)
        {
            var giveawayId = long.Parse(giveawayLink.Split("/").Last());
            var giveawayTweet = Tweet.GetTweet(giveawayId);
            var tweets = GetAllReplies(giveawayId).ToList();
            RemoveHostReplies(giveawayTweet.CreatedBy.Id, tweets);
            AllowOneReplyPerUser(tweets);

            var rand = new Random();
            var winnerIndex = rand.Next(0, tweets.Count - 1);
            var winnerTweet = tweets[winnerIndex];
            var oEmbedTweet = Tweet.GetOEmbedTweet(winnerTweet.Id);
            return new TweetModel { Html = oEmbedTweet.HTML, Total = tweets.Count, Index = winnerIndex };
        }

        private void AllowOneReplyPerUser(List<ITweet> tweets)
        {
            var usersWithMultipleRepliesIds = tweets.GroupBy(x => x.CreatedBy.Id)
             .Where(g => g.Count() > 1)
             .Select(y => y.Key)
             .ToList();
            foreach (var uId in usersWithMultipleRepliesIds)
            {
                var firstReply = tweets.First(t => t.CreatedBy.Id == uId);
                tweets.RemoveAll(t => t.CreatedBy.Id == uId);
                tweets.Add(firstReply);
            }
        }

        private void RemoveHostReplies(long hostUserId, List<ITweet> tweets)
        {
            tweets.RemoveAll(t => t.CreatedBy.Id == hostUserId);
        }

        private void OnlyFollowingUsers(long hostUserId, List<ITweet> tweets)
        {
            tweets.RemoveAll(t => t.CreatedBy.Id == hostUserId);
        }

        private IEnumerable<ITweet> GetAllReplies(long giveawayId)
        {
            var searchParameter = new SearchTweetsParameters("@louisim-yt")
            {
                SearchType = SearchResultType.Recent,
                MaximumNumberOfResults = 100,
                SinceId = giveawayId
            };
            var allMyTweets = Search.SearchTweets(searchParameter);
            var currentId = allMyTweets.Last().Id;

            while (currentId > giveawayId)
            {
                searchParameter = new SearchTweetsParameters("@louisim-yt")
                {
                    SearchType = SearchResultType.Recent,
                    MaximumNumberOfResults = 100,
                    SinceId = giveawayId,
                    MaxId = currentId
                };
                var search = Search.SearchTweets(searchParameter);
                if (search.Last().Id == currentId)
                {
                    break;
                }
                allMyTweets = allMyTweets.Concat(search);
                currentId = allMyTweets.Last().Id;
            }
            var duplicatedTweets = allMyTweets.GroupBy(x => x.Id)
              .Where(g => g.Count() > 1)
              .Select(y => y.Key)
              .ToList();

            List<ITweet> allMyTweetsList = allMyTweets.ToList();
            foreach (var tId in duplicatedTweets)
            {
                allMyTweetsList.Remove(allMyTweetsList.First(t => t.Id == tId));
            }
            var tweetsInReplyToStatus = allMyTweetsList.Where(t => t.InReplyToStatusId == giveawayId);

            return tweetsInReplyToStatus;
        }

    }
}
