# Angular i18n Research Results

## Overview
This document contains research findings from the official Angular documentation on internationalization (i18n) implementation and best practices.

## Angular Native i18n System

### Core Concepts
- **Internationalization (i18n)**: The process of designing and preparing your project for use in different locales around the world
- **Localization**: Building project versions for specific locales
- **Locale**: Identifies a region speaking a particular language variant and determines formatting for dates, numbers, currencies, etc.

### Implementation Components

#### 1. Text Marking for Translation
- Uses `i18n` attributes in templates to mark text for extraction
- Custom IDs can be specified with `@@` prefix: `<h1 i18n="@@introductionHeader">Hello i18n!</h1>`
- Supports adding descriptions and meanings for translator context
- Uses `$localize` tagged template strings in component code

#### 2. Key Features
- **Build-time extraction**: Text is extracted during build process
- **Static analysis**: All translatable text must be identifiable at build time
- **Multiple locale builds**: Each locale produces a separate build
- **Automatic locale-specific formatting**: Dates, numbers, currencies automatically formatted per locale

#### 3. Required Dependencies
- `@angular/localize` package (not found in current project)
- Angular CLI i18n commands for extraction and building
- Locale-specific build configurations

#### 4. Template Syntax Examples
```html
<!-- Basic i18n attribute -->
<p i18n>Hello World</p>

<!-- With custom ID -->
<p i18n="@@welcome.message">Welcome to our app</p>

<!-- With description and meaning -->
<p i18n="site.header|An introduction header for this sample@@myIntroductionId">Hello i18n!</p>
```

#### 5. Configuration Requirements
- `angular.json` build configurations for each locale
- Translation files (XLIFF, XMB, or ARML format)
- Locale-specific builds: `ng build --configuration=fr`

## Key Differences from Runtime Translation Systems

### Angular Native i18n
- **Build-time**: Translation happens during build process
- **Static**: All translatable content must be known at build time
- **Performance**: No runtime translation overhead
- **SEO-friendly**: Each locale gets its own build with pre-translated content
- **Bundle size**: Smaller bundles as only one language per build

### Runtime Translation Systems (like ngx-translate)
- **Runtime**: Translation happens in the browser
- **Dynamic**: Can load translations on-demand
- **Performance**: Translation processing at runtime
- **Single build**: One build serves all languages
- **Bundle considerations**: All translations or lazy-loaded translations

## Official Angular Recommendations

### When to Use Native i18n
- Production applications requiring optimal performance
- SEO-critical applications needing locale-specific URLs
- Applications with stable, well-defined translatable content
- When build-time complexity is acceptable for runtime performance gains

### Process Overview
1. **Add localize package**: `ng add @angular/localize`
2. **Mark translatable text**: Add i18n attributes to templates
3. **Extract text**: `ng extract-i18n` to generate translation files
4. **Translate content**: Work with translators on generated files
5. **Configure builds**: Set up locale-specific build configurations
6. **Build per locale**: Generate separate builds for each language
7. **Deploy**: Serve appropriate build based on user locale

## Best Practices
- Use meaningful custom IDs for important messages
- Provide context through descriptions and meanings
- Keep translatable text granular but not overly fragmented
- Plan locale-specific routing and deployment strategy
- Consider ICU message format for complex pluralization and selection

## Removal Considerations
To remove native i18n attributes while keeping runtime translation:
- Remove `i18n` attributes from templates
- Keep runtime translation pipes (e.g., `| translate`)
- Ensure consistent translation key usage
- Maintain single translation system to avoid conflicts