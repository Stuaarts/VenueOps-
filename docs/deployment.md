# Free Deployment Notes

VenueOps is designed to run locally for free with Docker Compose and can also be deployed on free-tier services when account limits allow it.

## Current Free Options Checked

- Render documents free web services and Render Postgres instances for hobby/testing workloads: https://render.com/docs/free
- Vercel documents a free Hobby plan suitable for personal projects and frontend hosting: https://vercel.com/docs/plans/hobby

Always review provider terms and limits before deploying. Do not add a paid database, paid compute plan, or paid add-on for this project unless you intentionally choose to upgrade later.

## Suggested No-Cost Deployment

1. Deploy the backend as a Render Web Service from `backend/Dockerfile`.
2. Create a free Render Postgres database if available in your workspace.
3. Set backend environment variables:
   - `ConnectionStrings__DefaultConnection`
   - `Jwt__Issuer`
   - `Jwt__Audience`
   - `Jwt__SigningKey`
   - `Cors__AllowedOrigins__0`
   - `SeedDemoData`
4. Deploy the frontend to Vercel as a Vite app.
5. Set `VITE_API_BASE_URL` to the Render backend URL plus `/api`.
6. Set the API CORS origin to the Vercel URL.

## Safe Fallback

If free hosting is unavailable or requires a payment method you do not want to provide, use:

```bash
docker compose up --build
```

Then open:

- Frontend: http://localhost:8080
- API Swagger: http://localhost:5000/swagger
