using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using V.SMART.Api.Contracts;
using V.SMART.Api.Controllers;
using V.SMART.Api.Middleware;
using V.SMART.Api.Tests.Infrastructure;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;
using Xunit;

namespace V.SMART.Api.Tests
{
    /// <summary>
    /// M2-A06 — the controller half of the error contract. The refusal tuples the services return
    /// are invisible to middleware, so these assertions are the contract for every controller that
    /// follows.
    /// </summary>
    public class CurrencyControllerErrorContractTests
    {
        private static CurrencyController Controller(FakeCurrencyService service, string path = "/api/currencies/7")
            => new(service)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = ErrorContractTestContext.Create(path)
                }
            };

        private static ProblemDetails Problem(IActionResult result, int expectedStatus)
        {
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(expectedStatus, objectResult.StatusCode);
            Assert.Contains(ApiProblems.ContentType, objectResult.ContentTypes);
            return Assert.IsAssignableFrom<ProblemDetails>(objectResult.Value);
        }

        // ---------------------------------------------------------------- 409

        [Fact]
        public async Task Delete_returns_409_with_the_service_message_verbatim()
        {
            // Verbatim from CurrencyService.CanDeleteCurrencyAsync (CurrencyService.cs:205).
            const string ServiceMessage = "Cannot delete Currency 'Rupee' because it is used in Item Screen.";

            var service = new FakeCurrencyService { CanDeleteResult = (false, ServiceMessage) };
            var problem = Problem(await Controller(service).Delete(7), StatusCodes.Status409Conflict);

            // The exact string, not a substring: BR-SO-001 forbids rewording, prefixing, truncating.
            Assert.Equal(ServiceMessage, problem.Title);
            Assert.Equal(ProblemTypes.BusinessRule, problem.Type);
            Assert.Equal(409, problem.Status);
            Assert.Equal("/api/currencies/7", problem.Instance);
            Assert.True(problem.Extensions.ContainsKey("traceId"));
        }

        [Fact]
        public async Task Delete_no_longer_returns_400_for_a_refused_delete()
        {
            var service = new FakeCurrencyService
            {
                CanDeleteResult = (false, "'INR' is a system-defined currency and cannot be deleted.")
            };

            var result = Assert.IsType<ObjectResult>(await Controller(service).Delete(1));

            // The deliberate breaking change recorded in the M2-A06 task file.
            Assert.NotEqual(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.Equal(StatusCodes.Status409Conflict, result.StatusCode);
        }

        [Fact]
        public async Task Create_refusal_returns_409_with_the_service_message_verbatim()
        {
            const string ServiceMessage = "Currency name already exists."; // CurrencyService.cs:85

            var service = new FakeCurrencyService { CreateResult = (false, ServiceMessage, null) };
            var problem = Problem(
                (await Controller(service, "/api/currencies").Create(new CurrencyVM())).Result!,
                StatusCodes.Status409Conflict);

            Assert.Equal(ServiceMessage, problem.Title);
        }

        [Fact]
        public async Task Update_refusal_returns_409_with_the_service_message_verbatim()
        {
            const string ServiceMessage = "You cannot modify a system-defined Currency."; // CurrencyService.cs:123

            var service = new FakeCurrencyService { UpdateResult = (false, ServiceMessage, null) };
            var problem = Problem(
                (await Controller(service).Update(7, new CurrencyVM())).Result!,
                StatusCodes.Status409Conflict);

            Assert.Equal(ServiceMessage, problem.Title);
        }

        // ---------------------------------------------------------------- 404

        [Fact]
        public async Task GetById_returns_canonical_404()
        {
            var service = new FakeCurrencyService { GetByIdResult = null };
            var problem = Problem((await Controller(service).GetById(7)).Result!, StatusCodes.Status404NotFound);

            Assert.Equal("Currency not found.", problem.Title);
            Assert.Equal(ProblemTypes.NotFound, problem.Type);
        }

        [Fact]
        public async Task Delete_returns_canonical_404_when_the_row_has_gone()
        {
            var service = new FakeCurrencyService { CanDeleteResult = (true, "ok"), DeleteResult = false };
            var problem = Problem(await Controller(service).Delete(7), StatusCodes.Status404NotFound);

            Assert.Equal("Currency not found.", problem.Title);
        }

        // ---------------------------------------------------------------- 400

        [Fact]
        public async Task Invalid_model_state_returns_400_with_an_errors_dictionary_keyed_by_field()
        {
            var controller = Controller(new FakeCurrencyService(), "/api/currencies");
            PopulateModelStateFromDataAnnotations(controller.ModelState, new CurrencyVM());

            var objectResult = Assert.IsType<ObjectResult>((await controller.Create(new CurrencyVM())).Result);
            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
            Assert.Contains(ApiProblems.ContentType, objectResult.ContentTypes);

            var problem = Assert.IsType<ValidationProblemDetails>(objectResult.Value);
            Assert.Equal(ProblemTypes.ValidationFailed, problem.Type);
            Assert.True(problem.Extensions.ContainsKey("traceId"));

            // The DataAnnotations messages from CurrencyVM.cs:14-25, verbatim, keyed by field.
            Assert.Equal(new[] { "Please Enter Currency Name" }, problem.Errors["CurrName"]);
            Assert.Equal(new[] { "Please Enter Sub Currency Name" }, problem.Errors["CurrSub"]);
            Assert.Equal(new[] { "Please Enter Symbol" }, problem.Errors["Symbol"]);

            var json = ErrorContractTestContext.Serialize(problem);
            Assert.Equal(
                "Please Enter Currency Name",
                json.GetProperty("errors").GetProperty("CurrName")[0].GetString());
        }

        // ---------------------------------------------------- success regression

        [Fact]
        public async Task Success_responses_are_unchanged()
        {
            var currency = new CurrencyVM { CurrId = 3, CurrName = "Rupee", CurrSub = "Paisa", Symbol = "₹" };
            var service = new FakeCurrencyService
            {
                SearchResult = (new List<CurrencyVM> { currency }, 1),
                GetByIdResult = currency,
                CreateResult = (true, "Currency Created Successfully", currency),
                UpdateResult = (true, "Currency Updated Successfully", currency),
                CanDeleteResult = (true, "Currency '3' can be safely deleted."),
                DeleteResult = true
            };

            // M2-B02 replaced the controller-local paged-response record with the shared
            // PagedResult<T>. The JSON is unchanged — the four property names were already
            // ADR-002 §2's — so this remains a success-shape regression, not a new assertion.
            var list = Assert.IsType<OkObjectResult>((await Controller(service).GetAll(new CurrencyQuery())).Result);
            var paged = Assert.IsType<PagedResult<CurrencyVM>>(list.Value);
            Assert.Equal(1, paged.TotalCount);
            Assert.Same(currency, Assert.Single(paged.Items));

            var byId = Assert.IsType<OkObjectResult>((await Controller(service).GetById(3)).Result);
            Assert.Same(currency, byId.Value);

            var created = Assert.IsType<CreatedAtActionResult>(
                (await Controller(service, "/api/currencies").Create(currency)).Result);
            Assert.Same(currency, created.Value);
            Assert.Equal(nameof(CurrencyController.GetById), created.ActionName);

            var updated = Assert.IsType<OkObjectResult>((await Controller(service).Update(3, currency)).Result);
            Assert.Same(currency, updated.Value);

            Assert.IsType<NoContentResult>(await Controller(service).Delete(3));
        }

        private static void PopulateModelStateFromDataAnnotations(ModelStateDictionary modelState, object model)
        {
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);

            foreach (var result in results)
            {
                foreach (var member in result.MemberNames)
                {
                    modelState.AddModelError(member, result.ErrorMessage ?? string.Empty);
                }
            }
        }
    }
}
