using Octokit;
using OpenTap.Diagnostic;
using System.Collections;
using System.Diagnostics;

string owner = "AsgerIversen";
string reponame = "pr-version-comment";
string token = "";


if (args.Length > 0)
{
    owner = Environment.GetEnvironmentVariable("GITHUB_REPOSITORY_OWNER");
    reponame = Environment.GetEnvironmentVariable("GITHUB_REPOSITORY").Split("/").Last();
    token = args[0];
}
Console.WriteLine("::group::Variables:");
foreach (DictionaryEntry v in Environment.GetEnvironmentVariables())
    Console.WriteLine($"  {v.Key}={v.Value}");
Console.WriteLine("::endgroup::");
var github = new GitHubClient(new ProductHeaderValue("pr-version-comment"));
github.Credentials = new Credentials(token);
var repo = await github.Repository.Get(owner, reponame);
var repoid = repo.Id;

// find version number for the current commit:
string sha = Environment.GetEnvironmentVariable("GITHUB_SHA");
string version = GetVersion(sha);
Console.WriteLine($"::set-output name=version::{version}");


// Find the PR that created this merge commit (if any)
var prs = await github.PullRequest.GetAllForRepository(repoid, new PullRequestRequest { State = ItemStateFilter.Closed });
PullRequest pullrequest = null;
Console.WriteLine("::group::Searching merged pull requests");
foreach (var pr in prs)
{
    if (pr.Merged)
    {
        if (pr.MergeCommitSha == sha)
        {
            pullrequest = pr;
        }
        Console.WriteLine($"{(pullrequest == pr ? "*" : " ")} PR #{pr.Id} : {pr.Title}");
    }
}
Console.WriteLine("::endgroup::");


if (pullrequest != null)
{
    Console.WriteLine($"Commenting on PR #{pullrequest.Number}.");
    await github.Issue.Comment.Create(repoid, pullrequest.Number, $"This change is part of version {version} or later.");
}
else
{
    Console.WriteLine($"This commit was not a merge commit for a PR. No action taken.");
}

string GetVersion(string sha)
{
    var test = OpenTap.PluginManager.DirectoriesToSearch;
    if (test.Count > 0)
    {
        var l = new VersionLogListener();
        OpenTap.Log.AddListener(l);

        var action = new OpenTap.Package.GitVersionAction();
        action.Sha = sha;
        action.RepoPath = Directory.GetCurrentDirectory();
        Console.WriteLine($"::group::running tap sdk gitversion {sha} --dir {Directory.GetCurrentDirectory()}");
        action.Execute(CancellationToken.None);

        OpenTap.Log.Flush();
        OpenTap.Log.RemoveListener(l);
        Console.WriteLine("::endgroup::");
        return l.Version?.ToString();
    }
    return null;
}


class VersionLogListener : OpenTap.Diagnostic.ILogListener
{
    public OpenTap.SemanticVersion Version { get; private set; }
    public void EventsLogged(IEnumerable<Event> Events)
    {
        foreach (Event e in Events)
        {
            if ((OpenTap.LogEventType)e.EventType == OpenTap.LogEventType.Debug)
                Console.Write($"::debug:: ");
            if ((OpenTap.LogEventType)e.EventType == OpenTap.LogEventType.Warning)
                Console.Write($"::warning:: ");
            if ((OpenTap.LogEventType)e.EventType == OpenTap.LogEventType.Error)
                Console.Write($"::error:: ");

            Console.WriteLine($"{e.Message}");
            if (e.Source == "GitVersion" && OpenTap.SemanticVersion.TryParse(e.Message, out var ver))
            {
                Version = ver;
            }
        }
    }

    public void Flush()
    {
    }
}