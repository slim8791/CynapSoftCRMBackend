import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  constructor(private authService: AuthService, private router: Router) { }

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(req).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status === 401) {
          // Unauthorized - clear auth and redirect to login
          this.authService.logout();
          this.router.navigate(['/login']);
        } else if (error.status === 403) {
          // Forbidden
          console.error('Access forbidden', error);
        } else if (error.status >= 500) {
          // Server error
          console.error('Server error', error);
        }

        return throwError(() => error);
      })
    );
  }
}
