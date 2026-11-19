/// <reference types="vitest" />
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

const api = 'http://localhost:5105';

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5205,
    proxy: { '/api': api, '/cdn': api, '/health': api },
  },
  test: {
    environment: 'jsdom',
    globals: true,
  },
});
