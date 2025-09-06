# --- deps stage --------------------------------------------------------------
FROM node:20-alpine AS deps
WORKDIR /app
 COPY package.json package-lock.json* ./
# If lockfile exists -> npm ci, else -> npm install
RUN if [ -f package-lock.json ]; then npm ci --omit=dev; else npm install --omit=dev; fi

# --- runtime stage -----------------------------------------------------------
FROM node:20-alpine AS runner
ENV NODE_ENV=production
WORKDIR /app

# (optional but recommended) proper signal handling
RUN apk add --no-cache tini

# copy node_modules from deps and your app files
COPY --from=deps /app/node_modules ./node_modules
COPY server.js ./server.js
COPY Build ./webgl

# run as non-root (the base image has a 'node' user)
USER node

# container port
EXPOSE 3000

# simple healthcheck (your server exposes GET /pin)
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s \
  CMD wget -qO- http://127.0.0.1:3000/pin || exit 1

ENTRYPOINT ["/sbin/tini","--"]
CMD ["node","server.js"]
