using NetArchTest.Rules;
using SlotSmart.Application;
using SlotSmart.Domain.Common;
using SlotSmart.Infrastructure;

namespace SlotSmart.Architecture.Tests;

/// <summary>
/// Enforces the Clean Architecture layer rules from
/// <c>docs/plan/00-architecture/solution-structure.md</c>.
/// Failure here means a project reference was added that violates the layer diagram.
/// </summary>
public sealed class LayerRulesTests
{
    private static readonly System.Reflection.Assembly DomainAssembly = typeof(Entity).Assembly;
    private static readonly System.Reflection.Assembly ApplicationAssembly = typeof(SlotSmart.Application.DependencyInjection).Assembly;
    private static readonly System.Reflection.Assembly InfrastructureAssembly = typeof(SlotSmart.Infrastructure.DependencyInjection).Assembly;
    private static readonly System.Reflection.Assembly ApiAssembly = typeof(Program).Assembly;

    [Fact]
    public void Domain_does_not_depend_on_Application_Infrastructure_or_Api()
    {
        var result = Types
            .InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "SlotSmart.Application",
                "SlotSmart.Infrastructure",
                "SlotSmart.Api")
            .GetResult();

        result.IsSuccessful
            .Should().BeTrue("Domain must be the most independent layer; offending types: " +
                             FormatFailing(result));
    }

    [Fact]
    public void Domain_does_not_depend_on_external_libraries_like_EFCore_or_AspNetCore()
    {
        var result = Types
            .InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "Microsoft.EntityFrameworkCore",
                "Microsoft.AspNetCore",
                "Microsoft.Extensions.Hosting")
            .GetResult();

        result.IsSuccessful
            .Should().BeTrue("Domain must depend only on the BCL; offending types: " +
                             FormatFailing(result));
    }

    [Fact]
    public void Application_does_not_depend_on_Infrastructure_or_Api()
    {
        var result = Types
            .InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "SlotSmart.Infrastructure",
                "SlotSmart.Api")
            .GetResult();

        result.IsSuccessful
            .Should().BeTrue("Application may not see Infrastructure or Api; offending types: " +
                             FormatFailing(result));
    }

    [Fact]
    public void Infrastructure_does_not_depend_on_Api()
    {
        var result = Types
            .InAssembly(InfrastructureAssembly)
            .ShouldNot()
            .HaveDependencyOn("SlotSmart.Api")
            .GetResult();

        result.IsSuccessful
            .Should().BeTrue("Infrastructure must not reference the API host; offending types: " +
                             FormatFailing(result));
    }

    [Fact]
    public void Api_is_a_composition_root_only_no_one_depends_on_it()
    {
        // Domain, Application, and Infrastructure must not depend on Api.
        var combined = Types
            .InAssemblies(new[] { DomainAssembly, ApplicationAssembly, InfrastructureAssembly })
            .ShouldNot()
            .HaveDependencyOn("SlotSmart.Api")
            .GetResult();

        combined.IsSuccessful
            .Should().BeTrue("Only Api references Infrastructure; nothing references Api. Offending types: " +
                             FormatFailing(combined));
    }

    private static string FormatFailing(TestResult result) =>
        result.FailingTypes is null
            ? "(none reported)"
            : string.Join(", ", System.Linq.Enumerable.Select(result.FailingTypes, t => t.FullName));
}
