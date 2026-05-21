namespace YAMCqrs.BackgroundWorker.Core.Tests.Core.YABackgroundWorkerTests;

internal static class Helper
{
    /// <summary>
    /// Bloquea el thread actual hasta que el token esté cancelado Y la task esté completada,
    /// o hasta que expire el timeout.
    /// </summary>
    /// <param name="token">Token de cancelación a esperar</param>
    /// <param name="executeTask">Task a esperar (opcional)</param>
    /// <param name="timeout">Tiempo máximo de espera total</param>
    /// <returns>True si ambas condiciones se cumplieron, False si hubo timeout</returns>
    public static bool WaitForWorkerCompletion(
        CancellationToken token,
        Task? executeTask,
        TimeSpan timeout)
    {
        try
        {
            var deadline = DateTime.UtcNow.Add(timeout);

            token.WaitHandle.WaitOne(timeout);
            executeTask?.Wait(timeout);

            return true;
        }
        catch
        {
            return executeTask?.IsCompleted ?? false;
        }
    }

}