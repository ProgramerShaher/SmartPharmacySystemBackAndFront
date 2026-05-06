import { Routes } from '@angular/router';

export const REPORTS_ROUTES: Routes = [
  {
    path: '',
    children: [
      {
        path: 'statement/:entityType/:entityId',
        loadComponent: () => import('./unified-statement/unified-statement.component')
          .then(m => m.UnifiedStatementComponent),
        title: 'كشف الحساب'
      },
      {
        path: 'net-profit',
        loadComponent: () => import('./net-profit/net-profit.component')
          .then(m => m.NetProfitComponent),
        title: 'صافي الأرباح'
      },
      {
        path: 'inventory-valuation',
        loadComponent: () => import('./inventory-valuation/inventory-valuation.component')
          .then(m => m.InventoryValuationComponent),
        title: 'تقييم المخزون'
      },
      {
        path: 'daily-sales',
        loadComponent: () => import('./daily-sales/daily-sales.component')
          .then(m => m.DailySalesComponent),
        title: 'المبيعات اليومية'
      },
      {
        path: 'employee-performance',
        loadComponent: () => import('./employee-performance/employee-performance.component')
          .then(m => m.EmployeePerformanceComponent),
        title: 'تقرير أداء الموظفين'
      },
      {
        path: 'best-selling',
        loadComponent: () => import('./best-selling/best-selling.component')
          .then(m => m.BestSellingComponent),
        title: 'الأدوية الأكثر مبيعاً'
      },
      {
        path: 'customer-debts',
        loadComponent: () => import('./customer-debts/customer-debts.component')
          .then(m => m.CustomerDebtsComponent),
        title: 'ديون العملاء'
      },
      {
        path: 'supplier-debts',
        loadComponent: () => import('./supplier-debts/supplier-debts.component')
          .then(m => m.SupplierDebtsComponent),
        title: 'ديون الموردين'
      },
      {
        path: '',
        redirectTo: 'daily-sales',
        pathMatch: 'full'
      }
    ]
  }
];
