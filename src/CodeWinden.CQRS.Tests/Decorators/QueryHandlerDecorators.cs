namespace CodeWinden.CQRS.Tests.Decorators;

using CodeWinden.CQRS.Decorators;
using CodeWinden.CQRS.Tests.Handlers;

public class TestQueryHandlerDecorator : ICQRSQueryHandlerDecorator<TestQuery, string>
{
    public Task<string> Handle(TestQuery query, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}

public class ParameterlessQueryHandlerDecorator : ICQRSQueryHandlerDecorator<bool>
{
    public Task<bool> Handle(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}

public class GenericTestQueryHandlerDecorator<TResult> : ICQRSQueryHandlerDecorator<TResult>
{
    public Task<TResult> Handle(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}