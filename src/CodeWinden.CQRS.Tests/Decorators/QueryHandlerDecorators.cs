namespace CodeWinden.CQRS.Tests.Decorators;

using CodeWinden.CQRS;
using CodeWinden.CQRS.Decorators;
using CodeWinden.CQRS.Tests.Handlers;

public class TestQueryHandlerDecorator :
    BaseDecorator<IQueryHandler<TestQuery, string>>,
    IQueryHandlerDecorator<TestQuery, string>
{
    public Task<string> Handle(TestQuery query, CancellationToken cancellationToken = default)
    {
        return _handler.Handle(new TestQuery
        {
            Id = query.Id + 1
        }, cancellationToken);
    }
}

public class ParameterlessQueryHandlerDecorator :
    BaseDecorator<IQueryHandler<bool>>,
    IQueryHandlerDecorator<bool>
{
    public async Task<bool> Handle(CancellationToken cancellationToken = default)
    {
        return !await _handler.Handle(cancellationToken);
    }
}

public class GenericTestQueryHandlerDecorator<TResult> :
    BaseDecorator<IQueryHandler<TResult>>,
    IQueryHandlerDecorator<TResult>
{
    public Task<TResult> Handle(CancellationToken cancellationToken = default)
    {
        return _handler.Handle(cancellationToken);
    }
}