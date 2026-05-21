export const environment = {
  production: false,
  apiBaseUrl: 'https://localhost:7212/api',
  // POC stand-in for the eventual Entra/MSAL flow — HeaderUserContext on the API reads these
  // two headers. Override per-developer via localStorage keys "pcrc.devUserId" and
  // "pcrc.devEntraObjectId" (see auth-headers.interceptor.ts).
  devUserId: 1 as number | null,
  devEntraObjectId: 'local-dev-user' as string | null,
};
