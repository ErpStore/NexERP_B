---
doc_id: KB-031
title: Customer Master — @code triage and business rules
module: business-rules
source_files:
  - V.SMART/V.SMART.Shared/Pages/Master_Module_pages/Customer_Pages/CustomerUpsert.razor
  - V.SMART/V.SMART.Shared/Pages/Master_Module_pages/Customer_Pages/CustomerList.razor
  - V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/MasterService/GeneralService/CustomerService.cs
  - V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/IBusinessService/IMasterServices/IGeneralService/ICustomerService.cs
  - V.SMART/V.SMART.Shared/ViewModels/MasterViewModel/GeneralViewModel/CustomerVM.cs
  - V.SMART/V.SMART.Shared/ViewModels/MasterViewModel/GeneralViewModel/CustomerIndirectVM.cs
  - V.SMART/V.SMART.Shared/ViewModels/MasterViewModel/GeneralViewModel/ContactPersonVM.cs
  - V.SMART/V.SMART.Shared/Data/Master/General_Module/Customer.cs
  - V.SMART/V.SMART.Shared/Data/Master/General_Module/CustomerIndirect.cs
  - V.SMART/V.SMART.Shared/Data/Master/General_Module/ContactPerson.cs
  - V.SMART/V.SMART.Shared/Mappings/MasterMapping/GeneralMasterProfile/CustomerMapping.cs
  - V.SMART/V.SMART.Shared/Mappings/MasterMapping/GeneralMasterProfile/ContactPersonMapping.cs
  - V.SMART/V.SMART.Shared/Repository/MasterRepository/Generals/CustomerRepository.cs
  - V.SMART/V.SMART.Shared/Repository/Repository.cs
  - V.SMART/V.SMART.Shared/Services/ForeignKeyUsageChecker.cs
  - V.SMART/V.SMART.Shared/Migrations/20260217110637_InitialCreate.cs
entities: [Customer, CustomerIndirect, ContactPerson, State, Currency, ItemSub]
database_tables: [Customer, CustomerIndirect, ContactPerson, State, Currency]
business_rules:
  [
    BR-CUST-001,
    BR-CUST-002,
    BR-CUST-003,
    BR-CUST-004,
    BR-CUST-005,
    BR-CUST-006,
    BR-CUST-007,
    BR-CUST-008,
    BR-CUST-009,
    BR-CUST-010,
    BR-CUST-011,
    BR-CUST-012,
    BR-CUST-013,
    BR-CUST-014,
    BR-CUST-015,
    BR-CUST-016,
    BR-CUST-017,
    BR-CUST-018,
  ]
status: complete
confidence: mixed
last_verified: 2026-08-28
dependencies: [KB-030, KB-003, KB-011, KB-020, ADR-002, ADR-004, TASK-M2-D02-01]
---

# Customer Master — `@code` triage and business rules

Produced by **M2-D02-01**, the first instantiation of **INV-024** (`@code` triage per module)
and the first application of migration principle 3, *extract before rebuild*
([KB-080 §3](../execution/README.md)). Everything here is a template for every later
`<W>-02` task.

**All `file:line` citations are pre-extraction unless marked "(after)".** They point at the
code as it stood on `master` at the start of this task; the same code now lives in
`CustomerService`, and the razor lines it came from no longer exist. That is deliberate: the
evidence for a rule is where the rule was found, and the migration note says where it went.

---

## 1. Measured `@code` density

Measured with `grep -n '^@code' <file>` and `awk 'END{print NR}' <file>`.

| File                    | Total lines | `@code` opens at | `@code` block             | Share      |
| ----------------------- | ----------- | ---------------- | ------------------------- | ---------- |
| `CustomerList.razor`    | 984         | `:394`           | **591 lines** (`:394-984`)  | 60.1 %     |
| `CustomerUpsert.razor` **before** | 1,309 | `:678`      | **632 lines** (`:678-1309`) | 48.3 %     |
| `CustomerUpsert.razor` **after**  | 1,082 | `:679`      | **404 lines** (`:679-1082`) | 37.3 %     |

`CustomerUpsert.razor`'s `@code` shrank by **228 lines (36.1 %)**. The plan expected "at least
250 lines from `:954-1214` alone"; the shortfall is honest arithmetic, not an incomplete
extraction — `UpsertCustomer` + `ValidateCustomer` (`:954-1213`, 260 lines) were removed
entirely, but the replacement `UpsertCustomer` (a VM projection, an error-toast loop, the
navigation guard and the navigation) costs ~55 lines back, and ~25 lines of explanatory
comments were added at the extraction seams. No forbidden construct remains — see §5.

`CustomerList.razor` is **unchanged**. It is triaged below and not touched, as the task
requires.

The two route declarations are `@page "/customer"` (`CustomerList.razor:1`) and
`@page "/customer/create"` + `@page "/customer/update/{CustId:int}"`
(`CustomerUpsert.razor:1-2`). Both pages declare
`protected override string ScreenName => "Customer"` (`CustomerList.razor:398`,
`CustomerUpsert.razor:681` pre-extraction), matching the seeded
`Screens { Id = 15, ScreenCode = 15, ScreenName = "Customer" }` at
`Data/ApplicationDbContext.cs:1172` (Confirmed — the plan's `:1166` was stale by +6).

---

## 2. Triage — `CustomerUpsert.razor` `@code` (pre-extraction `:678-1309`)

Buckets are exactly `Presentation` (discard — rebuilt natively in Angular),
`Data loading` (becomes an API call) and `Business logic` (extract to `CustomerService`).
A member that spans buckets is split into rows.

| Member                                | Lines (before) | Bucket         | Disposition                              | Note |
| ------------------------------------- | -------------- | -------------- | ---------------------------------------- | ---- |
| `ScreenName => "Customer"`            | `:681`         | Presentation   | Kept in page; becomes `[RequireScreen]` on the controller in M2-D02-02 | The permission model is UI-side by design (BR-AUTH-002, ADR-004). |
| `CustId` parameter                    | `:683-684`     | Presentation   | Discard (becomes a route param)          | |
| `isCustModelVisible`, `OnClose`       | `:686-689`     | Presentation   | Discard                                  | Modal-host plumbing. |
| `isProcessing`                        | `:691`         | Presentation   | Discard                                  | Busy flag. |
| `Customer` model                      | `:692`         | Presentation   | Discard                                  | Form model; the API's model is `CustomerVM`. |
| `Indirects`, `ContactPersons`         | `:695-696`     | Presentation   | Discard                                  | Editor lists for the child grids. |
| `States`, `Currencies`                | `:697-698`     | Data loading   | Reference-data endpoints (M2-B09)        | |
| `HasMfgDcForCustomer`                 | `:700`         | Presentation   | Discard                                  | Declared, never assigned. Dead. |
| `currentPage`                         | `:703-704`     | Presentation   | Discard                                  | Round-trips the list's paging state. |
| `activeTab`, `SetTab`, `IsNoTabActive`| `:706-708`     | Presentation   | Discard                                  | Tab state. |
| `SelectedBusinessType`                | `:710`         | Presentation   | Discard                                  | Declared, never read. Dead. |
| `CustomerTypes`                       | `:711`         | Presentation   | Discard (list is now served by the service) | Bound by the customer-type dropdown; its **contents** are BR-CUST-004 and moved. |
| `username`                            | `:713`         | Data loading   | Discard — the server knows the user       | |
| `customerFormBreadcrumbs`             | `:715-720`     | Presentation   | Discard                                  | |
| `OnCustomerIndirectGstChanged`        | `:722-742`     | **Business logic** | **Extracted** → `NormalizeConsigneeGst` + `DerivePanFromGst` | BR-CUST-002, BR-CUST-003. Page keeps a 6-line wrapper. |
| `OnMainCustomerGSTChanged`            | `:744-768`     | **Business logic** | **Extracted** → `NormalizeCustomerGst` + `DerivePanFromGst` | BR-CUST-002, BR-CUST-003. |
| `OnStateChanged`                      | `:770-799`     | Presentation   | Kept in page (lookup in the loaded list)  | **Justified (>20 lines):** the *decision* it encodes — that `StateName` is denormalised from the matched `State` — is BR-CUST-008 and is enforced authoritatively in `ValidateCustomerFields`. What is left here is a `FirstOrDefault` over an already-loaded reference list, which is exactly what a `<p-select>` will do natively. |
| `OnInitializedAsync`                  | `:801-835`     | Data loading   | Discard — replaced by a resolver/effect  | **Justified (>20 lines):** rights load, three sequential reference/data loads, then one business call (`LoadCustomerTypesForBusinessType`) whose logic is extracted. Pure orchestration otherwise. |
| `LoadCustomerTypesForBusinessType`    | `:837-878`     | **Business logic** (list + default) / Presentation (the bound list itself) | **Extracted** → `GetCustomerTypesForBusinessType`, `ResolveSupplyType` | BR-CUST-004. Split row: the allowed set and the default are business; `CustomerTypes.Clear()/AddRange` is binding. |
| `LoadStates`                          | `:880-894`     | Data loading   | Reference-data endpoint (M2-B09)          | Kept on `IUnitOfWork` for now — see §6. |
| `LoadCurrencies`                      | `:896-910`     | Data loading   | Reference-data endpoint (M2-B09)          | Kept on `IUnitOfWork` for now — see §6. |
| `LoadCustomerData`                    | `:912-944`     | Data loading   | `GET /customers/{id}` — service method added: `GetCustomerByIdAsync` | **Justified (>20 lines):** every branch is "fetch or start empty"; no decision. The page still calls the repository directly because its form binds the **entity** — see §6. |
| `BusinessTypes`                       | `:946-952`     | Presentation   | Discard                                  | A static four-item display list. The *meaning* of the four values is BR-CUST-004/005, which is extracted. |
| `UpsertCustomer` — duplicate check    | `:960-966`     | **Business logic** | **Extracted** → `ValidateCustomerAsync`   | BR-CUST-001. |
| `UpsertCustomer` — validate + toast loop | `:968-977`  | **Business logic** (validate) / Presentation (toasts) | **Extracted** (validate) / kept (toasts) | Split row. |
| `UpsertCustomer` — opening balance    | `:980-990`     | **Business logic** | **Extracted** → `ApplyOpeningBalance`     | BR-CUST-010. |
| `UpsertCustomer` — create branch      | `:992-1025`    | **Business logic** | **Extracted** → `UpsertCustomerAsync`     | BR-CUST-011, -012. |
| `UpsertCustomer` — update branch      | `:1027-1097`   | **Business logic** | **Extracted** → `UpsertCustomerAsync`     | BR-CUST-011, -012, -013. |
| `UpsertCustomer` — transaction + rollback | `:957`, `:1085-1086`, `:1113-1120` | **Business logic** | **Extracted**                             | The page no longer opens a transaction. |
| `UpsertCustomer` — `LogUserAction`    | `:1015-1021`, `:1087-1093` | **Business logic** | **Extracted**                             | Screen `"Customer"`, action text preserved verbatim. |
| `UpsertCustomer` — toasts, `NavigationGuard.clear`, navigation | `:1023`, `:1095`, `:1100-1111` | Presentation | **Kept in page**                          | The three things the task explicitly excludes from the extraction. |
| `ValidateCustomer`                    | `:1127-1213`   | **Business logic** | **Extracted** → `ValidateCustomerFields`  | BR-CUST-006, -007, -008, -009, -018. Message strings byte-identical. |
| `OnBusinessTypeChanged`               | `:1216-1253`   | **Business logic** (the forcing rule) / Presentation (the state lookup) | **Extracted** → `IsImportOrExport`, `ShouldClearOnBusinessTypeSwitch`, `UrpGstNo`, `UrpStateCode` | BR-CUST-005. Split row. |
| `AddContact`                          | `:1255-1275`   | Presentation   | Discard                                  | **Justified (>20 lines):** 15 of its 21 lines are a `try`/`catch` and a toast; the only rule is "do not add a second blank row", a grid-editor affordance the Angular editor implements natively. It is **not** the persistence rule — that is BR-CUST-012. |
| `RemoveContact`                       | `:1277-1280`   | Presentation   | Discard                                  | |
| `AddCustomerIndirect`                 | `:1282-1302`   | Presentation   | Discard                                  | Same justification as `AddContact`. |
| `RemoveCustomerIndirect`              | `:1304-1307`   | Presentation   | Discard                                  | Removes from the editor list only; the persisted row is deleted by the id-set diff (BR-CUST-013). |

---

## 3. Triage — `CustomerList.razor` `@code` (`:394-984`, unchanged by this task)

This page is the **contrast case** that makes the triage meaningful: 591 lines of `@code` with
essentially no business logic in it.

| Member                                        | Lines        | Bucket        | Disposition | Note |
| --------------------------------------------- | ------------ | ------------- | ----------- | ---- |
| `ScreenName => "Customer"`                    | `:398`       | Presentation  | Discard     | |
| `isProcessing`, `preferencesLoaded`           | `:400-401`   | Presentation  | Discard     | |
| `UserName`                                    | `:402`       | Data loading  | Discard     | |
| `DeleteCustId`                                | `:403`       | Presentation  | Discard     | Modal state. |
| `searchText`, `selectedStatus`                | `:404-405`   | Presentation  | Discard     | |
| `CustomerVMs`                                 | `:407`       | Data loading  | Becomes the grid's data source | |
| `currentPage`, `totalItems`, `pageSize`, `totalPages` | `:411-419` | Presentation | Discard | Paging state; the server owns the page **contents** (BR-CUST-016). |
| `Columns`                                     | `:421`       | Presentation  | Discard     | |
| Modal fields (`isModalVisible` … `cancelReason`) | `:425-434` | Presentation  | Discard     | |
| `Filter`, `SearchFilters`                     | `:437-438`   | Presentation  | Becomes query parameters | The **filter semantics** are BR-CUST-016 and already live in `CustomerService`. |
| `QuoteBreadcrumbs`                            | `:440-449`   | Presentation  | Discard     | |
| `OnInitializedAsync`                          | `:451-493`   | Data loading  | Discard     | **Justified (>20 lines):** rights, preferences, columns, first page. Orchestration only. |
| `LoadAllCustomers`                            | `:495-513`   | Data loading  | `GET /customers` (M2-D02-02) | Calls `SearchWithDynamicFilterAsync`; no logic of its own. |
| `AddNewQuote`                                 | `:515-524`   | Presentation  | Discard     | Navigation. |
| `InitializeColumns`                           | `:526-621`   | Presentation  | Discard     | **Justified (>20 lines):** 96 lines of column metadata — labels, widths, formats. Pure display description. |
| `LoadColumnPreferences`                       | `:623-644`   | Data loading  | Column-preference endpoint (M2-C05-02) | |
| `HandleColumnChange`                          | `:646-662`   | Presentation  | Discard     | |
| `FirstPage` / `PreviousPage` / `NextPage` / `LastPage` | `:664-751` | Presentation | Discard | Four near-identical 22-line paging handlers. **Justified (>20 lines):** each is bounds-check + `LoadAllCustomers()`. |
| `OnPageSizeChanged`                           | `:753-771`   | Presentation  | Discard     | |
| `RefreshPage`                                 | `:773-796`   | Presentation  | Discard     | |
| `HandleDelete`                                | `:798-825`   | Presentation (prompt) — the **decision** is already server-side | Calls `CanDeleteCustomerAsync` | **Justified (>20 lines):** the guard itself is BR-CUST-014 and already lives in `CustomerService:143-169`; the page only turns its message into a toast or a confirmation modal. This is what "already extracted" looks like. |
| `ConfirmDelete_Click`                         | `:827-863`   | Presentation  | Calls `DeleteCustomerByCustIdAsync` | **Justified (>20 lines):** confirmation handling, toasts, reload. The delete is BR-CUST-015, server-side already. |
| `HandleModalConfirmation`                     | `:865-884`   | Presentation  | Discard     | |
| `OpenModal` / `CloseModal`                    | `:886-924`   | Presentation  | Discard     | |
| `ApplySearchFilters`                          | `:926-951`   | Presentation  | Becomes query parameters | **Justified (>20 lines):** builds a `Dictionary<string,object>`; the keys' meaning is BR-CUST-016, already server-side. |
| `OnStatusChanged`                             | `:953-980`   | Presentation  | Becomes a query parameter | The `Active` / `In Active` mapping is BR-CUST-016 and is already in `CustomerFilterBuilder`. |

**Result:** `CustomerList.razor` contributes **zero** new extraction work. Every decision it
makes was already behind `ICustomerService`. That is the profile a `<W>-02` task should hope
for, and `CustomerUpsert.razor` is the profile it should expect.

---

## 4. Business rules

Format follows KB-030. **Disposition** says what M2-D02-01 did with the rule; **migration
note** says what the Angular/API side must do with it.

### BR-CUST-001 — Customer name must be unique

- **Statement:** Before any row is written, the save checks
  `ExistsByNameAsync("CustName", CustName?.Trim(), "CustId", excludeId)` — `excludeId` is
  `null` on create and `CustId` on update. On a hit the save aborts with
  **"Customer name already exists."** and nothing is written, and **no other validation message
  is produced**.
- **Evidence:** `CustomerUpsert.razor:960-966`; `Repository/Repository.cs:185-224`.
- **Confidence:** Confirmed.
- **Disposition:** Extracted to `CustomerService.ValidateCustomerAsync`, which runs the check
  first and returns that one message alone, reproducing the legacy short-circuit exactly.
- **Migration note:** `Repository.cs:218-224` catches every exception and **returns `false`** —
  the uniqueness check **fails open**. Also, the comparison is a plain SQL equality, so its case
  sensitivity is the column collation's, not the code's; the collation was not observed
  (recorded as an unknown in §7). M2-D02-02 must map this to a 409 or a field-keyed 400, not to
  a 500.

### BR-CUST-002 — PAN is derived from GST

- **Statement:** When GST is exactly 15 characters, `PANNo = GSTNo.Substring(2, 10)`; otherwise
  PAN is cleared to `string.Empty`. Applies to the customer and, independently, to every
  consignee row.
- **Evidence:** `CustomerUpsert.razor:729-736` (consignee), `:753-761` (customer).
- **Confidence:** Confirmed.
- **Disposition:** Extracted to `CustomerService.DerivePan` / `DerivePanFromGst`. The two legacy
  call sites differed trivially — the customer used `string.IsNullOrEmpty` and the consignee
  `string.IsNullOrWhiteSpace` — and the difference is unreachable because the customer path
  trims first. One method now serves both; the reconciliation is commented at the method.
- **Migration note:** the Angular form must **ask the server** for the PAN, not re-implement the
  substring (ADR-002 §3).

### BR-CUST-003 — GST casing is normalised asymmetrically

- **Statement:** The customer's GST is `.ToUpper().Trim()`; a consignee's GST is `.Trim()`
  **only**, never upper-cased.
- **Evidence:** `CustomerUpsert.razor:750` vs `:727`.
- **Confidence:** Confirmed that the asymmetry exists. **Unknown** whether it is intended.
- **Disposition:** **Preserved verbatim**, as two separately named methods
  (`NormalizeCustomerGst`, `NormalizeConsigneeGst`) so the asymmetry is visible rather than
  accidental. Raised as **Q-106**; not fixed here.
- **Migration note:** a lower-case consignee GST fails BR-CUST-009's pattern (which requires
  `[A-Z]`), so the user can be blocked by a value the UI itself produced. Do not "fix" this in
  Angular — it would diverge from Blazor and break the M2-D03 parity comparison.

### BR-CUST-004 — Business type constrains customer type (`SupTyp`)

- **Statement:** `Local` / `InterState` → `{B2B, SEZWP, SEZWOP}`, defaulting to `B2B`;
  `Imports` / `Exports` → `{SEZWP, SEZWOP, EXPWP, EXPWOP}`, defaulting to `SEZWP`; any other
  non-empty value sets `SupTyp = null`. The default is applied only when the current value is
  empty or not in the allowed set. An **empty** business type leaves `SupTyp` untouched.
- **Evidence:** `CustomerUpsert.razor:837-878`.
- **Confidence:** Confirmed.
- **Disposition:** Extracted to `GetCustomerTypesForBusinessType` + `ResolveSupplyType` (and the
  task-mandated `ApplyBusinessTypeDefaults(CustomerVM)`, which delegates to them). The allowed
  sets are `public static readonly` on the service, not repeated literals.
- **Migration note:** the Angular dropdown asks the server for its options.

### BR-CUST-005 — Imports/Exports forces URP

- **Statement:** Selecting `Imports` or `Exports` forces `GSTNo = "URP"`, selects the state with
  `StateCode == 99` and clears PAN. Switching **away** clears GST, PAN and state **only if** the
  current GST is blank, shorter than 15 characters, or literally `URP` (case-insensitive) — a
  real GST already typed survives the switch.
- **Evidence:** `CustomerUpsert.razor:1226-1247`.
- **Confidence:** Confirmed.
- **Disposition:** Extracted to `IsImportOrExport`, `ShouldClearOnBusinessTypeSwitch`, and the
  constants `UrpGstNo = "URP"` / `UrpStateCode = 99`. The page keeps only the lookup of state 99
  in its already-loaded list.
- **Migration note:** state 99 must exist in the tenant's `State` table for this to do anything;
  the legacy code silently does nothing if it does not.

### BR-CUST-006 — GST validation branches on business type

- **Statement:** For `Imports`/`Exports` (compared on the **trimmed** business type), GST must
  equal `URP` case-insensitively → **"For Imports/Exports, GST No must be 'URP'."**. Otherwise
  GST is required (**"Please enter GST No."**), must be exactly 15 characters (**"GST No must be
  15 characters."**) and must match
  `^(URP|[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z]{1}[1-9A-Z]{1}Z[0-9A-Z]{1})$` (**"Invalid GST No
  format."**). The three are `else if`-chained, so at most one fires.
- **Evidence:** `CustomerUpsert.razor:1140-1166`.
- **Confidence:** Confirmed.
- **Disposition:** Extracted verbatim into `CustomerService.ValidateCustomerFields`; the pattern
  is the constant `CustomerService.GstPattern`.
- **Migration note:** M2-D02-03 transcribes the pattern **from this document** as a display-time
  mirror only; the decision stays server-side.

### BR-CUST-007 — PAN validation

- **Statement:** For `Imports`/`Exports`, PAN is optional but, if non-blank, must match
  `^[A-Z]{5}[0-9]{4}[A-Z]{1}$` → **"Invalid PAN No format for Imports/Exports."**. Otherwise PAN
  is **required** (**"Please enter PAN No."**) and must match the same pattern (**"Invalid PAN No
  format."**).
- **Evidence:** `CustomerUpsert.razor:1147-1151`, `:1168-1175`.
- **Confidence:** Confirmed.
- **Disposition:** Extracted verbatim; pattern is `CustomerService.PanPattern`.
- **Migration note:** the same shape as BR-CUST-006 — M2-D02-03 may transcribe `PanPattern`
  **from this document** as a display-time mirror only, and must take the field's required-ness
  from the business type (optional for `Imports`/`Exports`, required otherwise) rather than
  marking PAN always-required; the decision stays server-side. M2-D02-02 returns all three
  messages unchanged, and the two "invalid format" messages differ by business type, so the
  client must not collapse them into one.

### BR-CUST-008 — State must exist, and `StateName` is denormalised

- **Statement:** `StateId` must be non-zero and must match a `State.StateCode`, otherwise
  **"Please select a valid State."**. On a match, `StateName` is **written onto the customer**
  from the matched row — both when the user changes the dropdown and again during validation.
- **Evidence:** `CustomerUpsert.razor:770-799`, `:1178-1186`.
- **Confidence:** Confirmed.
- **Disposition:** Extracted. `ValidateCustomerFields` is deliberately **not pure**: it keeps the
  `StateName` write, because a validation that only returned messages would silently drop the
  denormalisation. The page copies `vm.StateName` back onto its form model after a successful
  save so the open form matches what was persisted.
- **Migration note:** the client must not send `StateName` and expect it honoured — the server
  overwrites it from `StateId`.

### BR-CUST-009 — Consignee GST/PAN validation

- **Statement:** Every consignee row **with a non-blank `AltCustName`** must carry a GST of
  exactly 15 characters matching the GST pattern and a PAN matching the PAN pattern. Every
  message is prefixed `Consignee '{AltCustName}': `.
- **Evidence:** `CustomerUpsert.razor:1188-1211`.
- **Confidence:** Confirmed.
- **Disposition:** Extracted verbatim.
- **Migration note:** the message carries user data in its text, so M2-D02-02 must return it as
  a message, not as a translation key.

### BR-CUST-010 — Opening balance derives the pending balance

- **Statement:** If `OpenBal` has a value, `OpenBalPndg = OpenBal` and
  `OpenBalDate = DateTime.Now`; otherwise both become `null`. Applied on **every** save, create
  and update alike — so editing an existing customer **resets its pending balance to the
  opening balance and re-stamps the date**, discarding any part-payment.
- **Evidence:** `CustomerUpsert.razor:981-990`.
- **Confidence:** Confirmed for the behaviour. **Inferred** that the reset on edit is a defect.
- **Disposition:** **Preserved verbatim** in `ApplyOpeningBalance`, and pinned by a test that
  asserts the reset. Raised as **Q-107**; not fixed here.
- **Migration note:** `OpenBalPndg` and `OpenBalDate` are **server-derived**, like `StateName` in
  BR-CUST-008 — a client that sends either one will have it overwritten from `OpenBal`, so
  M2-D02-02 must not accept them as writable input and M2-D02-03 must render them read-only.
  Because Q-107 may change the rule for the edit path, do **not** design the API contract around
  the reset being permanent; the answer changes only the server, not the payload shape.

### BR-CUST-011 — Audit stamping

- **Statement:** Create stamps `CreatedBy` (current username) and `CreatedDate = DateTime.Now`
  and never sets `Modified*`. Update stamps `ModifiedBy`/`ModifiedDate` and never re-sets
  `Created*`.
- **Evidence:** `CustomerUpsert.razor:993-995`, `:1030-1031`.
- **Confidence:** Confirmed.
- **Disposition:** Extracted. Two collisions with `CustomerMapping` had to be neutralised
  explicitly, and this is the part of the extraction most worth reading before copying the
  pattern: the profile sets `ModifiedDate = DateTime.Now` **unconditionally**
  (`CustomerMapping.cs:27` pre-change), so the create path clears `ModifiedBy`/`ModifiedDate`
  back to `null` after mapping; and `CreatedBy` maps from the VM, so the update path captures
  and restores the persisted `Created*` rather than trusting the caller.
- **Migration note:** the server owns these fields. A client-supplied `CreatedBy` is ignored.

### BR-CUST-012 — Blank-named child rows are silently discarded

- **Statement:** Consignee rows with a blank `AltCustName`, and contact rows with a blank
  `ContactPersonName`, are never inserted and never updated. No warning is shown.
- **Evidence:** `CustomerUpsert.razor:1000`, `:1007`, `:1047`, `:1073`.
- **Confidence:** Confirmed.
- **Disposition:** Preserved verbatim as `PersistableIndirects` / `PersistableContacts`. Raised
  as **Q-108**.
- **Migration note:** **the plan's wording of this rule was wrong and is corrected here.** The
  plan said such rows are "deleted because they fall out of the retained-id set". They are not —
  see BR-CUST-013.

### BR-CUST-013 — Child collections are synchronised by id-set difference

- **Statement:** On update, the retained set is built from the editor list's **non-zero ids**
  (`Indirects.Where(i => i.AltCustId != 0)`); every originally loaded child whose id is absent
  from it is deleted; editor rows with id 0 are inserted; the rest are updated. Each operation
  is followed by its own `SaveAsync` (an N+1 pattern, deliberately preserved).
- **Evidence:** `CustomerUpsert.razor:1028-1029`, `:1034-1084`.
- **Confidence:** Confirmed.
- **Disposition:** Extracted to `IndirectIdsToDelete` / `ContactIdsToDelete` plus the loops in
  `UpsertCustomerAsync`.
- **Migration note:** **corrects the plan.** Because the retained set is keyed on **id**, not
  name, an *existing* consignee whose name the user blanks stays in the retained set (so it is
  not deleted) and is skipped by the update loop (so it is not updated) — **the row survives in
  the database unchanged, and the user sees no error.** Only id-0 rows are truly discarded.
  Pinned by the test `An_existing_consignee_whose_name_was_blanked_is_retained_not_deleted`.
  Raised with BR-CUST-012 under **Q-108**.

### BR-CUST-014 — Delete guard

- **Statement:** Delete is refused when the customer does not exist (**"Customer not found or
  already removed."**), or when `ForeignKeyUsageChecker.GetUsageTableAsync<Customer>(custId)`
  reports a referencing table (**"Cannot delete Customer '{name}' because it is used in {table}
  Screen."**), or when the customer is inactive (**"Customer is inactive. You cannot delete this
  record."**). Messages are product UX and must survive verbatim.
- **Evidence:** `CustomerService.cs:143-169`; `Services/ForeignKeyUsageChecker.cs:21`.
- **Confidence:** Confirmed.
- **Disposition:** **Already server-side. Unchanged by this task.** `CustomerList.razor:802` is
  the only caller; `DeleteCustomerByCustIdAsync` does **not** call the guard itself.
- **Migration note:** ADR-002 §4 — 409 carrying the message verbatim. Note the guard is not
  enforced by the delete method, so the controller must call it explicitly.

### BR-CUST-015 — Delete deletes only the `Customer` row

- **Statement:** Delete runs inside `BeginTransactionAsync`, removes only the `Customer` row,
  commits, then writes a `LogUserAction` entry with screen `"Customer List"`. Child rows are not
  removed by the service.
- **Evidence:** `CustomerService.cs:172-212`.
- **Confidence:** Confirmed for the service body. **The FK question the plan left open is now
  answered:** `FK_CustomerIndirect_Customer_CustId` is
  `ReferentialAction.**Cascade**` (`Migrations/20260217110637_InitialCreate.cs:1495-1499`) and
  `FK_ContactPerson_Customer_CustId` is `ReferentialAction.**Restrict**` (`:1754-1758`). No later
  migration alters either (`grep` over `Migrations/` returns only these two definitions plus the
  `Down()` drop at `:10227`). Confirmed.
- **Disposition:** Unchanged.
- **Migration note:** consignees cascade; contact persons do **not**, so a customer with a
  contact person would be refused by the database if it ever reached the DELETE. In practice it
  probably never does — see the unknown in §7 about `ForeignKeyUsageChecker` counting the
  children themselves as blocking usages.

### BR-CUST-016 — List filtering and ordering

- **Statement:** Exactly five filter keys are honoured — `Customer` (LIKE on `CustName`),
  `CreatedBy` (LIKE), `FromDate` (`CreatedDate >= date`), `ToDate` (`CreatedDate <=` end of
  day), `Status` (`Active` → `!Inactive`, `In Active` → `Inactive`). Unknown keys are ignored
  silently. Ordering is fixed: `OrderByDescending(CustId)`.
- **Evidence:** `CustomerService.cs:82-140`, `:66`.
- **Confidence:** Confirmed.
- **Disposition:** **Already server-side. Unchanged.**
- **Migration note:** the fixed ordering means the list cannot be re-sorted without a service
  change; INV-041's dynamic-sort shape applies when M2-D02-02 needs it.

### BR-CUST-017 — `CustomerVM` could not round-trip its children *(defect — fixed by this task)*

- **Statement:** `CustomerVM` declared `CustomerIndirectVMs` and `ContactPersonVMs`
  (`CustomerVM.cs:134-135`), but `CustomerMapping` mapped neither: entity→VM customised only
  `CurrName`, and VM→entity explicitly ignored `CustomerIndirects`, `ContactPersons` and
  `ItemSubs`. The name mismatch meant AutoMapper would not match them by convention either.
- **Evidence:** `Mappings/.../CustomerMapping.cs:16-27` (pre-change); `CustomerVM.cs:134-135`;
  negative grep (§8).
- **Confidence:** Confirmed.
- **Disposition:** **Fixed — the one behaviour change this task authorises.** Both collections
  are now mapped in both directions. A second gap surfaced while doing it:
  `ContactPersonMapping.cs:21` declared a **duplicate** `CustomerIndirectVM → CustomerIndirect`
  map (a copy-paste) and **no** `ContactPersonVM → ContactPerson` map at all, so the collection
  mapping had no element map. The missing map was added; the erroneous duplicate was left alone
  as out of scope and recorded as **R-84**.
- **Migration note:** this is behaviour-*adding*, not behaviour-*altering* — nothing read those
  properties before (negative grep, §8). Proven by
  `tests/V.SMART.Shared.Tests/Services/CustomerMappingChildCollectionTests.cs`.

### BR-CUST-018 — Name and business type are unconditionally required

- **Statement:** Validation opens with two unconditional checks: **"Customer Name is
  required."** and **"Business Type is required."**.
- **Evidence:** `CustomerUpsert.razor:1131-1138`.
- **Confidence:** Confirmed.
- **Disposition:** Extracted verbatim. **This rule is new** — the plan's 17-rule table omitted
  it, and its message set would have been silently lost.
- **Migration note:** it duplicates the `[Required]` DataAnnotations on both `Customer` and
  `CustomerVM`, but with **different message text**; the server-side message is the one the API
  returns.

---

## 5. What the extraction actually changed

`ICustomerService` gained eleven members (`ICustomerService.cs`, after): `GetCustomerByIdAsync`,
`UpsertCustomerAsync`, `ValidateCustomerAsync`, `ApplyBusinessTypeDefaults`,
`GetCustomerTypesForBusinessType`, `ResolveSupplyType`, `DerivePanFromGst`,
`NormalizeCustomerGst`, `NormalizeConsigneeGst`, `IsImportOrExport`,
`ShouldClearOnBusinessTypeSwitch`. The pure rules are also exposed as `static` methods on
`CustomerService` so they can be characterised without a `DbContext`.

`UpsertCustomerAsync` returns
`(bool Success, string Message, IReadOnlyList<string> Errors, CustomerVM? Customer)` — the
conventional `(Success, Message, Vm)` tuple (`ICurrencyService.cs:15-16`) plus an `Errors` list,
because the legacy page showed **one toast per validation message** and a single `Message` could
not reproduce that. M2-D02-02 maps `Errors` to a 400 and `Message` to the 409/500 body.

**Verified by grep, on the post-extraction file:** `CustomerUpsert.razor` contains no
`BeginTransactionAsync`, no `ExistsByNameAsync`, no `_unitOfWork.Customers.CreateAsync` /
`UpdateAsync`, no `_unitOfWork.CustomerIndirects.*`, no `_unitOfWork.ContactPersons.*` and no
`Regex.IsMatch` — a combined `grep -c` over all seven patterns returns **0**.

**Verified by grep, per string:** every message string in the legacy `ValidateCustomer`
(`:1127-1213`) appears byte-identical in `CustomerService.cs`, as do
`"Customer name already exists."`, `"Customer Created Successfully"`,
`"Customer Updated Successfully"` and `"An error occurred while saving the customer."`.

Two additions were forced by crossing the entity/VM boundary and are recorded here rather than
left implicit:

1. **`CustomerVM` gained `VendorCode`.** `CustomerUpsert.razor:258` binds
   `Customer.VendorCode`, but `CustomerVM` had no such property. Once the save runs through the
   VM, an unmapped `VendorCode` would be silently dropped on create. Additive, mirroring
   `Customer.cs:28-29`; pinned by a test.
2. **`UpsertCustomerAsync` gained a not-found branch** returning the existing BR-CUST-014 string
   `"Customer not found or already removed."`. The legacy page could not reach this state (it
   held the loaded entity); a service addressed by id can.

---

## 6. Decisions taken, stated explicitly

**`@inject IUnitOfWork` stays in `CustomerUpsert.razor`, and `LoadStates` / `LoadCurrencies`
stay on it.** Implementation step 8 required this choice to be explicit rather than implicit.
The injection cannot be removed regardless, because `LoadCustomerData` still calls
`_unitOfWork.Customers.GetCustomerWithAllRelatedDataAsync` — the form binds the **entity**
`Customer` across ~670 lines of markup (`<EditForm Model="Customer">`,
`CustomerUpsert.razor:42`), and converting that binding to `CustomerVM` is a rewrite of the
markup, not an extraction of the `@code`. Moving two reference-data reads onto the service while
leaving the injection in place would have bought nothing. `GetCustomerByIdAsync` exists on the
service for M2-D02-02's use; `States` and `Currency` become reference-data endpoints in M2-B09.

**The service takes `CustomerVM`, not the `Customer` entity.** This was the design decision the
task left open, and it fixes the template for every later `<W>-02`. Reasons: KB-011's convention
is that services take and return ViewModels (`CustomerService.cs:44`, `:216` already do);
ADR-002 §2 makes the `…VM` the API payload; and a service that took entities would be unusable
from a controller. The cost is the audit-stamping collision described under BR-CUST-011 and the
EF-tracking care described next.

**How the update path avoids EF tracking conflicts.** `GetCustomerWithAllRelatedDataAsync` is a
**tracking** query (`CustomerRepository.cs:31-38`), and in Blazor Server the page and the service
share one scoped `ApplicationDbContext`. Mapping the VM to a *new* `Customer` and calling
`Update` would throw "another instance with the same key is already being tracked". So the update
path re-loads the tracked entity, snapshots its children and its `Created*` fields, maps the VM
**onto** it, restores the snapshot, and updates existing children by mapping onto the tracked
child instances. The create path clears the mapped child collections before `CreateAsync` so EF
does not insert them twice — the legacy per-row insert loop is preserved.

---

## 7. Unknowns raised, not guessed

| Question | Why it cannot be answered from source | Recorded as |
| -------- | ------------------------------------- | ----------- |
| Is the GST casing asymmetry (BR-CUST-003) intended? | Product intent. | **Q-106** |
| Is the opening-balance reset on edit (BR-CUST-010) intended? | Product intent. | **Q-107** |
| Should blank-named child rows be discarded silently, and should an existing row whose name is blanked survive (BR-CUST-012/-013)? | Product intent. | **Q-108** |
| Can a customer that has consignees or contact persons be deleted at all today? `ForeignKeyUsageChecker.GetUsageTableAsync<Customer>` enumerates every entity with an FK to `Customer` and excludes nothing (`Services/ForeignKeyUsageChecker.cs:21-47`), and `CustomerIndirect` and `ContactPerson` are themselves in that set — so BR-CUST-014's guard may refuse every customer that has a child, before BR-CUST-015's cascade ever matters. | Needs a live non-production tenant database; the source shows the mechanism but not the outcome. | **Q-109** |
| Is the duplicate-name check case-sensitive? | Depends on the tenant database's column collation, which was not observed. | **Q-109** (same row's evidence request) |

---

## 8. Negative results — recorded so nobody re-derives them

- **Grepped `docs/kb/` for `BR-CUST` before starting — no hits.** The `BR-CUST-*` series was
  unallocated and `docs/kb/business-rules/` contained only `business-rule-inventory.md`.
- **Grepped `V.SMART/` for `CustomerIndirectVMs` and `ContactPersonVMs` — hits only in
  `CustomerVM.cs:134-135`.** No producer, no consumer, in either the .NET tree or the Angular
  workspace. This is what made the BR-CUST-017 fix safe.
- **`ICustomerService` had no write method and no get-by-id before this task** — its four
  members were `CanDeleteCustomerAsync`, `SearchWithDynamicFilterAsync`,
  `DeleteCustomerByCustIdAsync`, `GetCustomerListAsync`.
- **`CustomerRepository.cs` is 266 lines of which one method is live** —
  `GetCustomerWithAllRelatedDataAsync` (`:31-38`). The rest is commented-out code including a
  dead `UpdateAsync` override. Left exactly as found.
- **The API has no Customer surface.** `V.SMART/V.SMART.Api/Controllers/` holds no Customer
  controller; "Customer" appears only in `Authorization/ScreenCatalogue.cs`,
  `Contracts/ReportQueries.cs` and `Controllers/SalesTrackReportController.cs`.
- **The FK answer BR-CUST-015 asked for:** `CustomerIndirect` → **Cascade**, `ContactPerson` →
  **Restrict**. See BR-CUST-015.
- **The plan's claim that `ICustomerService` is not registered in the API host is stale.**
  `DependencyInjection/ServiceCollectionExtensions.cs:478` registers it and
  `V.SMART.Api/Program.cs` calls `AddVSmartDomain`. M2-B07 is satisfied; no one-off `AddScoped`
  was added, and `Program.cs` was not touched.
- **`ApplicationDbContext` applies exactly three `IEntityTypeConfiguration`s** —
  `UserConfiguration:432`, `ItemConfiguration:434`, `CustomerConfiguration:436` (the plan's
  `430/432/434` were stale by +2). Not modified by this task.

---

## 9. Verification evidence

| What | Command | Observed |
| ---- | ------- | -------- |
| API host builds | `dotnet build V.SMART/V.SMART.Api/V.SMART.Api.csproj` | **0 errors, 6,695 warnings** — exactly the KB-083 baseline |
| Blazor host builds | `dotnet build V.SMART/V.SMART.Web/V.SMART.Web.csproj` | **0 errors, 5 warnings** (incremental: `V.SMART.Shared` was already built by the previous command, so its ~6,690 warnings were not re-emitted) |
| Tests | `dotnet test tests/V.SMART.Shared.Tests/V.SMART.Shared.Tests.csproj` | **166 passed, 0 failed, 1 skipped** (the skip is the pre-existing `MfgPoServiceDeleteGuardTests` skip) |
| Customer tests alone | same, `--filter "FullyQualifiedName~Customer"` | **64 passed, 0 failed** |
| Service round trip over a real `DbContext` | same, `--filter "FullyQualifiedName~CustomerServiceRoundTripTests"` | **2 passed, 0 failed** |

### Service round trip through EF — `CustomerServiceRoundTripTests`

`tests/V.SMART.Shared.Tests/Services/CustomerServiceRoundTripTests.cs` drives the real
`CustomerService` over a real `ApplicationDbContext` (Microsoft.EntityFrameworkCore.**InMemory**,
per INV-031 — Sqlite cannot host this model) and the real `CustomerRepository`,
`CustomerIndirectRepository`, `ContactPersonRepository` and `StateRepository`. Only
`IUnitOfWork.BeginTransactionAsync` is faked, because the InMemory provider has no transactions.

It is what satisfies the task's *Testing Requirements* **integration** row: `GetCustomerByIdAsync`
→ `UpsertCustomerAsync` preserves consignees and contact persons, ids included. Because
`UpsertCustomerAsync` catches every exception and returns
`(false, "An error occurred while saving the customer.")`, an EF tracking failure in the update
path (§6) surfaces as a failed `Success` assertion rather than as a thrown exception — so these
two tests do reach the `_mapper.Map(vm, existing)` sequence that no test against the static
helpers can.

**What it does and does not settle.** It covers manual scenarios 1, 2 and 10 below *at the EF
change-tracker level*. It settles **nothing** that depends on SQL Server: InMemory does not
enforce foreign keys, does not translate LINQ to SQL, and has no collation, so the duplicate-name
comparison (Q-109), the delete guard's real refusal text and identity/`DELETE` behaviour are
still unobserved. The scenarios below remain owed in full.

### Manual Blazor validation — NOT PERFORMED

Implementation step 11 requires exercising the eleven scenarios through the running Blazor UI
against a non-production tenant database and inspecting the persisted rows. **This was not done,
and nothing here should be read as though it had been.** No tenant database was available to
this session, and no Blazor host was started. The extraction is therefore evidenced by the unit
tests, the build and the diff — not by observed runtime behaviour.

The scenarios still owed, each of which touches a code path the unit tests cannot reach because
it needs EF and a real `DbContext`:

1. Create a `Local` customer with two consignees and two contact persons; confirm both child
   tables receive rows with the new `CustId`.
2. Edit it, removing one consignee and adding another; confirm one `DELETE` and one `INSERT`.
3. Enter a blank-named consignee row; confirm it is still silently dropped (BR-CUST-012).
4. Blank the name of an *existing* consignee; confirm the row **survives unchanged**
   (BR-CUST-013, the corrected reading).
5. Switch business type to `Exports`; confirm GST becomes `URP`, state becomes 99, PAN clears.
6. Save a duplicate name; confirm the same single toast, and that no row was written.
7. Attempt to delete a referenced customer; confirm the same refusal text — and record which
   message appears, which answers **Q-109**.
8. Confirm the create path leaves `ModifiedBy`/`ModifiedDate` `NULL` (BR-CUST-011 vs the
   mapper's unconditional `ModifiedDate`) and the update path leaves `Created*` intact.
9. Confirm `VendorCode` survives a create (§5 item 1).
10. Confirm an update does not trip an EF tracking exception (§6, the tracking discussion) —
    this is the single highest-risk consequence of the entity→VM boundary crossing.
11. Confirm the "Customer Updated Successfully" toast, the navigation guard clear and the
    redirect to `/customer` are unchanged.
