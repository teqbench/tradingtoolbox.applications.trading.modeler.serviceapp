# Trading Postion Modeler API Service Application

## Overview
API service for trading position modeler database operations.

# Contents
- [Developer Environment Setup](#Developer+Environment+Setup)
- [Usage](#Usage)
- [DevOps - Configurations, Builds and Deployments](#DevOps)
- [References](#References)
- [License](#License)

# Developer Environment Setup
> [!NOTE]
> In order to access the TeqBench's package registry on GitHub, a personal access token needs to be created with the appropriate scopes and Visual Studio configured to use it. See the [TeqBench Organization's README](https://github.com/teqbench) which outlines how to create a PAT and configure Visual Studio to use it.

## Tooling
- .NET 7.0.x
- Visual Studio

## Dependencies
> [!NOTE]
> Referenced/restored via the project file

- Microsoft.AspNetCore.JsonPatch, 7.0.12
- Microsoft.AspNetCore.Mvc.NewtonsoftJson, 7.0.12
- Swashbuckle.AspNetCore, 6.5.0
- TeqBench.System.Cors, 1.0.0
- TradingToolbox.Trading.Modeler.Data.NoSql.MongoDB.Models, 1.1.0
- TradingToolbox.Trading.Modeler.Data.NoSql.MongoDB.Services, 1.1.0

# DevOps
## Configurations
- Release
    - This configuration is used for compilation of releases to non-debug environments, i.e. production and preview environments.
- Debug
    - This configuration is used for compilation of releases to development/debug environments.

## Branching Strategy
- GitHub Flow
  - [Introduction From GitHub](https://docs.github.com/en/get-started/quickstart/github-flow)
  - [Indepth Overview](https://githubflow.github.io)

## Branches
- main (production)

## Local - Build, Pack(age) & Deploy
- To build/pack locally use the "Debug" configuration.

### Build
- To create application build locally, can be done either in Visual Studio or command line.
  - Visual Studio
    - Load the project
    - Right-mouse clicking the project file to bring up the context menu and selecting "Build {project's name}".
  - Command Line
    - Open terminal.
    - Navigate to the project's root folder and issue the command "dotnet build -c:Debug".
  - Build Output
    - Build output for Visual Studio or command line, i.e. assembly, will be found in the "{project}/bin/Debug/" folder.
  
### Deployment

## Cloud - Build & Deploy
- Cloud based build and deployment requires a pull request and successful merge into the main branch in order to start the release workflow.
- As part of the pull request, the "Mergable" option must be set to "PR - Allow merge" in order for the pull request to be merged into the main branch, assuming all other validations pass.
- As part of the pull request, the "Release Type" option must be specified (e.g. "Major (Backwards-incompatible updates and/or bugfixes)", "Minor (Backwards-compatible updates and bugfixes)", or "Patch (Backwards-compatible bugfixes - ONLY)") to determine how the version number will be updated as part of the build. See [TeqBench Org's README](https://github.com/teqbench#version-numbers-in-teqbench) for more information on version numbers in TeqBench.

# License
&copy; 2021 TeqBench. All source code in this repository is only allowed for use by TeqBench; other usage by internal or external parties requires written consent from TeqBench.
