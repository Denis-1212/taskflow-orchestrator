import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import path from 'path'

export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
  server: {
    port: 5173,
    proxy: {
      '/auth': {
        target: 'http://localhost:8080',
        changeOrigin: true,
        rewrite: (path) => path,
      },
      '/project': {
        target: 'http://localhost:8080',
        changeOrigin: true,
      },
      '/task': {
        target: 'http://localhost:8080',
        changeOrigin: true,
      },
      '/notification': {
        target: 'http://localhost:8080',
        changeOrigin: true,
      },
      '/audit': {
        target: 'http://localhost:8080',
        changeOrigin: true,
      },
    },
  },
})
