namespace CodeWinden.CQRS.Tests.Handlers;

// Test queries
public record TestQuery : IQuery<string>;
public record AnotherTestQuery : IQuery<int>;

// Test query handlers
public class TestQueryHandler : IQueryHandler<TestQuery, string>
{
    public Task<string> Handle(TestQuery query, CancellationToken cancellationToken)
    {
        return Task.FromResult("test result");
    }
}

public class AnotherTestQueryHandler : IQueryHandler<AnotherTestQuery, int>
{
    public Task<int> Handle(AnotherTestQuery query, CancellationToken cancellationToken)
    {
        return Task.FromResult(123);
    }
}

public class ParameterlessQueryHandler : IQueryHandler<bool>
{
    public Task<bool> Handle(CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }
}
