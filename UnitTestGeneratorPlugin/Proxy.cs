using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using log4net;
using TechTalk.SpecFlow.Parser.SyntaxElements;

namespace UnitTestGeneratorPlugin.Generator.SpecFlowPlugin
{
    public class Proxy : MarshalByRefObject
    {
        public List<GherkinTableRow> DoWork(string assemblyPath, List<AdditionalTestCaseAttribute> rawTestCaseAttribute,
            Scenario scenario, string filterTypeIdentifier, string methodIdentifier)
        {
            try
            {
                Console.WriteLine("Path to the assembly is: " + assemblyPath);
                var assembly = Assembly.LoadFrom(assemblyPath);
                var filterType = assembly.GetType(filterTypeIdentifier);
                var methodInfo = filterType.GetMethod(methodIdentifier,
                    new[] { typeof(List<AdditionalTestCaseAttribute>), typeof(Scenario) });
                Type myType = typeof(Proxy);
                object myInstance = Activator.CreateInstance(myType);
                return
                    (List<GherkinTableRow>)methodInfo.Invoke(myInstance, new object[] { rawTestCaseAttribute, scenario });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int DoWork1(string assemblyPath, List<AdditionalTestCaseAttribute> aass)
        {
            try
            {
                var scenario = new Scenario();
                Console.WriteLine("Path to the assembly is: " + assemblyPath);
                var assembly = Assembly.LoadFrom(assemblyPath);
                var filterType = assembly.GetType("AutomatedUITesting.Framework.UnitTestGeneratorFilters.Filters");
                var methodInfo = filterType.GetMethod("TestCaseAttributeFilter",
                    new[] { typeof(List<AdditionalTestCaseAttribute>) });
                Type myType = typeof(Proxy);
                object myInstance = Activator.CreateInstance(myType);
                return (int)methodInfo.Invoke(myInstance, new object[] { aass });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}