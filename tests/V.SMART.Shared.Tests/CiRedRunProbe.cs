using Xunit;

namespace V.SMART.Shared.Tests;

/// <summary>
/// TEMPORARY — DELETE ME.
///
/// This file exists for exactly one purpose: acceptance criterion 6 of task
/// M0-12-01 requires proving that a failing test actually turns the CI run red,
/// rather than being silently swallowed. A green pipeline that cannot go red is
/// not a gate, it is decoration.
///
/// The step under test is "Test - V.SMART.Shared.Tests" in
/// .github/workflows/ci.yml, which checks $LASTEXITCODE explicitly because a
/// PowerShell pipeline swallows a native non-zero exit.
///
/// It is deliberately in its own file so that reverting it is a deletion and
/// touches none of the 11 real tests.
///
/// If you are reading this on any branch after M0-12-01 closed, it should not
/// be here — delete it.
/// </summary>
public class CiRedRunProbe
{
    [Fact]
    public void Deliberate_Failure_To_Prove_CI_Turns_Red()
    {
        // Assert.Fail, not Assert.True(false, ...): the latter raises xUnit2020,
        // a warning code absent from ci/warning-baseline.json, and the warning
        // gate is a ratchet that fails on unknown codes. A red run caused by the
        // warning gate would prove nothing about whether the TEST step gates the
        // build - which is the only thing criterion 6 is asking.
        Assert.Fail("Deliberate failure for M0-12-01 acceptance criterion 6. If CI is green with this present, the test step is not gating the build.");
    }
}
