import { Pipe, PipeTransform } from '@angular/core';

const ROLE_CSS: Record<string, string> = {
  ADMIN:       'badge-role-admin',
  SUPERVISEUR: 'badge-role-superviseur',
  DELEGUE:     'badge-role-delegue',
  MEDECIN:     'badge-role-medecin',
  CLIENT:      'badge-role-client',
};

@Pipe({ name: 'roleBadge', standalone: true })
export class RoleBadgePipe implements PipeTransform {
  transform(role: string): string {
    return ROLE_CSS[(role ?? '').toUpperCase()] ?? 'badge-role-default';
  }
}
