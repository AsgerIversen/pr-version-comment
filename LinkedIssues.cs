using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Octokit.GraphQL;
using Octokit.GraphQL.Model;
using static Octokit.GraphQL.Variable;

namespace PRVersionComment
{
    internal class LinkedIssues
    {
        private Connection connection;

        public LinkedIssues()
        {
            var productInformation = new ProductHeaderValue("pr-version-comment", "0.1");
            connection = new Connection(productInformation, Config.Token);
        }

        public async Task<List<int>> GetLinkedIssues(int prNumber)
        {
            var query = new Query()
            .RepositoryOwner(Config.Owner)
            .Repository(Config.RepoName)
            .PullRequest(prNumber)
            .Select(pr => new
            {
                pr.Number,
                IssueNumbers = pr.ClosingIssuesReferences(10,null,null,null,null).Nodes.Select(i => i.Number).ToList()
            }).Compile();

            var result = await connection.Run(query);
            return result.IssueNumbers;
        }
    }
}
