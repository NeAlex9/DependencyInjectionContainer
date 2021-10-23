using System;
using System.Collections;
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
        private static IEnumerable<TestCaseData> CaseData
        {
            get
            {
                yield return new TestCaseData(new Dictionary<Type, List<ImplementationsContainer>>() 
                {
                        {
                            typeof(IMessageSender),
                            new List<ImplementationsContainer>()
                            {
                                new ImplementationsContainer(typeof(Email), ImplementationsTTL.Singleton,
                                    ServiceImplementationNumber.None),
                                new ImplementationsContainer(typeof(Letter), ImplementationsTTL.Singleton,
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
            }
        }
        private IDependencyProvider _dependencyProvider;
        private MockRepository _mock;

        private void Init(Dictionary<Type, List<ImplementationsContainer>> dict)
        {
            this._mock = new MockRepository(MockBehavior.Default);
            var config = this._mock
                .Of<IDependenciesConfiguration>()
                .First(elem => elem.DependenciesDictionary == dict);
            this._dependencyProvider = new DependencyProvider.DependencyProvider(config);
        }

        [TestCaseSource(nameof(CaseData))]
        public void Resolve_GetSimpleImplementation_CorrectResults(Dictionary<Type, List<ImplementationsContainer>> dict)
        {
            Init(dict);
            var excepted = JsonConvert.SerializeObject((new Letter()));

            var result = JsonConvert.SerializeObject(this._dependencyProvider.Resolve<IMessageSender>());

            Assert.AreEqual(excepted, result);
        }

        [TestCaseSource(nameof(CaseData))]
        public void Resolve_GetGenericImplementation_CorrectResults(Dictionary<Type, List<ImplementationsContainer>> dict)
        {
            Init(dict);
            var expectedObject = new Ex<IRep>(new Rep());
            var excepted = JsonConvert.SerializeObject(expectedObject);

            var result = JsonConvert.SerializeObject(this._dependencyProvider.Resolve<IInterface<IRep>>());

            Assert.AreEqual(excepted, result);
        }

        [TestCaseSource(nameof(CaseData))]
        public void Resolve_GetIEnumerable_CorrectResults(Dictionary<Type, List<ImplementationsContainer>> dict)
        {
            Init(dict);
            IEnumerable<IMessageSender> expectedObject = new List<IMessageSender>()
            {
                new Letter(),
                new Email(new Rep()),
            };
            var excepted = JsonConvert.SerializeObject(expectedObject);

            var letter = this._dependencyProvider.Resolve<IMessageSender>();
            IEnumerable collection = (IEnumerable<IMessageSender>)this._dependencyProvider.Resolve(typeof(IEnumerable<IMessageSender>));
            var result = JsonConvert.SerializeObject(collection);
            Assert.AreEqual(excepted, result);
        }

        [TestCaseSource(nameof(CaseData))]
        public void Resolve_GetSingleton_CorrectResults(Dictionary<Type, List<ImplementationsContainer>> dict)
        {
            Init(dict);

            var letter = this._dependencyProvider.Resolve<IMessageSender>();
            var newLetter = this._dependencyProvider.Resolve<IMessageSender>();

            Assert.That(letter, Is.EqualTo(newLetter));
        }

        [TestCaseSource(nameof(CaseData))]
        public void Resolve_GetTransient_CorrectResults(Dictionary<Type, List<ImplementationsContainer>> dict)
        {
            Init(dict);

            var letter = this._dependencyProvider.Resolve<IRep>();
            var newLetter = this._dependencyProvider.Resolve<IRep>();

            Assert.That(letter, Is.Not.EqualTo(newLetter));
        }

        [TestCaseSource(nameof(CaseData))]
        public void Resolve_CheckSingletonInIEnumerable_CorrectResults(Dictionary<Type, List<ImplementationsContainer>> dict)
        {
            Init(dict);

            var letter = this._dependencyProvider.Resolve<IMessageSender>();
            List<IMessageSender> collection = (List<IMessageSender>)this._dependencyProvider.Resolve(typeof(IEnumerable<IMessageSender>));

            Assert.That(letter, Is.EqualTo(collection[0]));
        }
    }
}
