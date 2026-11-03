using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Homework2.Api.Interceptors;

public class LoggingInterceptor : Interceptor
{
    private readonly ILogger<LoggingInterceptor> _logger;

    public LoggingInterceptor(ILogger<LoggingInterceptor> logger)
    {
        _logger = logger;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        _logger.LogInformation("gRPC {Method} request: {Request}", context.Method, request);
        try
        {
            var response = await continuation(request, context);
            _logger.LogInformation("gRPC {Method} response: {Response}", context.Method, response);
            return response;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "gRPC {Method} failed", context.Method);
            throw;
        }
    }

    public override async Task ServerStreamingServerHandler<TRequest, TResponse>(
        TRequest request,
        IServerStreamWriter<TResponse> responseStream,
        ServerCallContext context,
        ServerStreamingServerMethod<TRequest, TResponse> continuation)
    {
        _logger.LogInformation("gRPC {Method} request: {Request}", context.Method, request);
        try
        {
            var loggingStream = new LoggingStreamWriter<TResponse>(responseStream, _logger, context.Method);
            await continuation(request, loggingStream, context);
            _logger.LogInformation("gRPC {Method} response stream completed", context.Method);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "gRPC {Method} failed", context.Method);
            throw;
        }
    }

    private sealed class LoggingStreamWriter<T> : IServerStreamWriter<T>
    {
        private readonly IServerStreamWriter<T> _inner;
        private readonly ILogger _logger;
        private readonly string _method;

        public LoggingStreamWriter(IServerStreamWriter<T> inner, ILogger logger, string method)
        {
            _inner = inner;
            _logger = logger;
            _method = method;
        }

        public WriteOptions? WriteOptions
        {
            get => _inner.WriteOptions;
            set => _inner.WriteOptions = value;
        }

        public async Task WriteAsync(T message)
        {
            await _inner.WriteAsync(message);
            _logger.LogInformation("gRPC {Method} response: {Response}", _method, message);
        }
    }
}
