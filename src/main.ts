import { isDevMode } from '@angular/core';
import { bootstrapApplication } from '@angular/platform-browser';
import {
  ModuleRegistry,
  ClientSideRowModelModule,
  ColumnAutoSizeModule,
  TextFilterModule,
  NumberFilterModule,
  DateFilterModule,
  CellStyleModule,
  TooltipModule,
  ValidationModule,
} from 'ag-grid-community';
import { appConfig } from './app/app.config';
import { App } from './app/app';

ModuleRegistry.registerModules([
  ClientSideRowModelModule,
  ColumnAutoSizeModule,
  TextFilterModule,
  NumberFilterModule,
  DateFilterModule,
  CellStyleModule,
  TooltipModule,
  ...(isDevMode() ? [ValidationModule] : []),
]);

bootstrapApplication(App, appConfig)
  .catch((err) => console.error(err));
