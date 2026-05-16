using System.Text.Json;
using JinnDev.JCMU.SDK.Interfaces;
using JinnDev.JCMU.SDK.Models;
using JinnDev.Utilities.Monad;
using JinnDev.Utilities.CommandLine;

namespace JinnDev.JCMU.Addon.GitInit;

public class GitInitAddon : IJcmuAddon
{
    public Maybe<PluginManifest> GetManifest()
    {
        return Maybe.Try<PluginManifest>(() =>
        {
            var assemblyLocation = typeof(GitInitAddon).Assembly.Location;
            var pluginDirectory = Path.GetDirectoryName(assemblyLocation)
                                  ?? throw new Exception("Could not determine plugin directory.");

            var manifestPath = Path.Combine(pluginDirectory, "manifest.json");

            if (!File.Exists(manifestPath))
                throw new FileNotFoundException($"Manifest not found at {manifestPath}");

            var json = File.ReadAllText(manifestPath);
            var manifest = JsonSerializer.Deserialize<PluginManifest>(json)
                           ?? throw new Exception("Failed to deserialize manifest.json");

            return manifest;
        });
    }

    public Maybe<MenuDefinition> GetMenuRegistration()
    {
        return Maybe.Some(new MenuDefinition
        {
            MenuItemName = "Initialize Git Repository",
            Ordinal = 10,
            Category = "Git Tools",
            RunInBackground = false,
            SubItems = null
        });
    }

    public async Task<Maybe> ExecuteAsync(ActionContext context)
    {
        var logger = context.HostServices.Logger;
        logger.LogInfo($"Starting 'git init' in: {context.TargetDirectory}");

        // 1. Build the command request targeting the folder the user right-clicked on
        var request = CommandBuilder.Create("git")
            .WithArgument("init")
            .InDirectory(context.TargetDirectory)
            .Build();

        // 2. Instantiate the stateless runner
        var runner = new StatelessRunner();

        // 3. Execute the command and process the result
        return await runner.RunBufferedAsync(request)
            .BindAsync(cmdResult =>
            {
                // ExitCode 0 means the OS successfully executed git init
                if (cmdResult.ExitCode == 0)
                {
                    logger.LogInfo(cmdResult.StandardOutput);

                    // We wait for a moment just so the user can read the success message 
                    // before the console window automatically closes.
                    Thread.Sleep(1500);

                    return Task.FromResult(Maybe.SUCCESS);
                }

                // If git failed (e.g., git is not installed, or permission denied)
                var errorMsg = string.IsNullOrWhiteSpace(cmdResult.StandardError)
                    ? cmdResult.StandardOutput
                    : cmdResult.StandardError;

                logger.LogError($"Git failed (Exit Code {cmdResult.ExitCode}): {errorMsg}");
                return Task.FromResult(Maybe.Fail(errorMsg));
            }).ConfigureAwait(false);
    }
}