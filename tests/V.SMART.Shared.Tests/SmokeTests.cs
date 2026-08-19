using Moq;
using V.SMART.Shared.Services;
using V.SMART.Shared.Tests.Infrastructure;
using V.SMART.Shared.Utility_Constants;
using Xunit;

namespace V.SMART.Shared.Tests;

/// <summary>
/// Infrastructure smoke tests created by task M0-12-01. These prove the loop -
/// discovery, the project reference to V.SMART.Shared, the EF fixture and CI -
/// and deliberately assert nothing about business behaviour.
///
/// Assertions about calculation values belong to M0-12-02 and assertions about
/// stock behaviour belong to M0-13. Do not add them here.
/// </summary>
public class SmokeTests
{
    [Fact]
    public void Smoke_TestRunner_Discovers_Tests()
    {
        Assert.True(true);
    }

    [Fact]
    public void Smoke_SharedAssembly_IsLoadable()
    {
        // Proves the project reference to the multi-targeted, Razor-SDK
        // V.SMART.Shared resolves from a plain net9.0 test project.
        var assembly = typeof(CalculationService).Assembly;
        Assert.Equal("V.SMART.Shared", assembly.GetName().Name);
    }

    [Fact]
    public async Task Smoke_CalculationService_ComputesSomething()
    {
        // CalculationService has no constructor dependencies
        // (V.SMART/V.SMART.Shared/Services/CalculationService.cs:10-12), so this
        // needs no fixture. It exercises BR-CALC-001 unmodified and asserts only
        // that a total was computed - NOT what the total is.
        var service = new CalculationService();

        var subItem = new Mock<ICalculationDocumentSubItem>();
        subItem.SetupGet(x => x.Qty).Returns(2m);
        subItem.SetupGet(x => x.UnitPrice).Returns(50m);
        subItem.SetupGet(x => x.LineGross).Returns(100m);
        subItem.SetupGet(x => x.LineBasicAmount).Returns(100m);

        var document = new TestCalculationDocument
        {
            SubItems = new[] { subItem.Object },
        };

        Assert.Equal(0m, document.GrandTotal);

        await service.UpdateTotalsAsync(document);

        Assert.NotEqual(0m, document.GrandTotal);
    }

    /// <summary>
    /// Minimal hand-rolled <see cref="ICalculationDocument"/>. The interface's 26
    /// settable members are declared at
    /// V.SMART/V.SMART.Shared/Utility_Constants/ICalculationDocument.cs:11-40.
    /// </summary>
    private sealed class TestCalculationDocument : ICalculationDocument
    {
        public decimal TotalGrossAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal DiscountPercent { get; set; }
        public bool DiscAmtOrPer { get; set; }
        public decimal TotalBasicAmount { get; set; }
        public decimal FreightCharges { get; set; }
        public decimal PackingCharges { get; set; }
        public decimal PackingPercent { get; set; }
        public bool PackingAmtOrPer { get; set; }
        public decimal InsuranceCharges { get; set; }
        public decimal InsurancePercent { get; set; }
        public bool InsuranceAmtOrPer { get; set; }
        public decimal TotalTaxable { get; set; }
        public decimal CGstRate { get; set; }
        public decimal SGstRate { get; set; }
        public decimal IGstRate { get; set; }
        public decimal TotalCGSTAmount { get; set; }
        public decimal TotalSGSTAmount { get; set; }
        public decimal TotalIGSTAmount { get; set; }
        public decimal TCSAmount { get; set; }
        public decimal TCSPercent { get; set; }
        public bool TCSAmtOrPer { get; set; }
        public decimal OtherCharges { get; set; }
        public bool IsRoundOffEnabled { get; set; }
        public decimal RoundOff { get; set; }
        public decimal GrandTotal { get; set; }

        public IEnumerable<ICalculationDocumentSubItem> SubItems { get; set; } = Array.Empty<ICalculationDocumentSubItem>();
        public IEnumerable<ICalculationDocumentSubItem> CalculationDocumentSubItem => SubItems;
        public bool HasItemWiseTax => false;
    }

    [Fact]
    public void Smoke_TestDoubles_Construct()
    {
        // Proves the non-mockable CurrentUserService can be constructed, and that
        // the ILoggingService double satisfies the interface.
        var authProvider = new TestAuthenticationStateProvider();
        var currentUser = new CurrentUserService(authProvider);
        Assert.NotNull(currentUser.MachineName);

        ILoggingService logger = new FakeLoggingService();
        Assert.NotNull(logger);
    }

    [Fact]
    public async Task Smoke_CurrentUserService_ReadsFakePrincipal()
    {
        var currentUser = new CurrentUserService(new TestAuthenticationStateProvider("Administrator", 1));

        Assert.Equal("Administrator", await currentUser.GetUsernameAsync());
        Assert.Equal(1, await currentUser.GetUserIdAsync());
    }
}
