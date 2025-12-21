namespace CodeWinden.CQRS;

/// <summary>
/// Helper class to track handler execution
/// </summary>
public class ExecutionTracker
{
    private readonly List<string> _executions = new List<string>();

    /// <summary>
    /// Records the execution of a handler.
    /// </summary>
    /// <param name="handlerName">The name of the handler that was executed.</param>
    public void RecordExecution(string handlerName)
    {
        _executions.Add(handlerName);
    }

    /// <summary>
    /// Checks if a handler was called.
    /// </summary>
    /// <param name="handlerName">The name of the handler to check.</param>
    /// <returns>True if the handler was called; otherwise, false.</returns>
    public bool WasCalled(string handlerName)
    {
        return _executions.Contains(handlerName);
    }
}
