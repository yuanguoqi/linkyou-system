FROM node:20-alpine AS build
WORKDIR /app

# 复制依赖文件（利用 Docker 层缓存）
COPY package.json ./
RUN npm install

# 复制源码并构建
COPY . .
RUN npm run build

# 使用 nginx 提供静态文件服务
FROM nginx:alpine AS final
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
