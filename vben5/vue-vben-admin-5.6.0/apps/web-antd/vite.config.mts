import { defineConfig } from '@vben/vite-config';

export default defineConfig(async () => {
  return {
    application: {},
    vite: {
      server: {
        proxy: {
          '/.well-known': {
            changeOrigin: true,
            target: 'http://localhost:44349/',
          },
          '/api': {
            changeOrigin: true,
            target: 'http://localhost:44349/',
          },
          '/connect': {
            changeOrigin: true,
            target: 'http://localhost:44349/',
          },
          '/signalr': {
            changeOrigin: true,
            target: 'http://localhost:44349/',
            ws: true,
          },
        },
      },
    },
  };
}) as ReturnType<typeof defineConfig>;
