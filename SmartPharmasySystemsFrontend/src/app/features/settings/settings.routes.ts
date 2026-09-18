import { Routes } from '@angular/router';
import { PharmacyProfileComponent } from './components/pharmacy-profile/pharmacy-profile.component';
import { BusinessProfileSettingsComponent } from './components/business-profile-settings/business-profile-settings.component';

export const SETTINGS_ROUTES: Routes = [
    {
        path: '',
        component: PharmacyProfileComponent,
        title: 'إعدادات المنشأة'
    },
    {
        path: 'business-profile',
        component: BusinessProfileSettingsComponent,
        title: 'نمط النشاط التجاري والسياسات'
    }
];
