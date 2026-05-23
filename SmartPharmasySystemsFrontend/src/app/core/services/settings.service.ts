import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, of, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PharmacySettings, UpdatePharmacySettingsDto } from '../models/settings/pharmacy-settings.interface';

@Injectable({
  providedIn: 'root'
})
export class SettingsService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/Settings`;
  private settingsSubject = new BehaviorSubject<PharmacySettings | null>(null);
  readonly settings$ = this.settingsSubject.asObservable();

  getSettings(forceRefresh = false): Observable<PharmacySettings> {
    const cachedSettings = this.settingsSubject.value;

    if (!forceRefresh && cachedSettings) {
      return of(cachedSettings);
    }

    return this.http.get<PharmacySettings>(this.apiUrl).pipe(
      tap((settings) => this.settingsSubject.next(settings))
    );
  }

  updateSettings(dto: UpdatePharmacySettingsDto): Observable<PharmacySettings> {
    return this.http.put<PharmacySettings>(this.apiUrl, dto).pipe(
      tap((settings) => this.settingsSubject.next(settings))
    );
  }

  uploadLogo(file: File): Observable<{ logoUrl: string; message: string }> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<{ logoUrl: string; message: string }>(`${this.apiUrl}/logo`, formData).pipe(
      tap((response) => {
        const currentSettings = this.settingsSubject.value;
        if (!currentSettings) {
          return;
        }

        this.settingsSubject.next({
          ...currentSettings,
          logoUrl: response.logoUrl
        });
      })
    );
  }
}
