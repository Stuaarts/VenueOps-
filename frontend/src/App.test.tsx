import { render, screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import App from './App'

describe('VenueOps app', () => {
  it('logs in as a manager and shows dashboard metrics', async () => {
    render(<App />)

    await userEvent.click(screen.getByRole('button', { name: /manager/i }))

    expect(await screen.findByText('Operations Dashboard')).toBeInTheDocument()
    expect(screen.getByText('Upcoming events')).toBeInTheDocument()
    expect(screen.getByText('Staff Assigned This Week')).toBeInTheDocument()
  })

  it('filters booking rows by search text', async () => {
    render(<App />)

    await userEvent.click(screen.getByRole('button', { name: /manager/i }))
    await screen.findByText('Operations Dashboard')
    await userEvent.click(screen.getByRole('button', { name: 'Bookings' }))

    const search = screen.getByPlaceholderText('Search events, clients, venues')
    await userEvent.clear(search)
    await userEvent.type(search, 'wedding')

    await waitFor(() => {
      const table = screen.getByRole('table')
      expect(within(table).getByText('Harlow Reed Wedding Reception')).toBeInTheDocument()
      expect(within(table).queryByText('Northstar Leadership Summit')).not.toBeInTheDocument()
    })
  })

  it('keeps demo users in read-only booking mode', async () => {
    render(<App />)

    await userEvent.click(screen.getByRole('button', { name: /demo/i }))
    await screen.findByText('Operations Dashboard')
    await userEvent.click(screen.getByRole('button', { name: 'Bookings' }))

    const workspace = screen.getByRole('main')
    expect(within(workspace).queryByText('New Booking')).not.toBeInTheDocument()
    expect(within(workspace).getByText('Bookings')).toBeInTheDocument()
  })
})
