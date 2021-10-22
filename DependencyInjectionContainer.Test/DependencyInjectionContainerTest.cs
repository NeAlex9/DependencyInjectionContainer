using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DependencyInjectionContainer.DependenciesConfiguration;
using DependencyInjectionContainer.DependenciesConfiguration.ImplementationData;
using DependencyInjectionContainer.DependencyProvider;
using Moq;
using NUnit;
using NUnit.Framework;

namespace DependencyInjectionContainer.Test
{
    [TestFixture]
    public class DependencyInjectionContainerTest
    {
        private IDependencyProvider _dependencyProvider;
        private MockRepository _mock;

        [SetUp]
        public void Init()
        {
            this._mock = new MockRepository(MockBehavior.Default);
            var config = this._mock
                .Of<IDependenciesConfiguration>()
                .First(elem => elem.DependenciesDictionary == new Dictionary<Type, List<ImplementationsContainer>>()
                {
                    {
                        typeof(IMessageSender),
                        new List<ImplementationsContainer>()
                        {
                            new ImplementationsContainer(typeof(Email), ImplementationsTTL.Singleton,
                                ServiceImplementationNumber.None),
                            new ImplementationsContainer(typeof(Letter), ImplementationsTTL.InstancePerDependency,
                                ServiceImplementationNumber.None)
                        }
                    },
                    {
                        typeof(IInterface<>),
                        new List<ImplementationsContainer>
                        {
                            new ImplementationsContainer(typeof(Ex<>), ImplementationsTTL.InstancePerDependency,
                                ServiceImplementationNumber.None)
                        }

                    }
                });
            this._dependencyProvider = new DependencyProvider.DependencyProvider(config);
        }

        public void 
    }
}
