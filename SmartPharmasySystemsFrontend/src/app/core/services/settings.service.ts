import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PharmacySettings, UpdatePharmacySettingsDto } from '../models/settings/pharmacy-settings.interface';

@Injectable({
  providedIn: 'root'
})
export class SettingsService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/Settings`;

  getSettings(): Observable<PharmacySettings> {
    return this.http.get<PharmacySettings>(this.apiUrl);
  }

  updateSettings(dto: UpdatePharmacySettingsDto): Observable<PharmacySettings> {
    return this.http.put<PharmacySettings>(this.apiUrl, dto);
  }

  uploadLogo(file: File): Observable<{ logoUrl: string; message: string }> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<{ logoUrl: string; message: string }>(`${this.apiUrl}/logo`, formData);
  }
}
