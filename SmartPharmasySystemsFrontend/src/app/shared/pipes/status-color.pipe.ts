import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
    name: 'statusColor',
    standalone: true
})
export class StatusColorPipe implements PipeTransform {

    transform(value: string): string {
        switch (value.toLowerCase()) {
            case 'active': return 'success';
            case 'inactive': return 'danger';
            case 'pending': return 'warning';
            default: return 'secondary';
        }
    }
}
