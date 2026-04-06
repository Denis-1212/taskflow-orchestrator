import { useAuthStore } from '@/features/auth/store/authStore'
import { StatsCard } from '../components/StatsCard'
import { ProjectProgressCard } from '../components/ProjectProgressCard'
import { DashboardTaskCard } from '../components/TaskCard'
import { useMyProjectsStats, useMyTaskStats, useRecentTasks } from '../hooks/useDashboard'
import { ListTodo, Clock, AlertCircle, FolderKanban } from 'lucide-react'

export default function DashboardPage() {
    const { user } = useAuthStore()
    const isAdmin = user?.roles?.includes('Admin') || false

    const { data: projectsStats, isLoading: projectsLoading } = useMyProjectsStats()
    const { data: taskStats, isLoading: tasksStatsLoading } = useMyTaskStats()
    const { data: recentTasks, isLoading: recentLoading } = useRecentTasks(5)

    const isLoading = projectsLoading || tasksStatsLoading || recentLoading

    if (isLoading) {
        return (
            <div className="space-y-6">
                <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                    {[1, 2, 3].map(i => (
                        <div key={i} className="h-32 animate-pulse bg-muted rounded-lg" />
                    ))}
                </div>
                <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                    <div className="h-64 animate-pulse bg-muted rounded-lg" />
                    <div className="h-64 animate-pulse bg-muted rounded-lg" />
                </div>
            </div>
        )
    }

    return (
        <div className="space-y-6">
            <div>
                <h1 className="text-3xl font-bold">Dashboard</h1>
                <p className="text-muted-foreground mt-1">
                    Welcome back, {user?.fullName}
                </p>
            </div>

            {/* Stats Cards */}
            <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                <StatsCard
                    title="Total Tasks"
                    value={taskStats?.total || 0}
                    icon={<ListTodo className="h-6 w-6 text-primary" />}
                />
                <StatsCard
                    title="In Progress"
                    value={taskStats?.inProgress || 0}
                    icon={<Clock className="h-6 w-6 text-primary" />}
                />
                <StatsCard
                    title="Overdue"
                    value={taskStats?.overdue || 0}
                    icon={<AlertCircle className="h-6 w-6 text-destructive" />}
                />
            </div>

            {/* Two column layout */}
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                {/* My Projects */}
                <div className="space-y-4">
                    <div className="flex items-center gap-2">
                        <FolderKanban className="h-5 w-5 text-muted-foreground" />
                        <h2 className="text-xl font-semibold">My Projects</h2>
                    </div>

                    {projectsStats?.length === 0 ? (
                        <div className="text-center py-8 text-muted-foreground border rounded-lg">
                            No projects yet
                        </div>
                    ) : (
                        <div className="space-y-3">
                            {projectsStats?.map((project) => (
                                <ProjectProgressCard key={project.id} project={project} />
                            ))}
                        </div>
                    )}
                </div>

                {/* Recent Tasks */}
                <div className="space-y-4">
                    <div className="flex items-center gap-2">
                        <ListTodo className="h-5 w-5 text-muted-foreground" />
                        <h2 className="text-xl font-semibold">Recent Tasks</h2>
                    </div>

                    {recentTasks?.length === 0 ? (
                        <div className="text-center py-8 text-muted-foreground border rounded-lg">
                            No tasks assigned
                        </div>
                    ) : (
                        <div className="space-y-3">
                            {recentTasks?.map((task) => (
                                <DashboardTaskCard key={task.id} task={task} />
                            ))}
                        </div>
                    )}
                </div>
            </div>

            {/* Admin Section */}
            {isAdmin && (
                <div className="space-y-4 mt-6 pt-6 border-t">
                    <h2 className="text-xl font-semibold">Admin Overview</h2>

                    <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                        <div className="border rounded-lg p-4">
                            <h3 className="font-medium mb-3">System Stats</h3>
                            <div className="space-y-2 text-sm">
                                <div className="flex justify-between">
                                    <span className="text-muted-foreground">Your Projects</span>
                                    <span className="font-medium">{projectsStats?.length || 0}</span>
                                </div>
                                <div className="flex justify-between">
                                    <span className="text-muted-foreground">Your Tasks</span>
                                    <span className="font-medium">{taskStats?.total || 0}</span>
                                </div>
                            </div>
                        </div>

                        <div className="border rounded-lg p-4">
                            <h3 className="font-medium mb-3">Quick Links</h3>
                            <div className="space-y-2">
                                <a href="/admin/audit" className="block text-sm text-primary hover:underline">
                                    View Audit Logs
                                </a>
                                <a href="/projects" className="block text-sm text-primary hover:underline">
                                    Manage Projects
                                </a>
                                <a href="/tasks" className="block text-sm text-primary hover:underline">
                                    Manage Tasks
                                </a>
                            </div>
                        </div>
                    </div>
                </div>
            )}
        </div>
    )
}