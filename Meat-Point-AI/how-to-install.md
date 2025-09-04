
  API Keys Needed:

  - OpenAI API Key (for recipe generation)
  - Stripe Keys (for payment processing - optional for demo)


  2. Backend Configuration (.NET API)

  Update Web.config with your settings:

  <appSettings>
    <!-- Database connection -->
    <add key="SqlConnectionString" value="Server=localhost;Database=BeefMaster;Trusted_Connection=True;" />

    <!-- API Keys -->
    <add key="OpenAI_API_Key" value="your-openai-api-key-here" />
    <add key="Stripe_Secret_Key" value="sk_test_your-stripe-secret-key" />
    <add key="Stripe_Publishable_Key" value="pk_test_your-stripe-publishable-key" />
  </appSettings>

  ---
  🌐 Access the Application

  1. Backend API: http://localhost:49476/api
  2. Frontend App: http://localhost:4200
  3. Default route: Redirects to login page

  ---
  🔧 Troubleshooting

  Common Issues:

  Database Connection Failed:
  # Check connection string in Web.config
  # Ensure SQL Server is running
  # Verify database exists and scripts were run

  OpenAI API Errors:
  # Verify API key is correct in Web.config
  # Check OpenAI account has credits
  # Test with a simple recipe generation

  Angular Build Errors:
  # Clear node modules and reinstall
  rm -rf node_modules package-lock.json
  npm install

  # Update Angular CLI if needed
  npm install -g @angular/cli@latest

  CORS Issues:
  # Backend already configured for CORS
  # Check WebApiConfig.cs has EnableCors attribute
  # Verify Angular environment.ts has correct API URL

  ---
  🎯 Key URLs to Test

  - Home: http://localhost:4200
  - API Health: http://localhost:49476/api/BeefCut (should return beef cuts)
  - PDF Generation: http://localhost:49476/api/PDF/GenerateRecipePDF?recipeId=1
  - User Registration: http://localhost:4200/register
  - Recipe Generator: http://localhost:4200/recipe-generator

  ---
  🥩 You're now ready to master beef cooking with AI! 🤖

  The app should start with the beautiful beef-themed login screen, guide users through the educational beef cut selection, generate personalized recipes, and export professional PDFs for shopping and cooking.



  # session
  after enabling mvc.session bla bla must add set-cookie
  https://stackoverflow.com/questions/55684110/in-every-request-new-session-is-generating-in-the-web-api