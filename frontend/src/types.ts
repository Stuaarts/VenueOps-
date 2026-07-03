export type UserRole = 'Admin' | 'Manager' | 'Staff' | 'Demo'
export type BookingStatus = 'Inquiry' | 'Confirmed' | 'InProgress' | 'Completed' | 'Cancelled'
export type AssignmentStatus = 'Assigned' | 'Confirmed' | 'Completed' | 'NoShow'
export type StaffRole = 'Server' | 'Bartender' | 'Supervisor' | 'Setup' | 'Kitchen' | 'Security'
export type ShiftNoteType =
  | 'SetupCompleted'
  | 'GuestCountChanged'
  | 'ClientRequest'
  | 'Incident'
  | 'Closing'
  | 'SuppliesIssue'
  | 'StaffingIssue'
  | 'General'

export interface AuthUser {
  id: string
  fullName: string
  email: string
  role: UserRole
}

export interface LoginResponse {
  token: string
  expiresAt: string
  user: AuthUser
}

export interface UserDto extends AuthUser {
  isActive: boolean
}

export interface ClientDto {
  id: string
  name: string
  contactName: string
  email: string
  phone?: string | null
  notes?: string | null
  bookingCount: number
}

export interface VenueRoomDto {
  id: string
  name: string
  location: string
  capacity: number
  isActive: boolean
  notes?: string | null
  upcomingBookings: number
}

export interface BookingSummaryDto {
  id: string
  eventName: string
  clientName: string
  venueRoomName: string
  eventDate: string
  startTime: string
  endTime: string
  guestCount: number
  eventType: string
  status: BookingStatus
  assignedStaffCount: number
}

export interface BookingDetailDto extends BookingSummaryDto {
  clientId: string
  venueRoomId: string
  internalNotes?: string | null
  createdAt: string
  updatedAt: string
  staffAssignments: StaffAssignmentDto[]
  shiftNotes: ShiftNoteDto[]
}

export interface StaffAssignmentDto {
  id: string
  eventBookingId: string
  eventName: string
  staffUserId: string
  staffName: string
  role: StaffRole
  shiftStart: string
  shiftEnd: string
  status: AssignmentStatus
  notes?: string | null
}

export interface ShiftNoteDto {
  id: string
  eventBookingId: string
  eventName: string
  staffUserId: string
  staffName: string
  noteType: ShiftNoteType
  body: string
  isPinned: boolean
  createdAt: string
}

export interface DashboardMetricsDto {
  upcomingEvents: number
  totalBookingsThisMonth: number
  totalGuestCountThisMonth: number
  staffAssignedThisWeek: number
  cancelledEvents: number
  eventsNeedingStaff: number
}

export interface StatusCountDto {
  status: BookingStatus
  count: number
}

export interface DashboardDto {
  metrics: DashboardMetricsDto
  eventsByStatus: StatusCountDto[]
  upcomingEvents: BookingSummaryDto[]
  staffAssignedThisWeek: StaffAssignmentDto[]
  recentShiftNotes: ShiftNoteDto[]
}

export interface UpsertBookingRequest {
  eventName: string
  clientId: string
  venueRoomId: string
  eventDate: string
  startTime: string
  endTime: string
  guestCount: number
  eventType: string
  status: BookingStatus
  internalNotes?: string | null
}

export interface UpsertClientRequest {
  name: string
  contactName: string
  email: string
  phone?: string | null
  notes?: string | null
}

export interface UpsertVenueRoomRequest {
  name: string
  location: string
  capacity: number
  isActive: boolean
  notes?: string | null
}

export interface UpsertStaffAssignmentRequest {
  eventBookingId: string
  staffUserId: string
  role: StaffRole
  shiftStart: string
  shiftEnd: string
  status: AssignmentStatus
  notes?: string | null
}

export interface CreateShiftNoteRequest {
  eventBookingId: string
  staffUserId?: string | null
  noteType: ShiftNoteType
  body: string
  isPinned: boolean
}

export interface CreateUserRequest {
  fullName: string
  email: string
  password: string
  role: UserRole
  isActive: boolean
}

export interface BookingFilters {
  search?: string
  status?: BookingStatus | ''
  venueRoomId?: string
  assignedStaffUserId?: string
  from?: string
  to?: string
}

export interface VenueOpsClient {
  setToken(token: string | null): void
  login(email: string, password: string): Promise<LoginResponse>
  getDashboard(): Promise<DashboardDto>
  getBookings(filters?: BookingFilters): Promise<BookingSummaryDto[]>
  getBooking(id: string): Promise<BookingDetailDto>
  createBooking(request: UpsertBookingRequest): Promise<BookingDetailDto>
  updateBookingStatus(id: string, status: BookingStatus): Promise<BookingDetailDto>
  getClients(): Promise<ClientDto[]>
  createClient(request: UpsertClientRequest): Promise<ClientDto>
  getVenues(): Promise<VenueRoomDto[]>
  createVenue(request: UpsertVenueRoomRequest): Promise<VenueRoomDto>
  getStaff(): Promise<UserDto[]>
  getUsers(): Promise<UserDto[]>
  createUser(request: CreateUserRequest): Promise<UserDto>
  getAssignments(eventBookingId?: string): Promise<StaffAssignmentDto[]>
  createAssignment(request: UpsertStaffAssignmentRequest): Promise<StaffAssignmentDto>
  updateAssignmentStatus(id: string, status: AssignmentStatus): Promise<StaffAssignmentDto>
  getShiftNotes(eventBookingId?: string): Promise<ShiftNoteDto[]>
  createShiftNote(request: CreateShiftNoteRequest): Promise<ShiftNoteDto>
}

export const bookingStatuses: BookingStatus[] = [
  'Inquiry',
  'Confirmed',
  'InProgress',
  'Completed',
  'Cancelled',
]

export const assignmentStatuses: AssignmentStatus[] = ['Assigned', 'Confirmed', 'Completed', 'NoShow']
export const staffRoles: StaffRole[] = ['Server', 'Bartender', 'Supervisor', 'Setup', 'Kitchen', 'Security']
export const shiftNoteTypes: ShiftNoteType[] = [
  'SetupCompleted',
  'GuestCountChanged',
  'ClientRequest',
  'Incident',
  'Closing',
  'SuppliesIssue',
  'StaffingIssue',
  'General',
]
