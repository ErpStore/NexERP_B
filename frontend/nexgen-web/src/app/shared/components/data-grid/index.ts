/**
 * The `DataGrid` surface (M2-C05-01). Specification: KB-051 Data display.
 *
 * Three rules govern everything exported here:
 *   1. **Server-driven.** Paging, sorting and filtering are requests. Nothing
 *      in this directory sorts or filters a result set locally.
 *   2. **The URL is the state**, unless the host asks for a detached grid.
 *   3. **No business rule.** The grid renders rows; every domain decision -
 *      calculation, validation, permission, numbering - stays behind the API.
 */
export { DataGridComponent } from './data-grid.component';
export { DataGridHeaderComponent } from './data-grid-header.component';
export { DataGridPaginationComponent } from './data-grid-pagination.component';

/** The five states and server-side export (M2-C05-03). */
export { DataGridStatesComponent } from './data-grid-states.component';
export type { DataGridStateKind } from './data-grid-states.component';
export {
  DataGridSkeletonComponent,
  DATA_GRID_MAX_SKELETON_ROWS,
} from './data-grid-skeleton.component';
export {
  DataGridErrorComponent,
  GRID_ERROR_FALLBACK_MESSAGE,
  toGridProblem,
} from './data-grid-error.component';
export type { GridProblemDetails } from './data-grid-error.component';
export {
  DataGridToolbarComponent,
  GRID_EXPORT_DEFAULT_FORMATS,
} from './data-grid-toolbar.component';
export type { GridExportFormat } from './data-grid-toolbar.component';
export {
  GRID_EXPORT_XLSX,
  GridExportService,
  contentDispositionFilename,
  exportQuery,
  fallbackFilename,
} from './grid-export.service';
export type { GridExportOperation, GridExportRequest } from './grid-export.service';

export { DataGridQueryState, createDataGridQueryState } from './data-grid-query-state';
export type {
  DataGridDataSource,
  DataGridQueryMode,
  DataGridQueryStateOptions,
} from './data-grid-query-state';

export {
  ROUTE_PAGE_PARAM,
  ROUTE_SIZE_PARAM,
  ROUTE_SORT_PARAM,
  WIRE_PAGE_NUMBER_PARAM,
  WIRE_PAGE_SIZE_PARAM,
  WIRE_SORT_PARAM,
  fromRouteParams,
  fromRouteSort,
  toRouteParams,
  toRouteSort,
  toWireQuery,
  toWireSort,
} from './data-grid-query.adapter';
export type { DataGridPage, DataGridWireQuery } from './data-grid-query.adapter';

export {
  DATA_GRID_DEFAULT_COLUMN_WIDTH_PX,
  DATA_GRID_DEFAULT_PAGE_SIZE,
  DATA_GRID_FILTER_DEBOUNCE_MS,
  DATA_GRID_MAX_COLUMN_WIDTH_PX,
  DATA_GRID_MAX_PAGE_SIZE,
  DATA_GRID_MIN_COLUMN_WIDTH_PX,
  DATA_GRID_PAGE_SIZE_OPTIONS,
  DATA_GRID_ROW_HEIGHT_PX,
  DATA_GRID_VIRTUAL_ROW_THRESHOLD,
  cellValue,
  clampPageSize,
  clampPageSizeState,
  columnAlign,
  columnFilterParam,
  defaultDataGridState,
  isNumericColumn,
} from './data-grid.model';
export type {
  DataGridAlign,
  DataGridColumn,
  DataGridColumnPriority,
  DataGridDensity,
  DataGridFilterKind,
  DataGridRowId,
  DataGridRowIdFn,
  DataGridSelectionMode,
  DataGridSort,
  DataGridSortDirection,
  DataGridState,
} from './data-grid.model';

export {
  HEADER_ROW_INDEX,
  ariaRowIndex,
  clampCell,
  handlesKey,
  nextFocusedCell,
  rovingTabIndex,
} from './grid-keyboard-navigation';
export type { GridBounds, GridCell } from './grid-keyboard-navigation';
