namespace CodeWinden.CQRS.Tests.Handlers;

// Test queries
public record TestQuery : IQuery<string>
{
    public required int Id { get; init; }
};

public record AnotherTestQuery : IQuery<int>
{
    public required int Value { get; init; }
};

// Test query handlers
public class TestQueryHandler : IQueryHandler<TestQuery, string>
{
    public Task<string> Handle(TestQuery query, CancellationToken cancellationToken)
    {
        return Task.FromResult($"Result for Id: {query.Id}");
    }
}

public class AnotherTestQueryHandler : IQueryHandler<AnotherTestQuery, int>
{
    public Task<int> Handle(AnotherTestQuery query, CancellationToken cancellationToken)
    {
        return Task.FromResult(query.Value * 2);
    }
}

public class ParameterlessQueryHandler : IQueryHandler<bool>
{
    public Task<bool> Handle(CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }
}
