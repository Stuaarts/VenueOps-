# GitHub Issue Board Seed

Create these issues after the repository is pushed to GitHub. Suggested labels: `enhancement`, `documentation`, `testing`, `deployment`, `good first issue`.

## Planned Features

1. **feat: add calendar view for event bookings**
   - Show bookings in month/week/day layouts.
   - Filter by venue and status.
   - Link calendar entries to booking details.

2. **feat: add CSV export for bookings and staff assignments**
   - Export filtered booking lists.
   - Export weekly staff assignments.
   - Include role, shift status, and guest count fields.

3. **feat: add audit log for status changes**
   - Track booking status changes.
   - Track assignment status changes.
   - Show actor, timestamp, previous value, and new value.

## Quality Improvements

4. **test: add browser end-to-end tests against Docker Compose**
   - Start the full stack.
   - Sign in as manager.
   - Create a booking, assign staff, and add a shift note.

5. **docs: add hosted demo walkthrough GIF**
   - Record manager workflow.
   - Add GIF to README.
   - Keep file size reasonable for GitHub.

6. **deployment: publish frontend and backend to free hosting**
   - Deploy frontend to Vercel Hobby if available.
   - Deploy backend and Postgres to Render free tier if available.
   - Document final environment variables and URLs.
