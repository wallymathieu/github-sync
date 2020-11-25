# github-sync

Simple program to sync github user or organisation repositories.

## Usage

First make sure to set environment variables with API information

- github_api_username
- github_api_token

To fetch users repositories from `myuser` into `/media/backupdrive/src/myuser`:

```cmd
dotnet run -p ~/src/github-sync/Console/ --dir /media/backupdrive/src/myuser/ --user myuser fetch
```

To fetch organisations repositories from `myorg` into `/media/backupdrive/src/myorg`:

```cmd
dotnet run -p ~/src/github-sync/Console/ --dir /media/backupdrive/src/myorg/ --org myorg fetch
```
