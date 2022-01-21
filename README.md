# Pull Request Version Comment Action

Github action that can comment on merged pull requests to indicate the first beta version to contain the change. 
This uses version numbers derived from git using `tap sdk gitversion` (see [OpenTAP docs](https://doc.opentap.io/Developer%20Guide/Plugin%20Packaging%20and%20Versioning/#git-assisted-versioning))

## Prerequisites

* A GitHub repository that follows a branching model compatible with the gitversion system of OpenTAP (see [OpenTAP docs](https://doc.opentap.io/Developer%20Guide/Plugin%20Packaging%20and%20Versioning/#git-assisted-versioning)).
* A .gitversion file in the root of the git repository

## Usage

To use get comments on merged PRs in your GitHub repository, add a workflow like below to `.github/workflows/pr-version-comment.yaml`:

```
push:
    branches:
      - 'main'

permissions:
  contents: read
  pull-requests: write

jobs:
  pr-version-comment:
    runs-on: ubuntu-latest
    name: pr-version-comment
    steps:
      - name: Checkout
        uses: actions/checkout@v2
        with:
          # To use this action, you must check out the entire history of the repository
          fetch-depth: 0
      - name: Run comment action
        uses: AsgerIversen/pr-version-comment
        with:
          token: ${{ secrets.GITHUB_TOKEN }}
          body: This change is part of version `{version}` or later.
```
