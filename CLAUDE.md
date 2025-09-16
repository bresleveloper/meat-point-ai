# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## project goals

the goal of this project is to help a causal person to quickly decide on a meal grocery list and recipe instruction according to his special prefrences and save time on research all that.

## Project Overview

MeatPointAI is an AI-powered meal planning application that generates beef recipes using OpenAI ChatGPT. It's a full-stack web application with .NET Framework 4.8 backend and Angular 18 frontend, deployed to freeasphosting.net.

## Essential Commands

### Development
- `nggo.bat` - Start Angular development server (navigates to frontend and runs `ng serve -o`)
- `ngb.bat` - Build Angular for production with base-href set to `/dist/`
- `npm test` - Run Angular unit tests (from `Meat-Point-AI/AngularFront/`)
- `npm run watch` - Build Angular in watch mode

### Build & Deploy
- `prepare-deploy.bat` - Complete deployment preparation:
  1. Builds Angular application
  2. Copies .NET bin folder to deploy directory
  3. Copies Angular dist files
  4. Creates deployment ZIP file

## Architecture

### Backend (.NET Framework 4.8)
- **Main Project**: `Meat-Point-AI/` - ASP.NET Web API
- **Controllers**: `Controllers/` - API endpoints for AI, Auth, Payment, PDF
- **Services**: `App_Data/` - Business logic (AIChefService, AuthService, DAL, JwtService)
- **Models**: `Models/` - Data models and database scripts
- **Configuration**: `Web.config` (excluded from git for security)

### Frontend (Angular 18)
- **Location**: `Meat-Point-AI/AngularFront/`
- **Pages**: `src/app/components/Pages/` - Main application pages
- **Components**: `src/app/components/General/` - Reusable components
- **Services**: `src/app/services/` - HTTP services for API communication

### Key Integrations
- **OpenAI ChatGPT**: Recipe generation via AIChefService
- **Stripe**: Payment processing (partially implemented)
- **JWT Authentication**: Token-based auth with usage limits
- **Angular Material**: UI components with rose-red theme
- **ngx-translate**: Multi-language support

## Database

SQL Server with LocalDB for development. Main tables:
- Users (authentication, subscription, usage tracking)
- Recipes (generated recipes with ingredients/instructions)
- BeefCuts (beef cut information in English/Hebrew)
- MealPlans, RecipeRatings

## Important Notes

- Based on starter project: [Starter-.NET-4.8-NG-18](https://github.com/bresleveloper/Starter-.NET-4.8-NG-18/)
- Web.config is gitignored for API key protection
- Deployment target: freeasphosting.net with combined frontend/backend hosting
- Angular files served from `/dist/` path, API endpoints under `/api/`
- Default route is `/recipe-generator` (main recipe generation page)

## User Tiers

- **Free Users**: 3 recipes per day
- **Premium Users**: 30 recipes per day
- Subscription handled via Stripe integration

## Development Workflow

1. Use `nggo.bat` for frontend development
2. Backend runs from Visual Studio or IIS Express
3. Build with `ngb.bat` before deployment
4. Deploy with `prepare-deploy.bat` which creates complete deployment package