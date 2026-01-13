import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.interface';
import { MasterDashboardStats } from '../models/master-dashboard.models';

/**
 * Master Dashboard Service
 * Fetches comprehensive real-time statistics from backend
 * Optimized for <100ms backend response + network time
 */
@Injectable({
  providedIn: 'root'
})
export class MasterDashboardService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/Dashboard`;

  /**
   * Get Master Dashboard Statistics
   * Single unified endpoint for all dashboard data
   * @returns Observable of master dashboard statistics
   */
  getMasterDashboardStats(): Observable<MasterDashboardStats> {
    return this.http
      .get<ApiResponse<MasterDashboardStats>>(`${this.apiUrl}/master-stats`)
      .pipe(
        map(response => {
          // Convert date strings to Date objects
          const stats = response.data;
          
          // Convert cash flow dates
          if (stats.financialIntelligence?.cashFlowInLast30Days) {
            stats.financialIntelligence.cashFlowInLast30Days = 
              stats.financialIntelligence.cashFlowInLast30Days.map(cf => ({
                ...cf,
                date: new Date(cf.date)
              }));
          }
          
          if (stats.financialIntelligence?.cashFlowOutLast30Days) {
            stats.financialIntelligence.cashFlowOutLast30Days = 
              stats.financialIntelligence.cashFlowOutLast30Days.map(cf => ({
                ...cf,
                date: new Date(cf.date)
              }));
          }
          
          // Convert activity stream timestamps
          if (stats.operationalPulse?.activityStream) {
            stats.operationalPulse.activityStream = 
              stats.operationalPulse.activityStream.map(activity => ({
                ...activity,
                timestamp: new Date(activity.timestamp)
              }));
          }
          
          return stats;
        })
      );
  }
}
