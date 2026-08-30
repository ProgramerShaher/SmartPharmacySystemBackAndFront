import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, shareReplay, tap } from 'rxjs';
import { environment } from '../../../environments/environment';

// ─── Models ──────────────────────────────────────────────────────────────────

export interface LicenseStatus {
  machineId:   string;
  isActivated: boolean;
}

export interface ActivateResponse {
  message: string;
}

// ─── Service ─────────────────────────────────────────────────────────────────

/**
 * Angular service for the hardware-bound licensing system.
 * Communicates with /api/license/status and /api/license/activate.
 */
@Injectable({ providedIn: 'root' })
export class LicenseService {
  private readonly http    = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/license`;

  /**
   * Cached status observable — avoids redundant HTTP calls on the same startup.
   * Refreshed by calling `refreshStatus()`.
   */
  private _status$: Observable<LicenseStatus> | null = null;

  // ── Public API ─────────────────────────────────────────────────────────────

  /**
   * Returns the current license status (machineId + isActivated).
   * Shares and replays a single HTTP call per page load.
   */
  getStatus(): Observable<LicenseStatus> {
    if (!this._status$) {
      this._status$ = this.http
        .get<LicenseStatus>(`${this.baseUrl}/status`)
        .pipe(shareReplay(1));
    }
    return this._status$;
  }

  /**
   * Forces a fresh status check (e.g., after successful activation).
   */
  refreshStatus(): Observable<LicenseStatus> {
    this._status$ = null;
    return this.getStatus();
  }

  /**
   * Posts the provided license key to the API for validation and persistence.
   */
  activate(licenseKey: string): Observable<ActivateResponse> {
    return this.http
      .post<ActivateResponse>(`${this.baseUrl}/activate`, { licenseKey })
      .pipe(
        tap(() => {
          // Invalidate cached status so the guard re-checks after activation
          this._status$ = null;
        })
      );
  }
}
