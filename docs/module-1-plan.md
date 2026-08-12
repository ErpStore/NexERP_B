# Module 1 Plan — Currency Master

> First Angular feature slice for V.SMART NexGen ERP modernization.  
> Goal: teach Standalone Components, Signals, HttpClient, Reactive Forms, and PrimeNG against a thin Web API that reuses existing business logic.

---

## 1. Why Currency?

| Factor | Currency | Customer (Module 2) |
|--------|----------|---------------------|
| Editable fields | 4 (`CurrName`, `CurrSub`, `Symbol`, audit) | 30+ + child collections |
| Legacy pattern | List + Upsert (standard ERP CRUD) | Same, but much heavier |
| Service surface | `ICurrencyService` + UoW create/update | Complex related-data upsert |
| Learning value | Full CRUD without form overwhelm | Apply patterns learned here |

**Legacy screens**

| Route | File |
|-------|------|
| `/currency` | `V.SMART.Shared/Pages/Master_Module_pages/Currency_Pages/CurrencyList.razor` |
| `/currency/create` | `CurrencyUpsert.razor` |
| `/currency/update/{CurrId}` | `CurrencyUpsert.razor` |

**Backend**

- Entity: `Data/Master/Accounts_Module/Currency.cs`
- DTO: `ViewModels/MasterViewModel/AccountsViewModel/CurrencyVM.cs`
- Service: `ICurrencyService` / `CurrencyService` (search, can-delete, delete)
- Create/Update today lives in the Razor page via `IUnitOfWork.Currencyis` — API will encapsulate this

---

## 2. User workflows & screen requirements

### 2.1 Currency List

1. User opens Currency List.
2. App loads paginated rows (default page size 10).
3. User can:
   - **Search** by `CurrName`, `CreatedBy`, `FromDate`, `ToDate`
   - Change **page size** (10 / 20 / 50)
   - **Refresh** the grid
   - **Add New** → navigate to create form
   - **Edit** a row → navigate to update form
   - **Delete** a row → confirm → API checks FK / system-defined → delete or show error

**List columns (from legacy):** CurrId, CurrName, CurrSub, Symbol, IsSystemDefined, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate

### 2.2 Currency Create / Update

1. Create: empty form. Update: load by `CurrId`.
2. Validate:
   - `CurrName` required, max 100
   - `CurrSub` required, max 100
   - `Symbol` required; one of `$ € ₹ ¥ £ ₩ ₿ ₽`
3. Business rules:
   - Reject save if `IsSystemDefined == true`
   - Reject save if duplicate `CurrName` (excluding current id)
4. On success: toast + navigate back to list.

---

## 3. Data model (DTO)

Align TypeScript 1:1 with `CurrencyVM`:

```typescript
export interface CurrencyVm {
  currId: number;
  currName: string | null;
  currSub: string | null;
  symbol: string | null;
  isSystemDefined: boolean;
  createdBy: string | null;
  createdDate: string | null;   // ISO date from API
  modifiedBy: string | null;
  modifiedDate: string | null;
}
```

> ASP.NET Core default JSON uses camelCase — property names above match the wire format.

**Paged response**

```typescript
export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
}
```

---

## 4. API endpoints (`V.SMART.Api`)

| Method | Route | Body / Query | Behavior |
|--------|-------|--------------|----------|
| `POST` | `/api/auth/login` | `{ username, password }` | Returns JWT (+ user claims) |
| `GET` | `/api/currencies` | `pageNumber`, `pageSize`, optional filter keys | `SearchWithDynamicFilterAsync` |
| `GET` | `/api/currencies/{id}` | — | Get single currency |
| `POST` | `/api/currencies` | `CurrencyVM` | Create (duplicate + audit) |
| `PUT` | `/api/currencies/{id}` | `CurrencyVM` | Update (system-defined guard) |
| `DELETE` | `/api/currencies/{id}` | — | `CanDelete` then delete |

All currency routes require `[Authorize]` (JWT Bearer).

---

## 5. Angular pieces to build first

```text
src/app/
  core/
    auth/
      auth.service.ts          # login, token storage (signal)
      auth.interceptor.ts      # attaches Bearer token
      auth.guard.ts            # protects feature routes
    api/
      api-config.ts            # base URL (environment)
  layout/
    shell/                     # sidebar + router-outlet
  features/
    currency/
      models/currency.model.ts
      currency.service.ts      # HttpClient → API
      currency-list/           # PrimeNG Table + signals
      currency-form/           # Reactive Forms + signals
    auth/
      login/                   # simple login page
```

### Routes

| Path | Component |
|------|-----------|
| `/login` | LoginComponent |
| `/` | ShellComponent (authGuard) |
| `/currency` | CurrencyListComponent |
| `/currency/create` | CurrencyFormComponent |
| `/currency/edit/:id` | CurrencyFormComponent |

### Concepts to learn in this module

| Concept | Where you see it |
|---------|------------------|
| **Standalone components** | Every `.ts` file uses `imports: [...]`, no NgModule |
| **Signals** | `loading`, `rows`, `totalCount`, `error` on list/form |
| **Observables vs Signals** | HttpClient returns Observable; subscribe or `firstValueFrom`, then set signals |
| **Reactive Forms** | `FormGroup` / validators on currency form |
| **HTTP Interceptor** | Adds `Authorization: Bearer …` to every API call |
| **Route guard** | Blocks `/currency` if no token |

---

## 6. Implementation slice order

1. Spec (this file)
2. Angular scaffold + PrimeNG + folder layout
3. `V.SMART.Api` + JWT + CurrencyController
4. Login + interceptor + guard
5. Currency list (read)
6. Currency form (create/update)
7. Delete + toasts + loading polish

---

## 7. Out of scope for Module 1

- Full UserRights / screen permission parity (`CanView`, `CanCreate`, …)
- Column preference persistence
- Correspondence status widget on upsert
- Customer / Vendor migration (Module 2+)

## 8. Related docs

- [ANGULAR_CONCEPTS_MODULE1.md](./ANGULAR_CONCEPTS_MODULE1.md) — Signals, Observables, Forms, Interceptors, Guards
- [FRONTEND_MIGRATION_ANGULAR_REACT.md](./FRONTEND_MIGRATION_ANGULAR_REACT.md) — overall Blazor → SPA strategy
