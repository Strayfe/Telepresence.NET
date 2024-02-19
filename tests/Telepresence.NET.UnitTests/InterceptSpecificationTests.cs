// using System.Reflection;
// using Telepresence.NET.CommandLineInterface;
// using Xunit;
// using Xunit.Abstractions;
// using YamlDotNet.Serialization;
// using YamlDotNet.Serialization.NamingConventions;
//
// namespace Telepresence.NET.UnitTests;
//
// public class InterceptSpecification
// {
//     private readonly ITestOutputHelper _testOutputHelper;
//
//     public InterceptSpecification(ITestOutputHelper testOutputHelper) =>
//         _testOutputHelper = testOutputHelper;
//
//     /// <summary>
//     /// Check that ToString() returns a YAML representation of an intercept specification.
//     /// </summary>
//     [Fact]
//     public void ToString_ShouldReturn_YamlSpecification()
//     {
//         const string name = "should-return-yaml-specification";
//         
//         var intercept = new Intercept(name);
//         var interceptSpecification = intercept.ToString();
//         
//         _testOutputHelper.WriteLine(interceptSpecification);
//         
//         var deserializer = new DeserializerBuilder()
//             .WithNamingConvention(CamelCaseNamingConvention.Instance)
//             .Build();
//         
//         var result = deserializer.Deserialize<Intercept>(interceptSpecification!);
//         
//         Assert.Equal(result.Name, name);
//     }
//
//     /// <summary>
//     /// When serialized and deserialized, the intercept specifications should remain the same.
//     /// Failure to match means that object creation may
//     /// </summary>
//     [Fact]
//     public void ToString_ShouldDeserialize_GenerateMatchingObject()
//     {
//         var intercept = new Intercept();
//         var interceptSpecification = intercept.ToString();
//         
//         _testOutputHelper.WriteLine(interceptSpecification);
//         
//         // check it can be parsed as a YAML document
//         var deserializer = new DeserializerBuilder()
//             .WithNamingConvention(CamelCaseNamingConvention.Instance)
//             .Build();
//         
//         var result = deserializer.Deserialize<Intercept>(interceptSpecification!);
//         
//         Assert.Equal(result.ToString(), interceptSpecification);
//     }
//
//     /// <summary>
//     /// Returning the name of the object means that the creation of the YAML specification has failed.
//     /// It would be good to know this before releasing new versions as it might indicate a processing error.
//     /// </summary>
//     [Fact]
//     public void ToString_ShouldNotReturn_NameOfObject()
//     {
//         var intercept = new Intercept();
//         var interceptSpecification = intercept.ToString();
//         
//         _testOutputHelper.WriteLine(interceptSpecification);
//
//         Assert.NotEqual(typeof(Intercept).ToString(), interceptSpecification);
//     }
//
//     /// <summary>
//     /// Check to make sure that a default intercept will try to create a convention-based default intercept.
//     /// The current convention is to use the normalized name of the entry project.
//     /// The rules follow the rules for kubernetes resources, lowercase, replacing dots and underscores with dashes.
//     /// </summary>
//     [Fact]
//     public void DefaultIntercept_ShouldReturn_ConventionBasedDefaults()
//     {
//         var name = Assembly
//             .GetEntryAssembly()?
//             .GetName()
//             .Name?
//             .Replace('.', '-')
//             .Replace('_', '-')
//             .ToLowerInvariant();
//
//         var intercept = new Intercept();
//         
//         Assert.Equal(name, intercept.Name);
//         
//         // todo: automatically determine connection from kubeconfig
//         Assert.Null(intercept.Connection);
//         
//         Assert.NotNull(intercept.Workloads);
//         Assert.Single(intercept.Workloads);
//         Assert.Collection(intercept.Workloads, workload =>
//         {
//             Assert.Equal(name, workload.Name);
//             Assert.Single(workload.Intercepts);
//             Assert.Collection(workload.Intercepts, workloadIntercept =>
//             {
//                 Assert.Equal(name, workloadIntercept.Name);
//                 Assert.Equal(name, workloadIntercept.Handler);
//                 Assert.Equal(name, workloadIntercept.Service);
//             });
//         });
//         
//         Assert.NotNull(intercept.Handlers);
//         Assert.Single(intercept.Handlers);
//         Assert.Collection(intercept.Handlers, handler =>
//         {
//             Assert.Equal(name, handler.Name);
//             Assert.NotNull(handler.External);
//         });
//     }
// }
