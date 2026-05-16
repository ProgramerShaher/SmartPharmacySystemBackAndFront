import { Routes } from '@angular/router';
import { PharmacyProfileComponent } from './components/pharmacy-profile/pharmacy-profile.component';

export const SETTINGS_ROUTES: Routes = [
    {
        path: '',
        component: PharmacyProfileComponent,
        title: 'إعدادات الصيدلية'
    }
];
