import { useQuery } from '@tanstack/react-query'
import { authService } from '../services/authService'

export function useSearchUsers(query: string) {
    return useQuery({
        queryKey: ['users', 'search', query],
        queryFn: () => authService.searchUsers(query),
        enabled: query.length >= 2,
        staleTime: 1000 * 60,
    })
}

export function useUser(userId: string) {
    return useQuery({
        queryKey: ['user', userId],
        queryFn: () => authService.getUserById(userId),
        enabled: !!userId,
    })
}