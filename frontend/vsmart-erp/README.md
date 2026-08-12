# vsmart-erp (Angular SPA)

Modern frontend for NexGen ERP. Module 1 = **Currency Master**.

## Prerequisites

- Node.js 20+ (LTS)
- `V.SMART.Api` running at `http://localhost:5144`

## Commands

```powershell
npm install
npm start
```

App: http://localhost:4200

## Structure

```text
src/app/
  core/auth/          JWT auth service, interceptor, guard
  layout/shell/       Sidebar shell
  features/auth/      Login
  features/currency/  List + form + API service
```

See also:

- [docs/module-1-plan.md](../../docs/module-1-plan.md)
- [docs/ANGULAR_CONCEPTS_MODULE1.md](../../docs/ANGULAR_CONCEPTS_MODULE1.md)
