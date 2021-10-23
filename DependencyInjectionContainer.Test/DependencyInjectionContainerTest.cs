using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DependencyInjectionContainer.DependenciesConfiguration;
using DependencyInjectionContainer.DependenciesConfiguration.ImplementationData;
using DependencyInjectionContainer.DependencyProvider;
using Moq;
using Newtonsoft.Json;
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

                    },
                    {
                        typeof(IRep),
                        new List<ImplementationsContainer>
                        {
                            new ImplementationsContainer(typeof(Rep), ImplementationsTTL.InstancePerDependency,
                                ServiceImplementationNumber.None)
                        }

                    }
                });
            this._dependencyProvider = new DependencyProvider.DependencyProvider(config);
        }

        [Test]
        public void Resolve_GetSimpleImplementation_CorrectResults()
        {
            var excepted = JsonConvert.SerializeObject((new Letter()));

            var result = JsonConvert.SerializeObject(this._dependencyProvider.Resolve<IMessageSender>());

            Assert.AreEqual(excepted, result);
        }

        [Test]
        public void Resolve_GetGenericImplementation_CorrectResults()
        {
            var expectedObject = new DependencyInjectionContainer.Ex<DependencyInjectionContainer.IRep>(new DependencyInjectionContainer.Rep());
            var excepted = JsonConvert.SerializeObject(expectedObject);

            var result = JsonConvert.SerializeObject(this._dependencyProvider.Resolve<IInterface<IRep>>());

            Assert.AreEqual(excepted, result);
        }

        [Test]
        public void Resolve_GetIEnumerable_CorrectResults()
        {
            IEnumerable<DependencyInjectionContainer.IMessageSender> expectedObject = new List<DependencyInjectionContainer.IMessageSender>()
            {
                new DependencyInjectionContainer.Email(new DependencyInjectionContainer.Rep()),
                new DependencyInjectionContainer.Letter()
            };
            var excepted = JsonConvert.SerializeObject(expectedObject);

            var result = JsonConvert.SerializeObject(this._dependencyProvider.Resolve(typeof(IEnumerable<IMessageSender>)));

            Assert.AreEqual(excepted, result);
        }
    }
}
