import { useCallback, useEffect, useMemo, useState, type FormEvent } from 'react'
import {
  AlertTriangle,
  Building2,
  CalendarDays,
  CheckCircle2,
  ClipboardCheck,
  ClipboardList,
  Clock3,
  LayoutDashboard,
  LogOut,
  Plus,
  RefreshCw,
  Search,
  ShieldCheck,
  StickyNote,
  Users,
} from 'lucide-react'
import './App.css'
import { ApiError, createVenueOpsClient } from './api'
import {
  assignmentStatuses,
  bookingStatuses,
  shiftNoteTypes,
  staffRoles,
  type AssignmentStatus,
  type AuthUser,
  type BookingFilters,
  type BookingStatus,
  type BookingSummaryDto,
  type ClientDto,
  type CreateShiftNoteRequest,
  type CreateUserRequest,
  type DashboardDto,
  type ShiftNoteDto,
  type StaffAssignmentDto,
  type StaffRole,
  type UpsertBookingRequest,
  type UpsertClientRequest,
  type UpsertStaffAssignmentRequest,
  type UpsertVenueRoomRequest,
  type UserDto,
  type UserRole,
  type VenueOpsClient,
  type VenueRoomDto,
} from './types'

type ViewKey = 'dashboard' | 'bookings' | 'clients' | 'venues' | 'staff' | 'notes' | 'admin'

interface Session {
  token: string
  user: AuthUser
}

const navItems: Array<{ key: ViewKey; label: string; icon: typeof LayoutDashboard; roles: UserRole[] }> = [
  { key: 'dashboard', label: 'Dashboard', icon: LayoutDashboard, roles: ['Admin', 'Manager', 'Staff', 'Demo'] },
  { key: 'bookings', label: 'Bookings', icon: CalendarDays, roles: ['Admin', 'Manager', 'Staff', 'Demo'] },
  { key: 'clients', label: 'Clients', icon: ClipboardList, roles: ['Admin', 'Manager', 'Demo'] },
  { key: 'venues', label: 'Venues', icon: Building2, roles: ['Admin', 'Manager', 'Demo'] },
  { key: 'staff', label: 'Staff', icon: Users, roles: ['Admin', 'Manager', 'Demo'] },
  { key: 'notes', label: 'Shift Notes', icon: StickyNote, roles: ['Admin', 'Manager', 'Staff', 'Demo'] },
  { key: 'admin', label: 'Admin', icon: ShieldCheck, roles: ['Admin'] },
]

const demoAccounts = [
  { label: 'Admin', email: 'admin@venueops.local' },
  { label: 'Manager', email: 'manager@venueops.local' },
  { label: 'Staff', email: 'sam.staff@venueops.local' },
  { label: 'Demo', email: 'demo@venueops.local' },
]

const defaultPassword = 'VenueOpsDemo!2026'

function App() {
  const api = useMemo<VenueOpsClient>(() => createVenueOpsClient(), [])
  const [session, setSession] = useState<Session | null>(null)
  const [activeView, setActiveView] = useState<ViewKey>('dashboard')
  const [dashboard, setDashboard] = useState<DashboardDto | null>(null)
  const [bookings, setBookings] = useState<BookingSummaryDto[]>([])
  const [clients, setClients] = useState<ClientDto[]>([])
  const [venues, setVenues] = useState<VenueRoomDto[]>([])
  const [staff, setStaff] = useState<UserDto[]>([])
  const [users, setUsers] = useState<UserDto[]>([])
  const [assignments, setAssignments] = useState<StaffAssignmentDto[]>([])
  const [notes, setNotes] = useState<ShiftNoteDto[]>([])
  const [filters, setFilters] = useState<BookingFilters>({})
  const [selectedBookingId, setSelectedBookingId] = useState<string>('')
  const [loading, setLoading] = useState(false)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const role = session?.user.role
  const canWrite = role === 'Admin' || role === 'Manager'
  const canManageVenues = role === 'Admin'
  const canManageUsers = role === 'Admin'
  const canAddNotes = role !== 'Demo'
  const visibleNav = navItems.filter((item) => (role ? item.roles.includes(role) : false))

  const loadData = useCallback(async () => {
    if (!session) {
      return
    }

    setLoading(true)
    setError(null)
    try {
      const [dashboardData, bookingData, clientData, venueData, assignmentData, noteData] = await Promise.all([
        api.getDashboard(),
        api.getBookings(filters),
        api.getClients(),
        api.getVenues(),
        api.getAssignments(),
        api.getShiftNotes(),
      ])

      setDashboard(dashboardData)
      setBookings(bookingData)
      setClients(clientData)
      setVenues(venueData)
      setAssignments(assignmentData)
      setNotes(noteData)

      if (session.user.role === 'Staff') {
        setStaff([{ ...session.user, isActive: true }])
        setUsers([])
      } else {
        const staffData = await api.getStaff()
        setStaff(staffData)
        setUsers(session.user.role === 'Admin' ? await api.getUsers() : [])
      }
    } catch (err) {
      setError(errorMessage(err))
    } finally {
      setLoading(false)
    }
  }, [api, filters, session])

  useEffect(() => {
    if (!session) {
      return
    }

    api.setToken(session.token)
    void loadData()
  }, [api, loadData, session])

  useEffect(() => {
    if (!bookings.length) {
      setSelectedBookingId('')
      return
    }

    if (!selectedBookingId || !bookings.some((booking) => booking.id === selectedBookingId)) {
      setSelectedBookingId(bookings[0].id)
    }
  }, [bookings, selectedBookingId])

  async function login(email: string, password: string) {
    setError(null)
    setLoading(true)
    try {
      const response = await api.login(email, password)
      api.setToken(response.token)
      setSession({ token: response.token, user: response.user })
      setActiveView('dashboard')
    } catch (err) {
      setError(errorMessage(err))
    } finally {
      setLoading(false)
    }
  }

  async function runMutation(action: () => Promise<unknown>) {
    setSaving(true)
    setError(null)
    try {
      await action()
      await loadData()
    } catch (err) {
      setError(errorMessage(err))
    } finally {
      setSaving(false)
    }
  }

  function logout() {
    api.setToken(null)
    setSession(null)
    setDashboard(null)
    setBookings([])
    setAssignments([])
    setNotes([])
    setError(null)
  }

  if (!session) {
    return <LoginScreen error={error} loading={loading} onLogin={login} />
  }

  const selectedBooking = bookings.find((booking) => booking.id === selectedBookingId)

  return (
    <div className="app-shell">
      <aside className="sidebar" aria-label="Primary navigation">
        <div className="brand-block">
          <div className="brand-mark">VO</div>
          <div>
            <h1>VenueOps</h1>
            <p>Event operations</p>
          </div>
        </div>

        <nav className="nav-list">
          {visibleNav.map((item) => {
            const Icon = item.icon
            return (
              <button
                className={activeView === item.key ? 'nav-item active' : 'nav-item'}
                key={item.key}
                onClick={() => setActiveView(item.key)}
                type="button"
              >
                <Icon size={18} aria-hidden />
                <span>{item.label}</span>
              </button>
            )
          })}
        </nav>

        <div className="account-panel">
          <div className="avatar">{initials(session.user.fullName)}</div>
          <div>
            <strong>{session.user.fullName}</strong>
            <span>{session.user.role}</span>
          </div>
        </div>
      </aside>

      <main className="workspace">
        <header className="topbar">
          <div>
            <p className="screen-label">{session.user.role} workspace</p>
            <h2>{titleFor(activeView)}</h2>
          </div>
          <div className="topbar-actions">
            <button className="icon-button" type="button" onClick={() => void loadData()} title="Refresh data">
              <RefreshCw size={18} aria-hidden />
            </button>
            <button className="text-button" type="button" onClick={logout}>
              <LogOut size={16} aria-hidden />
              Sign out
            </button>
          </div>
        </header>

        {error && (
          <div className="alert" role="alert">
            <AlertTriangle size={18} aria-hidden />
            {error}
          </div>
        )}

        {loading && <div className="loading-bar" aria-label="Loading data" />}

        {activeView === 'dashboard' && dashboard && (
          <DashboardView dashboard={dashboard} user={session.user} onSelectBooking={(idValue) => {
            setSelectedBookingId(idValue)
            setActiveView('bookings')
          }} />
        )}

        {activeView === 'bookings' && (
          <BookingsView
            bookings={bookings}
            clients={clients}
            venues={venues}
            staff={staff}
            assignments={assignments}
            selectedBooking={selectedBooking}
            selectedBookingId={selectedBookingId}
            filters={filters}
            canWrite={canWrite}
            saving={saving}
            onSelectBooking={setSelectedBookingId}
            onFiltersChange={setFilters}
            onCreateBooking={(request) => runMutation(() => api.createBooking(request))}
            onUpdateStatus={(idValue, status) => runMutation(() => api.updateBookingStatus(idValue, status))}
            onAssignStaff={(request) => runMutation(() => api.createAssignment(request))}
            onUpdateAssignmentStatus={(idValue, status) => runMutation(() => api.updateAssignmentStatus(idValue, status))}
          />
        )}

        {activeView === 'clients' && (
          <ClientsView
            clients={clients}
            canWrite={canWrite}
            saving={saving}
            onCreateClient={(request) => runMutation(() => api.createClient(request))}
          />
        )}

        {activeView === 'venues' && (
          <VenuesView
            venues={venues}
            canManage={canManageVenues}
            saving={saving}
            onCreateVenue={(request) => runMutation(() => api.createVenue(request))}
          />
        )}

        {activeView === 'staff' && (
          <StaffView staff={staff} assignments={assignments} bookings={bookings} />
        )}

        {activeView === 'notes' && (
          <NotesView
            notes={notes}
            bookings={bookings}
            staff={staff}
            currentUser={session.user}
            canAdd={canAddNotes}
            saving={saving}
            onCreateNote={(request) => runMutation(() => api.createShiftNote(request))}
          />
        )}

        {activeView === 'admin' && (
          <AdminView
            users={users}
            canManage={canManageUsers}
            saving={saving}
            onCreateUser={(request) => runMutation(() => api.createUser(request))}
          />
        )}
      </main>
    </div>
  )
}

function LoginScreen({
  error,
  loading,
  onLogin,
}: {
  error: string | null
  loading: boolean
  onLogin: (email: string, password: string) => Promise<void>
}) {
  const [email, setEmail] = useState('manager@venueops.local')
  const [password, setPassword] = useState(defaultPassword)

  function submit(event: FormEvent) {
    event.preventDefault()
    void onLogin(email, password)
  }

  return (
    <main className="login-screen">
      <section className="login-panel">
        <div className="brand-block login-brand">
          <div className="brand-mark">VO</div>
          <div>
            <h1>VenueOps</h1>
            <p>Event booking and staffing operations</p>
          </div>
        </div>

        <form className="form-grid" onSubmit={submit}>
          <label>
            Email
            <input value={email} onChange={(event) => setEmail(event.target.value)} type="email" />
          </label>
          <label>
            Password
            <input value={password} onChange={(event) => setPassword(event.target.value)} type="password" />
          </label>
          <button className="primary-button" disabled={loading} type="submit">
            {loading ? 'Signing in...' : 'Sign in'}
          </button>
        </form>

        {error && (
          <div className="alert" role="alert">
            <AlertTriangle size={18} aria-hidden />
            {error}
          </div>
        )}

        <div className="demo-grid" aria-label="Demo accounts">
          {demoAccounts.map((account) => (
            <button
              key={account.email}
              type="button"
              onClick={() => {
                setEmail(account.email)
                setPassword(defaultPassword)
                void onLogin(account.email, defaultPassword)
              }}
            >
              <span>{account.label}</span>
              <small>{account.email}</small>
            </button>
          ))}
        </div>
      </section>

      <section className="login-preview">
        <div className="preview-toolbar">
          <span>Today</span>
          <strong>Operations Dashboard</strong>
        </div>
        <div className="preview-grid">
          <MetricPreview label="Upcoming" value="7" />
          <MetricPreview label="Guests" value="1,286" />
          <MetricPreview label="Staff" value="15" />
        </div>
        <div className="preview-list">
          <PreviewRow status="Confirmed" title="Northstar Leadership Summit" meta="Summit Conference Hall" />
          <PreviewRow status="InProgress" title="Terrace Cocktail Preview" meta="Riverside Terrace" />
          <PreviewRow status="Inquiry" title="Civic Arts Donor Gala" meta="Grand Ballroom" />
        </div>
      </section>
    </main>
  )
}

function DashboardView({
  dashboard,
  user,
  onSelectBooking,
}: {
  dashboard: DashboardDto
  user: AuthUser
  onSelectBooking: (id: string) => void
}) {
  const metrics = [
    { label: 'Upcoming events', value: dashboard.metrics.upcomingEvents, icon: CalendarDays, tone: 'blue' },
    { label: 'Bookings this month', value: dashboard.metrics.totalBookingsThisMonth, icon: ClipboardList, tone: 'teal' },
    { label: 'Guests this month', value: dashboard.metrics.totalGuestCountThisMonth, icon: Users, tone: 'green' },
    { label: 'Staff this week', value: dashboard.metrics.staffAssignedThisWeek, icon: ClipboardCheck, tone: 'amber' },
    { label: 'Needs staff', value: dashboard.metrics.eventsNeedingStaff, icon: AlertTriangle, tone: 'red' },
    { label: 'Cancelled', value: dashboard.metrics.cancelledEvents, icon: Clock3, tone: 'slate' },
  ]

  return (
    <div className="view-stack">
      <section className="metric-grid">
        {metrics.map((metric) => {
          const Icon = metric.icon
          return (
            <article className={`metric-card tone-${metric.tone}`} key={metric.label}>
              <Icon size={20} aria-hidden />
              <div>
                <span>{metric.label}</span>
                <strong>{metric.value.toLocaleString()}</strong>
              </div>
            </article>
          )
        })}
      </section>

      <section className="content-grid two-columns">
        <Panel title={user.role === 'Staff' ? 'Assigned Events' : 'Upcoming Events'}>
          <table>
            <thead>
              <tr>
                <th>Event</th>
                <th>Date</th>
                <th>Venue</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              {dashboard.upcomingEvents.map((booking) => (
                <tr key={booking.id} onClick={() => onSelectBooking(booking.id)}>
                  <td>
                    <strong>{booking.eventName}</strong>
                    <span>{booking.clientName}</span>
                  </td>
                  <td>{formatDate(booking.eventDate)}</td>
                  <td>{booking.venueRoomName}</td>
                  <td>
                    <StatusBadge status={booking.status} />
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </Panel>

        <Panel title="Events by Status">
          <div className="status-list">
            {dashboard.eventsByStatus.map((item) => (
              <div className="status-row" key={item.status}>
                <StatusBadge status={item.status} />
                <div className="status-track">
                  <span style={{ width: `${Math.max(8, item.count * 18)}%` }} />
                </div>
                <strong>{item.count}</strong>
              </div>
            ))}
          </div>
        </Panel>

        <Panel title="Staff Assigned This Week">
          <div className="stack-list">
            {dashboard.staffAssignedThisWeek.map((assignment) => (
              <AssignmentRow key={assignment.id} assignment={assignment} />
            ))}
          </div>
        </Panel>

        <Panel title="Recent Shift Notes">
          <div className="stack-list">
            {dashboard.recentShiftNotes.map((note) => (
              <NoteRow key={note.id} note={note} />
            ))}
          </div>
        </Panel>
      </section>
    </div>
  )
}

function BookingsView(props: {
  bookings: BookingSummaryDto[]
  clients: ClientDto[]
  venues: VenueRoomDto[]
  staff: UserDto[]
  assignments: StaffAssignmentDto[]
  selectedBooking?: BookingSummaryDto
  selectedBookingId: string
  filters: BookingFilters
  canWrite: boolean
  saving: boolean
  onSelectBooking: (id: string) => void
  onFiltersChange: (filters: BookingFilters) => void
  onCreateBooking: (request: UpsertBookingRequest) => Promise<void>
  onUpdateStatus: (id: string, status: BookingStatus) => Promise<void>
  onAssignStaff: (request: UpsertStaffAssignmentRequest) => Promise<void>
  onUpdateAssignmentStatus: (id: string, status: AssignmentStatus) => Promise<void>
}) {
  const [form, setForm] = useState(() => defaultBookingForm())
  const [assignmentForm, setAssignmentForm] = useState(() => defaultAssignmentForm())
  const selectedAssignments = props.assignments.filter((assignment) => assignment.eventBookingId === props.selectedBookingId)

  useEffect(() => {
    setForm((current) => ({
      ...current,
      clientId: current.clientId || props.clients[0]?.id || '',
      venueRoomId: current.venueRoomId || props.venues[0]?.id || '',
    }))
  }, [props.clients, props.venues])

  useEffect(() => {
    setAssignmentForm((current) => ({
      ...current,
      eventBookingId: props.selectedBookingId || current.eventBookingId,
      staffUserId: current.staffUserId || props.staff[0]?.id || '',
    }))
  }, [props.selectedBookingId, props.staff])

  function submitBooking(event: FormEvent) {
    event.preventDefault()
    void props.onCreateBooking(form)
    setForm(defaultBookingForm(props.clients[0]?.id, props.venues[0]?.id))
  }

  function submitAssignment(event: FormEvent) {
    event.preventDefault()
    void props.onAssignStaff(assignmentForm)
  }

  return (
    <div className="content-grid booking-layout">
      <section className="main-column">
        <Panel
          title="Bookings"
          action={
            <div className="table-count">
              {props.bookings.length} {props.bookings.length === 1 ? 'event' : 'events'}
            </div>
          }
        >
          <div className="filters">
            <label className="search-field">
              <Search size={16} aria-hidden />
              <input
                value={props.filters.search ?? ''}
                onChange={(event) => props.onFiltersChange({ ...props.filters, search: event.target.value })}
                placeholder="Search events, clients, venues"
              />
            </label>
            <select
              value={props.filters.status ?? ''}
              onChange={(event) => props.onFiltersChange({ ...props.filters, status: event.target.value as BookingStatus | '' })}
            >
              <option value="">All statuses</option>
              {bookingStatuses.map((status) => (
                <option key={status} value={status}>
                  {formatEnum(status)}
                </option>
              ))}
            </select>
            <select
              value={props.filters.venueRoomId ?? ''}
              onChange={(event) => props.onFiltersChange({ ...props.filters, venueRoomId: event.target.value })}
            >
              <option value="">All venues</option>
              {props.venues.map((venue) => (
                <option key={venue.id} value={venue.id}>
                  {venue.name}
                </option>
              ))}
            </select>
          </div>

          <table>
            <thead>
              <tr>
                <th>Event</th>
                <th>Date</th>
                <th>Guests</th>
                <th>Staff</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              {props.bookings.map((booking) => (
                <tr
                  className={booking.id === props.selectedBookingId ? 'selected-row' : ''}
                  key={booking.id}
                  onClick={() => props.onSelectBooking(booking.id)}
                >
                  <td>
                    <strong>{booking.eventName}</strong>
                    <span>{booking.clientName} · {booking.venueRoomName}</span>
                  </td>
                  <td>
                    {formatDate(booking.eventDate)}
                    <span>{shortTime(booking.startTime)}-{shortTime(booking.endTime)}</span>
                  </td>
                  <td>{booking.guestCount.toLocaleString()}</td>
                  <td>{booking.assignedStaffCount}</td>
                  <td>
                    {props.canWrite ? (
                      <select
                        className="status-select"
                        value={booking.status}
                        onClick={(event) => event.stopPropagation()}
                        onChange={(event) => props.onUpdateStatus(booking.id, event.target.value as BookingStatus)}
                      >
                        {bookingStatuses.map((status) => (
                          <option key={status} value={status}>
                            {formatEnum(status)}
                          </option>
                        ))}
                      </select>
                    ) : (
                      <StatusBadge status={booking.status} />
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </Panel>
      </section>

      <aside className="side-column">
        {props.canWrite && (
          <Panel title="New Booking">
            <form className="form-grid compact" onSubmit={submitBooking}>
              <label>
                Event name
                <input value={form.eventName} onChange={(event) => setForm({ ...form, eventName: event.target.value })} required />
              </label>
              <label>
                Client
                <select value={form.clientId} onChange={(event) => setForm({ ...form, clientId: event.target.value })} required>
                  {props.clients.map((client) => (
                    <option key={client.id} value={client.id}>
                      {client.name}
                    </option>
                  ))}
                </select>
              </label>
              <label>
                Venue
                <select value={form.venueRoomId} onChange={(event) => setForm({ ...form, venueRoomId: event.target.value })} required>
                  {props.venues.map((venue) => (
                    <option key={venue.id} value={venue.id}>
                      {venue.name}
                    </option>
                  ))}
                </select>
              </label>
              <div className="form-pair">
                <label>
                  Date
                  <input value={form.eventDate} onChange={(event) => setForm({ ...form, eventDate: event.target.value })} type="date" />
                </label>
                <label>
                  Guests
                  <input
                    value={form.guestCount}
                    onChange={(event) => setForm({ ...form, guestCount: Number(event.target.value) })}
                    min={1}
                    type="number"
                  />
                </label>
              </div>
              <div className="form-pair">
                <label>
                  Start
                  <input value={form.startTime} onChange={(event) => setForm({ ...form, startTime: event.target.value })} type="time" />
                </label>
                <label>
                  End
                  <input value={form.endTime} onChange={(event) => setForm({ ...form, endTime: event.target.value })} type="time" />
                </label>
              </div>
              <label>
                Event type
                <input value={form.eventType} onChange={(event) => setForm({ ...form, eventType: event.target.value })} />
              </label>
              <button className="primary-button" disabled={props.saving} type="submit">
                <Plus size={16} aria-hidden />
                Add booking
              </button>
            </form>
          </Panel>
        )}

        <Panel title={props.selectedBooking ? 'Staff Assignments' : 'Select a Booking'}>
          {props.selectedBooking ? (
            <>
              <div className="selected-summary">
                <strong>{props.selectedBooking.eventName}</strong>
                <span>{formatDate(props.selectedBooking.eventDate)} · {props.selectedBooking.venueRoomName}</span>
              </div>
              <div className="stack-list">
                {selectedAssignments.map((assignment) => (
                  <AssignmentRow
                    assignment={assignment}
                    key={assignment.id}
                    onStatusChange={props.canWrite ? (status) => props.onUpdateAssignmentStatus(assignment.id, status) : undefined}
                  />
                ))}
                {selectedAssignments.length === 0 && <EmptyState text="No staff has been assigned yet." />}
              </div>
              {props.canWrite && (
                <form className="form-grid compact divider-top" onSubmit={submitAssignment}>
                  <label>
                    Staff member
                    <select
                      value={assignmentForm.staffUserId}
                      onChange={(event) => setAssignmentForm({ ...assignmentForm, staffUserId: event.target.value })}
                    >
                      {props.staff.map((staffUser) => (
                        <option key={staffUser.id} value={staffUser.id}>
                          {staffUser.fullName}
                        </option>
                      ))}
                    </select>
                  </label>
                  <label>
                    Role
                    <select
                      value={assignmentForm.role}
                      onChange={(event) => setAssignmentForm({ ...assignmentForm, role: event.target.value as StaffRole })}
                    >
                      {staffRoles.map((staffRole) => (
                        <option key={staffRole} value={staffRole}>
                          {formatEnum(staffRole)}
                        </option>
                      ))}
                    </select>
                  </label>
                  <div className="form-pair">
                    <label>
                      Start
                      <input
                        value={dateTimeLocalValue(assignmentForm.shiftStart)}
                        onChange={(event) => setAssignmentForm({ ...assignmentForm, shiftStart: toUtcISOString(event.target.value) })}
                        type="datetime-local"
                      />
                    </label>
                    <label>
                      End
                      <input
                        value={dateTimeLocalValue(assignmentForm.shiftEnd)}
                        onChange={(event) => setAssignmentForm({ ...assignmentForm, shiftEnd: toUtcISOString(event.target.value) })}
                        type="datetime-local"
                      />
                    </label>
                  </div>
                  <button className="secondary-button" disabled={props.saving} type="submit">
                    Assign staff
                  </button>
                </form>
              )}
            </>
          ) : (
            <EmptyState text="Choose an event from the table to see staffing." />
          )}
        </Panel>
      </aside>
    </div>
  )
}

function ClientsView(props: {
  clients: ClientDto[]
  canWrite: boolean
  saving: boolean
  onCreateClient: (request: UpsertClientRequest) => Promise<void>
}) {
  const [form, setForm] = useState<UpsertClientRequest>({
    name: '',
    contactName: '',
    email: '',
    phone: '',
    notes: '',
  })

  function submit(event: FormEvent) {
    event.preventDefault()
    void props.onCreateClient(form)
    setForm({ name: '', contactName: '', email: '', phone: '', notes: '' })
  }

  return (
    <div className="content-grid two-columns">
      <Panel title="Clients">
        <table>
          <thead>
            <tr>
              <th>Client</th>
              <th>Contact</th>
              <th>Bookings</th>
            </tr>
          </thead>
          <tbody>
            {props.clients.map((client) => (
              <tr key={client.id}>
                <td>
                  <strong>{client.name}</strong>
                  <span>{client.email}</span>
                </td>
                <td>{client.contactName}</td>
                <td>{client.bookingCount}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </Panel>

      {props.canWrite && (
        <Panel title="Add Client">
          <form className="form-grid" onSubmit={submit}>
            <label>
              Organization
              <input value={form.name} onChange={(event) => setForm({ ...form, name: event.target.value })} required />
            </label>
            <label>
              Contact name
              <input value={form.contactName} onChange={(event) => setForm({ ...form, contactName: event.target.value })} required />
            </label>
            <label>
              Email
              <input value={form.email} onChange={(event) => setForm({ ...form, email: event.target.value })} type="email" required />
            </label>
            <label>
              Phone
              <input value={form.phone ?? ''} onChange={(event) => setForm({ ...form, phone: event.target.value })} />
            </label>
            <button className="primary-button" disabled={props.saving} type="submit">
              Add client
            </button>
          </form>
        </Panel>
      )}
    </div>
  )
}

function VenuesView(props: {
  venues: VenueRoomDto[]
  canManage: boolean
  saving: boolean
  onCreateVenue: (request: UpsertVenueRoomRequest) => Promise<void>
}) {
  const [form, setForm] = useState<UpsertVenueRoomRequest>({
    name: '',
    location: '',
    capacity: 120,
    isActive: true,
    notes: '',
  })

  function submit(event: FormEvent) {
    event.preventDefault()
    void props.onCreateVenue(form)
    setForm({ name: '', location: '', capacity: 120, isActive: true, notes: '' })
  }

  return (
    <div className="content-grid two-columns">
      <Panel title="Venues and Rooms">
        <div className="venue-grid">
          {props.venues.map((venue) => (
            <article className="venue-card" key={venue.id}>
              <div>
                <strong>{venue.name}</strong>
                <span>{venue.location}</span>
              </div>
              <div className="venue-stats">
                <span>{venue.capacity.toLocaleString()} capacity</span>
                <span>{venue.upcomingBookings} upcoming</span>
              </div>
              <StatusBadge status={venue.isActive ? 'Active' : 'Inactive'} />
            </article>
          ))}
        </div>
      </Panel>

      {props.canManage && (
        <Panel title="Add Venue">
          <form className="form-grid" onSubmit={submit}>
            <label>
              Room name
              <input value={form.name} onChange={(event) => setForm({ ...form, name: event.target.value })} required />
            </label>
            <label>
              Location
              <input value={form.location} onChange={(event) => setForm({ ...form, location: event.target.value })} required />
            </label>
            <label>
              Capacity
              <input
                value={form.capacity}
                onChange={(event) => setForm({ ...form, capacity: Number(event.target.value) })}
                min={1}
                type="number"
              />
            </label>
            <label className="checkbox-line">
              <input
                checked={form.isActive}
                onChange={(event) => setForm({ ...form, isActive: event.target.checked })}
                type="checkbox"
              />
              Active for bookings
            </label>
            <button className="primary-button" disabled={props.saving} type="submit">
              Add venue
            </button>
          </form>
        </Panel>
      )}
    </div>
  )
}

function StaffView({
  staff,
  assignments,
  bookings,
}: {
  staff: UserDto[]
  assignments: StaffAssignmentDto[]
  bookings: BookingSummaryDto[]
}) {
  return (
    <div className="content-grid two-columns">
      <Panel title="Staff Directory">
        <div className="staff-grid">
          {staff.map((staffUser) => (
            <article className="staff-card" key={staffUser.id}>
              <div className="avatar">{initials(staffUser.fullName)}</div>
              <div>
                <strong>{staffUser.fullName}</strong>
                <span>{staffUser.email}</span>
              </div>
              <small>{assignments.filter((assignment) => assignment.staffUserId === staffUser.id).length} shifts</small>
            </article>
          ))}
        </div>
      </Panel>

      <Panel title="Upcoming Staff Assignments">
        <div className="stack-list">
          {assignments
            .filter((assignment) => bookings.some((booking) => booking.id === assignment.eventBookingId))
            .sort((a, b) => a.shiftStart.localeCompare(b.shiftStart))
            .slice(0, 14)
            .map((assignment) => (
              <AssignmentRow assignment={assignment} key={assignment.id} />
            ))}
        </div>
      </Panel>
    </div>
  )
}

function NotesView(props: {
  notes: ShiftNoteDto[]
  bookings: BookingSummaryDto[]
  staff: UserDto[]
  currentUser: AuthUser
  canAdd: boolean
  saving: boolean
  onCreateNote: (request: CreateShiftNoteRequest) => Promise<void>
}) {
  const [form, setForm] = useState<CreateShiftNoteRequest>({
    eventBookingId: '',
    staffUserId: props.currentUser.role === 'Staff' ? props.currentUser.id : props.staff[0]?.id,
    noteType: 'General',
    body: '',
    isPinned: false,
  })

  useEffect(() => {
    setForm((current) => ({
      ...current,
      eventBookingId: current.eventBookingId || props.bookings[0]?.id || '',
      staffUserId: props.currentUser.role === 'Staff' ? props.currentUser.id : current.staffUserId || props.staff[0]?.id,
    }))
  }, [props.bookings, props.currentUser.id, props.currentUser.role, props.staff])

  function submit(event: FormEvent) {
    event.preventDefault()
    void props.onCreateNote(form)
    setForm({ ...form, body: '', isPinned: false })
  }

  return (
    <div className="content-grid two-columns">
      <Panel title="Shift Notes">
        <div className="stack-list">
          {props.notes.map((note) => (
            <NoteRow key={note.id} note={note} />
          ))}
        </div>
      </Panel>

      <Panel title={props.canAdd ? 'Add Shift Note' : 'Read-only Demo'}>
        {props.canAdd ? (
          <form className="form-grid" onSubmit={submit}>
            <label>
              Event
              <select
                value={form.eventBookingId}
                onChange={(event) => setForm({ ...form, eventBookingId: event.target.value })}
                required
              >
                {props.bookings.map((booking) => (
                  <option key={booking.id} value={booking.id}>
                    {booking.eventName}
                  </option>
                ))}
              </select>
            </label>
            {props.currentUser.role !== 'Staff' && (
              <label>
                Note author
                <select value={form.staffUserId ?? ''} onChange={(event) => setForm({ ...form, staffUserId: event.target.value })}>
                  {props.staff.map((staffUser) => (
                    <option key={staffUser.id} value={staffUser.id}>
                      {staffUser.fullName}
                    </option>
                  ))}
                </select>
              </label>
            )}
            <label>
              Note type
              <select value={form.noteType} onChange={(event) => setForm({ ...form, noteType: event.target.value as ShiftNoteDto['noteType'] })}>
                {shiftNoteTypes.map((type) => (
                  <option key={type} value={type}>
                    {formatEnum(type)}
                  </option>
                ))}
              </select>
            </label>
            <label>
              Note
              <textarea value={form.body} onChange={(event) => setForm({ ...form, body: event.target.value })} rows={6} required />
            </label>
            {props.currentUser.role !== 'Staff' && (
              <label className="checkbox-line">
                <input
                  checked={form.isPinned}
                  onChange={(event) => setForm({ ...form, isPinned: event.target.checked })}
                  type="checkbox"
                />
                Pin note for operations review
              </label>
            )}
            <button className="primary-button" disabled={props.saving} type="submit">
              Add note
            </button>
          </form>
        ) : (
          <EmptyState text="Demo users can browse operational data but cannot add notes or change records." />
        )}
      </Panel>
    </div>
  )
}

function AdminView(props: {
  users: UserDto[]
  canManage: boolean
  saving: boolean
  onCreateUser: (request: CreateUserRequest) => Promise<void>
}) {
  const [form, setForm] = useState<CreateUserRequest>({
    fullName: '',
    email: '',
    password: defaultPassword,
    role: 'Staff',
    isActive: true,
  })

  function submit(event: FormEvent) {
    event.preventDefault()
    void props.onCreateUser(form)
    setForm({ fullName: '', email: '', password: defaultPassword, role: 'Staff', isActive: true })
  }

  return (
    <div className="content-grid two-columns">
      <Panel title="Users">
        <table>
          <thead>
            <tr>
              <th>Name</th>
              <th>Role</th>
              <th>Status</th>
            </tr>
          </thead>
          <tbody>
            {props.users.map((user) => (
              <tr key={user.id}>
                <td>
                  <strong>{user.fullName}</strong>
                  <span>{user.email}</span>
                </td>
                <td>{user.role}</td>
                <td>
                  <StatusBadge status={user.isActive ? 'Active' : 'Inactive'} />
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </Panel>

      {props.canManage && (
        <Panel title="Create User">
          <form className="form-grid" onSubmit={submit}>
            <label>
              Full name
              <input value={form.fullName} onChange={(event) => setForm({ ...form, fullName: event.target.value })} required />
            </label>
            <label>
              Email
              <input value={form.email} onChange={(event) => setForm({ ...form, email: event.target.value })} type="email" required />
            </label>
            <label>
              Role
              <select value={form.role} onChange={(event) => setForm({ ...form, role: event.target.value as UserRole })}>
                {(['Admin', 'Manager', 'Staff', 'Demo'] as UserRole[]).map((item) => (
                  <option key={item} value={item}>
                    {item}
                  </option>
                ))}
              </select>
            </label>
            <button className="primary-button" disabled={props.saving} type="submit">
              Create user
            </button>
          </form>
        </Panel>
      )}
    </div>
  )
}

function Panel({
  title,
  children,
  action,
}: {
  title: string
  children: React.ReactNode
  action?: React.ReactNode
}) {
  return (
    <section className="panel">
      <div className="panel-header">
        <h3>{title}</h3>
        {action}
      </div>
      {children}
    </section>
  )
}

function StatusBadge({ status }: { status: string }) {
  return <span className={`status-badge status-${status.toLowerCase()}`}>{formatEnum(status)}</span>
}

function AssignmentRow({
  assignment,
  onStatusChange,
}: {
  assignment: StaffAssignmentDto
  onStatusChange?: (status: AssignmentStatus) => void
}) {
  return (
    <article className="list-row">
      <div className="row-icon">
        <Clock3 size={16} aria-hidden />
      </div>
      <div>
        <strong>{assignment.staffName}</strong>
        <span>{assignment.eventName} · {formatEnum(assignment.role)} · {formatDateTime(assignment.shiftStart)}</span>
      </div>
      {onStatusChange ? (
        <select value={assignment.status} onChange={(event) => onStatusChange(event.target.value as AssignmentStatus)}>
          {assignmentStatuses.map((status) => (
            <option key={status} value={status}>
              {formatEnum(status)}
            </option>
          ))}
        </select>
      ) : (
        <StatusBadge status={assignment.status} />
      )}
    </article>
  )
}

function NoteRow({ note }: { note: ShiftNoteDto }) {
  return (
    <article className="list-row note-row">
      <div className="row-icon">
        {note.isPinned ? <CheckCircle2 size={16} aria-hidden /> : <StickyNote size={16} aria-hidden />}
      </div>
      <div>
        <strong>{formatEnum(note.noteType)}</strong>
        <p>{note.body}</p>
        <span>{note.eventName} · {note.staffName} · {formatDateTime(note.createdAt)}</span>
      </div>
    </article>
  )
}

function EmptyState({ text }: { text: string }) {
  return <div className="empty-state">{text}</div>
}

function MetricPreview({ label, value }: { label: string; value: string }) {
  return (
    <div className="preview-metric">
      <span>{label}</span>
      <strong>{value}</strong>
    </div>
  )
}

function PreviewRow({ title, meta, status }: { title: string; meta: string; status: BookingStatus }) {
  return (
    <div className="preview-row">
      <div>
        <strong>{title}</strong>
        <span>{meta}</span>
      </div>
      <StatusBadge status={status} />
    </div>
  )
}

function defaultBookingForm(clientId = '', venueRoomId = ''): UpsertBookingRequest {
  return {
    eventName: '',
    clientId,
    venueRoomId,
    eventDate: new Date(Date.now() + 14 * 24 * 60 * 60 * 1000).toISOString().slice(0, 10),
    startTime: '18:00',
    endTime: '22:00',
    guestCount: 120,
    eventType: 'Corporate',
    status: 'Inquiry',
    internalNotes: '',
  }
}

function defaultAssignmentForm(): UpsertStaffAssignmentRequest {
  const date = new Date(Date.now() + 14 * 24 * 60 * 60 * 1000).toISOString().slice(0, 10)
  return {
    eventBookingId: '',
    staffUserId: '',
    role: 'Server',
    shiftStart: `${date}T17:00:00.000Z`,
    shiftEnd: `${date}T22:00:00.000Z`,
    status: 'Assigned',
    notes: '',
  }
}

function titleFor(view: ViewKey) {
  return {
    dashboard: 'Operations Dashboard',
    bookings: 'Booking Management',
    clients: 'Client Records',
    venues: 'Venue Rooms',
    staff: 'Staff Assignments',
    notes: 'Shift Notes',
    admin: 'User Administration',
  }[view]
}

function errorMessage(error: unknown) {
  if (error instanceof ApiError || error instanceof Error) {
    return error.message
  }
  return 'Something went wrong.'
}

function initials(name: string) {
  return name
    .split(' ')
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0])
    .join('')
    .toUpperCase()
}

function formatEnum(value: string) {
  return value.replace(/([a-z])([A-Z])/g, '$1 $2')
}

function formatDate(value: string) {
  return new Intl.DateTimeFormat(undefined, { month: 'short', day: 'numeric', year: 'numeric' }).format(new Date(`${value}T00:00:00`))
}

function formatDateTime(value: string) {
  return new Intl.DateTimeFormat(undefined, { month: 'short', day: 'numeric', hour: 'numeric', minute: '2-digit' }).format(new Date(value))
}

function shortTime(value: string) {
  return value.slice(0, 5)
}

function dateTimeLocalValue(value: string) {
  const date = new Date(value)
  const offset = date.getTimezoneOffset()
  const local = new Date(date.getTime() - offset * 60 * 1000)
  return local.toISOString().slice(0, 16)
}

function toUtcISOString(value: string) {
  return new Date(value).toISOString()
}

export default App
