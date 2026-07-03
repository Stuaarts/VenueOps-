import {
  createDashboard,
  createSeedData,
  demoPassword,
  scopeAssignments,
  scopeBookings,
  scopeNotes,
} from './mockData'
import type {
  AssignmentStatus,
  AuthUser,
  BookingDetailDto,
  BookingFilters,
  BookingStatus,
  ClientDto,
  CreateShiftNoteRequest,
  CreateUserRequest,
  DashboardDto,
  LoginResponse,
  ShiftNoteDto,
  StaffAssignmentDto,
  UpsertBookingRequest,
  UpsertClientRequest,
  UpsertStaffAssignmentRequest,
  UpsertVenueRoomRequest,
  UserDto,
  VenueOpsClient,
  VenueRoomDto,
} from './types'

export class ApiError extends Error {
  status: number

  constructor(message: string, status: number) {
    super(message)
    this.status = status
  }
}

class HttpVenueOpsClient implements VenueOpsClient {
  private token: string | null = null
  private readonly baseUrl: string

  constructor(baseUrl: string) {
    this.baseUrl = baseUrl.replace(/\/$/, '')
  }

  setToken(token: string | null) {
    this.token = token
  }

  login(email: string, password: string) {
    return this.request<LoginResponse>('/auth/login', {
      method: 'POST',
      body: JSON.stringify({ email, password }),
      skipAuth: true,
    })
  }

  getDashboard() {
    return this.request<DashboardDto>('/dashboard')
  }

  getBookings(filters: BookingFilters = {}) {
    return this.request<BookingDetailDto[]>(`/bookings${queryString({
      search: filters.search,
      status: filters.status,
      venueRoomId: filters.venueRoomId,
      assignedStaffUserId: filters.assignedStaffUserId,
      from: filters.from,
      to: filters.to,
    })}`)
  }

  getBooking(id: string) {
    return this.request<BookingDetailDto>(`/bookings/${id}`)
  }

  createBooking(request: UpsertBookingRequest) {
    return this.request<BookingDetailDto>('/bookings', {
      method: 'POST',
      body: JSON.stringify(normalizeBookingRequest(request)),
    })
  }

  updateBookingStatus(id: string, status: BookingStatus) {
    return this.request<BookingDetailDto>(`/bookings/${id}/status`, {
      method: 'PATCH',
      body: JSON.stringify({ status }),
    })
  }

  getClients() {
    return this.request<ClientDto[]>('/clients')
  }

  createClient(request: UpsertClientRequest) {
    return this.request<ClientDto>('/clients', { method: 'POST', body: JSON.stringify(request) })
  }

  getVenues() {
    return this.request<VenueRoomDto[]>('/venues')
  }

  createVenue(request: UpsertVenueRoomRequest) {
    return this.request<VenueRoomDto>('/venues', { method: 'POST', body: JSON.stringify(request) })
  }

  getStaff() {
    return this.request<UserDto[]>('/users/staff')
  }

  getUsers() {
    return this.request<UserDto[]>('/users')
  }

  createUser(request: CreateUserRequest) {
    return this.request<UserDto>('/users', { method: 'POST', body: JSON.stringify(request) })
  }

  getAssignments(eventBookingId?: string) {
    return this.request<StaffAssignmentDto[]>(`/staff-assignments${queryString({ eventBookingId })}`)
  }

  createAssignment(request: UpsertStaffAssignmentRequest) {
    return this.request<StaffAssignmentDto>('/staff-assignments', {
      method: 'POST',
      body: JSON.stringify(request),
    })
  }

  updateAssignmentStatus(id: string, status: AssignmentStatus) {
    return this.request<StaffAssignmentDto>(`/staff-assignments/${id}/status`, {
      method: 'PATCH',
      body: JSON.stringify({ status }),
    })
  }

  getShiftNotes(eventBookingId?: string) {
    return this.request<ShiftNoteDto[]>(`/shift-notes${queryString({ eventBookingId })}`)
  }

  createShiftNote(request: CreateShiftNoteRequest) {
    return this.request<ShiftNoteDto>('/shift-notes', {
      method: 'POST',
      body: JSON.stringify(request),
    })
  }

  private async request<T>(path: string, options: RequestInit & { skipAuth?: boolean } = {}): Promise<T> {
    const headers = new Headers(options.headers)
    headers.set('Content-Type', 'application/json')
    if (!options.skipAuth && this.token) {
      headers.set('Authorization', `Bearer ${this.token}`)
    }

    const response = await fetch(`${this.baseUrl}${path}`, {
      ...options,
      headers,
    })

    if (!response.ok) {
      let message = `Request failed with status ${response.status}`
      try {
        const body = (await response.json()) as { message?: string; title?: string; detail?: string }
        message = body.message ?? body.detail ?? body.title ?? message
      } catch {
        // Keep the fallback status message.
      }
      throw new ApiError(message, response.status)
    }

    if (response.status === 204) {
      return undefined as T
    }

    return response.json() as Promise<T>
  }
}

class MockVenueOpsClient implements VenueOpsClient {
  private data = createSeedData()
  private currentUser: AuthUser | null = null

  setToken(token: string | null) {
    if (!token) {
      this.currentUser = null
    }
  }

  async login(email: string, password: string): Promise<LoginResponse> {
    const user = this.data.users.find((item) => item.email.toLowerCase() === email.toLowerCase())
    if (!user || password !== demoPassword) {
      throw new ApiError('Invalid email or password.', 401)
    }

    this.currentUser = user
    return {
      token: `mock-token-${user.id}`,
      expiresAt: new Date(Date.now() + 8 * 60 * 60 * 1000).toISOString(),
      user,
    }
  }

  async getDashboard() {
    return createDashboard(this.data, this.currentUser ?? undefined)
  }

  async getBookings(filters: BookingFilters = {}) {
    return scopeBookings(this.data.bookings, this.data.assignments, this.currentUser ?? undefined)
      .filter((booking) => {
        const text = `${booking.eventName} ${booking.clientName} ${booking.venueRoomName} ${booking.eventType}`.toLowerCase()
        return !filters.search || text.includes(filters.search.toLowerCase())
      })
      .filter((booking) => !filters.status || booking.status === filters.status)
      .filter((booking) => !filters.venueRoomId || booking.venueRoomId === filters.venueRoomId)
      .filter((booking) =>
        !filters.assignedStaffUserId ||
        this.data.assignments.some(
          (assignment) =>
            assignment.eventBookingId === booking.id && assignment.staffUserId === filters.assignedStaffUserId,
        ),
      )
      .filter((booking) => !filters.from || booking.eventDate >= filters.from)
      .filter((booking) => !filters.to || booking.eventDate <= filters.to)
      .sort((a, b) => `${a.eventDate}${a.startTime}`.localeCompare(`${b.eventDate}${b.startTime}`))
  }

  async getBooking(id: string) {
    return this.requireBooking(id)
  }

  async createBooking(request: UpsertBookingRequest) {
    this.requireWrite()
    const client = this.data.clients.find((item) => item.id === request.clientId)
    const venue = this.data.venues.find((item) => item.id === request.venueRoomId)
    if (!client || !venue) {
      throw new ApiError('Client and venue are required.', 400)
    }

    const booking: BookingDetailDto = {
      ...request,
      id: `booking-${crypto.randomUUID()}`,
      clientName: client.name,
      venueRoomName: venue.name,
      assignedStaffCount: 0,
      staffAssignments: [],
      shiftNotes: [],
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
    }

    this.data.bookings.push(booking)
    client.bookingCount += 1
    return booking
  }

  async updateBookingStatus(id: string, status: BookingStatus) {
    this.requireWrite()
    const booking = this.requireBooking(id)
    booking.status = status
    booking.updatedAt = new Date().toISOString()
    return booking
  }

  async getClients() {
    return [...this.data.clients].sort((a, b) => a.name.localeCompare(b.name))
  }

  async createClient(request: UpsertClientRequest) {
    this.requireWrite()
    const client: ClientDto = { ...request, id: `client-${crypto.randomUUID()}`, bookingCount: 0 }
    this.data.clients.push(client)
    return client
  }

  async getVenues() {
    return [...this.data.venues].sort((a, b) => a.name.localeCompare(b.name))
  }

  async createVenue(request: UpsertVenueRoomRequest) {
    this.requireAdmin()
    const venue: VenueRoomDto = { ...request, id: `venue-${crypto.randomUUID()}`, upcomingBookings: 0 }
    this.data.venues.push(venue)
    return venue
  }

  async getStaff() {
    return this.data.users.filter((user) => user.role === 'Staff' && user.isActive)
  }

  async getUsers() {
    this.requireAdmin()
    return this.data.users
  }

  async createUser(request: CreateUserRequest) {
    this.requireAdmin()
    const user: UserDto = {
      id: `user-${crypto.randomUUID()}`,
      fullName: request.fullName,
      email: request.email,
      role: request.role,
      isActive: request.isActive,
    }
    this.data.users.push(user)
    return user
  }

  async getAssignments(eventBookingId?: string) {
    return scopeAssignments(this.data.assignments, this.currentUser ?? undefined).filter(
      (assignment) => !eventBookingId || assignment.eventBookingId === eventBookingId,
    )
  }

  async createAssignment(request: UpsertStaffAssignmentRequest) {
    this.requireWrite()
    const booking = this.requireBooking(request.eventBookingId)
    const staff = this.data.users.find((user) => user.id === request.staffUserId)
    if (!staff) {
      throw new ApiError('Staff user not found.', 400)
    }

    const assignment: StaffAssignmentDto = {
      ...request,
      id: `assignment-${crypto.randomUUID()}`,
      eventName: booking.eventName,
      staffName: staff.fullName,
    }
    this.data.assignments.push(assignment)
    booking.staffAssignments.push(assignment)
    booking.assignedStaffCount = booking.staffAssignments.length
    return assignment
  }

  async updateAssignmentStatus(id: string, status: AssignmentStatus) {
    const assignment = this.data.assignments.find((item) => item.id === id)
    if (!assignment) {
      throw new ApiError('Assignment not found.', 404)
    }
    if (this.currentUser?.role === 'Staff' && assignment.staffUserId !== this.currentUser.id) {
      throw new ApiError('You can only update your own assignments.', 403)
    }
    assignment.status = status
    return assignment
  }

  async getShiftNotes(eventBookingId?: string) {
    return scopeNotes(this.data.notes, this.data.assignments, this.currentUser ?? undefined)
      .filter((note) => !eventBookingId || note.eventBookingId === eventBookingId)
      .sort((a, b) => Number(b.isPinned) - Number(a.isPinned) || b.createdAt.localeCompare(a.createdAt))
  }

  async createShiftNote(request: CreateShiftNoteRequest) {
    if (this.currentUser?.role === 'Demo') {
      throw new ApiError('Demo user is read-only.', 403)
    }

    const booking = this.requireBooking(request.eventBookingId)
    const authorId = this.currentUser?.role === 'Staff' ? this.currentUser.id : request.staffUserId || this.currentUser?.id
    const author = this.data.users.find((user) => user.id === authorId)
    if (!author) {
      throw new ApiError('Note author not found.', 400)
    }

    if (
      this.currentUser?.role === 'Staff' &&
      !this.data.assignments.some(
        (assignment) => assignment.eventBookingId === booking.id && assignment.staffUserId === this.currentUser?.id,
      )
    ) {
      throw new ApiError('Staff can only add notes to assigned events.', 403)
    }

    const note: ShiftNoteDto = {
      id: `note-${crypto.randomUUID()}`,
      eventBookingId: booking.id,
      eventName: booking.eventName,
      staffUserId: author.id,
      staffName: author.fullName,
      noteType: request.noteType,
      body: request.body,
      isPinned: request.isPinned && this.currentUser?.role !== 'Staff',
      createdAt: new Date().toISOString(),
    }

    this.data.notes.push(note)
    booking.shiftNotes.push(note)
    return note
  }

  private requireBooking(id: string) {
    const booking = this.data.bookings.find((item) => item.id === id)
    if (!booking) {
      throw new ApiError('Booking not found.', 404)
    }
    return booking
  }

  private requireWrite() {
    if (!this.currentUser || !['Admin', 'Manager'].includes(this.currentUser.role)) {
      throw new ApiError('This action requires an operations role.', 403)
    }
  }

  private requireAdmin() {
    if (this.currentUser?.role !== 'Admin') {
      throw new ApiError('This action requires an admin role.', 403)
    }
  }
}

export function createVenueOpsClient(): VenueOpsClient {
  const useMocks = import.meta.env.MODE === 'test' || import.meta.env.VITE_USE_MOCKS === 'true'
  if (useMocks) {
    return new MockVenueOpsClient()
  }

  return new HttpVenueOpsClient(import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000/api')
}

function queryString(values: Record<string, string | undefined | null>) {
  const params = new URLSearchParams()
  Object.entries(values).forEach(([key, value]) => {
    if (value) {
      params.set(key, value)
    }
  })
  const query = params.toString()
  return query ? `?${query}` : ''
}

function normalizeBookingRequest(request: UpsertBookingRequest): UpsertBookingRequest {
  return {
    ...request,
    startTime: normalizeTime(request.startTime),
    endTime: normalizeTime(request.endTime),
  }
}

function normalizeTime(value: string) {
  return value.length === 5 ? `${value}:00` : value
}
