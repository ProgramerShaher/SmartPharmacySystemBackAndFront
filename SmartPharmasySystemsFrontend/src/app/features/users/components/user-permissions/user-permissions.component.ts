import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { CheckboxModule } from 'primeng/checkbox';
import { MessageService } from 'primeng/api';
import { AccordionModule } from 'primeng/accordion';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { UsersService } from '../../services/users.service';
import { PermissionService, PermissionGroup } from '../../../../core/services/permission.service';
import { User } from '../../../../core/models';
import { forkJoin } from 'rxjs';

@Component({
  selector: 'app-user-permissions',
  standalone: true,
  imports: [CommonModule, FormsModule, ButtonModule, CheckboxModule, AccordionModule, ProgressSpinnerModule],
  templateUrl: './user-permissions.component.html',
  styleUrls: ['./user-permissions.component.scss']
})
export class UserPermissionsComponent implements OnInit {
  @Input() user: User | null = null;
  @Output() cancel = new EventEmitter<void>();
  @Output() save = new EventEmitter<void>();

  permissionGroups: PermissionGroup[] = [];
  selectedPermissionIds: number[] = [];
  loading = false;
  loadingData = true;

  constructor(
    private usersService: UsersService,
    private permissionService: PermissionService,
    private messageService: MessageService
  ) {}

  ngOnInit(): void {
    if (this.user) {
      this.loadData();
    }
  }

  loadData(): void {
    this.loadingData = true;
    
    // Fetch all grouped permissions and the user's current effective permissions concurrently
    forkJoin({
      groups: this.permissionService.getAllPermissionsGrouped(),
      userPerms: this.usersService.getUserPermissions(this.user!.id)
    }).subscribe({
      next: (result) => {
        this.permissionGroups = result.groups;
        this.selectedPermissionIds = result.userPerms;
        this.loadingData = false;
      },
      error: (err) => {
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل في تحميل بيانات الصلاحيات' });
        this.loadingData = false;
      }
    });
  }

  savePermissions(): void {
    if (!this.user) return;
    
    this.loading = true;
    this.usersService.updateUserPermissions(this.user.id, this.selectedPermissionIds).subscribe({
      next: () => {
        this.loading = false;
        this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم تحديث صلاحيات المستخدم بنجاح' });
        this.save.emit();
      },
      error: (err) => {
        this.loading = false;
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: err.error?.message || 'حدث خطأ أثناء حفظ الصلاحيات' });
      }
    });
  }

  onCancel(): void {
    this.cancel.emit();
  }
}
