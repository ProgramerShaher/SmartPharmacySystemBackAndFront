import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';

@Component({
    selector: 'app-unauthorized',
    standalone: true,
    imports: [CommonModule, ButtonModule],
    templateUrl: './unauthorized.component.html',
    styleUrls: ['./unauthorized.component.scss']
})
export class UnauthorizedComponent {
    constructor(private router: Router) {}

    goHome() {
        this.router.navigate(['/dashboard']);
    }

    goBack() {
        window.history.back();
    }
}
