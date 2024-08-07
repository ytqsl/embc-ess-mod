import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'enumToArray',
  standalone: true
})
export class EnumToArrayPipe implements PipeTransform {
  transform(value): Array<string> {
    return Object.keys(value).filter((e) => e);
  }
}
