import { Directive, Input, TemplateRef, ViewContainerRef, inject, OnInit, OnDestroy } from '@angular/core';
import { PermissionService } from '../../core/services/permission.service';
import { AuthService } from '../../features/auth/services/auth.service';
import { Subscription } from 'rxjs';

@Directive({
    selector: '[appHasPermission]',
    standalone: true
})
export class HasPermissionDirective implements OnInit, OnDestroy {
    private permissionService = inject(PermissionService);
    private authService = inject(AuthService);
    private templateRef = inject(TemplateRef<any>);
    private viewContainer = inject(ViewContainerRef);

    private permissionCode: string = '';
    private isHidden = true;
    private sub?: Subscription;

    @Input() set appHasPermission(val: string) {
        this.permissionCode = val;
        this.updateView();
    }

    ngOnInit() {
        this.sub = this.permissionService.permissions$.subscribe(() => {
            this.updateView();
        });
    }

    private updateView() {
        const isAdmin = this.authService.isAdmin();
        const hasPermission = isAdmin || this.permissionService.hasPermission(this.permissionCode);

        if (hasPermission && this.isHidden) {
            this.viewContainer.createEmbeddedView(this.templateRef);
            this.isHidden = false;
        } else if (!hasPermission && !this.isHidden) {
            this.viewContainer.clear();
            this.isHidden = true;
        }
    }

    ngOnDestroy() {
        this.sub?.unsubscribe();
    }
}
