import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ConfirmationService, MessageService } from 'primeng/api';
import { UsersService } from '../../services/users.service';
import { User } from '../../../../core/models';
import { Table } from 'primeng/table';
import { TableModule } from 'primeng/table';
import { ToastModule } from 'primeng/toast';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { InputTextModule } from 'primeng/inputtext';
import { TooltipModule } from 'primeng/tooltip';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ToolbarModule } from "primeng/toolbar";
import { UserFormComponent } from "../user-add-edit/user-add-edit.component";

@Component({
    selector: 'app-user-list',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        TableModule,
        DialogModule,
        ButtonModule,
        InputTextModule,
        ConfirmDialogModule,
        TooltipModule,
        ProgressSpinnerModule,
        ToastModule,
        ToolbarModule,
        UserFormComponent
    ],
    providers: [MessageService, ConfirmationService],
    templateUrl: './user-list.component.html',
    styleUrls: ['./user-list.component.scss']
})
export class UserListComponent implements OnInit {
    @ViewChild('dt') dt!: Table;

    users: User[] = [];
    filteredUsers: User[] = [];
    loading: boolean = true;
    userDialog: boolean = false;
    viewDialog: boolean = false;
    selectedUser: User | null = null;
    viewedUser: User | null = null;
    searchTerm: string = '';

    // إحصائيات
    adminCount: number = 0;
    regularUserCount: number = 0;

    constructor(
        private usersService: UsersService,
        private messageService: MessageService,
        private confirmationService: ConfirmationService
    ) { }

    ngOnInit() {
        this.loadUsers();
    }

    /**
     * تحميل قائمة المستخدمين
     */
    loadUsers(): void {
        this.loading = true;
        this.usersService.search({}).subscribe({
            next: (data) => {
                this.users = data.items || [];
                this.filteredUsers = [...this.users];
                this.calculateStats();
                this.loading = false;
            },
            error: (error) => {
                console.error('Error loading users:', error);
                this.messageService.add({
                    severity: 'error',
                    summary: 'خطأ',
                    detail: 'فشل في تحميل بيانات المستخدمين',
                    life: 3000
                });
                this.loading = false;
            }
        });
    }

    /**
     * حساب الإحصائيات
     */
    private calculateStats(): void {
        this.adminCount = this.users.filter(user =>
            user.roleName?.toLowerCase() === 'admin' ||
            user.roleName?.toLowerCase() === 'administrator'
        ).length;

        this.regularUserCount = this.users.length - this.adminCount;
    }

    /**
     * فتح نافذة إضافة مستخدم جديد
     */
    openNew(): void {
        this.selectedUser = null;
        this.userDialog = true;
    }

    /**
     * فتح نافذة تعديل مستخدم
     */
    editUser(user: User): void {
        this.selectedUser = { ...user };
        this.userDialog = true;
    }

    /**
     * فتح نافذة عرض تفاصيل المستخدم
     */
    viewUser(user: User): void {
        this.viewedUser = { ...user };
        this.viewDialog = true;
    }

    /**
     * تعديل المستخدم المعروض
     */
    editViewedUser(): void {
        if (this.viewedUser) {
            this.selectedUser = { ...this.viewedUser };
            this.viewDialog = false;
            this.userDialog = true;
        }
    }

    /**
     * حذف مستخدم
     */
    deleteUser(user: User): void {
        this.confirmationService.confirm({
            message: `هل أنت متأكد من حذف المستخدم "${user.username}"؟`,
            header: 'تأكيد الحذف',
            icon: 'pi pi-exclamation-triangle',
            acceptLabel: 'نعم، احذف',
            rejectLabel: 'إلغاء',
            acceptButtonStyleClass: 'p-button-danger',
            rejectButtonStyleClass: 'p-button-text',
            accept: () => {
                this.usersService.delete(user.id).subscribe({
                    next: () => {
                        this.messageService.add({
                            severity: 'success',
                            summary: 'تم الحذف',
                            detail: `تم حذف المستخدم "${user.username}" بنجاح`,
                            life: 3000
                        });
                        this.loadUsers();
                    },
                    error: (error) => {
                        console.error('Error deleting user:', error);
                        this.messageService.add({
                            severity: 'error',
                            summary: 'خطأ',
                            detail: 'فشل في حذف المستخدم',
                            life: 3000
                        });
                    }
                });
            }
        });
    }

    /**
     * البحث في الجدول
     */
    onSearch(value: string): void {
        this.searchTerm = value;
        this.dt.filterGlobal(value, 'contains');

        // تحديث filteredUsers للعرض الصحيح
        if (!value.trim()) {
            this.filteredUsers = [...this.users];
        } else {
            const searchLower = value.toLowerCase();
            this.filteredUsers = this.users.filter(user =>
                (user.username?.toLowerCase().includes(searchLower)) ||
                (user.fullName?.toLowerCase().includes(searchLower)) ||
                (user.roleName?.toLowerCase().includes(searchLower)) ||
                (user.phoneNumber?.includes(value))
            );
        }
    }

    /**
     * مسح البحث
     */
    clearSearch(): void {
        this.searchTerm = '';
        this.dt.filterGlobal('', 'contains');
        this.filteredUsers = [...this.users];
    }

    /**
     * الحصول على كلاس CSS للصلاحية
     */
    getRoleClass(role: string | undefined): string {
        if (!role) return 'role-unknown';

        const roleLower = role.toLowerCase();
        if (roleLower.includes('admin')) return 'role-admin';
        if (roleLower.includes('manager')) return 'role-manager';
        if (roleLower.includes('user') || roleLower.includes('regular')) return 'role-user';
        if (roleLower.includes('editor')) return 'role-editor';
        return 'role-other';
    }

    /**
     * الحصول على أيقونة للصلاحية
     */
    getRoleIcon(role: string | undefined): string {
        if (!role) return 'pi pi-question-circle';

        const roleLower = role.toLowerCase();
        if (roleLower.includes('admin')) return 'pi pi-shield';
        if (roleLower.includes('manager')) return 'pi pi-briefcase';
        if (roleLower.includes('user') || roleLower.includes('regular')) return 'pi pi-user';
        if (roleLower.includes('editor')) return 'pi pi-file-edit';
        return 'pi pi-tag';
    }

    /**
     * الحصول على اسم عرضي للصلاحية
     */
    getRoleDisplayName(role: string | undefined): string {
        if (!role) return 'غير محدد';

        const roleLower = role.toLowerCase();
        if (roleLower.includes('admin')) return 'مدير';
        if (roleLower.includes('manager')) return 'مدير';
        if (roleLower.includes('user') || roleLower.includes('regular')) return 'مستخدم';
        if (roleLower.includes('editor')) return 'محرر';
        return role;
    }

    /**
     * إغلاق نافذة الحوار
     */
    hideDialog(): void {
        this.userDialog = false;
        this.selectedUser = null;
    }

    /**
     * عند حفظ المستخدم
     */
    onSave(event: any): void {
        this.userDialog = false;
        this.loadUsers();
        this.messageService.add({
            severity: 'success',
            summary: 'تم الحفظ',
            detail: event?.id ? 'تم تحديث المستخدم بنجاح' : 'تم إضافة المستخدم بنجاح',
            life: 3000
        });
    }
}