import type {
  AssignmentStatus,
  AuthUser,
  BookingDetailDto,
  BookingStatus,
  ClientDto,
  DashboardDto,
  ShiftNoteDto,
  StaffAssignmentDto,
  StaffRole,
  UserDto,
  UserRole,
  VenueRoomDto,
} from './types'

const dayMs = 24 * 60 * 60 * 1000

const isoDate = (offset: number) => new Date(Date.now() + offset * dayMs).toISOString().slice(0, 10)
const isoDateTime = (date: string, time: string) => `${date}T${time.length === 5 ? `${time}:00` : time}Z`
const id = (prefix: string, index: number) => `${prefix}-${index.toString().padStart(2, '0')}`

export const demoPassword = 'VenueOpsDemo!2026'

export function createSeedData() {
  const users: UserDto[] = [
    user('user-01', 'Avery Chen', 'admin@venueops.local', 'Admin'),
    user('user-02', 'Morgan Patel', 'manager@venueops.local', 'Manager'),
    user('user-03', 'Riley Demo', 'demo@venueops.local', 'Demo'),
    user('user-04', 'Sam Rivera', 'sam.staff@venueops.local', 'Staff'),
    user('user-05', 'Jordan Kim', 'jordan.staff@venueops.local', 'Staff'),
    user('user-06', 'Taylor Brooks', 'taylor.staff@venueops.local', 'Staff'),
    user('user-07', 'Casey Nguyen', 'casey.staff@venueops.local', 'Staff'),
    user('user-08', "Jamie O'Neil", 'jamie.staff@venueops.local', 'Staff'),
    user('user-09', 'Devon Lee', 'devon.staff@venueops.local', 'Staff'),
    user('user-10', 'Alex Carter', 'alex.staff@venueops.local', 'Staff'),
    user('user-11', 'Priya Shah', 'priya.staff@venueops.local', 'Staff'),
    user('user-12', 'Noah Green', 'noah.staff@venueops.local', 'Staff'),
    user('user-13', 'Mia Wilson', 'mia.staff@venueops.local', 'Staff'),
  ]

  const clients: ClientDto[] = [
    client('client-01', 'Northstar Finance', 'Elena Morris', 'elena.morris@example.com', '555-0134', 3),
    client('client-02', 'Harlow & Reed', 'Marcus Reed', 'marcus.reed@example.com', '555-0188', 2),
    client('client-03', 'Civic Arts Council', 'Dina Alvarez', 'dina.alvarez@example.com', '555-0119', 2),
    client('client-04', 'Bridgeway Labs', 'Nora Singh', 'nora.singh@example.com', '555-0192', 2),
  ]

  const venues: VenueRoomDto[] = [
    venue('venue-01', 'Grand Ballroom', 'Main Floor', 420, 3),
    venue('venue-02', 'Riverside Terrace', 'Outdoor Level', 180, 2),
    venue('venue-03', 'Summit Conference Hall', 'Second Floor', 260, 3),
  ]

  const bookings: BookingDetailDto[] = [
    booking(1, 'Northstar Leadership Summit', clients[0], venues[2], 2, '08:00', '15:30', 180, 'Corporate', 'Confirmed'),
    booking(2, 'Harlow Reed Wedding Reception', clients[1], venues[0], 5, '16:00', '23:30', 245, 'Wedding', 'Confirmed'),
    booking(3, 'Civic Arts Donor Gala', clients[2], venues[0], 9, '17:00', '22:00', 310, 'Gala', 'Inquiry'),
    booking(4, 'Bridgeway Product Launch', clients[3], venues[2], 12, '10:00', '16:00', 210, 'Conference', 'Confirmed'),
    booking(5, 'Terrace Cocktail Preview', clients[1], venues[1], 15, '18:00', '21:00', 95, 'Private', 'InProgress'),
    booking(6, 'Community Volunteer Lunch', clients[2], venues[1], -3, '11:00', '14:00', 126, 'Private', 'Completed'),
    booking(7, 'Finance Board Dinner', clients[0], venues[0], 20, '18:30', '22:00', 80, 'Corporate', 'Inquiry'),
    booking(8, 'Bridgeway Training Workshop', clients[3], venues[2], 27, '09:00', '13:00', 140, 'Conference', 'Cancelled'),
  ]

  const staff = users.filter((user) => user.role === 'Staff')
  const assignments: StaffAssignmentDto[] = [
    assignment(1, bookings[0], staff[0], 'Supervisor', '07:00', '16:00', 'Confirmed'),
    assignment(2, bookings[0], staff[1], 'Setup', '06:00', '12:00', 'Confirmed'),
    assignment(3, bookings[0], staff[2], 'Server', '08:00', '15:00', 'Assigned'),
    assignment(4, bookings[1], staff[3], 'Supervisor', '15:00', '23:30', 'Confirmed'),
    assignment(5, bookings[1], staff[4], 'Bartender', '16:00', '23:30', 'Confirmed'),
    assignment(6, bookings[2], staff[5], 'Setup', '12:00', '18:00', 'Assigned'),
    assignment(7, bookings[2], staff[6], 'Security', '17:00', '23:00', 'Assigned'),
    assignment(8, bookings[3], staff[7], 'Supervisor', '09:00', '17:00', 'Confirmed'),
    assignment(9, bookings[3], staff[0], 'Kitchen', '08:00', '16:00', 'Assigned'),
    assignment(10, bookings[4], staff[1], 'Bartender', '17:00', '22:00', 'Assigned'),
    assignment(11, bookings[4], staff[2], 'Server', '17:00', '22:00', 'Assigned'),
    assignment(12, bookings[5], staff[4], 'Supervisor', '10:00', '15:00', 'Completed'),
    assignment(13, bookings[5], staff[5], 'Server', '10:00', '15:00', 'Completed'),
    assignment(14, bookings[6], staff[6], 'Server', '18:00', '22:00', 'Assigned'),
    assignment(15, bookings[0], staff[7], 'Security', '07:30', '15:30', 'Assigned'),
  ]

  const notes: ShiftNoteDto[] = [
    note(1, bookings[0], staff[0], 'SetupCompleted', 'Registration tables and signage are staged outside Summit Hall.', true),
    note(2, bookings[0], staff[1], 'SuppliesIssue', 'Need two extra water stations before lunch service.'),
    note(3, bookings[1], staff[3], 'ClientRequest', 'Client requested sweetheart table moved closer to the dance floor.'),
    note(4, bookings[1], staff[4], 'General', 'Bar inventory confirmed with beverage manager.'),
    note(5, bookings[2], staff[5], 'ClientRequest', 'Awaiting approval on revised donor wall placement.'),
    note(6, bookings[2], staff[6], 'StaffingIssue', 'Security may need one additional person for VIP arrivals.'),
    note(7, bookings[3], staff[7], 'SetupCompleted', 'Main stage and demo counter measurements confirmed.'),
    note(8, bookings[3], staff[0], 'SuppliesIssue', 'Coffee urn count increased from 4 to 6.'),
    note(9, bookings[4], staff[1], 'General', 'Weather backup room held through noon on event day.'),
    note(10, bookings[4], staff[2], 'ClientRequest', 'Client asked for citrus-free mocktail option.'),
    note(11, bookings[5], staff[4], 'GuestCountChanged', 'Final count changed from 120 to 126 at check-in.'),
    note(12, bookings[5], staff[5], 'Closing', 'Terrace reset completed and linens returned.'),
    note(13, bookings[6], staff[6], 'General', 'Menu tasting follow-up scheduled with catering.'),
    note(14, bookings[0], staff[2], 'General', 'Extra extension cords staged near breakout hallway.'),
    note(15, bookings[1], staff[4], 'Incident', 'Minor spill near service station cleaned immediately.'),
  ]

  bookings.forEach((booking) => {
    booking.staffAssignments = assignments.filter((assignment) => assignment.eventBookingId === booking.id)
    booking.shiftNotes = notes.filter((note) => note.eventBookingId === booking.id)
    booking.assignedStaffCount = booking.staffAssignments.length
  })

  return { users, clients, venues, bookings, assignments, notes }
}

export function createDashboard(data: ReturnType<typeof createSeedData>, currentUser?: AuthUser): DashboardDto {
  const visibleBookings = scopeBookings(data.bookings, data.assignments, currentUser)
  const today = new Date().toISOString().slice(0, 10)
  const month = today.slice(0, 7)
  const weekEnd = new Date(Date.now() + 7 * dayMs).toISOString()
  const assignments = scopeAssignments(data.assignments, currentUser)
  const notes = scopeNotes(data.notes, data.assignments, currentUser)
  const monthBookings = visibleBookings.filter((booking) => booking.eventDate.startsWith(month))

  return {
    metrics: {
      upcomingEvents: visibleBookings.filter((booking) => booking.eventDate >= today && booking.status !== 'Cancelled').length,
      totalBookingsThisMonth: monthBookings.length,
      totalGuestCountThisMonth: monthBookings.reduce((sum, booking) => sum + booking.guestCount, 0),
      staffAssignedThisWeek: assignments.filter((assignment) => assignment.shiftStart < weekEnd).length,
      cancelledEvents: visibleBookings.filter((booking) => booking.status === 'Cancelled').length,
      eventsNeedingStaff: visibleBookings.filter(
        (booking) => booking.eventDate >= today && booking.status !== 'Cancelled' && booking.assignedStaffCount === 0,
      ).length,
    },
    eventsByStatus: ['Inquiry', 'Confirmed', 'InProgress', 'Completed', 'Cancelled'].map((status) => ({
      status: status as BookingStatus,
      count: visibleBookings.filter((booking) => booking.status === status).length,
    })),
    upcomingEvents: visibleBookings
      .filter((booking) => booking.eventDate >= today && booking.status !== 'Cancelled')
      .sort((a, b) => a.eventDate.localeCompare(b.eventDate))
      .slice(0, 8),
    staffAssignedThisWeek: assignments
      .filter((assignment) => assignment.shiftStart < weekEnd)
      .sort((a, b) => a.shiftStart.localeCompare(b.shiftStart))
      .slice(0, 8),
    recentShiftNotes: notes
      .sort((a, b) => Number(b.isPinned) - Number(a.isPinned) || b.createdAt.localeCompare(a.createdAt))
      .slice(0, 8),
  }
}

export function scopeBookings(bookings: BookingDetailDto[], assignments: StaffAssignmentDto[], user?: AuthUser) {
  if (user?.role !== 'Staff') {
    return bookings
  }

  const assignedIds = new Set(assignments.filter((assignment) => assignment.staffUserId === user.id).map((x) => x.eventBookingId))
  return bookings.filter((booking) => assignedIds.has(booking.id))
}

export function scopeAssignments(assignments: StaffAssignmentDto[], user?: AuthUser) {
  return user?.role === 'Staff' ? assignments.filter((assignment) => assignment.staffUserId === user.id) : assignments
}

export function scopeNotes(notes: ShiftNoteDto[], assignments: StaffAssignmentDto[], user?: AuthUser) {
  if (user?.role !== 'Staff') {
    return notes
  }

  const assignedIds = new Set(assignments.filter((assignment) => assignment.staffUserId === user.id).map((x) => x.eventBookingId))
  return notes.filter((note) => assignedIds.has(note.eventBookingId))
}

function user(idValue: string, fullName: string, email: string, role: UserRole): UserDto {
  return { id: idValue, fullName, email, role, isActive: true }
}

function client(idValue: string, name: string, contactName: string, email: string, phone: string, bookingCount: number): ClientDto {
  return { id: idValue, name, contactName, email, phone, bookingCount, notes: 'Recurring event partner.' }
}

function venue(idValue: string, name: string, location: string, capacity: number, upcomingBookings: number): VenueRoomDto {
  return { id: idValue, name, location, capacity, isActive: true, upcomingBookings, notes: 'Active operations room.' }
}

function booking(
  index: number,
  eventName: string,
  client: ClientDto,
  venue: VenueRoomDto,
  dateOffset: number,
  startTime: string,
  endTime: string,
  guestCount: number,
  eventType: string,
  status: BookingStatus,
): BookingDetailDto {
  const date = isoDate(dateOffset)
  const createdAt = new Date(Date.now() - Math.abs(dateOffset) * dayMs).toISOString()
  return {
    id: id('booking', index),
    eventName,
    clientId: client.id,
    clientName: client.name,
    venueRoomId: venue.id,
    venueRoomName: venue.name,
    eventDate: date,
    startTime,
    endTime,
    guestCount,
    eventType,
    status,
    assignedStaffCount: 0,
    internalNotes: 'Operational notes and client preferences tracked for the event team.',
    createdAt,
    updatedAt: createdAt,
    staffAssignments: [],
    shiftNotes: [],
  }
}

function assignment(
  index: number,
  booking: BookingDetailDto,
  staff: UserDto,
  role: StaffRole,
  startTime: string,
  endTime: string,
  status: AssignmentStatus,
): StaffAssignmentDto {
  return {
    id: id('assignment', index),
    eventBookingId: booking.id,
    eventName: booking.eventName,
    staffUserId: staff.id,
    staffName: staff.fullName,
    role,
    shiftStart: isoDateTime(booking.eventDate, startTime),
    shiftEnd: isoDateTime(booking.eventDate, endTime),
    status,
    notes: `${role} shift for ${booking.eventName}.`,
  }
}

function note(
  index: number,
  booking: BookingDetailDto,
  staff: UserDto,
  noteType: ShiftNoteDto['noteType'],
  body: string,
  isPinned = false,
): ShiftNoteDto {
  return {
    id: id('note', index),
    eventBookingId: booking.id,
    eventName: booking.eventName,
    staffUserId: staff.id,
    staffName: staff.fullName,
    noteType,
    body,
    isPinned,
    createdAt: new Date(Date.now() - index * 60 * 60 * 1000).toISOString(),
  }
}
