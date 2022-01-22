using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRVersionComment
{
    internal static class Config
    {
        public static string Owner { get;set;} = "AsgerIversen";
        public static string RepoName { get; set; } = "pr-version-comment";
        public static string Token { get; set; } = "";
        public static string Body { get; set; } = "This change is part of version `{version}` or later.";
        public static string IssueBody { get; set; } = "A fix for this is in version `{version}` or later.";
    }
}
