# UnitTestGeneratorPlugin
A plugin which can be used to add custom tags, attributes and steps to generated unit tests.

Purpouse
==================

The main idea of this plug in is to: 

1.If no configuration element is app.config file is provided to just add unique category to each scenario. The unique category added by default is 'uniqueId:998349' where the number is randomly generated and the prefix is a constant. This category can be used by the 'NunitTestsParallelConsoleRunner' to run scenarios in parallel.
2.If configuration element is provided in app.config then the plug in can be used to add category attributes, test case attributes and steps to generated unit tests. It actually intercept the generation and insert mentioned attributes and steps and don't interfere with the normal work of the generator. 

How to use? 
==================

The plug in is just a spec flow generator plug, so to use it in app.config following entry should be added in the “<specFlow>” section. 

<plugins>      
      <add name="UnitTestGeneratorPlugin.Generator" path="..\UnitTestGeneratorPlugin\bin\Debug" />
</plugins> 

Where the pat is the location of  “UnitTestGeneratorPlugin.Generator.SpecFlowPlugin.dll” and all relevant libs. 

1. If only the default behavior is needed then changing the app.config will be enough to enable the plug in. 

2. If specific configuration for the plug in is needed then custom configuration section needs to be added in app.config and a property in “App setting” section which points to the location of the dll which contains the configuration section (the dll is actually the plug in itself so it should again point to “UnitTestGeneratorPlugin.Generator.SpecFlowPlugin.dll”)

Example: 

<add key="Custom.plugin.generator.configuration" value="..\UnitTestGeneratorPlugin\bin\Debug\UnitTestGeneratorPlugin.Generator.SpecFlowPlugin.dll" />

Custom configuration section is defined like this: 

<section name="GeneratorPluginConfiguration" type="UnitTestGeneratorPlugin.Generator.SpecFlowPlugin.GeneratorPluginConfiguration, UnitTestGeneratorPlugin.Generator.SpecFlowPlugin" requirePermission="true" restartOnExternalChanges="true" allowLocation="true"/>

And it contains this configuration elements: 


  <GeneratorPluginConfiguration>
    <AdditionalCategoryAttributes>
      <AdditionalCategoryAttribute type="unique" value="uniqueId:w" />
      <AdditionalCategoryAttribute type="custom" value="cat1" />
      <AdditionalCategoryAttribute type="custom" value="cat2" />
    </AdditionalCategoryAttributes>
    <AdditionalTestCaseAttributes>
      <AdditionalTestCaseAttribute type="Site" value="Bingo Godz" />
      <AdditionalTestCaseAttribute type="Site" value="Health Bingo" />
      <AdditionalTestCaseAttribute type="Site" value="STV" />
      <AdditionalTestCaseAttribute type="Site" value="Slot Mob" />
      <AdditionalTestCaseAttribute type="Site" value="Bingo Stars" />
      <AdditionalTestCaseAttribute type="Browser" value="WIN8, Chrome 35" />
    </AdditionalTestCaseAttributes>
    <Steps>
      <Step position="1" type="Given" value="Write message for test start with {Browser} and {Site}" />
      <Step position="2" type="Given" value="I am using {Browser} as my browser" />
      <Step position="4" type="Given" value="I am on the {Site} site" />
      <Step position="3" type="Given" value="I am on the {Site} site" />
    </Steps>
    <FilterAssembly filepath="C:\Users\USER\Documents\GitHub\AutomatedUItesting\AutomatedUITesting.Framework\bin\Debug\AutomatedUITesting.Framework.dll" />
    <AdditionalCategoryAttributeFilter classname="AutomatedUITesting.Framework.SupportingFunctionality.Filters" method="CategoriesFilter" />
    <AdditionalTestCaseAttributeFilter classname="AutomatedUITesting.Framework.SupportingFunctionality.Filters" method="TestCaseAttributeFilter"/>
    <StepFilter classname="AutomatedUITesting.Framework.SupportingFunctionality.Filters" method="StepsFilter" />
  </GeneratorPluginConfiguration>

Additions explanation
==================

The plug in can add 3 ting to generate unit tests, this things are: 

Category attributes presented by: <AdditionalCategoryAttribute type="unique" value="uniqueId:w" />
Test case attributes presented by: <AdditionalTestCaseAttribute type="Site" value="Bingo Godz" />
Steps presented by: <Step position="1" type="Given" value="Write message for test start with {Browser} and {Site}" />

Each of this elements have specific properties: 

For category attributes we have type and value the type will be used by filtering and the value is actual value which will be added ([Nunit.Framework.CategoryAttribute("cat1")]). The only exception of this is if the type is set to unique then unique number will be added to the value and this will form the category ([Nunit.Framework.CategoryAttribute("uniqueId:998349")]).

For test case attributes the situation is same as category attributes, if the type is unique then unique number will be added to the value and this will form the test case attribute ([NUnit.Framework.TestCaseAttribute("uniqueId:998349", null)])

Have in mind that if no filtering is used then each test case attribute will be added on separate row and there will not gonna be combined

Example: 

        [NUnit.Framework.TestCaseAttribute("Bingo Godz", null)]
        [NUnit.Framework.TestCaseAttribute("Health Bingo", null)]
        [NUnit.Framework.TestCaseAttribute("STV", null)]
        [NUnit.Framework.TestCaseAttribute("Slot Mob", null)]

For steps element there is additional property called “position”, this is actually the position on which the step needs to be added in the scenario, if position is bigger then the position of last step then step will be added as last step. The type property here specifies type of step. Types should be “given”, “when” and “then” this mean that if other type is used exception will be thrown. Important here is that since steps parameters are marked as <param> and we are using xml format then in the xml we use {} which during generation will be replaced by <>. 

Example: 
<Step position="4" type="Given" value="I am on the {Site} site" />

will be translated to: 

testRunner.Given("I am on the <Site> site", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given");

Value is the actual text which will be used as step.
 
Custom configuration
==================

Custom configuration contains 3 section of elements which will be added to generated tests (AdditionalCategoryAttributes, AdditionalTestCaseAttribute and Step) and mechanism to filtrate what of this attributes to be added in each scenario. 

Filtration is done by a specific class defined in your project. To load the class and call  method for filtration “FilterAssembly” element is used, the path property is pointing to the dll which contains the class and methods for filtration. Each of the specified elements like “AdditionalCategoryAttributeFilter , AdditionalTestCaseAttributeFilter , StepFilter” is used to instantiate specific filter for the specific attributes like “AdditionalCategoryAttributes , AdditionalTestCaseAttributes , Step”. 

Example: 

<AdditionalCategoryAttributeFilter classname="AutomatedUITesting.Framework.SupportingFunctionality.Filters" method="CategoriesFilter" />

In this case the class which will be used for filtering the AdditionalCategoryAttribute is “Filters” (it should be defined with its fully clarified class name) the class is in the dll defined in filter assembly elements path property and the method which will be used is   CategoriesFilter. 

The actual method should be implemented like this: 

public List<AdditionalCategoryAttribute> CategoriesFilter(List<AdditionalCategoryAttribute> param, Scenario scenario)

This means that it will receive a list with all additional category attributes defined in <AdditionalCategoryAttributes>, the scenario for which filtering will be applied and needs to return again list with only relevant category attributes which we need.  

All other filters are working exactly the same way, how ever important here is to have in mind that if filtering is used then all specified categories and test case attributes defined in the feature files (for example @caseID and tables) will be cleared and then only filtrated one will be added. For the steps filtrated steps will be added on relevant positions and none of the scenario spets will be removed.

All filtering method signitures should be like this: 

public List<Step> StepsFilter(List<Step> param, Scenario scenario)
 
public List<AdditionalCategoryAttribute> CategoriesFilter(List<AdditionalCategoryAttribute> param, Scenario scenario) 

public List<GherkinTableRow> TestCaseAttributeFilter(List<AdditionalTestCaseAttribute> param, Scenario scenario)

If the Filter assembly attribute is not present in the configuration section then all additions like additional categories, test case attributes and steps will be added. If filter assembly is present but some of the filters is not present then all items from relevant section will be added. 

Example if filter assembly is present but the <AdditionalCategoryAttributeFilter/> is missing then all  AdditionalCategoryAttribute if present will be added
