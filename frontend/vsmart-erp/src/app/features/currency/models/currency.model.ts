/**
 * LEARNING — DTOs aligned with C# CurrencyVM
 * ASP.NET Core serializes PascalCase → camelCase by default.
 * Keep these properties in sync with ViewModels/MasterViewModel/AccountsViewModel/CurrencyVM.cs
 */
export interface CurrencyVm {
  currId: number;
  currName: string | null;
  currSub: string | null;
  symbol: string | null;
  isSystemDefined: boolean;
  createdBy: string | null;
  createdDate: string | null;
  modifiedBy: string | null;
  modifiedDate: string | null;
}

export interface PagedCurrencyResult {
  items: CurrencyVm[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
}

export interface CurrencySearchParams {
  pageNumber: number;
  pageSize: number;
  currName?: string;
  createdBy?: string;
  fromDate?: string;
  toDate?: string;
}
