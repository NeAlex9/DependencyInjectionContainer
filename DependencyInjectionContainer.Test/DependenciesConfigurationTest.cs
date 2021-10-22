using System;
using System.Collections.Generic;
using DependencyInjectionContainer.DependenciesConfiguration;
using DependencyInjectionContainer.DependenciesConfiguration.ImplementationData;
using NUnit.Framework;
using Newtonsoft.Json;

namespace DependencyInjectionContainer.Test
{
    [TestFixture]
    public class DependenciesConfigurationTest
    {
        private DependenciesConfiguration.DependenciesConfiguration _configuration;

        [SetUp]
        public void Init()
        {
            this._configuration = new DependenciesConfiguration.DependenciesConfiguration();
        }

        [Test]
        public void Register_WithCommonInterface_CorrectResult()
        {
            var expected = new Dictionary<Type, List<ImplementationsContainer>>()
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
                }
            };

            this._configuration.Register<IMessageSender, Email>(ImplementationsTTL.Singleton);
            this._configuration.Register<IMessageSender, Letter>(ImplementationsTTL.InstancePerDependency);
            var expectedJSon = JsonConvert.SerializeObject(this._configuration.DependenciesDictionary);
            var result = JsonConvert.SerializeObject(expected);

            Assert.AreEqual(expectedJSon, result);
        }

        [Test]
        public void Register_WithDifferentInterface_CorrectResult()
        {
            var expected = new Dictionary<Type, List<ImplementationsContainer>>()
            {
                {
                    typeof(IMessageSender),
                    new List<ImplementationsContainer>(){
                        new ImplementationsContainer(typeof(Email), ImplementationsTTL.Singleton,
                            ServiceImplementationNumber.None),
                    }
                },
                {
                    typeof(IRep),
                    new List<ImplementationsContainer>
                    {
                        new ImplementationsContainer(typeof(Rep), ImplementationsTTL.Singleton,
                            ServiceImplementationNumber.None)
                    }
                }
            };

            this._configuration.Register<IMessageSender, Email>(ImplementationsTTL.Singleton);
            this._configuration.Register<IRep, Rep>(ImplementationsTTL.Singleton);
            var expectedJSon = JsonConvert.SerializeObject(this._configuration.DependenciesDictionary);
            var result = JsonConvert.SerializeObject(expected);

            Assert.AreEqual(expectedJSon, result);
        }

        [Test]
        public void Register_WithCommonImplementation_ReplaceInContainer()
        {
            var expected = new Dictionary<Type, List<ImplementationsContainer>>()
            {
                {
                    typeof(IMessageSender),
                    new List<ImplementationsContainer>
                    {
                        new ImplementationsContainer(typeof(Email), ImplementationsTTL.Singleton,
                            ServiceImplementationNumber.None)
                    }
                },
                {
                typeof(IRep),
                new List<ImplementationsContainer>
                {
                    new ImplementationsContainer(typeof(Rep), ImplementationsTTL.Singleton,
                        ServiceImplementationNumber.None)
                }
            }
            };

            this._configuration.Register<IMessageSender, Email>(ImplementationsTTL.Singleton);
            this._configuration.Register<IRep, Rep>(ImplementationsTTL.Singleton);
            this._configuration.Register<IMessageSender, Email>(ImplementationsTTL.Singleton);
            var expectedJSon = JsonConvert.SerializeObject(this._configuration.DependenciesDictionary);
            var result = JsonConvert.SerializeObject(expected);

            Assert.AreEqual(expectedJSon, result);
        }

        [Test]
        public void Register_WithCommonImplementationDifferentTTL_ReplaceInContainer()
        {
            var expected = new Dictionary<Type, List<ImplementationsContainer>>()
            {
                {
                    typeof(IMessageSender),
                    new List<ImplementationsContainer>
                    {
                        new ImplementationsContainer(typeof(Email), ImplementationsTTL.InstancePerDependency,
                            ServiceImplementationNumber.None)
                    }
                },
                {
                    typeof(IRep),
                    new List<ImplementationsContainer>
                    {
                        new ImplementationsContainer(typeof(Rep), ImplementationsTTL.Singleton,
                            ServiceImplementationNumber.None)
                    }
                }
            };

            this._configuration.Register<IMessageSender, Email>(ImplementationsTTL.Singleton);
            this._configuration.Register<IRep, Rep>(ImplementationsTTL.Singleton);
            this._configuration.Register<IMessageSender, Email>(ImplementationsTTL.InstancePerDependency);
            var expectedJSon = JsonConvert.SerializeObject(this._configuration.DependenciesDictionary);
            var result = JsonConvert.SerializeObject(expected);

            Assert.AreEqual(expectedJSon, result);
        }
    }
}
