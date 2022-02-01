using Octokit;
using OpenTap.Diagnostic;
using System.Collections;
using System.Diagnostics;
using PRVersionComment;



if (args.Length > 0)
{

    foreach (var arg in args)
    {
        var option = arg.Split('=').First();
        var value = arg.Substring(2);
        if (String.IsNullOrWhiteSpace(value))
            continue;
        switch (option)
        {
            case "t":
                Config.Token = value;
                break;
            case "p":
                Config.Body = value;
                break;
            case "i":
                Config.IssueBody = value;
                break;
        }
    }
}
else
{
    Config.Token = Environment.GetEnvironmentVariable("token") ?? Config.Token;
    Config.Body = Environment.GetEnvironmentVariable("body") ?? Config.Body;
    Config.IssueBody = Environment.GetEnvironmentVariable("issue-body") ?? Config.IssueBody;
}

if (String.IsNullOrEmpty(Config.Token))
{
    Console.WriteLine("::error:: Missing required github token. See expected usage below for how to pass this in.");
    Console.WriteLine("::error:: Expected usage is is either:");
    Console.WriteLine("::error::   - name: Run comment action");
    Console.WriteLine("::error::     uses: AsgerIversen/pr-version-comment");
    Console.WriteLine("::error::     with:");
    Console.WriteLine("::error::       token: ${{ secrets.GITHUB_TOKEN }}");
    Console.WriteLine("::error:: or:");
    Console.WriteLine("::error::   - name: Run comment action");
    Console.WriteLine("::error::     uses: docker://ghcr.io/asgeriversen/pr-version-comment:main");
    Console.WriteLine("::error::     env:");
    Console.WriteLine("::error::       token: ${{ secrets.GITHUB_TOKEN }}");
}


if (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_REPOSITORY_OWNER")))
{ // These will be set when running in a github action. When debugging, we use the defaults set in Config
    Config.Owner = Environment.GetEnvironmentVariable("GITHUB_REPOSITORY_OWNER");
    Config.RepoName = Environment.GetEnvironmentVariable("GITHUB_REPOSITORY").Split("/").Last();
}

// Console.WriteLine("::group::Variables:");
// foreach (DictionaryEntry v in Environment.GetEnvironmentVariables())
//     Console.WriteLine($"  {v.Key}={v.Value}");
// Console.WriteLine("::endgroup::");

var github = new GitHubClient(new ProductHeaderValue("pr-version-comment"));
github.Credentials = new Credentials(Config.Token);
var repo = await github.Repository.Get(Config.Owner, Config.RepoName);
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
        Console.WriteLine($"{(pullrequest == pr ? "*" : " ")} PR #{pr.Number} : {pr.Title}");
    }
}
Console.WriteLine("::endgroup::");

if (pullrequest != null)
{
    int prNum = pullrequest.Number;
    Console.WriteLine($"Commenting on PR #{prNum}.");
    var body = Config.Body.Replace("{version}", version);
    await github.Issue.Comment.Create(repoid, prNum, body);

    // Also add comments to linked issues
    var issueBody = Config.IssueBody.Replace("{version}", version);
    var linkedIssues = await new LinkedIssues().GetLinkedIssues(prNum);
    foreach (int issueNumber in linkedIssues)
    {
        Console.WriteLine($"Commenting on linked issue #{issueNumber}.");
        await github.Issue.Comment.Create(repoid, issueNumber, issueBody);
    }
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