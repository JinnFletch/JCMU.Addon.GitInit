using JinnDev.JCMU.SDK.Interfaces;
using JinnDev.JCMU.SDK.Models;
using JinnDev.Utilities.Monad;
using JinnDev.Utilities.CommandLine;

namespace JinnDev.JCMU.Addon.GitInit;

public class GitInitAddon : IJcmuAddon
{
    public async Task<Maybe<int>> ExecuteAsync(ActionContext context)
    {
        var request = CommandBuilder.Create("git").WithArgument("init").InDirectory(context.TargetDirectory).Build();

        var logger = context.HostServices.Logger;
        var runner = new StatelessRunner();
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
                    logger.LogInfo("Git initialized successfully.");
                    await Task.Delay(1500).ConfigureAwait(false);
                },
                async failure =>
                {
                    logger.LogError($"Git execution failed: {failure.Message}");
                    await Task.Delay(3000).ConfigureAwait(false);
                })
            .WithValueAsync(() => 0)
            .ConfigureAwait(false);
    }
}