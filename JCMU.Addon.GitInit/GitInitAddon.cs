using JinnDev.JCMU.SDK.Interfaces;
using JinnDev.JCMU.SDK.Models;
using JinnDev.Utilities.Monad;
using JinnDev.Utilities.CommandLine;

namespace JinnDev.JCMU.Addon.GitInit;

public class GitInitAddon : IJcmuAddon
{
    public async Task<Maybe> ExecuteAsync(ActionContext context)
    {
        var logger = context.HostServices.Logger;
        logger.LogInfo($"Starting 'git init' at: {context.TargetDirectory}");

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