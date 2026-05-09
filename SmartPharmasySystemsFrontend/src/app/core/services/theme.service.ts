import { Injectable, Renderer2, RendererFactory2, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser, DOCUMENT } from '@angular/common';
import { BehaviorSubject, Observable } from 'rxjs';

export type Theme = 'light' | 'dark' | 'system';

@Injectable({
    providedIn: 'root'
})
export class ThemeService {
    private renderer: Renderer2;
    private currentThemeSub: BehaviorSubject<Theme>;
    currentTheme$: Observable<Theme>;

    constructor(
        rendererFactory: RendererFactory2,
        @Inject(PLATFORM_ID) private platformId: Object,
        @Inject(DOCUMENT) private document: Document
    ) {
        this.renderer = rendererFactory.createRenderer(null, null);
        this.currentThemeSub = new BehaviorSubject<Theme>(this.getInitialTheme());
        this.currentTheme$ = this.currentThemeSub.asObservable();

        if (isPlatformBrowser(this.platformId)) {
            this.applyTheme(this.currentThemeSub.value);

            // Listen to system preference changes
            window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', e => {
                if (this.currentThemeSub.value === 'system') {
                    this.applyTheme('system');
                }
            });
        }
    }

    private getInitialTheme(): Theme {
        if (isPlatformBrowser(this.platformId)) {
            const saved = localStorage.getItem('app-theme') as Theme;
            return saved || 'light'; // Default to light if nothing saved
        }
        return 'light';
    }

    setTheme(theme: Theme) {
        if (isPlatformBrowser(this.platformId)) {
            localStorage.setItem('app-theme', theme);
            this.currentThemeSub.next(theme);
            this.applyTheme(theme);
        }
    }

    toggleTheme() {
        const nextTheme = this.currentThemeSub.value === 'dark' ? 'light' : 'dark';
        this.setTheme(nextTheme);
    }

    private applyTheme(theme: Theme) {
        if (!isPlatformBrowser(this.platformId)) return;

        let actualTheme = theme;
        if (theme === 'system') {
            actualTheme = window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
        }

        if (actualTheme === 'dark') {
            this.renderer.addClass(this.document.body, 'dark-theme');
            this.renderer.removeClass(this.document.body, 'light-theme');
        } else {
            this.renderer.addClass(this.document.body, 'light-theme');
            this.renderer.removeClass(this.document.body, 'dark-theme');
        }
    }

    get currentThemeValue(): Theme {
        return this.currentThemeSub.value;
    }
}
