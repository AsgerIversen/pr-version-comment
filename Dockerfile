# Set the base image as the .NET 5.0 SDK (this includes the runtime)
FROM mcr.microsoft.com/dotnet/sdk:6.0 as build-env

# Copy everything and publish the release (publish implicitly restores and builds)
COPY . ./src
RUN dotnet publish ./src/pr-version-comment.csproj -c Release -o ./out --no-self-contained

# Label the container
LABEL maintainer="Asger Iversen <asger.iversen@gmail.com>"
LABEL repository="https://github.com/AsgerIversen/pr-version-comment"

# Label as GitHub action
LABEL com.github.actions.name="pr-version-comment"
# Limit to 160 characters
LABEL com.github.actions.description="A Github action that adds a comments to a closed PR with a version number derived from the git commit that merged the PR."
# See branding:
# https://docs.github.com/actions/creating-actions/metadata-syntax-for-github-actions#branding
LABEL com.github.actions.icon="git-pull-request"
LABEL com.github.actions.color="orange"

# Relayer the .NET SDK, anew with the build output  
FROM mcr.microsoft.com/dotnet/runtime:6.0
COPY --from=build-env /out /opt/tap
ENTRYPOINT [ "dotnet", "/opt/tap/pr-version-comment.dll" ]
