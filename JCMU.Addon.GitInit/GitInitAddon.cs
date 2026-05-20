using JinnDev.JCMU.SDK.Interfaces;
using JinnDev.JCMU.SDK.Models;
using JinnDev.Utilities.Monad;
using JinnDev.Utilities.CommandLine;

namespace JinnDev.JCMU.Addon.GitInit;

public class GitInitAddon : IJcmuAddon
{
    public async Task<Maybe<int>> ExecuteAsync(ActionContext context)
    {
        var host = context.HostServices;
        var runner = new StatelessRunner();

        var request = CommandBuilder.Create("git")
            .WithArgument("init")
            .InDirectory(context.TargetDirectory)
            .Build();

        // Use the new UI for a colored header!
        host.UI.WriteLine("--- Initializing Git Repository ---", ConsoleColor.Cyan);

        return await runner.RunBufferedAsync(request)
            .BindAsync(result =>
            {
                if (result.ExitCode == 0)
                    return Maybe.SUCCESS;

                var errorMsg = string.IsNullOrWhiteSpace(result.StandardError)
                    ? result.StandardOutput
                    : result.StandardError;

                if (string.IsNullOrWhiteSpace(errorMsg))
                    errorMsg = $"Process exited with code {result.ExitCode} but provided no output.";

                return Maybe.Fail(errorMsg);
            })
            .TapAsync(
                async success =>
                {
                    // Green for the user, standard info for the telemetry log file
                    host.UI.WriteLine("\n[SUCCESS] Git initialized successfully.", ConsoleColor.Green);
                    host.Logger.LogInfo("Git initialized successfully.");
                    await Task.Delay(1500).ConfigureAwait(false);
                },
                async failure =>
                {
                    // Red for the user, error block for the telemetry log file
                    host.UI.WriteLine($"\n[FAILED] Git execution failed:\n{failure.Message}", ConsoleColor.Red);
                    host.Logger.LogError($"Git execution failed: {failure.Message}");
                    await Task.Delay(3000).ConfigureAwait(false);
                })
            .WithValueAsync(() => -1) // -1 ensures the console stays open until they press a key
            .ConfigureAwait(false);
    }
}