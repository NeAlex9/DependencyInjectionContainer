using System;
using DependencyInjectionContainer.DependencyProvider;

namespace DependencyInjectionContainer.DependenciesConfiguration.ImplementationData
{
    public class ImplementationsContainer
    {
        public Type ImplementationsType { get; private set; }
        public ImplementationsTTL TimeToLive { get; private set; }
        public ServiceImplementationNumber ImplNumber { get; private set; }

        public ImplementationsContainer(Type implementationsType, ImplementationsTTL timeToLive,
            ServiceImplementationNumber implNumber)
        {
            this.ImplNumber = implNumber;
            this.ImplementationsType = implementationsType;
            this.TimeToLive = timeToLive;
        }
    }
}
