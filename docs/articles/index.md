# BdziamPak

![BdziamPak Icon](images/Icon_64.png)

BdziamPak is a package management system designed to handle the installation and resolution of packages. This repository contains the core library, api, and unit tests for BdziamPak.

## What

BdziamPak is a step-based package management system that allows for the resolution and installation of packages. It is designed to be extensible, enabling users to define custom processes and steps for package management.

## Why

BdziamPak provides a flexible and modular approach to package management, making it easy to extend and customize the resolution process. This allows developers to tailor the package management system to their specific needs and workflows.
I am planning to develop multiple plugin based solutions, and I needed a simple way to implement package management.

## How

- BdziamPak can be used as-is with built-in steps for resolving other BdziamPak dependencies, installing NuGet packages, and cloning repositories.
- The step-based approach ensures that each part of the resolution process is modular and can be easily extended or replaced.
- It can be as simple or as complex as you need it to be.


## Features

### Package Sources
Sources for packages defined in one, easily hostable json file.

#### Example API with built in client
There is an example api project, that can serve as basis for more complex APIs, with client implemented.
It has a dockerfile, so you can easily deploy it wherever you need.
Keep in mind that you do not need this for Source management.

**You can point to any url containing the json file**


### Package resolution
Resolution is described by a process, which contains multiple steps.
**Steps can have executing conditions, so only the steps needed to resolve specific BdziamPak are executed.**
You can define your own implementation of `IResolveProcessService` to implement custom processes or just use `BdziamPakResolveProcess`. See Docs for more details.

#### Built-in steps
- Cloning Repository by URL (With optional credentials via json file)
- Downloading and extracting NuGet packages with dependencies
- Resolving BdziamPaks which are dependencies of the main resolved bdziamPak

#### Example Clone Repository Step

```csharp
public class CloneRepositoryStep(GitService gitService) : BdziamPakResolveStep
{
    public override string StepName => "CloneRepository";
    public override string StepDescription => "Clones the repository for the package.";

    public override bool CanExecute(ICheckResolveContext context)
    {
        return context.HasMetadata("Repository");
    }

    public override async Task ExecuteAsync(IExecutionResolveContext context)
    {
        context.UpdateStatus("Cloning repository...");
        var repo = context.GetMetadata<BdziamPakRepositoryReference>("Repository")!;
        gitService.CloneRepo(context.ResolveDirectory, repo.Url, repo.CommitHash);
        context.Complete();
    }
}
```

#### Metadata
Each BdziamPakMetadata json can be extended with custom objects.

## Getting Started

### Prerequisites

- .NET 9.0 SDK

### Installation

Clone the repository or install via NuGet:

```bash
dotnet add package BdziamPak
```

### Running Tests

To run the unit tests, use the test runner integrated in your IDE or run the following command in the terminal:

```bash
dotnet test
```

## Usage

### Resolving a Package

To resolve a package, use the `BdziamPakService`:

```csharp
var bdziamPakService = serviceProvider.GetRequiredService<BdziamPakService>();
var progress = new Progress<BdziamPakResolveProgress>();
progress.ProgressChanged += (p, e) => Console.WriteLine($"{e.Percent}, Progress: {e.Message}");

var result = await bdziamPakService.ResolveBdziamPakAsync("packageId", "version", progress);

if (result.Success)
{
    Console.WriteLine(result.Message);
}
else
{
    Console.WriteLine($"Failed to resolve package: {result.Message}");
}
```

## Contributing

Contributions are welcome! Please open an issue or submit a pull request.

## License

This project is licensed under the MPL 2.0 License. See the `LICENSE` file for details.

## Contact

For any inquiries, please contact the repository owner at [pmikstacki](https://github.com/pmikstacki).