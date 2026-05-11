import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models';

@Injectable({
  providedIn: 'root'
})
export class UploadService {
  private readonly http = inject(HttpClient);
  private readonly medicineImageUrl = `${environment.apiUrl}/medicine-images/upload`;
  private readonly categoryImageUrl = `${environment.apiUrl}/category-images/upload`;

  uploadMedicineImage(file: File, category: string = 'other', subCategory: string = 'general', medicineName: string = 'medicine'): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);

    const params = new HttpParams()
      .set('category', category)
      .set('subCategory', subCategory)
      .set('medicineName', medicineName);

    return this.http.post<ApiResponse<any>>(this.medicineImageUrl, formData, { params }).pipe(
      map(response => response.data)
    );
  }

  uploadCategoryImage(file: File, categoryName: string): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);

    const params = new HttpParams().set('categoryName', categoryName);

    return this.http.post<ApiResponse<any>>(this.categoryImageUrl, formData, { params }).pipe(
      map(response => response.data)
    );
  }
}
