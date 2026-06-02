// ── cloudinary.service.ts ─────────────────────────────────────────────────────
// Uses HttpBackend directly to bypass the token interceptor — Cloudinary's API
// must not receive the app's Authorization header.
//
// PRESETS REQUIRED (Cloudinary dashboard → Settings → Upload → Upload presets):
//   cynapharm_upload  — Signing: Unsigned, resource type: Image
//   cynapharm_raw     — Signing: Unsigned, resource type: Raw  (PDF, doc, mp4…)

import { Injectable } from '@angular/core';
import { HttpBackend, HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

@Injectable({ providedIn: 'root' })
export class CloudinaryService {

  private readonly cloudName   = 'dezdp9rcc';
  private readonly imagePreset = 'cynapharm_upload';
  private readonly rawPreset   = 'cynapharm_raw';
  private readonly base        = `https://api.cloudinary.com/v1_1/${this.cloudName}`;

  private readonly imageExts   = new Set(['jpg','jpeg','png','webp','gif']);

  private readonly http: HttpClient;

  constructor(handler: HttpBackend) {
    this.http = new HttpClient(handler);
  }

  /** Upload a product image (jpg/png/webp). */
  uploadImage(file: File): Observable<string> {
    const form = new FormData();
    form.append('file', file);
    form.append('upload_preset', this.imagePreset);

    return this.http
      .post<{ secure_url: string }>(`${this.base}/image/upload`, form)
      .pipe(map(res => res.secure_url));
  }

  /**
   * Upload any support file — routes to /image/upload or /raw/upload
   * based on the file extension so each preset stays in scope.
   */
  uploadFile(file: File): Observable<string> {
    const ext    = file.name.split('.').pop()?.toLowerCase() ?? '';
    const isImg  = this.imageExts.has(ext);

    const form = new FormData();
    form.append('file', file);
    form.append('upload_preset', isImg ? this.imagePreset : this.rawPreset);

    const endpoint = isImg ? 'image/upload' : 'raw/upload';
    return this.http
      .post<{ secure_url: string }>(`${this.base}/${endpoint}`, form)
      .pipe(map(res => res.secure_url));
  }
}
