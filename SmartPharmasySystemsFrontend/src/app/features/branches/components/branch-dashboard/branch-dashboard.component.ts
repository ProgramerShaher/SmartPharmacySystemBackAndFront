import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EmployeeService } from '../../../employees/services/employee.service';
import { CardModule } from 'primeng/card';
import { TableModule } from 'primeng/table';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';

export interface BranchDashboardDto {
    branchId: number;
    branchName: string;
    totalEmployees: number;
    totalSalaries: number;
    totalLoans: number;
    totalRemainingLoans: number;
    departmentStats: { [key: string]: number };
    recentEmployees: any[];
}

@Component({
  selector: 'app-branch-dashboard',
  standalone: true,
  imports: [CommonModule, CardModule, TableModule, ProgressSpinnerModule, ToastModule],
  providers: [MessageService],
  template: `
    <div class="p-4" dir="rtl">
        <p-toast></p-toast>
        <div class="flex justify-content-between align-items-center mb-4">
            <h2>لوحة تحكم الفرع: <span class="text-primary">{{ dashboardData?.branchName || '...' }}</span></h2>
        </div>

        <div *ngIf="loading" class="flex justify-content-center mt-6">
            <p-progressSpinner></p-progressSpinner>
        </div>

        <div *ngIf="!loading && dashboardData">
            <div class="grid">
                <div class="col-12 md:col-6 lg:col-3">
                    <p-card styleClass="shadow-2 border-round-xl">
                        <div class="flex justify-content-between mb-3">
                            <div>
                                <span class="block text-500 font-medium mb-3">إجمالي الموظفين</span>
                                <div class="text-900 font-medium text-xl">{{ dashboardData.totalEmployees }}</div>
                            </div>
                            <div class="flex align-items-center justify-content-center bg-blue-100 border-round" style="width:2.5rem;height:2.5rem">
                                <i class="pi pi-users text-blue-500 text-xl"></i>
                            </div>
                        </div>
                    </p-card>
                </div>
                <div class="col-12 md:col-6 lg:col-3">
                    <p-card styleClass="shadow-2 border-round-xl">
                        <div class="flex justify-content-between mb-3">
                            <div>
                                <span class="block text-500 font-medium mb-3">إجمالي الرواتب</span>
                                <div class="text-900 font-medium text-xl">{{ dashboardData.totalSalaries | currency:'':'':'1.0-2' }}</div>
                            </div>
                            <div class="flex align-items-center justify-content-center bg-green-100 border-round" style="width:2.5rem;height:2.5rem">
                                <i class="pi pi-money-bill text-green-500 text-xl"></i>
                            </div>
                        </div>
                    </p-card>
                </div>
                <div class="col-12 md:col-6 lg:col-3">
                    <p-card styleClass="shadow-2 border-round-xl">
                        <div class="flex justify-content-between mb-3">
                            <div>
                                <span class="block text-500 font-medium mb-3">إجمالي السلف</span>
                                <div class="text-900 font-medium text-xl">{{ dashboardData.totalLoans | currency:'':'':'1.0-2' }}</div>
                            </div>
                            <div class="flex align-items-center justify-content-center bg-orange-100 border-round" style="width:2.5rem;height:2.5rem">
                                <i class="pi pi-wallet text-orange-500 text-xl"></i>
                            </div>
                        </div>
                    </p-card>
                </div>
                <div class="col-12 md:col-6 lg:col-3">
                    <p-card styleClass="shadow-2 border-round-xl">
                        <div class="flex justify-content-between mb-3">
                            <div>
                                <span class="block text-500 font-medium mb-3">السلف المتبقية</span>
                                <div class="text-900 font-medium text-xl">{{ dashboardData.totalRemainingLoans | currency:'':'':'1.0-2' }}</div>
                            </div>
                            <div class="flex align-items-center justify-content-center bg-red-100 border-round" style="width:2.5rem;height:2.5rem">
                                <i class="pi pi-chart-pie text-red-500 text-xl"></i>
                            </div>
                        </div>
                    </p-card>
                </div>
            </div>

            <div class="grid mt-4">
                <div class="col-12 md:col-6">
                    <p-card header="إحصائيات الأقسام" styleClass="shadow-2 border-round-xl h-full">
                        <ul class="list-none p-0 m-0">
                            <li *ngFor="let dept of objectKeys(dashboardData.departmentStats)" class="flex align-items-center py-3 px-2 border-bottom-1 surface-border">
                                <div class="w-3rem h-3rem flex align-items-center justify-content-center bg-blue-100 border-circle mr-3 flex-shrink-0 ml-3">
                                    <i class="pi pi-briefcase text-xl text-blue-500"></i>
                                </div>
                                <span class="text-900 line-height-3">{{ dept }}</span>
                                <span class="text-600 ml-auto mr-auto">{{ dashboardData.departmentStats[dept] }} موظف</span>
                            </li>
                            <li *ngIf="objectKeys(dashboardData.departmentStats).length === 0" class="text-500 text-center py-3">
                                لا توجد بيانات للأقسام
                            </li>
                        </ul>
                    </p-card>
                </div>

                <div class="col-12 md:col-6">
                    <p-card header="أحدث الموظفين" styleClass="shadow-2 border-round-xl h-full">
                        <p-table [value]="dashboardData.recentEmployees" [rows]="5" [paginator]="false" responsiveLayout="scroll">
                            <ng-template pTemplate="header">
                                <tr>
                                    <th>الاسم</th>
                                    <th>القسم</th>
                                    <th>المسمى الوظيفي</th>
                                </tr>
                            </ng-template>
                            <ng-template pTemplate="body" let-employee>
                                <tr>
                                    <td>
                                        <div class="flex align-items-center">
                                            <div class="w-2rem h-2rem flex align-items-center justify-content-center bg-indigo-100 border-circle mr-2 ml-2">
                                                <span class="text-indigo-500 font-bold text-sm">{{ employee.fullName.charAt(0) }}</span>
                                            </div>
                                            <span class="font-bold">{{ employee.fullName }}</span>
                                        </div>
                                    </td>
                                    <td>{{ employee.departmentName }}</td>
                                    <td>
                                        <span class="customer-badge status-qualified">{{ employee.jobTitle }}</span>
                                    </td>
                                </tr>
                            </ng-template>
                            <ng-template pTemplate="emptymessage">
                                <tr>
                                    <td colspan="3" class="text-center p-4">لا يوجد موظفين</td>
                                </tr>
                            </ng-template>
                        </p-table>
                    </p-card>
                </div>
            </div>
        </div>
    </div>
  `,
  styles: [`
    .customer-badge {
        border-radius: 2px;
        padding: .25em .5rem;
        text-transform: uppercase;
        font-weight: 700;
        font-size: 12px;
        letter-spacing: .3px;
    }
    .customer-badge.status-qualified {
        background-color: #C8E6C9;
        color: #256029;
    }
  `]
})
export class BranchDashboardComponent implements OnInit {
    loading = true;
    dashboardData: BranchDashboardDto | null = null;
    objectKeys = Object.keys;

    constructor(
        private employeeService: EmployeeService,
        private messageService: MessageService
    ) {}

    ngOnInit() {
        this.loadDashboard();
    }

    loadDashboard() {
        this.loading = true;
        this.employeeService.getBranchDashboard().subscribe({
            next: (data) => {
                this.dashboardData = data;
                this.loading = false;
            },
            error: (err) => {
                this.loading = false;
                this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'حدث خطأ أثناء تحميل بيانات الفرع' });
                console.error(err);
            }
        });
    }
}
