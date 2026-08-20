import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { TabViewModule } from 'primeng/tabview';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TooltipModule } from 'primeng/tooltip';

import { EmployeeService } from '../../services/employee.service';
import { MonthlySalaryService } from '../../services/monthly-salary.service';
import { EmployeeLoanService } from '../../services/employee-loan.service';

@Component({
  selector: 'app-employee-profile',
  standalone: true,
  imports: [
    CommonModule,
    ButtonModule,
    TabViewModule,
    TableModule,
    TagModule,
    ProgressSpinnerModule,
    TooltipModule
  ],
  templateUrl: './employee-profile.component.html',
  styleUrls: ['./employee-profile.component.scss']
})
export class EmployeeProfileComponent implements OnInit {
  employeeId: number = 0;
  employee = signal<any>(null);
  salaries = signal<any[]>([]);
  loans = signal<any[]>([]);
  remainingLoan = signal<number>(0);
  loading = signal<boolean>(true);
  filterDate = new Date(); // Used for print date

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private employeeService: EmployeeService,
    private salaryService: MonthlySalaryService,
    private loanService: EmployeeLoanService
  ) {}

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.employeeId = +idParam;
      this.loadProfileData();
    }
  }

  loadProfileData(): void {
    this.loading.set(true);

    // Fetch basic employee data
    this.employeeService.getById(this.employeeId).subscribe({
      next: (emp) => {
        this.employee.set(emp);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      }
    });

    this.salaryService.getByEmployeeId(this.employeeId).subscribe({
      next: (sals) => {
        this.salaries.set(sals);
      }
    });

    this.loanService.getByEmployeeId(this.employeeId).subscribe({
      next: (ls) => {
        this.loans.set(ls);
      }
    });

    this.loanService.getRemaining(this.employeeId).subscribe({
      next: (rem) => {
        this.remainingLoan.set(rem);
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/employees']);
  }

  printProfile(): void {
    // Show the print-only template before printing
    const printEl = document.querySelector('.print-only') as HTMLElement;
    if (printEl) printEl.style.display = 'block';
    setTimeout(() => {
      window.print();
      if (printEl) printEl.style.display = 'none';
    }, 300);
  }

  addLoan(): void {
    // Navigate to add loan or open dialog
    console.log('Add Loan clicked');
  }

  addLeave(): void {
    // Navigate to add leave or open dialog
    console.log('Add Leave clicked');
  }
}
