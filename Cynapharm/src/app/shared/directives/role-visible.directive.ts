import { Directive, Input, OnInit, TemplateRef, ViewContainerRef } from '@angular/core';
import { AuthService, UserRole } from '../../core/services/auth.service';

/**
 * Structural directive — shows the element only if the current user has one of the given roles.
 * Usage: *appRoleVisible="['ADMIN', 'SUPERVISEUR']"
 */
@Directive({
  selector: '[appRoleVisible]',
  standalone: true
})
export class RoleVisibleDirective implements OnInit {

  @Input('appRoleVisible') allowedRoles: string[] = [];

  private hasView = false;

  constructor(
    private templateRef:     TemplateRef<any>,
    private viewContainer:   ViewContainerRef,
    private authService:     AuthService
  ) {}

  ngOnInit(): void {
    const role = this.authService.getUserRole();
    const allowed = this.allowedRoles.includes(role as string);

    if (allowed && !this.hasView) {
      this.viewContainer.createEmbeddedView(this.templateRef);
      this.hasView = true;
    } else if (!allowed && this.hasView) {
      this.viewContainer.clear();
      this.hasView = false;
    }
  }
}
