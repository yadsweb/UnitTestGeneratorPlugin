//using System.CodeDom;
//using System.Collections.Generic;
//using System.Linq;
//using TechTalk.SpecFlow.Generator;
//using TechTalk.SpecFlow.Generator.UnitTestProvider;
//using TechTalk.SpecFlow.Utils;

//namespace UnitTestGeneratorPlugin.Generator.SpecFlowPlugin
//{
//    class UnitTestGeneratorProviderCustomAttributes : IUnitTestGeneratorProvider
//    {
//        private  NUnitTestGeneratorProvider _nunit;
//        private  CodeDomHelper _codeDomHelper;

//        public UnitTestGeneratorProviderCustomAttributes(CodeDomHelper codeDomHelper)
//        {
//            _nunit = new NUnitTestGeneratorProvider(codeDomHelper);
//            _codeDomHelper = codeDomHelper;
//        }

//        public void SetTestClass(TestClassGenerationContext generationContext, string featureTitle, string featureDescription)
//        {
//            _nunit.SetTestClass(generationContext, featureTitle, featureDescription);
//        }

//        public void SetTestClassCategories(TestClassGenerationContext generationContext, IEnumerable<string> featureCategories)
//        {
//            _nunit.SetTestClassCategories(generationContext, featureCategories);
//        }

//        public void SetTestClassIgnore(TestClassGenerationContext generationContext)
//        {
//            _nunit.SetTestClassIgnore(generationContext);
//        }

//        public void FinalizeTestClass(TestClassGenerationContext generationContext)
//        {
//            _nunit.FinalizeTestClass(generationContext);
//        }

//        public void SetTestClassInitializeMethod(TestClassGenerationContext generationContext)
//        {
//            _nunit.SetTestClassInitializeMethod(generationContext);
//        }

//        public void SetTestClassCleanupMethod(TestClassGenerationContext generationContext)
//        {
//            _nunit.SetTestClassCleanupMethod(generationContext);
//        }

//        public void SetTestInitializeMethod(TestClassGenerationContext generationContext)
//        {
//            _nunit.SetTestInitializeMethod(generationContext);
//        }

//        public void SetTestCleanupMethod(TestClassGenerationContext generationContext)
//        {
//            _nunit.SetTestCleanupMethod(generationContext);
//        }

//        public void SetTestMethod(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, string scenarioTitle)
//        {
//            if (generationContext.Feature != null)
//            {
//                if (generationContext.Feature.Scenarios != null)
//                {
//                    var scenario = generationContext.Feature.Scenarios.FirstOrDefault(s => s.Title == scenarioTitle);

//                    if (scenario != null)
//                    {
//                        if (scenario.Tags != null)
//                        {
//                            var repeatTag = scenario.Tags.FirstOrDefault(t => t.Name.ToLower().StartsWith("repeat"));

//                            if (repeatTag != null)
//                            {
//                                var repeatNumber = repeatTag.Name.ToLower().Replace("repeat", "");

//                                _codeDomHelper.AddAttribute(testMethod, @"NUnit.Framework.RepeatAttribute", int.Parse(repeatNumber));
//                            }
//                        }
//                    }
//                }
//            }

//            _nunit.SetTestMethod(generationContext, testMethod, scenarioTitle);
//        }

//        public void SetTestMethodCategories(TestClassGenerationContext generationContext, CodeMemberMethod testMethod,
//            IEnumerable<string> scenarioCategories)
//        {
//            _nunit.SetTestMethodCategories(generationContext, testMethod, scenarioCategories);
//        }

//        public void SetTestMethodIgnore(TestClassGenerationContext generationContext, CodeMemberMethod testMethod)
//        {
//            _nunit.SetTestMethodIgnore(generationContext, testMethod);
//        }

//        public void SetRowTest(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, string scenarioTitle)
//        {

//            if (generationContext.Feature != null)
//            {
//                if (generationContext.Feature.Scenarios != null)
//                {
//                    var scenario = generationContext.Feature.Scenarios.FirstOrDefault(s => s.Title == scenarioTitle);

//                    if (scenario != null)
//                    {
//                        if (scenario.Tags != null)
//                        {
//                            var repeatTag = scenario.Tags.FirstOrDefault(t => t.Name.ToLower().StartsWith("repeat"));

//                            if (repeatTag != null)
//                            {
//                                var repeatNumber = repeatTag.Name.ToLower().Replace("repeat", "");

//                                _codeDomHelper.AddAttribute(testMethod, @"NUnit.Framework.RepeatAttribute", int.Parse(repeatNumber));
//                            }
//                        }
//                    }
//                }
//            }
//            _nunit.SetRowTest(generationContext, testMethod, scenarioTitle);
//        }

//        public void SetRow(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, IEnumerable<string> arguments,
//            IEnumerable<string> tags, bool isIgnored)
//        {
//            _nunit.SetRow(generationContext, testMethod, arguments, tags, isIgnored);
//        }

//        public void SetTestMethodAsRow(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, string scenarioTitle,
//            string exampleSetName, string variantName, IEnumerable<KeyValuePair<string, string>> arguments)
//        {
//            _nunit.SetTestMethodAsRow(generationContext, testMethod, scenarioTitle, exampleSetName, variantName, arguments);
//        }

//        public bool SupportsRowTests
//        {
//            get
//            {
//                return _nunit.SupportsRowTests;

//            }
//        }

//        public bool SupportsAsyncTests
//        {
//            get
//            {
//                return _nunit.SupportsAsyncTests;

//            }
//        }
//    }
//}
