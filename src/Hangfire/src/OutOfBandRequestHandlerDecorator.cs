using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Fusonic.Extensions.Hangfire.Internal;
using Hangfire;
using MediatR;

namespace Fusonic.Extensions.Hangfire
{
    public class OutOfBandRequestHandlerDecorator<TRequest> : IRequestHandler<TRequest, Unit> where TRequest : IRequest
    {
        private readonly IRequestHandler<TRequest, Unit> inner;
        private readonly IBackgroundJobClient client;

        public OutOfBandRequestHandlerDecorator(IRequestHandler<TRequest, Unit> inner, IBackgroundJobClient client)
        {
            this.inner = inner;
            this.client = client;
        }

        public Task<Unit> Handle(TRequest request, CancellationToken cancellationToken)
        {
            EnqueueHangfireJob(inner.GetType(), request);
            return Task.FromResult(Unit.Value);
        }

        private void EnqueueHangfireJob(Type handler, object command)
        {
            var job = new HangfireJob
            {
                Message = command,
                HandlerType = handler.AssemblyQualifiedName,
                Culture = CultureInfo.CurrentCulture,
                UiCulture = CultureInfo.CurrentUICulture
            };

            client.Enqueue<JobProcessor>(c => c.ProcessAsync(job));
        }
    }
}