namespace CodeWinden.CQRS.Tests;

/// <summary>
/// Context for CQRS service tests.
/// </summary>
public class CQRSServiceTestContext
{
    public required ICQRSService Service { get; init; }
    public required ExecutionTracker Tracker { get; init; }
}
