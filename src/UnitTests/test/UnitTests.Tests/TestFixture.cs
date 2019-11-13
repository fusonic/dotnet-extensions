using MediatR;
using MediatR.Pipeline;
using SimpleInjector;

namespace Fusonic.Extensions.UnitTests.Tests
{
    public class TestFixture : UnitTestFixture
    {
        protected override void RegisterDependencies(Container container)
        {
            base.RegisterDependencies(container);

            //Mediator
            var mediatorAssemblies = new[] { typeof(IMediator).Assembly, typeof(TestFixture).Assembly };
            container.Register(() => new ServiceFactory(container.GetInstance), Lifestyle.Singleton);
            container.RegisterSingleton<IMediator, Mediator>();
            container.Register(typeof(IRequestHandler<,>), mediatorAssemblies);
            container.Collection.Register(typeof(INotificationHandler<>), mediatorAssemblies);

            container.Collection.Register(typeof(IPipelineBehavior<,>),
                new[]
                {
                    typeof(RequestPreProcessorBehavior<,>),
                    typeof(RequestPostProcessorBehavior<,>)
                });

            container.Collection.Register(typeof(IRequestPreProcessor<>), mediatorAssemblies);
            container.Collection.Register(typeof(IRequestPostProcessor<,>), mediatorAssemblies);
        }
    }
}