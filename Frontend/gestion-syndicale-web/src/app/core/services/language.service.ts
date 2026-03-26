import { Injectable, Renderer2, RendererFactory2 } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { firstValueFrom } from 'rxjs';

export type AppLanguage = 'fr' | 'ar';

@Injectable({ providedIn: 'root' })
export class LanguageService {
  private renderer: Renderer2;
  private readonly STORAGE_KEY = 'app_language';

  constructor(
    private translate: TranslateService,
    rendererFactory: RendererFactory2
  ) {
    this.renderer = rendererFactory.createRenderer(null, null);
  }

  async initAsync(): Promise<void> {
    const saved = localStorage.getItem(this.STORAGE_KEY) as AppLanguage | null;
    const lang: AppLanguage = saved === 'ar' ? 'ar' : 'fr';
    this.applyDirection(lang);
    await firstValueFrom(this.translate.use(lang));
  }

  init(): void {
    const saved = localStorage.getItem(this.STORAGE_KEY) as AppLanguage | null;
    const lang: AppLanguage = saved === 'ar' ? 'ar' : 'fr';
    this.setLanguage(lang);
  }

  private applyDirection(lang: AppLanguage): void {
    const html = document.documentElement;
    if (lang === 'ar') {
      this.renderer.setAttribute(html, 'dir', 'rtl');
      this.renderer.setAttribute(html, 'lang', 'ar');
    } else {
      this.renderer.setAttribute(html, 'dir', 'ltr');
      this.renderer.setAttribute(html, 'lang', 'fr');
    }
  }

  setLanguage(lang: AppLanguage): void {
    this.translate.use(lang);
    localStorage.setItem(this.STORAGE_KEY, lang);
    this.applyDirection(lang);
  }

  getCurrentLang(): AppLanguage {
    return (this.translate.currentLang as AppLanguage) || 'fr';
  }

  isRtl(): boolean {
    return this.getCurrentLang() === 'ar';
  }
}
