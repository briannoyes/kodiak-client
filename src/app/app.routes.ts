import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'clients', pathMatch: 'full' },
  {
    path: 'clients',
    loadComponent: () =>
      import('./features/clients/clients').then((m) => m.Clients),
  },
  {
    path: 'clients/:clientExternalId',
    loadComponent: () =>
      import('./features/client-detail/client-detail').then(
        (m) => m.ClientDetail
      ),
  },
  {
    path: 'clients/:clientExternalId/uploads/:uploadExternalId/documents',
    loadComponent: () =>
      import('./features/documents/documents').then((m) => m.Documents),
  },
];
