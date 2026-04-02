import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { projectService } from '../services/projectService'
import { CreateProjectRequest, UpdateProjectRequest } from '../types/project.types'

export function useProjects() {
  return useQuery({
    queryKey: ['projects'],
    queryFn: () => projectService.getProjects(),
    staleTime: 0,  // данные сразу считаются устаревшими
    refetchOnMount: true,  // обновлять при монтировании
    refetchOnWindowFocus: true,  // обновлять при возвращении на вкладку
    retry: 1,
  })
}

export function useCreateProject() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (data: CreateProjectRequest) => projectService.createProject(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['projects'] })
      toast.success('Project created successfully')
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Failed to create project')
    },
  })
}

export function useDeleteProject() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: string) => projectService.deleteProject(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['projects'] })
      toast.success('Project deleted successfully')
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Failed to delete project')
    },
  })
}

export function useUpdateProject() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateProjectRequest }) =>
      projectService.updateProject(id, data),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: ['projects'] })
      queryClient.invalidateQueries({ queryKey: ['project', id] })
      toast.success('Project updated successfully')
    },
    onError: (error: any) => {
      const status = error.response?.status
      if (status === 500) {
        toast.error('Server error. Please try again later.')
      } else if (status === 403) {
        toast.error('You do not have permission to edit this project.')
      } else {
        toast.error('Failed to update project')
      }
    },
  })
}

export function useProject(id: string) {
  return useQuery({
    queryKey: ['project', id],
    queryFn: () => projectService.getProjectById(id),
    enabled: !!id,
  })
}