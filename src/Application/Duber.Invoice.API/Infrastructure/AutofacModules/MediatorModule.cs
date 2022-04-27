using Autofac;
using Duber.Invoice.API.Application.DomainEventHandlers;
using MediatR;
using System.Collections.Generic;
using System.Reflection;

namespace Duber.Invoice.API.Infrastructure.AutofacModules
{
    public class MediatorModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {

            Autofac.Builder.IRegistrationBuilder<Mediator, Autofac.Builder.ConcreteReflectionActivatorData, Autofac.Builder.SingleRegistrationStyle> registrationBuilder2 = builder.RegisterType<Mediator>().As<IMediator>();

            // Register all the event classes (they implement INotificationHandler) in assembly holding the Commands
            builder.RegisterAssemblyTypes(typeof(InvoiceCreatedDomainEventHandler).GetTypeInfo().Assembly)
                .AsClosedTypesOf(typeof(INotificationHandler<>)).AsImplementedInterfaces().InstancePerRequest();

            Autofac.Builder.IRegistrationBuilder<ServiceFactory, Autofac.Builder.SimpleActivatorData, Autofac.Builder.SingleRegistrationStyle> registrationBuilder1 = builder.Register<ServiceFactory>(context =>
            {
                IComponentContext componentContext = context.Resolve<IComponentContext>();
                return t =>
                {
                    ServiceFactory p = t => { return componentContext.TryResolve(t, out object o) ? o : null; };
                    return t;
                };
            });

            Autofac.Builder.IRegistrationBuilder<ServiceFactory, Autofac.Builder.SimpleActivatorData, Autofac.Builder.SingleRegistrationStyle> registrationBuilder = builder.Register<ServiceFactory>(context =>
            {
                IComponentContext componentContext = context.Resolve<IComponentContext>();
                return t =>
                {
                    IEnumerable<object> resolved = (IEnumerable<object>)componentContext.Resolve(typeof(IEnumerable<>).MakeGenericType(t));
                    return resolved;
                };
            });
        }

    }
}
