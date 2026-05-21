import { HttpInterceptorFn } from '@angular/common/http';
import { environment } from '../../environments/environment';

const USER_ID_KEY = 'pcrc.devUserId';
const ENTRA_OBJECT_ID_KEY = 'pcrc.devEntraObjectId';

// Attaches the POC auth headers (X-User-Id, X-Entra-Object-Id) to every request that
// targets environment.apiBaseUrl. Values come from localStorage first (so dev can switch
// users without rebuilding), then environment.ts. Skipped when both values are missing,
// so the interceptor is a no-op in production until the real MSAL flow lands.
export const authHeadersInterceptor: HttpInterceptorFn = (req, next) => {
  if (!req.url.startsWith(environment.apiBaseUrl)) {
    return next(req);
  }

  const userId = readUserId();
  const entraObjectId = readEntraObjectId();

  if (userId === null && entraObjectId === null) {
    return next(req);
  }

  const headers: Record<string, string> = {};
  if (userId !== null) headers['X-User-Id'] = String(userId);
  if (entraObjectId !== null) headers['X-Entra-Object-Id'] = entraObjectId;

  return next(req.clone({ setHeaders: headers }));
};

function readUserId(): number | null {
  const override = readLocalStorage(USER_ID_KEY);
  if (override !== null) {
    const parsed = Number(override);
    return Number.isFinite(parsed) ? parsed : null;
  }
  return environment.devUserId;
}

function readEntraObjectId(): string | null {
  const override = readLocalStorage(ENTRA_OBJECT_ID_KEY);
  if (override !== null) return override;
  return environment.devEntraObjectId;
}

function readLocalStorage(key: string): string | null {
  if (typeof localStorage === 'undefined') return null;
  const value = localStorage.getItem(key);
  return value && value.length > 0 ? value : null;
}
