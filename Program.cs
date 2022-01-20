using Octokit;
using System.Collections;
using System.Diagnostics;

string owner = "AsgerIversen";
string reponame = "pr-version-comment";

if (args.Length > 0)
{
    owner = Environment.GetEnvironmentVariable("GITHUB_REPOSITORY_OWNER");
    reponame = Environment.GetEnvironmentVariable("GITHUB_REPOSITORY");
    token = args[0];
}
Console.WriteLine("Variables:");
foreach (DictionaryEntry v in Environment.GetEnvironmentVariables())
Console.WriteLine($"  {v.Key}={v.Value}");
var github = new GitHubClient(new ProductHeaderValue("pr-version-comment"));
github.Credentials = new Credentials(token);
var repo = await github.Repository.Get(owner, reponame);
var repoid = repo.Id;

var prs = await github.PullRequest.GetAllForRepository(repoid, new PullRequestRequest { State = ItemStateFilter.Closed });
Console.WriteLine("Closed Pull Requests:");
foreach (var pr in prs)
{
    Console.WriteLine($"  {pr.Id}:{pr.Title}");
    if(pr.Merged)
    {
        Process.Start("tap", $"sdk gitversion {pr.MergeCommitSha}");
    }
}


