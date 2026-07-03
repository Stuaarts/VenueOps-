# VenueOps Frontend

React + TypeScript + Vite frontend for VenueOps.

## Commands

```bash
npm ci
npm run dev
npm run lint
npm test
npm run build
```

## Environment

```bash
VITE_API_BASE_URL=http://localhost:5000/api
VITE_USE_MOCKS=false
```

Use `VITE_USE_MOCKS=true` for UI-only screenshots or frontend tests when the ASP.NET Core API is not running.

## Main UI Areas

- Login screen with seeded demo account shortcuts
- Role-aware app shell and navigation
- Dashboard metrics
- Booking table with filters and status updates
- Booking creation form
- Staff assignment workflow
- Shift notes workflow
- Client, venue, staff, and admin screens
