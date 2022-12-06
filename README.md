# Pull Request Version Comment Action

Github action that can comment on merged pull requests (and any linked issues) to indicate the first beta version to contain the change. 
This uses version numbers derived from git using `tap sdk gitversion` (see [OpenTAP docs](https://doc.opentap.io/Developer%20Guide/Plugin%20Packaging%20and%20Versioning/#git-assisted-versioning))

## Prerequisites

* A GitHub repository that follows a branching model compatible with the gitversion system of OpenTAP (see [OpenTAP docs](https://doc.opentap.io/Developer%20Guide/Plugin%20Packaging%20and%20Versioning/#git-assisted-versioning)).
* A .gitversion file in the root of the git repository

## Usage

To use get comments on merged PRs in your GitHub repository, create a workflow (eg: `.github/workflows/pr-version-comment.yaml` see [Creating a Workflow file](https://help.github.com/en/articles/configuring-a-workflow#creating-a-workflow-file)) with content like below:


```yaml
on:
  push:
    branches:
      - 'main'

# This grants access to the GITHUB_TOKEN so the action can make calls to GitHub's rest API
permissions:
  contents: read
  pull-requests: write
  issues: write

jobs:
  pr-version-comment:
    runs-on: ubuntu-latest
    name: pr-version-comment
    steps:
      - name: Checkout
        uses: actions/checkout@v2
        with:
          # This action needs the entire history of the repository to calculate the version
          fetch-depth: 0
      - name: Run comment action
        uses: docker://ghcr.io/asgeriversen/pr-version-comment:v1
        env:
          token: ${{ secrets.GITHUB_TOKEN }}
          # (Optional) Content of the comment to add to a merged pull request. Use {version} 
          # to insert the version number.
          body: This change is part of version `{version}` or later.
          # (Optional) Content of the comment to add to an issue that was closed as a 
          # consequence of a PR merge. Use {version} to insert the version number.
          issue-body: A fix for this is in version `{version}` or later.
          # (Optional) Directory to look for .gitversion file in. Relative to the git
          # root directory.
          working-directory: subFolder
```

### Legacy (slower) way of using this action

```yaml
      - name: Run comment action
        uses: AsgerIversen/pr-version-comment
        with:
          token: ${{ secrets.GITHUB_TOKEN }}
          # (Optional) Content of the comment to add to a merged pull request. Use {version} 
          # to insert the version number.
          body: This change is part of version `{version}` or later.
          # (Optional) Content of the comment to add to an issue that was closed as a 
          # consequence of a PR merge. Use {version} to insert the version number.
          issue-body: A fix for this is in version `{version}` or later.
          # (Optional) Directory to look for .gitversion file in. Relative to the git
          # root directory.
          working-directory: subFolder
```
