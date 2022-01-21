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
        string ver = GetVersion(pr.MergeCommitSha);
        Console.WriteLine($"::set-output name=version::{ver}");
    }
}


string GetVersion(string sha)
{
    var l = new VersionLogListener();
    OpenTap.Log.AddListener(l);
    var test = OpenTap.PluginManager.DirectoriesToSearch;
    if (test.Count > 0)
    {
        var action = new OpenTap.Package.GitVersionAction();
        action.Sha = sha;
        action.Execute(CancellationToken.None);
        OpenTap.Log.RemoveListener(l);
        return l.Version?.ToString();
    }
    return null;
}


class VersionLogListener : OpenTap.Diagnostic.ILogListener
{
    public OpenTap.SemanticVersion Version { get; set; }
    public void EventsLogged(IEnumerable<Event> Events)
    {
        foreach (Event e in Events)
        {
            if(e.Source == "GitVersion" && OpenTap.SemanticVersion.TryParse(e.Message, out var ver))
            {
                Version = ver;
            }
        }
    }

    public void Flush()
    {
    }
}