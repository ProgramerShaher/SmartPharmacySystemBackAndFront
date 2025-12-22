import { Injectable, Renderer2, RendererFactory2 } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export type Theme = 'light' | 'dark' | 'system';

@Injectable({
    providedIn: 'root'
})
export class ThemeService {
    private renderer: Renderer2;
    private currentThemeSub = new BehaviorSubject<Theme>(this.getInitialTheme());
    currentTheme$ = this.currentThemeSub.asObservable();

    constructor(rendererFactory: RendererFactory2) {
        this.renderer = rendererFactory.createRenderer(null, null);
        this.applyTheme(this.currentThemeSub.value);

        // Listen to system preference changes
        window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', e => {
            if (this.currentThemeSub.value === 'system') {
                this.applyTheme('system');
            }
        });
    }

    private getInitialTheme(): Theme {
        const saved = localStorage.getItem('app-theme') as Theme;
        return saved || 'light'; // Default to light if nothing saved
    }

    setTheme(theme: Theme) {
        localStorage.setItem('app-theme', theme);
        this.currentThemeSub.next(theme);
        this.applyTheme(theme);
    }

    toggleTheme() {
        const nextTheme = this.currentThemeSub.value === 'dark' ? 'light' : 'dark';
        this.setTheme(nextTheme);
    }

    private applyTheme(theme: Theme) {
        let actualTheme = theme;
        if (theme === 'system') {
            actualTheme = window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
        }

        if (actualTheme === 'dark') {
            this.renderer.addClass(document.body, 'dark-theme');
            this.renderer.removeClass(document.body, 'light-theme');
        } else {
            this.renderer.addClass(document.body, 'light-theme');
            this.renderer.removeClass(document.body, 'dark-theme');
        }
    }

    get currentThemeValue(): Theme {
        return this.currentThemeSub.value;
    }
}
