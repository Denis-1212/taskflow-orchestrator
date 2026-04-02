import { Button } from '@/components/ui/button'
import { Card } from '@/components/ui/card'
import { useProjects, useDeleteProject } from '../hooks/useProjects'
import { CreateProjectDialog } from '../components/CreateProjectDialog'
import { useAuthStore } from '@/features/auth/store/authStore'
import { Plus, Trash2, RefreshCw, Pencil } from 'lucide-react'
import { Project } from '../types/project.types'
import { useState } from 'react'
import { EditProjectDialog } from '../components/EditProjectDialog'
import { useNavigate } from 'react-router-dom'

export default function ProjectsPage() {
  const { user } = useAuthStore()
  const { data: projects, isLoading, error, refetch } = useProjects()
  const deleteProject = useDeleteProject()
  const [editingProject, setEditingProject] = useState<Project | null>(null)
  const navigate = useNavigate()

  // Показываем ошибку загрузки
  if (error) {
    const isServerError = (error as any)?.response?.status === 500
    const errorMessage = isServerError
      ? 'Unable to connect to server. Please check if backend is running.'
      : (error as any)?.response?.data?.message || (error as Error).message;
    return (
      <div className="space-y-6">
        <div className="flex justify-between items-center">
          <h1 className="text-3xl font-bold">Projects</h1>
          <Button variant="outline" onClick={() => refetch()}>
            <RefreshCw className="h-4 w-4 mr-2" />
            Retry
          </Button>
        </div>
        <Card className="p-12 text-center">
          <div className="text-destructive mb-4">
            <div className="text-5xl mb-2">⚠️</div>
            <p className="text-lg font-semibold">Failed to load projects</p>
            <p className="text-sm text-muted-foreground mt-2">{errorMessage}</p>
          </div>
          <Button variant="outline" onClick={() => refetch()} className="mt-4">
            Try Again
          </Button>
        </Card>
      </div>
    )
  }

  if (isLoading) {
    return (
      <div className="space-y-6">
        <div className="flex justify-between items-center">
          <h1 className="text-3xl font-bold">Projects</h1>
          <Button variant="outline" disabled>
            <RefreshCw className="h-4 w-4 mr-2 animate-spin" />
            Loading...
          </Button>
        </div>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {[1, 2, 3].map((i) => (
            <Card key={i} className="p-6 h-40 animate-pulse bg-muted" />
          ))}
        </div>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <h1 className="text-3xl font-bold">Projects</h1>
        <div className="flex gap-2">
          <Button
            variant="outline"
            onClick={() => refetch()}
            className="flex items-center gap-2"
          >
            <RefreshCw className="h-4 w-4" />
            Refresh
          </Button>
          <CreateProjectDialog>
            <Button>
              <Plus className="h-4 w-4 mr-2" />
              New Project
            </Button>
          </CreateProjectDialog>
        </div>
      </div>

      {!projects || projects.length === 0 ? (
        <Card className="p-12 text-center">
          <p className="text-muted-foreground">No projects yet</p>
          <CreateProjectDialog>
            <Button variant="outline" className="mt-4">
              Create your first project
            </Button>
          </CreateProjectDialog>
        </Card>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {projects.map((project) => (
            <Card key={project.id}
              className="p-6 cursor-pointer hover:shadow-lg transition-shadow"
              onClick={() => navigate(`/projects/${project.id}`)}
            >
              <div className="flex justify-between items-start gap-2">
                <h3 className="font-semibold truncate flex-1" title={project.name}>
                  {project.name.length > 30 ? `${project.name.substring(0, 30)}...` : project.name}
                </h3>
                {project.ownerId === user?.id && (
                  <div className="flex gap-1 flex-shrink-0">
                    <Button
                      variant="ghost"
                      size="icon"
                      onClick={() => setEditingProject(project)}
                      className="h-8 w-8"
                    >
                      <Pencil className="h-4 w-4" />
                    </Button>
                    <Button
                      variant="ghost"
                      size="icon"
                      onClick={() => deleteProject.mutate(project.id)}
                      className="h-8 w-8"
                    >
                      <Trash2 className="h-4 w-4" />
                    </Button>
                  </div>
                )}
              </div>

              <p
                className="text-muted-foreground text-sm mt-2 line-clamp-2"
                title={project.description || ''}
              >
                {project.description
                  ? (project.description.length > 100
                    ? `${project.description.substring(0, 100)}...`
                    : project.description)
                  : 'No description'}
              </p>

              <p className="text-xs text-muted-foreground mt-4">
                Created: {new Date(project.createdAt).toLocaleDateString()}
              </p>
            </Card>
          ))}
        </div>
      )}

      {editingProject && (
        <EditProjectDialog
          project={editingProject}
          open={!!editingProject}
          onClose={() => setEditingProject(null)}
        />
      )}
    </div>
  )

}
