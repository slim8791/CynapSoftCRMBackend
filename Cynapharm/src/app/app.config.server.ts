import { mergeApplicationConfig, ApplicationConfig } from '@angular/core';
import { provideServerRendering, withRoutes } from '@angular/ssr';
import { appConfig } from './app.config';
import { serverRoutes } from './app.routes.server';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class AppConfigService {
  private readonly apiEndpoints = {
    auth: '/api/auth',
    products: '/api/products',
    orders: '/api/orders',
    inventory: '/api/inventory',
    field: '/api/field',
    documents: '/api/documents',
  };

  getApiEndpoint(service: keyof typeof this.apiEndpoints): string {
    return this.apiEndpoints[service];
  }
}

const serverConfig: ApplicationConfig = {
  providers: [
    provideServerRendering(withRoutes(serverRoutes))
  ]
};

export const config = mergeApplicationConfig(appConfig, serverConfig);
