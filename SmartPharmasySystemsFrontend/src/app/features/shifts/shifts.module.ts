import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { ShiftsRoutingModule } from './shifts-routing.module';
import { ShiftListComponent } from './components/shift-list/shift-list.component';
import { ShiftDetailsComponent } from './components/shift-details/shift-details.component';

import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { ToastModule } from 'primeng/toast';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { MessagesModule } from 'primeng/messages';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TabViewModule } from 'primeng/tabview';
import { CheckboxModule } from 'primeng/checkbox';

@NgModule({
  declarations: [
    ShiftListComponent,
    ShiftDetailsComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    ShiftsRoutingModule,
    TableModule,
    ButtonModule,
    DialogModule,
    InputTextModule,
    InputNumberModule,
    InputTextareaModule,
    ToastModule,
    TagModule,
    TooltipModule,
    MessagesModule,
    ConfirmDialogModule,
    TabViewModule,
    CheckboxModule
  ]
})
export class ShiftsModule { }
