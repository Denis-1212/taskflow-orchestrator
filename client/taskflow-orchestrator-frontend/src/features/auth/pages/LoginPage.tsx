import { LoginForm } from '../components/LoginForm'

export default function LoginPage() {
  return (
    <div className="rounded-lg border bg-card p-8 shadow-lg">
      <div className="mb-6 text-center">
        <h1 className="text-2xl font-bold">Welcome Back</h1>
        <p className="text-sm text-muted-foreground mt-2">
          Sign in to your account to continue
        </p>
      </div>
      <LoginForm />
    </div>
  )
}
