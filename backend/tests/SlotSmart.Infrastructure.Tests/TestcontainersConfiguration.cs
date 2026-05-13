using System.Runtime.CompilerServices;

namespace SlotSmart.Infrastructure.Tests;

/// <summary>
/// Test-assembly bootstrapping for Testcontainers. Runs before any test method.
/// </summary>
internal static class TestcontainersConfiguration
{
    /// <summary>
    /// Disables the Testcontainers "Ryuk" resource reaper.
    /// </summary>
    /// <remarks>
    /// Why: Testcontainers .NET starts a sidecar container (`testcontainers/ryuk`) that watches
    /// the test session and force-removes any orphaned containers if the host process dies
    /// abnormally. Pulling that image goes through Docker.DotNet, which on developer machines
    /// with stored ECR credentials in `~/.docker/config.json` can be misrouted to a registry
    /// that rejects with 401 Unauthorized — even though `docker pull` from the CLI works fine.
    ///
    /// Trade-off: if the test process is `kill -9`-ed mid-run we may leak containers until the
    /// next reboot. For our use case (local + CI runs that complete or fail fast) this is
    /// acceptable. Re-enable by removing this initializer or setting the env var to `false`.
    ///
    /// Setting via env var `TESTCONTAINERS_RYUK_DISABLED=true` is the documented Testcontainers
    /// way; the assignment must happen before any Testcontainers type is touched, which is why
    /// we use <see cref="ModuleInitializerAttribute"/>.
    /// </remarks>
    [ModuleInitializer]
    public static void DisableRyukReaper()
    {
        Environment.SetEnvironmentVariable("TESTCONTAINERS_RYUK_DISABLED", "true");
    }
}
