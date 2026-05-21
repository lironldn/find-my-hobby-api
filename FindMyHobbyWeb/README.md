# FindMyHobbyWeb

React + TypeScript frontend for `find-my-hobby-api`.

## Local development

```bash
cd FindMyHobbyWeb
npm install
npm run dev
```

The Vite dev server proxies `/api` to `http://localhost:5001` by default.

## Production build

```bash
npm run build
```

## Container

The `Dockerfile` builds the frontend and serves it with Nginx.

The Nginx config proxies `/api/*` to the API service inside AKS.
