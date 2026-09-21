import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'resolveImageUrl',
  standalone: true,
  pure: true,
})
export class ResolveImageUrlPipe implements PipeTransform {
  transform(url: string | null | undefined): string {
    if (!url) return '';
    if (/^https?:\/\//i.test(url)) return url;
    if (url.startsWith('/')) return url;
    return `/${url}`;
  }
}
