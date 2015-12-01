using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using TechTalk.SpecFlow.Generator;
using TechTalk.SpecFlow.Generator.UnitTestProvider;
using TechTalk.SpecFlow.Utils;

namespace UnitTestGeneratorPlugin.Generator.SpecFlowPlugin
{
    class UnitTestGeneratorProviderCustomAttributes : IUnitTestGeneratorProvider
    {
        private NUnitTestGeneratorProvider _nunit;
        private CodeDomHelper _codeDomHelper;

        public UnitTestGeneratorProviderCustomAttributes(CodeDomHelper codeDomHelper)
        {
            _nunit = new NUnitTestGeneratorProvider(codeDomHelper);
            _codeDomHelper = codeDomHelper;
        }

        public void SetTestClass(TestClassGenerationContext generationContext, string featureTitle, string featureDescription)
        {
            _nunit.SetTestClass(generationContext, featureTitle, featureDescription);
        }

        public void SetTestClassCategories(TestClassGenerationContext generationContext, IEnumerable<string> featureCategories)
        {
            _nunit.SetTestClassCategories(generationContext, featureCategories);
        }

        public void SetTestClassIgnore(TestClassGenerationContext generationContext)
        {
            _nunit.SetTestClassIgnore(generationContext);
        }

        public void FinalizeTestClass(TestClassGenerationContext generationContext)
        {
            _nunit.FinalizeTestClass(generationContext);
        }

        public void SetTestClassInitializeMethod(TestClassGenerationContext generationContext)
        {
            _nunit.SetTestClassInitializeMethod(generationContext);
        }

        public void SetTestClassCleanupMethod(TestClassGenerationContext generationContext)
        {
            _nunit.SetTestClassCleanupMethod(generationContext);
        }

        public void SetTestInitializeMethod(TestClassGenerationContext generationContext)
        {
            _nunit.SetTestInitializeMethod(generationContext);
        }

        public void SetTestCleanupMethod(TestClassGenerationContext generationContext)
        {
            _nunit.SetTestCleanupMethod(generationContext);
        }

        public void SetTestMethod(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, string scenarioTitle)
        {
            AddAttributsFromScenarioCategories(generationContext, testMethod, scenarioTitle);
            _nunit.SetTestMethod(generationContext, testMethod, scenarioTitle);
        }

        public void SetTestMethodCategories(TestClassGenerationContext generationContext, CodeMemberMethod testMethod,
            IEnumerable<string> scenarioCategories)
        {
            _nunit.SetTestMethodCategories(generationContext, testMethod, scenarioCategories);
        }

        public void SetTestMethodIgnore(TestClassGenerationContext generationContext, CodeMemberMethod testMethod)
        {
            _nunit.SetTestMethodIgnore(generationContext, testMethod);
        }

        public void SetRowTest(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, string scenarioTitle)
        {

            AddAttributsFromScenarioCategories(generationContext, testMethod, scenarioTitle);
            _nunit.SetRowTest(generationContext, testMethod, scenarioTitle);
        }

        public void SetRow(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, IEnumerable<string> arguments,
            IEnumerable<string> tags, bool isIgnored)
        {
            _nunit.SetRow(generationContext, testMethod, arguments, tags, isIgnored);
        }

        public void SetTestMethodAsRow(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, string scenarioTitle,
            string exampleSetName, string variantName, IEnumerable<KeyValuePair<string, string>> arguments)
        {
            _nunit.SetTestMethodAsRow(generationContext, testMethod, scenarioTitle, exampleSetName, variantName, arguments);
        }

        public bool SupportsRowTests
        {
            get
            {
                return _nunit.SupportsRowTests;

            }
        }

        public bool SupportsAsyncTests
        {
            get
            {
                return _nunit.SupportsAsyncTests;

            }
        }
        public void AddAttributsFromScenarioCategories(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, string scenarioTitle)
        {
            if (generationContext.Feature == null) return;
            if (generationContext.Feature.Scenarios == null) return;
            var scenario = generationContext.Feature.Scenarios.FirstOrDefault(s => s.Title == scenarioTitle);
            if (scenario == null) return;
            if (scenario.Tags == null) return;
            var nunitAttributeTags = scenario.Tags.Where(t => t.Name.ToLower().StartsWith("nunitattribute")).Select(t => t.Name).ToList();
            if (!nunitAttributeTags.Any()) return;
            foreach (var nunitAttributeTag in nunitAttributeTags)
            {
                if (nunitAttributeTag.Contains(":"))
                {
                    var attributeWithParameter = nunitAttributeTag.Substring(14);
                    int number;
                    var parseSuccess = Int32.TryParse((attributeWithParameter.Split(new[] { ":" }, new StringSplitOptions())[1]), out number);
                    if (parseSuccess)
                    {
                        _codeDomHelper.AddAttribute(testMethod, @"NUnit.Framework." + attributeWithParameter.Split(new[] { ":" }, new StringSplitOptions())[0], number);
                    }
                    else
                    {
                        _codeDomHelper.AddAttribute(testMethod, @"NUnit.Framework." + attributeWithParameter.Split(new[] { ":" }, new StringSplitOptions())[0], attributeWithParameter.Split(new[] { ":" }, new StringSplitOptions())[1]);
                    }
                }
                else
                {
                    _codeDomHelper.AddAttribute(testMethod, @"NUnit.Framework." + nunitAttributeTag.Substring(14));
                }
            }
        }
    }
}
