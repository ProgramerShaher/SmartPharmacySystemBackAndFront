import { Component, OnInit, inject, PLATFORM_ID, Renderer2, Inject } from '@angular/core';
import { isPlatformBrowser, DOCUMENT } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { ToastModule } from 'primeng/toast';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, ToastModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent implements OnInit {
  title = 'SmartPharmasySystems';
  private platformId = inject(PLATFORM_ID);
  private renderer = inject(Renderer2);
  
  constructor(@Inject(DOCUMENT) private document: Document) {}

  ngOnInit() {
    if (isPlatformBrowser(this.platformId)) {
      // Apply light theme by default
      this.renderer.addClass(this.document.body, 'light-theme');
    }
  }
}
