using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DependencyInjectionContainer.DependenciesConfiguration;
using DependencyInjectionContainer.DependenciesConfiguration.ImplementationData;
using DependencyInjectionContainer.DependencyProvider.ConfigValidator;
using Moq;
using NUnit.Framework;

namespace DependencyInjectionContainer.Test
{
    [TestFixture]
    public class ConfigValidatorTest
    {
        private static IEnumerable<TestCaseData> CorrectCaseData
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

                yield return new TestCaseData(new Dictionary<Type, List<ImplementationsContainer>>()
                {
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

        private static IEnumerable<TestCaseData> InvalidCaseData
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
                    }
                });

                yield return new TestCaseData(new Dictionary<Type, List<ImplementationsContainer>>()
                {
                    {
                        typeof(IInterface<>),
                        new List<ImplementationsContainer>
                        {
                            new ImplementationsContainer(typeof(Ex<>), ImplementationsTTL.InstancePerDependency,
                                ServiceImplementationNumber.None)
                        }

                    },
                });
            }
        }

        private IConfigValidator _configValidator;
        private MockRepository _mock;

        private void Init(Dictionary<Type, List<ImplementationsContainer>> dict)
        {
            this._mock = new MockRepository(MockBehavior.Default);
            var config = this._mock
                .Of<IDependenciesConfiguration>()
                .First(elem => elem.DependenciesDictionary == dict);
            this._configValidator = new ConfigValidator(config);
        }

        [TestCaseSource(nameof(CorrectCaseData))]
        public void Validate_CorrectData_ReturnTrue(Dictionary<Type, List<ImplementationsContainer>> dict) 
        {
            this.Init(dict);

            var actual = this._configValidator.Validate();

            Assert.That(actual, Is.EqualTo(true));
        }

        [TestCaseSource(nameof(InvalidCaseData))]
        public void Validate_InvalidData_ReturnFalse(Dictionary<Type, List<ImplementationsContainer>> dict)
        {
            this.Init(dict);

            Assert.IsFalse(this._configValidator.Validate());
        }
    }
}
