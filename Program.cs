using Octokit;
using System.Collections;

string owner = args[0];
string repo = args[1];
string token = args[2];

Console.WriteLine("Variables:");
foreach (DictionaryEntry v in Environment.GetEnvironmentVariables())
Console.WriteLine($"  {v.Key}={v.Value}");
var github = new GitHubClient(new ProductHeaderValue("LabToHub"));
github.Credentials = new Credentials(token);