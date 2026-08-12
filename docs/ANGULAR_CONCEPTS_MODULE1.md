# Angular Concepts Used in Module 1 (Currency)

Short reference for concepts introduced while building Currency Master. Code comments in the feature files expand on these.

---

## 1. Standalone Components (no NgModule)

Every component declares its own `imports: [...]`. There is no `AppModule` / feature module.

```ts
@Component({
  standalone: true,
  imports: [ReactiveFormsModule, Card, Button, Toast],
  // ...
})
```

**Why it matters:** Less boilerplate; tree-shaking is clearer; matches Angular 19+ defaults.

---

## 2. Signals vs Observables

| Concern | Use |
|---------|-----|
| Local UI state (`loading`, `rows`, `error`) | `signal()` / `computed()` |
| HTTP streams from `HttpClient` | `Observable` |

Pattern used in Module 1:

```ts
readonly loading = signal(false);
const result = await firstValueFrom(this.currencyService.search(...));
this.rows.set(result.items);
```

`firstValueFrom` turns the first Observable emission into a Promise (familiar if you know C# `await`).

---

## 3. Reactive Forms

`FormGroup` + validators mirror C# DataAnnotations on `CurrencyVM`:

- `Validators.required` ≈ `[Required]`
- `Validators.maxLength(100)` ≈ `[StringLength(100)]`
- `Validators.pattern(/[$€₹¥£₩₿₽]/)` ≈ `[RegularExpression(...)]`

---

## 4. HTTP Interceptors

`authInterceptor` adds `Authorization: Bearer <jwt>` to every request — similar to ASP.NET middleware.

Registered in `app.config.ts`:

```ts
provideHttpClient(withInterceptors([authInterceptor]))
```

---

## 5. Route Guards

`authGuard` blocks `/currency` routes when no JWT is stored, redirecting to `/login`.

---

## 6. Feature folder layout

```text
core/       cross-cutting (auth, interceptors, guards)
shared/     reusable UI (empty for Module 1 — add later)
features/   domain screens (currency, auth/login)
layout/     app shell (sidebar + outlet)
```

---

## Run locally

```powershell
# Terminal 1 — API
cd V.SMART\V.SMART.Api
dotnet run --launch-profile http

# Terminal 2 — Angular
cd frontend\vsmart-erp
npm start
```

Open http://localhost:4200 → login with an existing ERP user → Currency Master.
