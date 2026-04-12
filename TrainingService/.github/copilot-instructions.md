# Copilot Instructions

## Project Guidelines
- The project should keep logging extensible so it can later send logs to Azure Application Insights and/or a database sink.
- Prefer registering custom middleware via extension methods in Program.cs for consistency (e.g., CorrelationId like GlobalException/Serilog setups).

## API Contract Guidelines
- Use Request/Response suffixes for API/application contract types instead of the Dto suffix.