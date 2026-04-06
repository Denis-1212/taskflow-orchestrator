import { format, formatDistanceToNow, isAfter, parseISO } from 'date-fns'

export function formatDate(date: string | Date): string {
    const d = typeof date === 'string' ? parseISO(date) : date
    return format(d, 'PPP')
}

export function formatDateTime(date: string | Date): string {
    const d = typeof date === 'string' ? parseISO(date) : date
    return format(d, 'PPP p')
}

export function formatRelativeDate(date: string | Date): string {
    const d = typeof date === 'string' ? parseISO(date) : date
    return formatDistanceToNow(d, { addSuffix: true })
}

export function isOverdue(dueDate: string | Date | null): boolean {
    if (!dueDate) return false
    const d = typeof dueDate === 'string' ? parseISO(dueDate) : dueDate
    return isAfter(new Date(), d)
}