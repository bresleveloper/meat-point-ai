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

  constructor(private translateService: TranslateService) {
    // Set available languages
    this.translateService.addLangs(['en', 'es', 'fr', 'he']);
    
    // Set default language
    this.translateService.setDefaultLang('en');
    
    // Get saved language from localStorage or use default
    const savedLanguage = localStorage.getItem('selectedLanguage') || 'en';
    // Ensure the saved language is supported
    if (['en', 'es', 'fr', 'he'].includes(savedLanguage)) {
      this.currentLanguage = savedLanguage;
      this.translateService.use(savedLanguage);
    } else {
      this.currentLanguage = 'en';
      this.translateService.use('en');
    }
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
