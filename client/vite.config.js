import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    // Forward /api requests to the ASP.NET Core backend during development.
    // Uses the https profile (the auth cookie is Secure); secure: false accepts the dev certificate.
    proxy: {
      '/api': {
        target: 'https://localhost:7073',
        secure: false,
      },
    },
  },
})
