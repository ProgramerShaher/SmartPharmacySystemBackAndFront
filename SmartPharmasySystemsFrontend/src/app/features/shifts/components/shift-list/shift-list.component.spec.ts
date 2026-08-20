import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ShiftListComponent } from './shift-list.component';

describe('ShiftListComponent', () => {
  let component: ShiftListComponent;
  let fixture: ComponentFixture<ShiftListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ShiftListComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(ShiftListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
