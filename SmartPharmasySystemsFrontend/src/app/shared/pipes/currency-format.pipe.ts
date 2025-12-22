import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
    name: 'currencyFormat',
    standalone: true
})
export class CurrencyFormatPipe implements PipeTransform {

    transform(value: number, currencyCode: string = 'USD'): string {
        return new Intl.NumberFormat('en-US', { style: 'currency', currency: currencyCode }).format(value);
    }
}
