﻿//-----------------------------------------------------------------------
// <copyright file="ExportedProcessParameterTransformerTests.cs">(c) https://github.com/tfsbuildextensions/BuildManager. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TFSBuildManager.UnitTests
{
    using System;
    using FluentAssertions;
    using Microsoft.TeamFoundation.Build.Common;
    using Microsoft.TeamFoundation.Build.Workflow.Activities;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using TfsBuildManager.Repository;
    using TfsBuildManager.Repository.Transformers;

    [TestClass]
    public class ExportedProcessParameterTransformerTests
    {
        [TestMethod]
        public void ProcessParameterDeserializer_PassesStringsStraightThrough()
        {
            var procParam = new[] { "anything custom", "should go straight through" };
            Assert.AreEqual(procParam[1], ExportedProcessParameterTransformer.ProcessParameterDeserializer(procParam), "because non special params should pass straight through the deserializer");
        }

        [TestMethod]
        public void ProcessParameterDeserializer_ConvertsExportedTestSpec()
        {
            var testSpec = new AgileTestPlatformSpec
            {
                AssemblyFileSpec = @"**\*.Tests.dll",
                ExecutionPlatform = ExecutionPlatformType.X86,
                FailBuildOnFailure = true,
                RunName = "Unit Tests",
                RunSettingsForTestRun = new RunSettings { ServerRunSettingsFile = "run.settings", TypeRunSettings = RunSettingsType.CodeCoverageEnabled },
                TestCaseFilter = "*FakeTests"
            };

            var procParam = new[] { "AgileTestSpecs", this.Serialize(new ExportedAgileTestPlatformSpec[] { testSpec }) };
            ExportedProcessParameterTransformer.ProcessParameterDeserializer(procParam).ShouldBeEquivalentTo(new TestSpecList(testSpec));
        }

        [TestMethod]
        public void ProcessParameterDeserializer_ConvertsExportedBuildSettings()
        {
            var buildSettings = new BuildSettings
            {
                PlatformConfigurations = PlatformConfigurationList.Default,
                ProjectsToBuild = new StringList("projectA,projectB")
            };
            var expBuildDef = this.Serialize(new { ProjectsToBuild = buildSettings.ProjectsToBuild, ConfigurationsToBuild = buildSettings.PlatformConfigurations });
            var procParam = new[] { "BuildSettings", expBuildDef };
            ExportedProcessParameterTransformer.ProcessParameterDeserializer(procParam).ShouldBeEquivalentTo(buildSettings);
        }

        [TestMethod]
        public void ProcessParameterDeserializer_ConvertsExportedAgentSettings_TFS()
        {
            var agentSettings = new AgentSettings
            {
                TagComparison = TagComparison.MatchExactly,
                MaxExecutionTime = TimeSpan.MaxValue,
                MaxWaitTime = TimeSpan.MaxValue,
                Name = "test",
                Tags = new StringList("tagA,tagB")
            };
            var expBuildDef = this.Serialize(new { TfvcAgentSettings = (AgentSettingsBuildParameter)agentSettings });
            var procParam = new[] { "AgentSettings", expBuildDef };
            ExportedProcessParameterTransformer.ProcessParameterDeserializer(procParam).ShouldBeEquivalentTo(agentSettings);
        }

        [TestMethod]
        public void ProcessParameterDeserializer_ConvertsExportedAgentSettings_Git()
        {
            var agentSettings = new BuildParameter
            {
                Json = @"{""unknown"": ""please help""}"
            };
            var expBuildDef = this.Serialize(new { GitAgentSettings = agentSettings });
            var procParam = new[] { "AgentSettings", expBuildDef };
            ExportedProcessParameterTransformer.ProcessParameterDeserializer(procParam).ShouldBeEquivalentTo(agentSettings);
        }

        private string Serialize(object obj)
        {
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize
            };

            return JsonConvert.SerializeObject(obj, Formatting.Indented, jsonSerializerSettings);
        }
    }
}
