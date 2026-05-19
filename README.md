CloudNativeInventory.Api
=======================================================================================================================================================
A REST API designed for inventory management, integrated with a GitHub Actions CI/CD pipeline and Azure infrastructure using managed identities and RBAC.

Azure Services Used
* Azure Container Apps (ACA) - Serverless hosting for running the containerized API.
* Azure Container Registry (ACR) - Registry for storage of Docker container images.
* Azure Key Vault - Centralized vault used to protect sensitive external API keys.

=========================================================================================================================================================
Architecture Decision Record (ADR)

Context:
The application requires integration with an external vendor API (`VendorApiKey`). No credentials or keys may be hardcoded or committed to version control.

Decisions & Motivations:
* System-assigned Managed Identity: Enabled on the Container App so it can authenticate against Azure Key Vault natively without explicitly managing credentials.
* Azure RBAC: The Container App is assigned the 'Key Vault Secrets User' role (Least Privilege), restricting its permissions strictly to reading secrets. Administrative accounts use 'Key Vault Secrets Officer' to manage values.
* Configuration Mapping: The secret is named 'ExternalServices--VendorApiKey' in the vault. (`ExternalServices:VendorApiKey`).

============================================================================================

Inventory Resource

| Method | Endpoint | Description | Auth |
| :--- | :--- | :--- | :--- |
| GET | `/api/inventory` | Get all products from the database | Public |
| GET | `/api/inventory/system/verify-integration` | Verifies external Key Vault integration | Public |

============================================================================================

To build and run this API:

docker build -t cloudnative-api -f CloudNativeInventory.Api/Dockerfile .

docker run -d -p 8080:8080 -e ASPNETCORE_ENVIRONMENT=Development -e ExternalServices__VendorApiKey="your-local-test-key" cloudnative-api

http://localhost:8080/api/inventory/system/verify-integration in your webbrowser.

============================================================================================

CI/CD Pipeline Architecture
Workflow managed via GitHub Actions.

Pipeline Triggers:
* Automated execution occurs on every git push to the main branch.

Execution Steps:
1. Setup: Pulls the repository code and initializes the .NET SDK environment.
2. Build: Resolves NuGet package dependencies and compiles the source code.
3. Test: Runs the automated xUnit suite utilizing Moq. Any test failure aborts the deployment.
4. Docker: Compiles the container using a multi-stage, rootless Dockerfile and tags it with the Commit-SHA.
5. Push & Deploy: Pushes the image to ACR and signals Azure Container Apps to perform a rolling update.

============================================================================================

Deployment and Verification

Endpoint: https://inventory-api-app.bluewater-778a5a82.germanywestcentral.azurecontainerapps.io/api/inventory/system/verify-integration

Expected Response:
{
  "status": "Secured",
  "message": "Hemlighet laddades framgångsrikt via säker konfiguration."
}

Conclusion: The successful response confirms that the Container App successfully resolves its Managed Identity, passes RBAC validation, translates the secret naming convention, and loads the production value without exposing credentials in Git.
