import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { projectService } from '../services/projectService'
import { AddMemberRequest, ProjectRole } from '../types/project.types'

export const memberKeys = {
    all: (projectId: string) => ['project-members', projectId] as const,
}

export function useProjectMembers(projectId: string) {
    return useQuery({
        queryKey: memberKeys.all(projectId),
        queryFn: async () => {
            const response = await projectService.getProjectMembers(projectId)
            // response может быть массивом или объектом с полем value
            if (Array.isArray(response)) {
                return response
            }
            if (response && typeof response === 'object' && 'value' in response) {
                return (response as any).value
            }
            return response
        },
        enabled: !!projectId,
    })
}

export function useAddMember(projectId: string) {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: (data: AddMemberRequest) => projectService.addMember(projectId, data),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: memberKeys.all(projectId) })
            toast.success('Member added successfully')
        },
        onError: (error: any) => {
            toast.error(error.response?.data?.message || 'Failed to add member')
        },
    })
}

export function useRemoveMember(projectId: string) {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: (userId: string) => projectService.removeMember(projectId, userId),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: memberKeys.all(projectId) })
            toast.success('Member removed successfully')
        },
        onError: (error: any) => {
            toast.error(error.response?.data?.message || 'Failed to remove member')
        },
    })
}

export function useUpdateMemberRole(projectId: string) {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: ({ userId, role }: { userId: string; role: ProjectRole }) =>
            projectService.updateMemberRole(projectId, userId, role),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: memberKeys.all(projectId) })
            toast.success('Role updated successfully')
        },
        onError: (error: any) => {
            toast.error(error.response?.data?.message || 'Failed to update role')
        },
    })
}