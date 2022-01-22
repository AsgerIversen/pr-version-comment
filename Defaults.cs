using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRVersionComment
{
    internal static class Defaults
    {
        public const string Owner = "AsgerIversen";
        public const string RepoName = "pr-version-comment";
        public const string Token = "";
        public const string Body = "This change is part of version `{version}` or later.";
    }
}
