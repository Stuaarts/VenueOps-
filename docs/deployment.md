# Free Deployment Notes

VenueOps is designed to run locally for free with Docker Compose and can also be deployed on free-tier services when account limits allow it.

## Current Free Options Checked

- Render documents free web services and Render Postgres instances for hobby/testing workloads: https://render.com/docs/free
- Vercel documents a free Hobby plan suitable for personal projects and frontend hosting: https://vercel.com/docs/plans/hobby
- Neon documents a free Postgres plan suitable for prototypes and side projects: https://neon.com/docs/introduction/plans

Always review provider terms and limits before deploying. Do not add a paid database, paid compute plan, or paid add-on for this project unless you intentionally choose to upgrade later.

## Suggested No-Cost Deployment For A Public Demo

Use Vercel for the static React frontend, Render for the ASP.NET Core API, and Neon for the free Postgres database. This avoids Render's free Postgres 30-day expiration while keeping the demo on free services.

### 1. Create Neon Postgres

Create a Neon free project and copy the pooled or direct connection string. VenueOps accepts either an ASP.NET/Npgsql keyword connection string or a standard provider URL like:

```text
postgresql://user:password@host:5432/database?sslmode=require
```

### 2. Deploy The API To Render

The repository includes `render.yaml` for the backend web service.

1. Open the Render Blueprint URL for this repo:

   ```text
   https://dashboard.render.com/blueprint/new?repo=https://github.com/Stuaarts/VenueOps-
   ```

2. Use the free web service plan.
3. Set `DATABASE_URL` to the Neon connection string.
4. Keep `SeedDemoData=true` so the demo accounts are created at startup.
5. After deploy, verify:

   ```text
   https://YOUR_RENDER_SERVICE.onrender.com/health
   ```

### 3. Deploy The Frontend To Vercel

The repository includes `vercel.json` so Vercel can build the React app from the monorepo root.

Set this Vercel environment variable before the production deploy:

```text
VITE_API_BASE_URL=https://YOUR_RENDER_SERVICE.onrender.com/api
```

Then deploy from the GitHub repo or with:

```bash
vercel --prod
```

### 4. Tighten CORS After The Vercel URL Is Known

`render.yaml` uses `Cors__AllowedOrigins__0=*` to avoid a chicken-and-egg setup problem before the Vercel URL exists. After the frontend URL is stable, replace it in Render with the exact Vercel production URL:

```text
Cors__AllowedOrigins__0=https://YOUR_VERCEL_APP.vercel.app
```

Redeploy the Render service after changing the variable.

## Current Hosting Status

Live URLs should be added here after the external account deploys complete:

- Frontend: pending
- API health check: pending

## Safe Fallback

If free hosting is unavailable or requires a payment method you do not want to provide, use:

```bash
docker compose up --build
```

Then open:

- Frontend: http://localhost:8080
- API Swagger: http://localhost:5000/swagger
