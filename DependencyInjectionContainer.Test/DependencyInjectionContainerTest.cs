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
        private static IEnumerable<TestCaseData> NamedConfigCaseData
        {
            get
            {
                yield return new TestCaseData(new Dictionary<Type, List<ImplementationsContainer>>()
                {
                    {
                        typeof(IMessageSender),
                        new List<ImplementationsContainer>()
                        {
                            new ImplementationsContainer(typeof(Email), ImplementationsTTL.InstancePerDependency,
                                ServiceImplementationNumber.First),
                            new ImplementationsContainer(typeof(Letter), ImplementationsTTL.Singleton,
                                ServiceImplementationNumber.Second)
                        }
                    },
                    {
                        typeof(IInterface<>),
                        new List<ImplementationsContainer>
                        {
                            new ImplementationsContainer(typeof(Ex<>), ImplementationsTTL.InstancePerDependency,
                                ServiceImplementationNumber.Second)
                        }

                    },
                    {
                        typeof(IRep),
                        new List<ImplementationsContainer>
                        {
                            new ImplementationsContainer(typeof(Rep), ImplementationsTTL.Singleton,
                                ServiceImplementationNumber.First)
                        }
                    }
                });
            }
        }
        private static IEnumerable<TestCaseData> NamedSingletonConfigCaseData
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
                                ServiceImplementationNumber.First),
                            new ImplementationsContainer(typeof(Letter), ImplementationsTTL.Singleton,
                                ServiceImplementationNumber.Second),
                            new ImplementationsContainer(typeof(Chat), ImplementationsTTL.InstancePerDependency,
                                ServiceImplementationNumber.None),
                        }
                    },
                    {
                        typeof(ICloneable),
                        new List<ImplementationsContainer>()
                        {
                            new ImplementationsContainer(typeof(Messanger), ImplementationsTTL.InstancePerDependency,
                                ServiceImplementationNumber.None),
                        }
                    },
                    {
                        typeof(IInterface<>),
                        new List<ImplementationsContainer>
                        {
                            new ImplementationsContainer(typeof(Ex<>), ImplementationsTTL.InstancePerDependency,
                                ServiceImplementationNumber.Second)
                        }

                    },
                    {
                        typeof(IRep),
                        new List<ImplementationsContainer>
                        {
                            new ImplementationsContainer(typeof(Rep), ImplementationsTTL.Singleton,
                                ServiceImplementationNumber.First)
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

        [TestCaseSource(nameof(NamedSingletonConfigCaseData))]
        public void Resolve_GetServiceNumberInConstructor_CorrectResults(Dictionary<Type, List<ImplementationsContainer>> dict)
        {
            Init(dict);
            var expectedInstance = new Messanger(new Email(new Rep()));
            var excepted = JsonConvert.SerializeObject(expectedInstance);
            var actualInstance = this._dependencyProvider.Resolve<ICloneable>();
            var result = JsonConvert.SerializeObject(actualInstance);
            Assert.AreEqual(excepted, result);
        }

        [TestCaseSource(nameof(NamedSingletonConfigCaseData))]
        public void Resolve_GetInstanceWithIEnumerableInConstructor_CorrectResults(Dictionary<Type, List<ImplementationsContainer>> dict)
        {
            Init(dict);
            IEnumerable<IRep> reps = new List<IRep>()
            {
                new Rep()
            };
            var expectedInstance = new Chat(reps);
            var excepted = JsonConvert.SerializeObject(expectedInstance);

            var actualInstance = this._dependencyProvider.Resolve<IMessageSender>();
            var result = JsonConvert.SerializeObject(actualInstance);

            Assert.AreEqual(excepted, result);
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

        [TestCaseSource(nameof(NamedConfigCaseData))]
        public void Resolve_GetNamedInstance_CorrectResult(Dictionary<Type, List<ImplementationsContainer>> dict)
        {
            Init(dict);

            var email = this._dependencyProvider.Resolve<IMessageSender>(ServiceImplementationNumber.First);
            List<IMessageSender> collection = (List<IMessageSender>)this._dependencyProvider.Resolve(typeof(IEnumerable<IMessageSender>));
            var letter = this._dependencyProvider.Resolve<IMessageSender>(ServiceImplementationNumber.Second);
            var letterWithNone = this._dependencyProvider.Resolve<IMessageSender>(ServiceImplementationNumber.None);

            Assert.That(email, Is.Not.EqualTo(new Email(new Rep())));
            Assert.That(letter, Is.EqualTo(collection[1]));
            Assert.That(letterWithNone, Is.EqualTo(collection[1]));
        }

        [TestCaseSource(nameof(NamedSingletonConfigCaseData))]
        public void Resolve_GetSingletonNamedInstance_CorrectResult(Dictionary<Type, List<ImplementationsContainer>> dict)
        {
            Init(dict);

            var email = this._dependencyProvider.Resolve<IMessageSender>(ServiceImplementationNumber.First);
            List<IMessageSender> collection = (List<IMessageSender>)this._dependencyProvider.Resolve(typeof(IEnumerable<IMessageSender>));
            var letter = this._dependencyProvider.Resolve<IMessageSender>(ServiceImplementationNumber.Second);
            var letterWithNone = this._dependencyProvider.Resolve<IMessageSender>(ServiceImplementationNumber.None);
            var rep = this._dependencyProvider.Resolve<IRep>();

            Assert.That(email, Is.EqualTo(collection[0]));
            Assert.That(letter, Is.EqualTo(collection[1]));
            Assert.That(letterWithNone, Is.EqualTo(collection[1]));
            Assert.That(rep, Is.EqualTo((collection[0] as Email)?.Rep));
        }
    }
}
