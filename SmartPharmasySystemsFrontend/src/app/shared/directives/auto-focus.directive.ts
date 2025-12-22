import { Directive, ElementRef, AfterViewInit } from '@angular/core';

@Directive({
    selector: '[appAutoFocus]',
    standalone: true
})
export class AutoFocusDirective implements AfterViewInit {

    constructor(private el: ElementRef) { }

    ngAfterViewInit(): void {
        this.el.nativeElement.focus();
    }
}
