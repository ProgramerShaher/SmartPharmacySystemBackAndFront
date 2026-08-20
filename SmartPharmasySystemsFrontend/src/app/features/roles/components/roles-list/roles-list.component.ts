import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { ToastModule } from 'primeng/toast';
import { MessageService, ConfirmationService } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TooltipModule } from 'primeng/tooltip';
import { RolesService, RoleDto } from '../../services/roles.service';
import { PermissionService, PermissionGroup } from '../../../../core/services/permission.service';
import { CheckboxModule } from 'primeng/checkbox';
import { CardModule } from 'primeng/card';
import { HasPermissionDirective } from '../../../../shared/directives/has-permission.directive';
import { ColorPickerModule } from 'primeng/colorpicker';
import { InputSwitchModule } from 'primeng/inputswitch';

@Component({
  selector: 'app-roles-list',
  standalone: true,
  imports: [
    CommonModule, FormsModule, ReactiveFormsModule, TableModule, ButtonModule,
    DialogModule, InputTextModule, ToastModule, ConfirmDialogModule, TooltipModule,
    CheckboxModule, CardModule, HasPermissionDirective, ColorPickerModule, InputSwitchModule
  ],
  templateUrl: './roles-list.component.html',
  styleUrls: ['./roles-list.component.scss']
})
export class RolesListComponent implements OnInit {
  private rolesService = inject(RolesService);
  private permissionService = inject(PermissionService);
  private messageService = inject(MessageService);
  private confirmationService = inject(ConfirmationService);
  private fb = inject(FormBuilder);

  roles: RoleDto[] = [];
  loading = false;

  // Dialogs
  displayDialog = false;
  displayPermissionsDialog = false;
  isEdit = false;

  // Form
  roleForm = this.fb.group({
    id: [0],
    name: ['', Validators.required],
    nameAr: ['', Validators.required],
    description: [''],
    color: ['#000000'],
    isActive: [true]
  });

  // Permissions
  permissionGroups: PermissionGroup[] = [];
  selectedPermissionIds: number[] = [];
  selectedRoleForPermissions: RoleDto | null = null;

  ngOnInit() {
    this.loadRoles();
    this.loadAllPermissions();
  }

  loadRoles() {
    this.loading = true;
    this.rolesService.getAll().subscribe({
      next: (res) => {
        this.roles = res.data;
        this.loading = false;
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل في تحميل الأدوار' });
        this.loading = false;
      }
    });
  }

  loadAllPermissions() {
    this.permissionService.getAllPermissionsGrouped().subscribe(res => {
      this.permissionGroups = res;
    });
  }

  openNew() {
    this.isEdit = false;
    this.roleForm.reset({ isActive: true, color: '3b82f6', id: 0 });
    this.displayDialog = true;
  }

  getValidColor(color: string | null | undefined): string {
    if (!color) return '#3b82f6';
    return color.startsWith('#') ? color : '#' + color;
  }

  openEdit(role: RoleDto) {
    if (role.isSystemRole) {
      this.messageService.add({ severity: 'warn', summary: 'تنبيه', detail: 'لا يمكن تعديل أدوار النظام الأساسية' });
      return;
    }
    this.isEdit = true;
    this.roleForm.patchValue({
      id: role.id,
      name: role.name,
      nameAr: role.nameAr,
      description: role.description,
      color: role.color ? role.color.replace('#', '') : '3b82f6',
      isActive: role.isActive
    });
    this.displayDialog = true;
  }

  save() {
    if (this.roleForm.invalid) return;

    const val = this.roleForm.value;
    const dto = {
      name: val.name,
      nameAr: val.nameAr,
      description: val.description,
      color: this.getValidColor(val.color),
      isActive: val.isActive
    };

    if (this.isEdit) {
      this.rolesService.update(val.id!, dto).subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم تحديث الدور' });
          this.displayDialog = false;
          this.loadRoles();
        },
        error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل التحديث' })
      });
    } else {
      this.rolesService.create(dto).subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم إنشاء الدور' });
          this.displayDialog = false;
          this.loadRoles();
        },
        error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل الإنشاء' })
      });
    }
  }

  delete(role: RoleDto) {
    if (role.isSystemRole) {
      this.messageService.add({ severity: 'warn', summary: 'تنبيه', detail: 'لا يمكن حذف أدوار النظام الأساسية' });
      return;
    }
    
    this.confirmationService.confirm({
      message: `هل أنت متأكد من حذف الدور ${role.nameAr}?`,
      accept: () => {
        this.rolesService.delete(role.id).subscribe({
          next: () => {
            this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم الحذف' });
            this.loadRoles();
          },
          error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل الحذف' })
        });
      }
    });
  }

  openPermissions(role: RoleDto) {
    this.selectedRoleForPermissions = role;
    this.selectedPermissionIds = [];
    
    this.rolesService.getPermissions(role.id).subscribe(res => {
      this.selectedPermissionIds = res.data || [];
      this.displayPermissionsDialog = true;
    });
  }

  getThemeForModule(moduleName: string) {
    const module = (moduleName || '').toLowerCase();
    if (module.includes('sale') || module.includes('shift')) return { header: 'theme-green-header', bg: 'theme-green-bg', icon: 'pi-shopping-cart' };
    if (module.includes('inventory') || module.includes('warehouse')) return { header: 'theme-blue-header', bg: 'theme-blue-bg', icon: 'pi-box' };
    if (module.includes('finance') || module.includes('account') || module.includes('treasury')) return { header: 'theme-teal-header', bg: 'theme-teal-bg', icon: 'pi-wallet' };
    if (module.includes('purchase')) return { header: 'theme-orange-header', bg: 'theme-orange-bg', icon: 'pi-truck' };
    if (module.includes('partner') || module.includes('customer')) return { header: 'theme-cyan-header', bg: 'theme-cyan-bg', icon: 'pi-users' };
    if (module.includes('admin') || module.includes('setting') || module.includes('role')) return { header: 'theme-purple-header', bg: 'theme-purple-bg', icon: 'pi-cog' };
    if (module.includes('hr') || module.includes('employee')) return { header: 'theme-pink-header', bg: 'theme-pink-bg', icon: 'pi-id-card' };
    if (module.includes('report')) return { header: 'theme-indigo-header', bg: 'theme-indigo-bg', icon: 'pi-chart-bar' };
    
    // Default
    return { header: 'theme-gray-header', bg: 'theme-gray-bg', icon: 'pi-folder' };
  }

  toggleGroup(group: PermissionGroup, checked: boolean) {
    const groupIds = group.permissions.map(p => p.id);
    let newSelection = [...this.selectedPermissionIds];

    if (checked) {
      for (let id of groupIds) {
        if (!newSelection.includes(id)) {
          newSelection.push(id);
        }
      }
    } else {
      newSelection = newSelection.filter(id => !groupIds.includes(id));
    }
    
    // Reassign the array to trigger UI change detection in PrimeNG
    this.selectedPermissionIds = newSelection;
  }

  isGroupSelected(group: PermissionGroup): boolean {
    if (!group.permissions || group.permissions.length === 0) return false;
    return group.permissions.every(p => this.selectedPermissionIds.includes(p.id));
  }

  savePermissions() {
    if (!this.selectedRoleForPermissions) return;
    
    this.rolesService.updatePermissions(this.selectedRoleForPermissions.id, this.selectedPermissionIds).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم حفظ الصلاحيات' });
        this.displayPermissionsDialog = false;
        
        // If the user updated their own role, we might need to reload their permissions!
      },
      error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل الحفظ' })
    });
  }
}
