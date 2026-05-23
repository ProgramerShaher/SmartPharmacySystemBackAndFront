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
            this.applyColor(this.currentColorSub.value);

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

    // --- Color System ---
    
    private currentColorSub = new BehaviorSubject<string>(this.getInitialColor());
    currentColor$ = this.currentColorSub.asObservable();

    private getInitialColor(): string {
        if (isPlatformBrowser(this.platformId)) {
            const saved = localStorage.getItem('app-primary-color');
            return saved || 'emerald'; // Default color
        }
        return 'emerald';
    }

    setColor(colorKey: string) {
        if (isPlatformBrowser(this.platformId)) {
            localStorage.setItem('app-primary-color', colorKey);
            this.currentColorSub.next(colorKey);
            this.applyColor(colorKey);
        }
    }

    private applyColor(colorKey: string) {
        if (!isPlatformBrowser(this.platformId)) return;

        const palette = this.getColorPalette(colorKey);
        if (palette) {
            const root = this.document.documentElement;
            // Apply RGB components and Base hex
            root.style.setProperty('--primary-color', palette.base);
            root.style.setProperty('--primary-rgb', palette.rgb);
            
            // Apply Shades
            root.style.setProperty('--primary-50', palette[50]);
            root.style.setProperty('--primary-100', palette[100]);
            root.style.setProperty('--primary-200', palette[200]);
            root.style.setProperty('--primary-300', palette[300]);
            root.style.setProperty('--primary-400', palette[400]);
            root.style.setProperty('--primary-500', palette[500]);
            root.style.setProperty('--primary-600', palette[600]);
            root.style.setProperty('--primary-700', palette[700]);
            root.style.setProperty('--primary-800', palette[800]);
            root.style.setProperty('--primary-900', palette[900]);
        }
    }

    get currentColorValue(): string {
        return this.currentColorSub.value;
    }

    // Pre-defined Professional Color Palettes
    get colorOptions() {
        return [
            { key: 'emerald', name: 'زمردي (افتراضي)', color: getComputedStyle(document.documentElement).getPropertyValue('--primary-color').trim() || '#10b981' },
            { key: 'blue', name: 'أزرق احترافي', color: '#3b82f6' },
            { key: 'indigo', name: 'نيلي عصري', color: '#6366f1' },
            { key: 'purple', name: 'بنفسجي ملكي', color: '#8b5cf6' },
            { key: 'rose', name: 'وردي', color: '#f43f5e' },
            { key: 'orange', name: 'برتقالي حيوي', color: '#f97316' },
            { key: 'cyan', name: 'سماوي', color: '#06b6d4' }
        ];
    }

    private getColorPalette(key: string): any {
        const palettes: any = {
            'emerald': {
                base: getComputedStyle(document.documentElement).getPropertyValue('--primary-color').trim() || '#10b981', rgb: '16, 185, 129',
                50: '#ecfdf5', 100: '#d1fae5', 200: '#a7f3d0', 300: '#6ee7b7',
                400: '#34d399', 500: getComputedStyle(document.documentElement).getPropertyValue('--primary-color').trim() || '#10b981', 600: getComputedStyle(document.documentElement).getPropertyValue('--primary-600').trim() || '#059669', 700: '#047857',
                800: '#065f46', 900: '#064e3b'
            },
            'blue': {
                base: '#3b82f6', rgb: '59, 130, 246',
                50: '#eff6ff', 100: '#dbeafe', 200: '#bfdbfe', 300: '#93c5fd',
                400: '#60a5fa', 500: '#3b82f6', 600: '#2563eb', 700: '#1d4ed8',
                800: '#1e40af', 900: '#1e3a8a'
            },
            'indigo': {
                base: '#6366f1', rgb: '99, 102, 241',
                50: '#eef2ff', 100: '#e0e7ff', 200: '#c7d2fe', 300: '#a5b4fc',
                400: '#818cf8', 500: '#6366f1', 600: '#4f46e5', 700: '#4338ca',
                800: '#3730a3', 900: '#312e81'
            },
            'purple': {
                base: '#8b5cf6', rgb: '139, 92, 246',
                50: '#f5f3ff', 100: '#ede9fe', 200: '#ddd6fe', 300: '#c4b5fd',
                400: '#a78bfa', 500: '#8b5cf6', 600: '#7c3aed', 700: '#6d28d9',
                800: '#5b21b6', 900: '#4c1d95'
            },
            'rose': {
                base: '#f43f5e', rgb: '244, 63, 94',
                50: '#fff1f2', 100: '#ffe4e6', 200: '#fecdd3', 300: '#fda4af',
                400: '#fb7185', 500: '#f43f5e', 600: '#e11d48', 700: '#be123c',
                800: '#9f1239', 900: '#881337'
            },
            'orange': {
                base: '#f97316', rgb: '249, 115, 22',
                50: '#fff7ed', 100: '#ffedd5', 200: '#fed7aa', 300: '#fdba74',
                400: '#fb923c', 500: '#f97316', 600: '#ea580c', 700: '#c2410c',
                800: '#9a3412', 900: '#7c2d12'
            },
            'cyan': {
                base: '#06b6d4', rgb: '6, 182, 212',
                50: '#ecfeff', 100: '#cffafe', 200: '#a5f3fc', 300: '#67e8f9',
                400: '#22d3ee', 500: '#06b6d4', 600: '#0891b2', 700: '#0e7490',
                800: '#155e75', 900: '#164e63'
            }
        };
        return palettes[key];
    }
}
