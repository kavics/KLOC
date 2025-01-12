using System.CommandLine;
using KLOC8;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection()
    .AddSingleton<IDisk, Disk>()
    .AddSingleton<KlocCommand>()
    .AddSingleton<IFilter, CommonListFilter>()
    .AddSingleton<IFilter, KlocIgnoreFileFilter>()
    .AddSingleton<IFilter, GlobalKlocIgnoreFileFilter>()
    .AddSingleton<IFilterFactory, FilterFactory>()
    .BuildServiceProvider();

var rootCommand = new RootCommand();

var pathArgument = new Argument<string>("path", "Location of source code directory (required).");
rootCommand.AddArgument(pathArgument);

var isContainerOption = new Option<bool>(name: "--isContainer", description: "Enumerate and count sub-directories and displays a name-count pairs in a table.");
isContainerOption.AddAlias("-c");
rootCommand.Add(isContainerOption);

var fileTypesOption = new Option<string>(name: "--fileTypes", description: "Whitelist of enabled file-extensions (e.g. -t .cs,.sql).");
fileTypesOption.AddAlias("-t");
rootCommand.Add(fileTypesOption);

var globalIgnoreOption = new Option<string>(name: "--ignore", description: "Global klocignore file path.");
globalIgnoreOption.AddAlias("-i");
rootCommand.Add(globalIgnoreOption);

rootCommand.SetHandler((path, isContainer, fileTypes, globalKlocIgnorePath) =>
{
    var command = services.GetRequiredService<KlocCommand>();
    command.Execute(path, isContainer, fileTypes, globalKlocIgnorePath);
}, pathArgument, isContainerOption, fileTypesOption, globalIgnoreOption);

await rootCommand.InvokeAsync(args);
