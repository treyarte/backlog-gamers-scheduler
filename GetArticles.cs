using System;
using backlog_gamers_api.Repositories.Interfaces;
using backlog_gamers_api.Repositories;
using backlog_gamers_api.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace BacklogGamersArticleScheduler
{
    public class GetArticles
    {
        private GamingArticlesService _gamingService = new GamingArticlesService();
        private IArticlesRepository _articlesRepo = new ArticlesRepository("articles");
        private IArticleSourceRepo _sourceRepo = new ArticleSourceRepo("articleSources");
        [FunctionName("GetArticles")]
        public async Task Run([TimerTrigger("0 17 * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.UtcNow}");
            // Test
            int articlesCreated = await GetAndInsertArticles(log);

            log.LogInformation($"Articles created: {articlesCreated}");
        }

        private async Task<int> GetAndInsertArticles(ILogger log)
        {
            try
            {
                var sources = await _sourceRepo.GetAll();
                var articleSources = sources.ToList();

                if (articleSources.Count <= 0)
                {
                    log.LogInformation("Failed to get Articles, no sources");
                }
                var articles = await _gamingService.GetExternalArticles(articleSources);

                int addCount = await _articlesRepo.PostMultiple(articles);

                return addCount;
            }
            catch (Exception e)
            {
                log.LogError(e.Message);
                throw;
            }

        }
    }
}
