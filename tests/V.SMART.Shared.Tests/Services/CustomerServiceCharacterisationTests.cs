using V.SMART.Shared.BusinessLayer.BusinessService.MasterService.GeneralService;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using Xunit;

namespace V.SMART.Shared.Tests.Services;

/// <summary>
/// Characterisation tests for the Customer Master business rules that M2-D02-01 extracted from
/// <c>Pages/Master_Module_pages/Customer_Pages/CustomerUpsert.razor</c>'s <c>@code</c> block into
/// <see cref="CustomerService"/>.
///
/// They pin the behaviour of the *legacy* code, defects included. BR-CUST-003 (the GST casing
/// asymmetry), BR-CUST-010 (the opening balance reset on edit) and BR-CUST-012/-013 (the blank
/// -named child rows that survive an update because the retained set is keyed on id) are
/// asserted as they behave today, not as they arguably should. Q-106, Q-107 and Q-108 track
/// them; if an owner decision changes one, the test that fails is the one to update.
///
/// The rules and their pre-extraction file:line evidence are in
/// docs/kb/business-rules/customer-master-rules.md.
/// </summary>
public class CustomerServiceCharacterisationTests
{
    private static IReadOnlyList<State> States() => new List<State>
    {
        new State { StateCode = 33, StateName = "Tamil Nadu" },
        new State { StateCode = 99, StateName = "URP" }
    };

    private static CustomerVM ValidLocalCustomer() => new CustomerVM
    {
        CustName = "Acme Engineering",
        BusiType = "Local",
        GSTNo = "33AAACA1111A1Z5",
        PANNo = "AAACA1111A",
        StateId = 33
    };

    // ------------------------------------------------------------------ BR-CUST-002

    [Theory]
    [InlineData("33AAACA1111A1Z", "")]        // 14 characters
    [InlineData("33AAACA1111A1Z5", "AAACA1111A")] // 15 characters
    [InlineData("33AAACA1111A1Z55", "")]      // 16 characters
    [InlineData("", "")]
    [InlineData("   ", "")]
    [InlineData(null, "")]
    public void DerivePan_only_derives_from_a_15_character_gst(string? gst, string expected)
    {
        Assert.Equal(expected, CustomerService.DerivePan(gst));
    }

    [Fact]
    public void DerivePan_takes_characters_three_to_twelve()
    {
        Assert.Equal("AAACA1111A", CustomerService.DerivePan("33AAACA1111A1Z5"));
    }

    // ------------------------------------------------------------------ BR-CUST-003

    [Fact]
    public void Customer_gst_is_upper_cased_and_trimmed()
    {
        Assert.Equal("33AAACA1111A1Z5", CustomerService.NormalizeCustomerGstValue("  33aaaca1111a1z5 "));
    }

    [Fact]
    public void Consignee_gst_is_trimmed_but_deliberately_not_upper_cased()
    {
        // BR-CUST-003 — the asymmetry is preserved on purpose. See Q-106.
        Assert.Equal("33aaaca1111a1z5", CustomerService.NormalizeConsigneeGstValue("  33aaaca1111a1z5 "));
    }

    // ------------------------------------------------------------------ BR-CUST-004

    [Theory]
    [InlineData("Local", new[] { "B2B", "SEZWP", "SEZWOP" })]
    [InlineData("InterState", new[] { "B2B", "SEZWP", "SEZWOP" })]
    [InlineData("Imports", new[] { "SEZWP", "SEZWOP", "EXPWP", "EXPWOP" })]
    [InlineData("Exports", new[] { "SEZWP", "SEZWOP", "EXPWP", "EXPWOP" })]
    public void Business_type_constrains_the_customer_types(string busiType, string[] expected)
    {
        Assert.Equal(expected, CustomerService.GetCustomerTypes(busiType).ToArray());
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("Something else")]
    public void An_unrecognised_business_type_offers_no_customer_types(string? busiType)
    {
        Assert.Empty(CustomerService.GetCustomerTypes(busiType));
    }

    [Theory]
    [InlineData("Local", null, "B2B")]
    [InlineData("Local", "", "B2B")]
    [InlineData("Local", "EXPWP", "B2B")]      // not in the allowed set -> defaulted
    [InlineData("Local", "SEZWP", "SEZWP")]    // already valid -> kept
    [InlineData("InterState", null, "B2B")]
    [InlineData("Imports", null, "SEZWP")]
    [InlineData("Exports", "B2B", "SEZWP")]    // not in the allowed set -> defaulted
    [InlineData("Exports", "EXPWOP", "EXPWOP")]
    [InlineData("Anything else", "B2B", null)] // default branch clears SupTyp
    public void Supply_type_default_is_applied_only_when_the_current_value_is_not_allowed(
        string busiType, string? current, string? expected)
    {
        Assert.Equal(expected, CustomerService.ResolveSupplyTypeCore(busiType, current));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void An_empty_business_type_leaves_the_current_supply_type_untouched(string? busiType)
    {
        Assert.Equal("B2B", CustomerService.ResolveSupplyTypeCore(busiType, "B2B"));
    }

    // ------------------------------------------------------------------ BR-CUST-005

    [Theory]
    [InlineData("Imports", true)]
    [InlineData("Exports", true)]
    [InlineData("Local", false)]
    [InlineData("InterState", false)]
    [InlineData(null, false)]
    public void Import_and_export_are_the_only_overseas_business_types(string? busiType, bool expected)
    {
        Assert.Equal(expected, CustomerService.IsImportOrExportBusinessType(busiType));
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData("   ", true)]
    [InlineData("URP", true)]
    [InlineData("urp", true)]                 // case-insensitive
    [InlineData("33AAACA1111A1Z", true)]      // shorter than 15
    [InlineData("33AAACA1111A1Z5", false)]    // a real GST is kept
    public void Switching_away_from_imports_clears_only_a_blank_short_or_urp_gst(string? gst, bool expected)
    {
        Assert.Equal(expected, CustomerService.ShouldClearOnBusinessTypeSwitchCore(gst));
    }

    // ------------------------------------------------------------------ BR-CUST-018

    [Fact]
    public void Customer_name_and_business_type_are_required()
    {
        var vm = new CustomerVM { CustName = " ", BusiType = null!, StateId = 33 };

        var errors = CustomerService.ValidateCustomerFields(vm, States(), null);

        Assert.Contains("Customer Name is required.", errors);
        Assert.Contains("Business Type is required.", errors);
    }

    // ------------------------------------------------------------------ BR-CUST-006

    [Fact]
    public void A_valid_local_customer_produces_no_errors()
    {
        Assert.Empty(CustomerService.ValidateCustomerFields(ValidLocalCustomer(), States(), null));
    }

    [Fact]
    public void Local_customer_gst_is_required()
    {
        var vm = ValidLocalCustomer();
        vm.GSTNo = "";

        Assert.Contains("Please enter GST No.", CustomerService.ValidateCustomerFields(vm, States(), null));
    }

    [Fact]
    public void Local_customer_gst_must_be_fifteen_characters()
    {
        var vm = ValidLocalCustomer();
        vm.GSTNo = "33AAACA1111A1Z";

        Assert.Contains("GST No must be 15 characters.", CustomerService.ValidateCustomerFields(vm, States(), null));
    }

    [Fact]
    public void Local_customer_gst_must_match_the_pattern()
    {
        var vm = ValidLocalCustomer();
        vm.GSTNo = "AA AACA1111A1Z5";

        Assert.Contains("Invalid GST No format.", CustomerService.ValidateCustomerFields(vm, States(), null));
    }

    [Theory]
    [InlineData("Imports")]
    [InlineData("Exports")]
    public void Overseas_customers_must_carry_urp_as_their_gst(string busiType)
    {
        var vm = ValidLocalCustomer();
        vm.BusiType = busiType;
        vm.StateId = 99;
        vm.PANNo = null;

        Assert.Contains("For Imports/Exports, GST No must be 'URP'.",
            CustomerService.ValidateCustomerFields(vm, States(), null));

        vm.GSTNo = "urp"; // the comparison is case-insensitive
        Assert.DoesNotContain("For Imports/Exports, GST No must be 'URP'.",
            CustomerService.ValidateCustomerFields(vm, States(), null));
    }

    // ------------------------------------------------------------------ BR-CUST-007

    [Fact]
    public void Local_customer_pan_is_required_and_format_checked()
    {
        var vm = ValidLocalCustomer();
        vm.PANNo = null;
        Assert.Contains("Please enter PAN No.", CustomerService.ValidateCustomerFields(vm, States(), null));

        vm.PANNo = "aaaca1111a";
        Assert.Contains("Invalid PAN No format.", CustomerService.ValidateCustomerFields(vm, States(), null));
    }

    [Fact]
    public void Overseas_customer_pan_is_optional_but_validated_when_present()
    {
        var vm = ValidLocalCustomer();
        vm.BusiType = "Exports";
        vm.GSTNo = "URP";
        vm.StateId = 99;

        vm.PANNo = null;
        Assert.Empty(CustomerService.ValidateCustomerFields(vm, States(), null));

        vm.PANNo = "NOTAPAN";
        Assert.Contains("Invalid PAN No format for Imports/Exports.",
            CustomerService.ValidateCustomerFields(vm, States(), null));
    }

    // ------------------------------------------------------------------ BR-CUST-008

    [Fact]
    public void An_unselected_or_unknown_state_is_a_validation_error()
    {
        var vm = ValidLocalCustomer();
        vm.StateId = 0;
        Assert.Contains("Please select a valid State.", CustomerService.ValidateCustomerFields(vm, States(), null));

        vm.StateId = 4242;
        Assert.Contains("Please select a valid State.", CustomerService.ValidateCustomerFields(vm, States(), null));
    }

    [Fact]
    public void Validation_denormalises_the_state_name_onto_the_model()
    {
        // The legacy ValidateCustomer had this side effect; extraction preserves it.
        var vm = ValidLocalCustomer();
        vm.StateName = "whatever the user last saw";

        CustomerService.ValidateCustomerFields(vm, States(), null);

        Assert.Equal("Tamil Nadu", vm.StateName);
    }

    // ------------------------------------------------------------------ BR-CUST-009 / BR-CUST-012

    [Fact]
    public void Named_consignees_must_carry_a_valid_gst_and_pan_with_the_name_in_the_message()
    {
        var vm = ValidLocalCustomer();
        var indirects = new List<CustomerIndirectVM>
        {
            new CustomerIndirectVM { AltCustName = "Beta Works" }
        };

        var errors = CustomerService.ValidateCustomerFields(vm, States(), indirects);

        Assert.Contains("Consignee 'Beta Works': Please enter GST No.", errors);
        Assert.Contains("Consignee 'Beta Works': Please enter PAN No.", errors);
    }

    [Fact]
    public void Consignee_gst_length_and_format_are_checked_separately()
    {
        var vm = ValidLocalCustomer();

        var shortGst = CustomerService.ValidateCustomerFields(vm, States(), new List<CustomerIndirectVM>
        {
            new CustomerIndirectVM { AltCustName = "Beta Works", GSTNo = "33AAACA1111A1Z", PANNo = "AAACA1111A" }
        });
        Assert.Contains("Consignee 'Beta Works': GST No must be 15 characters.", shortGst);

        var badGst = CustomerService.ValidateCustomerFields(vm, States(), new List<CustomerIndirectVM>
        {
            new CustomerIndirectVM { AltCustName = "Beta Works", GSTNo = "AA AACA1111A1Z5", PANNo = "AAACA1111A" }
        });
        Assert.Contains("Consignee 'Beta Works': Invalid GST No format.", badGst);

        var badPan = CustomerService.ValidateCustomerFields(vm, States(), new List<CustomerIndirectVM>
        {
            new CustomerIndirectVM { AltCustName = "Beta Works", GSTNo = "33AAACA1111A1Z5", PANNo = "nope" }
        });
        Assert.Contains("Consignee 'Beta Works': Invalid PAN No format.", badPan);
    }

    [Fact]
    public void Blank_named_consignee_rows_are_skipped_by_validation()
    {
        // BR-CUST-012 — a blank-named row is silently ignored, never reported.
        var vm = ValidLocalCustomer();
        var indirects = new List<CustomerIndirectVM>
        {
            new CustomerIndirectVM { AltCustName = "  ", GSTNo = null, PANNo = null }
        };

        Assert.Empty(CustomerService.ValidateCustomerFields(vm, States(), indirects));
    }

    [Fact]
    public void Blank_named_child_rows_are_never_persisted()
    {
        var indirects = new List<CustomerIndirectVM>
        {
            new CustomerIndirectVM { AltCustName = "Beta Works" },
            new CustomerIndirectVM { AltCustName = "   " },
            new CustomerIndirectVM { AltCustName = null }
        };
        Assert.Single(CustomerService.PersistableIndirects(indirects));

        var contacts = new List<ContactPersonVM>
        {
            new ContactPersonVM { ContactPersonName = "Ravi" },
            new ContactPersonVM { ContactPersonName = "" }
        };
        Assert.Single(CustomerService.PersistableContacts(contacts));
    }

    // ------------------------------------------------------------------ BR-CUST-010

    [Fact]
    public void An_opening_balance_derives_the_pending_balance_and_its_date()
    {
        var customer = new Customer { OpenBal = 1500m };

        CustomerService.ApplyOpeningBalance(customer);

        Assert.Equal(1500m, customer.OpenBalPndg);
        Assert.NotNull(customer.OpenBalDate);
    }

    [Fact]
    public void No_opening_balance_clears_both_derived_fields()
    {
        var customer = new Customer { OpenBal = null, OpenBalPndg = 900m, OpenBalDate = new DateTime(2020, 1, 1) };

        CustomerService.ApplyOpeningBalance(customer);

        Assert.Null(customer.OpenBalPndg);
        Assert.Null(customer.OpenBalDate);
    }

    [Fact]
    public void Re_saving_an_existing_customer_resets_the_pending_balance()
    {
        // BR-CUST-010 — applied on update as well as create, so a part-paid opening balance is
        // silently reset by an unrelated edit. Preserved, not fixed. See Q-107.
        var customer = new Customer
        {
            CustId = 7,
            OpenBal = 1000m,
            OpenBalPndg = 250m,
            OpenBalDate = new DateTime(2024, 4, 1)
        };

        CustomerService.ApplyOpeningBalance(customer);

        Assert.Equal(1000m, customer.OpenBalPndg);
        Assert.NotEqual(new DateTime(2024, 4, 1), customer.OpenBalDate);
    }

    // ------------------------------------------------------------------ BR-CUST-013

    [Fact]
    public void Consignee_rows_missing_from_the_editor_list_are_deleted()
    {
        var original = new List<CustomerIndirect>
        {
            new CustomerIndirect { AltCustId = 1, AltCustName = "Kept" },
            new CustomerIndirect { AltCustId = 2, AltCustName = "Removed" }
        };
        var current = new List<CustomerIndirectVM>
        {
            new CustomerIndirectVM { AltCustId = 1, AltCustName = "Kept" },
            new CustomerIndirectVM { AltCustId = 0, AltCustName = "Newly added" }
        };

        Assert.Equal(new[] { 2 }, CustomerService.IndirectIdsToDelete(original, current).ToArray());
    }

    [Fact]
    public void An_existing_consignee_whose_name_was_blanked_is_retained_not_deleted()
    {
        // BR-CUST-013 — the retained set is built from ids, not names, so blanking the name of a
        // persisted row neither deletes it nor updates it: the row survives unchanged. This is a
        // defect, preserved verbatim. See Q-108.
        var original = new List<CustomerIndirect>
        {
            new CustomerIndirect { AltCustId = 1, AltCustName = "Beta Works" }
        };
        var current = new List<CustomerIndirectVM>
        {
            new CustomerIndirectVM { AltCustId = 1, AltCustName = "   " }
        };

        Assert.Empty(CustomerService.IndirectIdsToDelete(original, current));
        Assert.Empty(CustomerService.PersistableIndirects(current));
    }

    [Fact]
    public void Contact_rows_missing_from_the_editor_list_are_deleted()
    {
        var original = new List<ContactPerson>
        {
            new ContactPerson { Id = 10, ContactPersonName = "Kept" },
            new ContactPerson { Id = 11, ContactPersonName = "Removed" }
        };
        var current = new List<ContactPersonVM>
        {
            new ContactPersonVM { Id = 10, ContactPersonName = "Kept" },
            new ContactPersonVM { Id = 0, ContactPersonName = "Newly added" }
        };

        Assert.Equal(new[] { 11 }, CustomerService.ContactIdsToDelete(original, current).ToArray());
    }

    [Fact]
    public void An_empty_editor_list_deletes_every_persisted_child()
    {
        var original = new List<CustomerIndirect>
        {
            new CustomerIndirect { AltCustId = 1 },
            new CustomerIndirect { AltCustId = 2 }
        };

        Assert.Equal(new[] { 1, 2 }, CustomerService.IndirectIdsToDelete(original, new List<CustomerIndirectVM>()).ToArray());
    }
}
