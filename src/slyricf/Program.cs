using slyricf.commands;
using Spectre.Console.Cli;

var app = new CommandApp<FetchLyricCommand>();
await app.RunAsync(args);
