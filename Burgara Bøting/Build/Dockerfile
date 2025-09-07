# Use a small Node image
FROM node:20-alpine

# Create app dir
WORKDIR /app

# Copy only app code and static assets (no certs)
COPY server.mjs ./
COPY index.html ./
COPY admin.html ./
COPY Build ./Build

# Environment
ENV NODE_ENV=production

# Expose the ports your app listens on
EXPOSE 8080 8443

# Run the server
CMD ["node", "server.mjs"]
