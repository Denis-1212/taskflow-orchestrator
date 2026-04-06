import { Card } from '@/components/ui/card'
import { ProjectStats } from '../types/dashboard.types'
import { useNavigate } from 'react-router-dom'

interface ProjectProgressCardProps {
    project: ProjectStats
}

export function ProjectProgressCard({ project }: ProjectProgressCardProps) {
    const navigate = useNavigate()

    return (
        <Card
            className="p-4 cursor-pointer hover:shadow-md transition-shadow"
            onClick={() => navigate(`/projects/${project.id}`)}
        >
            <div className="flex justify-between items-start mb-2">
                <h3 className="font-semibold truncate flex-1">{project.name}</h3>
                <span className="text-sm text-muted-foreground ml-2">
                    {project.completedTasks}/{project.totalTasks}
                </span>
            </div>

            <div className="w-full bg-muted rounded-full h-2">
                <div
                    className="bg-primary rounded-full h-2 transition-all"
                    style={{ width: `${project.progress}%` }}
                />
            </div>

            <p className="text-xs text-muted-foreground mt-2">
                {project.progress.toFixed(0)}% completed
            </p>
        </Card>
    )
}