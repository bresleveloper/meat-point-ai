


  1. API Keys Needed:

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


  3. create DB
  
  * create db in sql
  * run scripts under `Models\scripts`

