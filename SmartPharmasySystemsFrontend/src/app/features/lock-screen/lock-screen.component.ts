import {
  Component,
  OnInit,
  inject,
  signal,
  computed
} from '@angular/core';
import { CommonModule }        from '@angular/common';
import { FormsModule }         from '@angular/forms';
import { Router }              from '@angular/router';
import { LicenseService }      from '../../core/services/license.service';
import { ToastrService }       from 'ngx-toastr';
import { finalize }            from 'rxjs';

@Component({
  selector:    'app-lock-screen',
  standalone:  true,
  imports:     [CommonModule, FormsModule],
  templateUrl: './lock-screen.component.html',
  styleUrls:   ['./lock-screen.component.scss']
})
export class LockScreenComponent implements OnInit {

  private readonly licenseService = inject(LicenseService);
  private readonly router         = inject(Router);
  private readonly toastr         = inject(ToastrService);

  // ── Reactive State ─────────────────────────────────────────────────────────
  readonly machineId   = signal<string>('جارٍ التحميل...');
  readonly licenseKey  = signal<string>('');
  readonly isLoading   = signal<boolean>(false);
  readonly isFetching  = signal<boolean>(true);
  readonly hasError    = signal<boolean>(false);
  readonly currentYear = new Date().getFullYear();


  /** True only when the input has text (drives the submit button state). */
  readonly canSubmit = computed(() =>
    this.licenseKey().trim().length > 10 && !this.isLoading()
  );

  // ── Lifecycle ──────────────────────────────────────────────────────────────
  ngOnInit(): void {
    this.licenseService.getStatus().subscribe({
      next: status => {
        this.machineId.set(status.machineId);
        this.isFetching.set(false);

        // If somehow the system is already activated, go to dashboard
        if (status.isActivated) {
          this.router.navigate(['/dashboard']);
        }
      },
      error: () => {
        this.machineId.set('تعذّر الاتصال بالخادم');
        this.isFetching.set(false);
        this.hasError.set(true);
      }
    });
  }

  // ── Actions ────────────────────────────────────────────────────────────────

  onKeyInput(event: Event): void {
    this.licenseKey.set((event.target as HTMLInputElement).value);
  }

  onActivate(): void {
    const key = this.licenseKey().trim();
    if (!key) return;

    this.isLoading.set(true);

    this.licenseService
      .activate(key)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: res => {
          this.toastr.success(res.message, 'تم التفعيل', { timeOut: 4000 });
          // Small delay so the user sees the success toast before navigation
          setTimeout(() => this.router.navigate(['/dashboard']), 1500);
        },
        error: err => {
          const msg = err?.error?.message ?? 'مفتاح الترخيص غير صالح.';
          this.toastr.error(msg, 'فشل التفعيل', { timeOut: 5000 });
        }
      });
  }

  /** Copies the Machine ID to the clipboard. */
  copyMachineId(): void {
    navigator.clipboard.writeText(this.machineId()).then(() => {
      this.toastr.info('تم نسخ معرّف الجهاز', '', { timeOut: 2000 });
    });
  }
}
