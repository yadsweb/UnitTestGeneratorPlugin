using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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

            //var text = new StringBuilder();
            //text.Append(" directory: " + Directory.GetCurrentDirectory());

            var fileMap = new ExeConfigurationFileMap
            {
                ExeConfigFilename = Directory.GetCurrentDirectory() + @"\App.config"
            };

            var appConfig = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);

            //text.Append(" Custom.plugin.generator.configuration value: " + appConfig.AppSettings.Settings["Custom.plugin.generator.configuration"].Value);

            if (appConfig.AppSettings.Settings["Custom.plugin.generator.configuration"] == null)
            {
                foreach (var scenario in feature.Scenarios)
                {
                    scenario.Tags = scenario.Tags ?? new Tags();
                    scenario.Tags.Add(new Tag("uniqueId:" + GenerateuniqueId()));
                }
            }
            else
            {
                try
                {
                    //text.AppendLine(appConfig.Sections["GeneratorPluginConfiguration"].CurrentConfiguration.ToString());
                    //text.AppendLine("--------===-----------");
                    var additionalCategoryAttributes = GeneratorPluginConfiguration.GetConfig(appConfig);
                    //text.AppendLine("-------------------------");
                    var additionalCategoryAttributes1 = ((GeneratorPluginConfiguration)additionalCategoryAttributes).AdditionalCategoryAttributes;
                    //text.AppendLine("++++++++++");
                    //var additionalTestCaseAttributes =
                    //    GeneratorPluginConfiguration.GetConfig(appConfig).AdditionalTestCaseAttributes;
                    //var additionalStesps =
                    //    GeneratorPluginConfiguration.GetConfig(appConfig).AdditionalCategoryAttributes;

                    //foreach (var scenario in feature.Scenarios)
                    //{
                    //    scenario.Tags = scenario.Tags ?? new Tags();

                    //    text.Append("AdditionalCategoryAttribute: " + additionalCategoryAttributes.Count);

                    //    if (additionalCategoryAttributes.Count > 0)
                    //    {
                    //        for (int counter = 0; counter < additionalCategoryAttributes.Count; counter++)
                    //        {
                    //            scenario.Tags.Add(additionalCategoryAttributes[counter].Type.Contains("unique")
                    //                ? new Tag(additionalCategoryAttributes[counter].Value + GenerateuniqueId())
                    //                : new Tag(additionalCategoryAttributes[counter].Value));
                    //        }
                    //    }
                    //}
                }
                catch (Exception e)
                {

                    //text.Append(e.ToString());
                }
            }
           
            //File.WriteAllText(@"C:\log.txt", text.ToString());
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

