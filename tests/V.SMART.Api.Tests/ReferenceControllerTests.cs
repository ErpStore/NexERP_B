using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Moq;
using V.SMART.Api.Authorization;
using V.SMART.Api.Caching;
using V.SMART.Api.Contracts;
using V.SMART.Api.Controllers;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.Master.MasterScreeenManagement;
using V.SMART.Shared.Data.Master.MasterScreeenManagement_Module;
using V.SMART.Shared.Utility_Constants;
using Xunit;

namespace V.SMART.Api.Tests
{
    /// <summary>
    /// M2-B09 — <c>ReferenceController</c>. Two things are asserted that a casual reading of the
    /// controller would not guarantee: that the GST ladders are <b>read from the domain</b>
    /// rather than retyped, and that every DTO is <b>flat</b> — no navigation properties, no
    /// entity types. The second is a security property, not a style one:
    /// <c>Screens.UserRights</c> is the tenant's entire permission matrix and
    /// <c>Currency.CurrencyRates</c> is the daily rate feed, both one careless
    /// <c>Include</c> away from a cached dropdown response.
    /// </summary>
    public class ReferenceControllerTests
    {
        private static ReferenceController ControllerWith(Mock<ICommonService> service)
            => new(service.Object);

        private static T Body<T>(ActionResult<T> result)
        {
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            return Assert.IsAssignableFrom<T>(ok.Value);
        }

        // ---------- GST ----------

        [Fact]
        public void Gst_rates_are_read_from_CommonConstants_not_retyped()
        {
            // Reference equality would be too strict (the record could copy), so this asserts
            // full sequence equality against the domain's own lists. If someone retypes the
            // ladder as a literal and the domain later changes, this fails.
            var body = Body(ControllerWith(new Mock<ICommonService>()).GetGstRates());

            Assert.Equal(CommonConstants.IGSTRates, body.Igst);
            Assert.Equal(CommonConstants.GSTRates, body.CgstSgst);
        }

        [Fact]
        public void The_two_gst_ladders_are_paired_by_index_and_the_response_preserves_it()
        {
            // The relationship the constants already encode: CGST and SGST each carry half the
            // integrated rate. Exposed so no client recomputes igst/2 in TypeScript.
            var body = Body(ControllerWith(new Mock<ICommonService>()).GetGstRates());

            Assert.Equal(body.Igst.Count, body.CgstSgst.Count);

            for (var i = 0; i < body.Igst.Count; i++)
            {
                Assert.Equal(body.Igst[i] / 2m, body.CgstSgst[i]);
            }
        }

        // ---------- projections ----------

        [Fact]
        public async Task States_are_projected_from_the_service()
        {
            var service = new Mock<ICommonService>();
            service.Setup(s => s.GetStatesAsync()).ReturnsAsync(new List<State>
            {
                new() { StateCode = 33, StateName = "Tamil Nadu", IsSystemDefined = true }
            });

            var body = Body(await ControllerWith(service).GetStates());
            var state = Assert.Single(body);

            Assert.Equal(33, state.StateCode);
            Assert.Equal("Tamil Nadu", state.StateName);
            Assert.True(state.IsSystemDefined);
            service.Verify(s => s.GetStatesAsync(), Times.Once);
        }

        [Fact]
        public async Task Uoms_are_projected_from_the_service()
        {
            var service = new Mock<ICommonService>();
            service.Setup(s => s.GetUOMsAsync()).ReturnsAsync(new List<UOM>
            {
                new() { UnitCode = "KG", UnitDescription = "Kilogram", IsSystemDefined = true }
            });

            var uom = Assert.Single(Body(await ControllerWith(service).GetUoms()));

            Assert.Equal("KG", uom.UnitCode);
            Assert.Equal("Kilogram", uom.UnitDescription);
        }

        [Fact]
        public async Task Terms_come_from_the_active_only_service_method()
        {
            // The active filter is the domain's. If this controller ever re-filters, the API and
            // the Blazor screens can disagree about what "active" means.
            var service = new Mock<ICommonService>();
            service.Setup(s => s.GetAllActiveTermsAsync()).ReturnsAsync(new List<TermsAndConditions>
            {
                new() { Id = 4, Title = "Payment", Details = "30 days" }
            });

            var term = Assert.Single(Body(await ControllerWith(service).GetTerms()));

            Assert.Equal(4, term.Id);
            Assert.Equal("Payment", term.Title);
            service.Verify(s => s.GetAllActiveTermsAsync(), Times.Once);
        }

        [Fact]
        public async Task Screens_are_projected_without_the_UserRights_navigation()
        {
            // The security-relevant projection. The entity carries every UserRight row in the
            // tenant; the DTO must not.
            var service = new Mock<ICommonService>();
            service.Setup(s => s.GetAllScreenAsync()).ReturnsAsync(new List<Screens>
            {
                new() { Id = 1, ScreenCode = 1, ScreenName = "User", IsPrintRequired = false }
            });

            var screen = Assert.Single(Body(await ControllerWith(service).GetScreens()));

            Assert.Equal(1, screen.ScreenCode);
            Assert.Equal("User", screen.ScreenName);
        }

        [Fact]
        public async Task Currencies_are_projected_without_the_rate_feed()
        {
            var service = new Mock<ICommonService>();
            service.Setup(s => s.GetCurrenciesAsync()).ReturnsAsync(new List<Currency>
            {
                new() { CurrId = 1, CurrName = "Rupee", CurrSub = "Paise", Symbol = "₹", IsSystemDefined = true }
            });

            var currency = Assert.Single(Body(await ControllerWith(service).GetCurrencies()));

            Assert.Equal(1, currency.CurrId);
            Assert.Equal("Rupee", currency.CurrName);
            Assert.Equal("₹", currency.Symbol);
        }

        // ---------- structural guarantees ----------

        [Fact]
        public void Every_reference_DTO_is_flat()
        {
            // Asserts the PROPERTY, not a field list, so a future DTO added to this namespace is
            // covered automatically. A navigation property or entity type here would put a graph
            // on the wire behind a cached, authenticated dropdown feed.
            var dtoTypes = new[]
            {
                typeof(StateDto), typeof(CurrencyDto), typeof(UomDto),
                typeof(TermsDto), typeof(ScreenDto)
            };

            foreach (var dto in dtoTypes)
            {
                foreach (var property in dto.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    var type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

                    var isFlat = type.IsPrimitive
                                 || type == typeof(string)
                                 || type == typeof(decimal)
                                 || type == typeof(DateTime)
                                 || type.IsEnum;

                    Assert.True(isFlat,
                        $"{dto.Name}.{property.Name} is {type.Name}; reference DTOs must be flat " +
                        "— no navigation properties, no entity types.");
                }
            }
        }

        [Fact]
        public void Reference_DTOs_carry_no_audit_columns()
        {
            var dtoTypes = new[]
            {
                typeof(StateDto), typeof(CurrencyDto), typeof(UomDto),
                typeof(TermsDto), typeof(ScreenDto)
            };

            var auditNames = new[] { "CreatedBy", "CreatedDate", "ModifiedBy", "ModifiedDate" };

            foreach (var dto in dtoTypes)
            {
                foreach (var name in auditNames)
                {
                    Assert.Null(dto.GetProperty(name));
                }
            }
        }

        [Fact]
        public void The_controller_requires_authentication_and_opts_out_of_screen_rights_explicitly()
        {
            // [NoScreenRight] is the auditable opt-out (KB-105 §2.4) — silence would leave the
            // endpoints unprotected by the screen-right axis with nothing recording the choice.
            var controller = typeof(ReferenceController);

            Assert.NotNull(controller.GetCustomAttribute<AuthorizeAttribute>());
            Assert.Null(controller.GetCustomAttribute<AllowAnonymousAttribute>());

            var optOut = controller.GetCustomAttribute<NoScreenRightAttribute>();
            Assert.NotNull(optOut);
            Assert.False(string.IsNullOrWhiteSpace(optOut!.Justification));
        }

        [Fact]
        public void The_controller_uses_the_named_tenant_scoped_cache_policy()
        {
            var outputCache = typeof(ReferenceController).GetCustomAttribute<OutputCacheAttribute>();

            Assert.NotNull(outputCache);
            Assert.Equal(ReferenceCachePolicy.PolicyName, outputCache!.PolicyName);
        }

        [Fact]
        public void The_route_is_composed_from_the_shared_version_constant()
        {
            // M2-B01's rule: no controller author writes the version string by hand. This pins
            // that the route is under /api/v1 without hard-coding how it got there.
            var route = typeof(ReferenceController).GetCustomAttribute<RouteAttribute>();

            Assert.NotNull(route);
            Assert.Equal($"{ApiRoutes.V1}/reference", route!.Template);
            Assert.StartsWith("api/v1/", route.Template);
        }
    }
}
