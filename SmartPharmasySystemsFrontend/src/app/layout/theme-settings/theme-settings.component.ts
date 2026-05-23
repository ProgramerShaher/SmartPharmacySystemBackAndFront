import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SidebarModule } from 'primeng/sidebar';
import { ButtonModule } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';
import { DividerModule } from 'primeng/divider';
import { ThemeService } from '../../core/services/theme.service';

@Component({
  selector: 'app-theme-settings',
  standalone: true,
  imports: [CommonModule, SidebarModule, ButtonModule, TooltipModule, DividerModule],
  templateUrl: './theme-settings.component.html',
  styleUrls: ['./theme-settings.component.scss']
})
export class ThemeSettingsComponent implements OnInit {
  private themeService = inject(ThemeService);

  visible = false;
  currentTheme = 'light';
  currentColor = 'emerald';
  colorOptions = this.themeService.colorOptions;

  ngOnInit() {
    this.themeService.currentTheme$.subscribe(theme => {
      this.currentTheme = theme;
    });

    this.themeService.currentColor$.subscribe(color => {
      this.currentColor = color;
    });
  }

  show() {
    this.visible = true;
  }

  setTheme(theme: 'light' | 'dark' | 'system') {
    this.themeService.setTheme(theme);
  }

  setColor(colorKey: string) {
    this.themeService.setColor(colorKey);
  }
}
