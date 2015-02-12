using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using TechTalk.SpecFlow.Generator;
using TechTalk.SpecFlow.Generator.Configuration;
using TechTalk.SpecFlow.Generator.UnitTestConverter;
using TechTalk.SpecFlow.Generator.UnitTestProvider;
using TechTalk.SpecFlow.Parser.SyntaxElements;
using TechTalk.SpecFlow.Utils;

namespace UnitTestGeneratorPlugin.Generator.SpecFlowPlugin
{
    class CustomUnitTestFeatureGenerator : UnitTestFeatureGenerator, IFeatureGenerator
    {
        private readonly List<int> _uniqueIdList;
        private readonly Random _rnd;

        protected CustomUnitTestFeatureGenerator(IUnitTestGeneratorProvider testGeneratorProvider,
                CodeDomHelper codeDomHelper,
                GeneratorConfiguration generatorConfiguration,
                IDecoratorRegistry decoratorRegistry)
            : base(testGeneratorProvider, codeDomHelper, generatorConfiguration, decoratorRegistry)
        {
            _uniqueIdList = new List<int>();
            _rnd = new Random();
        }

        public new CodeNamespace GenerateUnitTestFixture(Feature feature, string testClassName, string targetNamespace)
        {
            if (ConfigurationManager.AppSettings["Execution.type"] == null)
            {
                foreach (var scenario in feature.Scenarios)
                {
                    scenario.Tags = scenario.Tags ?? new Tags();
                    scenario.Tags.Add(new Tag("uniqueId:" + GenerateuniqueId()));
                }
            }
            return base.GenerateUnitTestFixture(feature, testClassName, targetNamespace);
        }

        private int GenerateuniqueId()
        {
            var uniqueId = _rnd.Next(0, 999999);
            if (_uniqueIdList.Contains(uniqueId))
            {
                _uniqueIdList.Sort();
                uniqueId = _uniqueIdList.Last() + 1;
            }
            _uniqueIdList.Add(uniqueId);
            return uniqueId;
        }
    }
}

