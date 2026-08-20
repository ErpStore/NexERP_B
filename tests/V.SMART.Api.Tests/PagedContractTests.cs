using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using V.SMART.Api.Contracts;
using V.SMART.Api.Controllers;
using V.SMART.Api.Middleware;
using V.SMART.Api.Tests.Infrastructure;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;
using Xunit;

namespace V.SMART.Api.Tests
{
    /// <summary>
    /// M2-B02 — the paged list contract. The sort parser and the filter adapter are pure
    /// functions and are asserted directly; the controller is exercised over the fake service,
    /// which records exactly what it was handed.
    ///
    /// <para><b>What this file cannot cover, so nobody reads it as more than it is.</b> The test
    /// project has no <c>Microsoft.AspNetCore.Mvc.Testing</c> host (see its <c>.csproj</c>), so
    /// query-string model binding — and with it the 400 produced for an unparseable
    /// <c>fromDate</c> — is not exercised here. What is exercised is the validation metadata that
    /// binding would evaluate, plus the controller's own <c>ModelState</c> path.</para>
    /// </summary>
    public class PagedContractTests
    {
        private static CurrencyController Controller(FakeCurrencyService service, string path = "/api/currencies")
            => new(service)
            {
                ControllerContext = new ControllerContext { HttpContext = ErrorContractTestContext.Create(path) }
            };

        private static IReadOnlyList<ValidationResult> Validate(object model)
        {
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);
            return results;
        }

        // ------------------------------------------------------------------ sort parser

        [Fact]
        public void An_absent_sort_parses_to_no_terms_which_means_the_default_ordering()
        {
            Assert.True(SortSpecification.TryParse(null, CurrencyQuery.Sortable, out var terms, out var error));
            Assert.Empty(terms);
            Assert.Null(error);
            Assert.Null(SortSpecification.ToServiceSort(terms));
        }

        [Theory]
        [InlineData("currName", "currName", false)]
        [InlineData("-createdDate", "createdDate", true)]
        [InlineData("  -CURRNAME  ", "currName", true)] // case-insensitive, trimmed
        public void A_single_term_parses_to_its_canonical_field_and_direction(string sort, string field, bool descending)
        {
            Assert.True(SortSpecification.TryParse(sort, CurrencyQuery.Sortable, out var terms, out _));
            var term = Assert.Single(terms);
            Assert.Equal(field, term.Field);
            Assert.Equal(descending, term.Descending);
        }

        [Fact]
        public void Multiple_terms_keep_their_order()
        {
            Assert.True(SortSpecification.TryParse("-createdDate,currName", CurrencyQuery.Sortable, out var terms, out _));
            Assert.Equal(new[] { "createdDate", "currName" }, terms.Select(t => t.Field));
            Assert.Equal("-createdDate,currName", SortSpecification.ToServiceSort(terms));
        }

        [Fact]
        public void An_unknown_field_fails_and_the_message_lists_the_permitted_values()
        {
            Assert.False(SortSpecification.TryParse("password", CurrencyQuery.Sortable, out _, out var error));

            Assert.NotNull(error);
            Assert.Contains("password", error);
            foreach (var permitted in CurrencyQuery.Sortable)
                Assert.Contains(permitted, error);
        }

        [Fact]
        public void A_repeated_field_fails_rather_than_being_silently_collapsed()
            => Assert.False(SortSpecification.TryParse("currName,-currName", CurrencyQuery.Sortable, out _, out _));

        [Fact]
        public void The_allow_list_is_the_seven_user_visible_currency_fields()
            => Assert.Equal(
                new[] { "currId", "currName", "currSub", "symbol", "isSystemDefined", "createdBy", "createdDate" },
                CurrencyQuery.Sortable);

        // ------------------------------------------------------------------ filter adapter

        [Fact]
        public void No_filters_adapt_to_null_which_is_the_shape_the_service_expects()
            => Assert.Null(FilterDictionaryAdapter.ForCurrency(new CurrencyQuery()));

        [Fact]
        public void Whitespace_only_filters_are_dropped()
            => Assert.Null(FilterDictionaryAdapter.ForCurrency(new CurrencyQuery { CurrName = "   ", CreatedBy = "" }));

        [Fact]
        public void The_adapter_uses_the_keys_CurrencyFilterBuilder_understands()
        {
            var filters = FilterDictionaryAdapter.ForCurrency(new CurrencyQuery
            {
                CurrName = "Rup",
                CreatedBy = "admin",
                FromDate = new DateTime(2026, 1, 2, 13, 45, 0),
                ToDate = new DateTime(2026, 3, 4, 23, 59, 0)
            });

            Assert.NotNull(filters);

            // The PascalCase labels of the field switch at CurrencyService.cs:193-207. A wrong key
            // here would be swallowed by the trailing discard arm and would filter nothing.
            Assert.Equal(new[] { "CurrName", "CreatedBy", "FromDate", "ToDate" }, filters!.Keys);
            Assert.Equal("Rup", filters["CurrName"]);
            Assert.Equal("admin", filters["CreatedBy"]);

            // Culture-independent, date-only: both predicates apply .Date, so the time component
            // carries no information, and an ISO string parses the same under any CurrentCulture.
            Assert.Equal("2026-01-02", filters["FromDate"]);
            Assert.Equal("2026-03-04", filters["ToDate"]);
        }

        // ------------------------------------------------------------------ response shape

        [Fact]
        public void PagedResult_serialises_to_items_totalCount_pageNumber_pageSize()
        {
            var json = JsonSerializer.Serialize(
                new PagedResult<CurrencyVM>(new[] { new CurrencyVM { CurrId = 1 } }, 42, 2, 20),
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            Assert.Equal(
                new[] { "items", "totalCount", "pageNumber", "pageSize" },
                root.EnumerateObject().Select(p => p.Name));
            Assert.Equal(42, root.GetProperty("totalCount").GetInt32());
            Assert.Equal(2, root.GetProperty("pageNumber").GetInt32());
            Assert.Equal(20, root.GetProperty("pageSize").GetInt32());
        }

        // ------------------------------------------------------------------ validation metadata

        [Theory]
        [InlineData(0, PagedQuery.DefaultPageSize, "PageNumber")]
        [InlineData(-1, PagedQuery.DefaultPageSize, "PageNumber")]
        [InlineData(1, 0, "PageSize")]
        [InlineData(1, PagedQuery.MaxPageSize + 1, "PageSize")]
        public void Out_of_range_paging_is_a_validation_error_keyed_by_its_field(int pageNumber, int pageSize, string field)
        {
            var errors = Validate(new CurrencyQuery { PageNumber = pageNumber, PageSize = pageSize });
            Assert.Contains(errors, e => e.MemberNames.Contains(field));
        }

        [Fact]
        public void The_boundary_page_sizes_are_valid()
        {
            Assert.Empty(Validate(new CurrencyQuery { PageSize = 1 }));
            Assert.Empty(Validate(new CurrencyQuery { PageSize = PagedQuery.MaxPageSize }));
        }

        [Fact]
        public void An_unknown_sort_field_is_a_validation_error_listing_the_permitted_values()
        {
            var error = Assert.Single(Validate(new CurrencyQuery { Sort = "password" }));

            Assert.Contains("Sort", error.MemberNames);
            Assert.Contains("currName", error.ErrorMessage);
        }

        [Fact]
        public void FromDate_after_ToDate_is_a_validation_error_on_both_fields()
        {
            var error = Assert.Single(Validate(new CurrencyQuery
            {
                FromDate = new DateTime(2026, 5, 2),
                ToDate = new DateTime(2026, 5, 1)
            }));

            Assert.Contains("FromDate", error.MemberNames);
            Assert.Contains("ToDate", error.MemberNames);
        }

        [Fact]
        public void The_same_day_on_both_bounds_is_valid_because_ToDate_is_inclusive_of_that_day()
            => Assert.Empty(Validate(new CurrencyQuery
            {
                FromDate = new DateTime(2026, 5, 1, 8, 0, 0),
                ToDate = new DateTime(2026, 5, 1, 23, 59, 0)
            }));

        [Fact]
        public void The_documented_defaults_are_pageNumber_1_and_pageSize_20()
        {
            var query = new CurrencyQuery();
            Assert.Equal(1, query.PageNumber);
            Assert.Equal(20, query.PageSize);
            Assert.Equal(20, PagedQuery.DefaultPageSize);
            Assert.Equal(100, PagedQuery.MaxPageSize);
        }

        // ------------------------------------------------------------------ controller wiring

        [Fact]
        public async Task GetAll_passes_paging_filters_and_sort_through_to_the_service()
        {
            var service = new FakeCurrencyService { SearchResult = (new List<CurrencyVM>(), 7) };

            var result = await Controller(service).GetAll(new CurrencyQuery
            {
                PageNumber = 3,
                PageSize = 25,
                Sort = "-createdDate,currName",
                CurrName = "Rup"
            });

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var paged = Assert.IsType<PagedResult<CurrencyVM>>(ok.Value);

            Assert.Equal(7, paged.TotalCount);          // the filtered, unpaged count (CurrencyService.cs:76)
            Assert.Equal(3, paged.PageNumber);
            Assert.Equal(25, paged.PageSize);

            Assert.Equal(3, service.LastSearch.PageNumber);
            Assert.Equal(25, service.LastSearch.PageSize);
            Assert.Equal("-createdDate,currName", service.LastSearch.Sort);
            Assert.Equal("Rup", service.LastSearch.Filters!["CurrName"]);
        }

        [Fact]
        public async Task GetAll_without_a_sort_hands_the_service_null_so_its_default_ordering_stands()
        {
            var service = new FakeCurrencyService();
            await Controller(service).GetAll(new CurrencyQuery { PageNumber = 1, PageSize = 10 });

            Assert.Null(service.LastSearch.Sort);
            Assert.Null(service.LastSearch.Filters);
        }

        [Fact]
        public async Task GetAll_returns_the_canonical_400_when_model_state_is_invalid()
        {
            var controller = Controller(new FakeCurrencyService());
            controller.ModelState.AddModelError("PageSize", "pageSize must be between 1 and 100.");

            var objectResult = Assert.IsType<ObjectResult>((await controller.GetAll(new CurrencyQuery())).Result);

            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
            Assert.Contains(ApiProblems.ContentType, objectResult.ContentTypes);

            var problem = Assert.IsType<ValidationProblemDetails>(objectResult.Value);
            Assert.Equal(ProblemTypes.ValidationFailed, problem.Type);
            Assert.Contains("PageSize", problem.Errors.Keys);
            Assert.True(problem.Extensions.ContainsKey("traceId"));
        }

        [Fact]
        public void An_invalid_sort_that_reached_the_service_boundary_fails_loudly_rather_than_sorting_nothing()
            => Assert.Throws<InvalidOperationException>(() => new CurrencyQuery { Sort = "password" }.ToServiceSort());
    }
}
