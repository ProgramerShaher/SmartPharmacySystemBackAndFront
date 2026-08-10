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
import { TreeModule } from 'primeng/tree';
import { TreeNode } from 'primeng/api';
import { HasPermissionDirective } from '../../../../shared/directives/has-permission.directive';
import { ColorPickerModule } from 'primeng/colorpicker';
import { InputSwitchModule } from 'primeng/inputswitch';

@Component({
  selector: 'app-roles-list',
  standalone: true,
  imports: [
    CommonModule, FormsModule, ReactiveFormsModule, TableModule, ButtonModule,
    DialogModule, InputTextModule, ToastModule, ConfirmDialogModule, TooltipModule,
    TreeModule, HasPermissionDirective, ColorPickerModule, InputSwitchModule
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

  // Permissions Tree
  permissionGroups: PermissionGroup[] = [];
  permissionsTree: TreeNode[] = [];
  selectedPermissions: TreeNode[] = [];
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
      this.permissionsTree = this.buildTree(res);
    });
  }

  buildTree(groups: PermissionGroup[]): TreeNode[] {
    return groups.map(g => {
      return {
        key: 'g_' + g.module,
        label: g.moduleAr || g.module,
        data: 'module',
        expanded: true,
        children: g.permissions.map(p => ({
          key: p.id.toString(),
          label: p.actionAr || p.action,
          data: p.id
        }))
      };
    });
  }

  openNew() {
    this.isEdit = false;
    this.roleForm.reset({ isActive: true, color: '#3b82f6', id: 0 });
    this.displayDialog = true;
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
      color: role.color,
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
      color: val.color,
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
    this.selectedPermissions = [];
    
    this.rolesService.getPermissions(role.id).subscribe(res => {
      const perms = res.data || [];
      this.selectedPermissions = this.findNodes(this.permissionsTree, perms);
      this.displayPermissionsDialog = true;
    });
  }

  findNodes(tree: TreeNode[], ids: number[]): TreeNode[] {
    let result: TreeNode[] = [];
    for (let node of tree) {
      if (node.children) {
        const childMatches = this.findNodes(node.children, ids);
        result = [...result, ...childMatches];
        // PrimeNG Tree automatically checks parents if all children are checked
      } else {
        if (ids.includes(node.data)) {
          result.push(node);
        }
      }
    }
    return result;
  }

  savePermissions() {
    if (!this.selectedRoleForPermissions) return;
    
    // Only get leaves (actual permission IDs), ignore group nodes
    const ids = this.selectedPermissions
      .filter(n => typeof n.data === 'number')
      .map(n => n.data as number);
      
    this.rolesService.updatePermissions(this.selectedRoleForPermissions.id, ids).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم حفظ الصلاحيات' });
        this.displayPermissionsDialog = false;
        
        // If the user updated their own role, we might need to reload their permissions!
      },
      error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل الحفظ' })
    });
  }
}
