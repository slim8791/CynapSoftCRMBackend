import { Component, EventEmitter, OnInit, OnDestroy, Output } from '@angular/core';
import { environment } from '../../../../environments/environment';

declare const turnstile: any;

@Component({
  selector: 'app-turnstile',
  standalone: true,
  template: `<div id="turnstile-container"></div>`
})
export class TurnstileComponent implements OnInit, OnDestroy {

  @Output() resolved = new EventEmitter<string>();
  @Output() errored  = new EventEmitter<void>();

  private widgetId: string | null = null;
  private pollInterval: any = null;

  ngOnInit(): void {
    // Wait for Cloudflare script to load
    this.pollInterval = setInterval(() => {
      if (typeof turnstile !== 'undefined') {
        clearInterval(this.pollInterval);
        this.pollInterval = null;
        this.widgetId = turnstile.render('#turnstile-container', {
          sitekey: environment.turnstileSiteKey,
          callback: (token: string) => {
            this.resolved.emit(token);
          },
          'error-callback': () => {
            this.errored.emit();
          },
          'expired-callback': () => {
            this.resolved.emit('');
          }
        });
      }
    }, 100);
  }

  public reset(): void {
    if (this.widgetId !== null && typeof turnstile !== 'undefined') {
      try {
        turnstile.reset(this.widgetId);
      } catch (_) {}
    }
    this.resolved.emit('');
  }

  ngOnDestroy(): void {
    if (this.pollInterval) {
      clearInterval(this.pollInterval);
    }
    if (this.widgetId !== null && typeof turnstile !== 'undefined') {
      try {
        turnstile.remove(this.widgetId);
      } catch (_) {
        // Ignore removal errors during teardown
      }
    }
  }
}
