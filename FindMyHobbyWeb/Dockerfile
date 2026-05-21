FROM node:20-alpine AS build
WORKDIR /app

COPY FindMyHobbyWeb/package.json ./
RUN npm install

COPY FindMyHobbyWeb/. .
RUN npm run build

FROM nginx:1.27-alpine
COPY FindMyHobbyWeb/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
