using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using TechTalk.SpecFlow.Parser.SyntaxElements;
using UnitTestGeneratorPlugin.Generator.SpecFlowPlugin;


namespace UnitTestGeneratorPluginTests
{
    [TestFixture]
    public class GeneratorPluginConfigurationTests
    {
        [Test]
        public void RandomnumberTest()
        {
            var generator = new CustomUnitTestFeatureGenerator(null,null,null,null);
            var rnd1 = generator.GenerateuniqueId();
            var rnd2 = generator.GenerateuniqueId();
            Assert.AreNotEqual(rnd1,rnd2);
        }

        [Test]
        public void RandomnumberUniqunesTest()
        {
            var generator = new CustomUnitTestFeatureGenerator(null, null, null, null);
            for (var counter = 0; counter < 999998; counter++)
            {
               generator.UniqueIdList.Add(counter);
            }
            var rnd0 = generator.GenerateuniqueId();
            Assert.True(generator.UniqueIdList.Contains(rnd0), "The list contains value '" + rnd0 + "'!");
        }

        [Test]
        public void InitializeConfigurationNoAppConfig()
        {
            var generator = new CustomUnitTestFeatureGenerator(null, null, null, null);
            generator.InitializeConfiguration(Directory.GetCurrentDirectory() + @"\App.config");
            Assert.False(generator.SuccessfulInitialization);
        }
        [Test]
        public void InitializeConfigurationWithAppConfig()
        {
            var generator = new CustomUnitTestFeatureGenerator(null, null, null, null);
            generator.InitializeConfiguration(@"..\..\Resources\AppWithConfigurationSection.config");
            Assert.True(generator.SuccessfulInitialization);
        }
        [Test]
        public void InitializeConfigurationException()
        {
            var generator = new CustomUnitTestFeatureGenerator(null, null, null, null);
            generator.InitializeConfiguration(null);
            Assert.False(generator.SuccessfulInitialization);
        }
        [Test]
        public void InitializeCustomConfigurationSection()
        {
            var generator = new CustomUnitTestFeatureGenerator(null, null, null, null);
            generator.InitializeConfiguration(@"..\..\Resources\AppWithConfigurationSection.config");
            generator.InitializeCustomConfigurationSection(@"..\..\Resources\AppWithConfigurationSection.config");
            Assert.True(generator.CustomeConfigurationSection != null);
        }
        [Test]
        public void AddUnfilteredCategoriesNoAdditionalCategoryAttribute()
        {
            var generator = new CustomUnitTestFeatureGenerator(null, null, null, null);
            generator.InitializeConfiguration(@"..\..\Resources\AppNoAdditionalCategoriesAttribute.config");
            generator.InitializeCustomConfigurationSection(@"..\..\Resources\AppNoAdditionalCategoriesAttribute.config");
            var scenario = new Scenario();
            generator.AddUnFiltratedCategoryAttributes(scenario);
            Assert.True(scenario.Tags==null);
        }
        [Test]
        public void AddUnfilteredCategoriesNoAdditionalCategoryAttributes()
        {
            var generator = new CustomUnitTestFeatureGenerator(null, null, null, null);
            generator.InitializeConfiguration(@"..\..\Resources\AppNoAdditionalCategoriesAttributes.config");
            generator.InitializeCustomConfigurationSection(@"..\..\Resources\AppNoAdditionalCategoriesAttributes.config");
            var scenario = new Scenario();
            generator.AddUnFiltratedCategoryAttributes(scenario);
            Assert.True(scenario.Tags == null);
        }
        [Test]
        public void AddUnfilteredCategoriesWithAdditionalCategoryAttributes()
        {
            var generator = new CustomUnitTestFeatureGenerator(null, null, null, null);
            generator.InitializeConfiguration(@"..\..\Resources\AppWithConfigurationSection.config");
            generator.InitializeCustomConfigurationSection(@"..\..\Resources\AppWithConfigurationSection.config");
            var scenario = new Scenario();
            generator.AddUnFiltratedCategoryAttributes(scenario);
            Assert.True(scenario.Tags[0].Name.StartsWith("uniqueID:"), scenario.Tags[0].Name + " don't start with uniqueID:");
            Assert.True(scenario.Tags[1].Name.StartsWith("uniqueUser:"), scenario.Tags[0].Name + " don't start with uniqueUser:");
            Assert.True(scenario.Tags[2].Name.StartsWith("cat"), scenario.Tags[0].Name + " don't start with uniqueID:");
            Assert.True(scenario.Tags[3].Name.StartsWith("category"), scenario.Tags[0].Name + " don't start with uniqueID:");
        }

        [Test]
        public void AddUnFiltratedTestCaseAttributesNoTestCaseAttributes()
        {
            var generator = new CustomUnitTestFeatureGenerator(null, null, null, null);
            generator.InitializeConfiguration(@"..\..\Resources\AppWithConfigurationSectionNoAdditionalTestCaseAttributes.config");
            generator.InitializeCustomConfigurationSection(@"..\..\Resources\AppWithConfigurationSectionNoAdditionalTestCaseAttributes.config");
            var scenario = new Scenario();
            var feature = new Feature();
            generator.AddUnFiltratedTestCaseAttributes(scenario,feature);
            var scenarioOutline = scenario as ScenarioOutline;
            Assert.True(scenarioOutline == null);
        }
        [Test]
        public void AddUnFiltratedTestCaseAttributesNoTestCaseAttribute()
        {
            var generator = new CustomUnitTestFeatureGenerator(null, null, null, null);
            generator.InitializeConfiguration(@"..\..\Resources\AppWithConfigurationSectionNoAdditionalTestCaseAttribute.config");
            generator.InitializeCustomConfigurationSection(@"..\..\Resources\AppWithConfigurationSectionNoAdditionalTestCaseAttribute.config");
            var scenario = new Scenario();
            var feature = new Feature();
            generator.AddUnFiltratedTestCaseAttributes(scenario, feature);
            var scenarioOutline = scenario as ScenarioOutline;
            Assert.True(scenarioOutline == null);
        }
        [Test]
        public void AddUnfilteredCategoriesWithTestCaseAttribute()
        {
            var generator = new CustomUnitTestFeatureGenerator(null, null, null, null);
            generator.InitializeConfiguration(@"..\..\Resources\AppWithConfigurationSection.config");
            generator.InitializeCustomConfigurationSection(@"..\..\Resources\AppWithConfigurationSection.config");
            var scenario = new Scenario(null, "Test scenario name", null, new Tags(), null);
            var feature = new Feature(null, null, null, null, null, new[] { scenario}, null);
            generator.AddUnFiltratedTestCaseAttributes(scenario, feature);
            var scenarioOutline = feature.Scenarios[0] as ScenarioOutline;
            Assert.True(scenarioOutline != null);
            Assert.True(scenarioOutline.Examples.ExampleSets.First().Table.Body[0].Cells[0].Value == "Bingo Godz");
            Assert.True(scenarioOutline.Examples.ExampleSets.First().Table.Body[1].Cells[0].Value == "Health Bingo");
            Assert.True(scenarioOutline.Examples.ExampleSets.First().Table.Body[2].Cells[0].Value == "STV");
            Assert.True(scenarioOutline.Examples.ExampleSets.First().Table.Body[3].Cells[0].Value == "Slot Mob");
            Assert.True(scenarioOutline.Examples.ExampleSets.First().Table.Body[4].Cells[0].Value == "Bingo Stars");
            Assert.True(scenarioOutline.Examples.ExampleSets.First().Table.Body[5].Cells[0].Value == "WIN8, Chrome 35");
            Assert.True(scenarioOutline.Examples.ExampleSets.First().Table.Body[6].Cells[0].Value.StartsWith("uniqueChrome:"));
        }
        [Test]
        public void AddUnfilteredStepsNoStepsAttribute()
        {
            var generator = new CustomUnitTestFeatureGenerator(null, null, null, null);
            generator.InitializeConfiguration(@"..\..\Resources\AppWithConfigurationSectionNoSteps.config");
            generator.InitializeCustomConfigurationSection(@"..\..\Resources\AppWithConfigurationSectionNoSteps.config");
            var scenario = new Scenario();
            generator.AddUnFiltratedSteps(scenario);
            Assert.True(scenario.Steps == null);
        }
        [Test]
        public void AddUnfilteredStepsNoStepAttribute()
        {
            var generator = new CustomUnitTestFeatureGenerator(null, null, null, null);
            generator.InitializeConfiguration(@"..\..\Resources\AppWithConfigurationSectionNoStep.config");
            generator.InitializeCustomConfigurationSection(@"..\..\Resources\AppWithConfigurationSectionNoStep.config");
            var scenario = new Scenario();
            generator.AddUnFiltratedSteps(scenario);
            Assert.True(scenario.Steps == null);
        }
        [Test]
        public void AddUnfilteredStepsTest()
        {
            var generator = new CustomUnitTestFeatureGenerator(null, null, null, null);
            generator.InitializeConfiguration(@"..\..\Resources\AppWithConfigurationSection.config");
            generator.InitializeCustomConfigurationSection(@"..\..\Resources\AppWithConfigurationSection.config");
            var scenario = new Scenario(null, null, null, null, new ScenarioSteps(new[] { new ScenarioStep(), new ScenarioStep(), new ScenarioStep(), new ScenarioStep(), new ScenarioStep()}));
            generator.AddUnFiltratedSteps(scenario);
            Assert.True(scenario.Steps != null);
            Assert.True(scenario.Steps[1].Text.Equals("Write message for test start with <Browser> and <Site>"));
            Assert.True(scenario.Steps[2].Text.Equals("I am using <Browser> as my browser"));
            Assert.True(scenario.Steps[3].Text.Equals("I write hellou message"), "Expected 'I write hellou message' actual " + scenario.Steps[2].Text);
            Assert.True(scenario.Steps[5].Text.Equals("I am on the <Site> site"));
            Assert.True(scenario.Steps[9].Text.Equals("I write hellou message"));
            Assert.True(scenario.Steps[10].Text.Equals("I write hellou message"));
            Assert.True(scenario.Steps[11].Text.Equals("I write hellou message"));

            Assert.True(scenario.Steps[1].GetType() == typeof(Given));
            Assert.True(scenario.Steps[2].GetType() == typeof(When));
            Assert.True(scenario.Steps[3].GetType() == typeof(Given));
            Assert.True(scenario.Steps[5].GetType() == typeof(Then));
            Assert.True(scenario.Steps[9].GetType() == typeof(Given));
            Assert.True(scenario.Steps[10].GetType() == typeof(When));
            Assert.True(scenario.Steps[11].GetType() == typeof(Then));
        }
        [Test]
        public void AddUnfilteredStepsProblematicStepType()
        {
            try
            {
                var generator = new CustomUnitTestFeatureGenerator(null, null, null, null);
                generator.InitializeConfiguration(@"..\..\Resources\AppWithConfigurationProblematicStep.config");
                generator.InitializeCustomConfigurationSection(@"..\..\Resources\AppWithConfigurationProblematicStep.config");
                var scenario = new Scenario(null, null, null, null, new ScenarioSteps(new[] { new ScenarioStep()}));
                generator.AddUnFiltratedSteps(scenario);
                throw new Exception("Parsing of problematic step passed!");
            }
            catch (Exception e)
            {
                Assert.True(e != null);
            }

        }
        [Test]
        public void LogerSetUptest()
        {
            var generator = new CustomUnitTestFeatureGenerator(null, null, null, null);
            generator.SetUpLogger();
            Assert.True(generator.SuccessfulLoggerConfiguration);
        }
    }
}
