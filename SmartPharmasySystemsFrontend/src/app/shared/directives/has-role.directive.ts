import { Directive, Input, TemplateRef, ViewContainerRef } from '@angular/core';

@Directive({
    selector: '[appHasRole]',
    standalone: true
})
export class HasRoleDirective {
    @Input() set appHasRole(role: string) {
        // TODO: Check user role
        const hasRole = true;
        if (hasRole) {
            this.viewContainer.createEmbeddedView(this.templateRef);
        } else {
            this.viewContainer.clear();
        }
    }

    constructor(
        private templateRef: TemplateRef<any>,
        private viewContainer: ViewContainerRef
    ) { }
}
