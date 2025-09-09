import { Component, OnInit } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-language-switcher',
  templateUrl: './language-switcher.component.html',
  styleUrl: './language-switcher.component.css'
})
export class LanguageSwitcherComponent implements OnInit {
  
  languages = [
    { code: 'en', name: 'English', flag: '🇺🇸' },
    { code: 'es', name: 'Español', flag: '🇪🇸' },
    { code: 'fr', name: 'Français', flag: '🇫🇷' },
    { code: 'he', name: 'עברית', flag: '🇮🇱' }
  ];

  isDropdownOpen = false;
  currentLanguage: string = 'en';

  private detectBrowserLanguage(): string {
    const browserLang = navigator.language || 'en';
    const supportedLanguages = ['en', 'es', 'fr', 'he'];
    
    // Map browser language codes to supported languages
    const langMappings: { [key: string]: string } = {
      'en': 'en', 'en-US': 'en', 'en-GB': 'en', 'en-AU': 'en', 'en-CA': 'en',
      'es': 'es', 'es-ES': 'es', 'es-MX': 'es', 'es-AR': 'es', 'es-CL': 'es', 'es-CO': 'es',
      'fr': 'fr', 'fr-FR': 'fr', 'fr-CA': 'fr', 'fr-BE': 'fr', 'fr-CH': 'fr',
      'he': 'he', 'he-IL': 'he', 'iw': 'he', 'iw-IL': 'he'
    };
    
    // First try exact match, then try language part only (e.g., 'en-US' -> 'en')
    return langMappings[browserLang] || 
           langMappings[browserLang.split('-')[0]] || 
           'en';
  }

  constructor(private translateService: TranslateService) {
    // Set available languages
    this.translateService.addLangs(['en', 'es', 'fr', 'he']);
    
    // Set default language
    this.translateService.setDefaultLang('en');
    
    // Determine initial language with priority: localStorage → browser → default
    const savedLanguage = localStorage.getItem('selectedLanguage');
    const browserLanguage = this.detectBrowserLanguage();
    const supportedLanguages = ['en', 'es', 'fr', 'he'];
    
    let initialLanguage = 'en'; // default fallback
    
    if (savedLanguage && supportedLanguages.includes(savedLanguage)) {
      // Priority 1: Use saved language preference
      initialLanguage = savedLanguage;
    } else if (browserLanguage && supportedLanguages.includes(browserLanguage)) {
      // Priority 2: Use browser detected language
      initialLanguage = browserLanguage;
    }
    
    this.currentLanguage = initialLanguage;
    this.translateService.use(initialLanguage);
  }

  ngOnInit() {
    // Listen to language changes
    this.translateService.onLangChange.subscribe(event => {
      this.currentLanguage = event.lang;
    });
  }

  toggleDropdown() {
    this.isDropdownOpen = !this.isDropdownOpen;
  }

  getCurrentLanguage() {
    return this.languages.find(lang => lang.code === this.currentLanguage) || this.languages[0];
  }

  switchLanguage(languageCode: string) {
    // Save selected language to localStorage
    localStorage.setItem('selectedLanguage', languageCode);
    
    // Switch the language immediately
    this.translateService.use(languageCode);
    
    // Close dropdown
    this.isDropdownOpen = false;
  }
}
