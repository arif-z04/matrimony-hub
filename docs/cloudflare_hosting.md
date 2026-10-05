# Hosting Matrimony Hub Through Cloudflare

This guide provides end-to-end instructions for deploying, securing, and hosting **Matrimony Hub** behind **Cloudflare**.

---

## 1. Architecture Overview

Matrimony Hub is a full-stack monolithic application comprising:
- **ASP.NET Core 10 Web Application** (MVC, REST APIs, SignalR hubs, Razor views)
- **MariaDB / MySQL Database** (Relational data, transactions, user records)
- **Persistent Media Storage** (User photos, NID verification documents)

Cloudflare acts as the **Edge Acceleration & Security Layer**:
```
  [ Client Browser / Mobile ]
               │
               ▼  (HTTPS / 443)
 ┌───────────────────────────────┐
 │       Cloudflare Edge         │
 │  • Global Anycast DNS         │
 │  • DDoS & Bot Protection      │
 │  • Web Application Firewall   │
 │  • SSL/TLS Termination        │
 │  • Static Asset CDN Caching   │
 └──────────────┬────────────────┘
                │
     ┌──────────┴──────────┐
     ▼                     ▼
[Method 1: Tunnel]   [Method 2: Reverse Proxy]
(cloudflared daemon)   (Direct HTTPS / Strict SSL)
     │                     │
     └──────────┬──────────┘
                ▼
 ┌───────────────────────────────┐
 │      Origin Server (VPS)      │
 │  • Kestrel / ASP.NET Core     │
 │  • MariaDB / MySQL Container  │
 │  • Local / Block Media Vol    │
 └───────────────────────────────┘
```

There are **two proven deployment methods**:
1. **Method 1: Cloudflare Tunnel (`cloudflared`)** *(Recommended)*: Zero incoming firewall ports open on your server; the origin establishes outbound encrypted connections to Cloudflare.
2. **Method 2: Cloudflare Reverse Proxy (Orange Cloud DNS + Nginx/Kestrel)**: Standard public IP setup with Cloudflare Origin CA certificates and firewall IP restrictions.

---

## 2. Method 1: Cloudflare Tunnel (Zero Trust) — Recommended

Cloudflare Tunnel creates an encrypted, outbound-only connection between your origin server and Cloudflare Edge. You do **not** need a static public IP or open incoming ports (80/443).

### Step 1: Prerequisites
1. A registered domain (e.g., `matrimonyhub.com.bd`) with nameservers pointed to Cloudflare.
2. A Linux VPS (Ubuntu 22.04/24.04 or Debian 12) or Docker-enabled server.
3. Docker & Docker Compose installed on the VPS.

### Step 2: Create a Cloudflare Tunnel
1. Log in to the [Cloudflare Dashboard](https://dash.cloudflare.com/).
2. Navigate to **Zero Trust** > **Networks** > **Tunnels**.
3. Click **Create a Tunnel** and select **Cloudflare (cloudflared)**.
4. Name your tunnel (e.g. `matrimony-hub-prod`) and click **Save Tunnel**.
5. Copy the generated **Tunnel Token** (a long base64 string). You will use this in your Docker Compose file.

### Step 3: Production Docker Compose Setup

Create a production compose file `docker-compose.prod.yml` on your server:

```yaml
version: '3.8'

services:
  # 1. MariaDB Database
  db:
    image: mariadb:10.11
    container_name: matrimony-db-prod
    restart: unless-stopped
    environment:
      MYSQL_ROOT_PASSWORD: ${DB_ROOT_PASSWORD:-StrongRootPass2026!}
      MYSQL_DATABASE: MatrimonyHubDb
      MYSQL_USER: matrimony_user
      MYSQL_PASSWORD: ${DB_USER_PASSWORD:-MatrimonySecurePass2026!}
    volumes:
      - mariadb_data:/var/lib/mysql
    networks:
      - matrimony-network

  # 2. Matrimony Hub Web Application
  web:
    build:
      context: .
      dockerfile: Dockerfile
    container_name: matrimony-web-prod
    restart: unless-stopped
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:8080
      - ConnectionStrings__DefaultConnection=Server=db;Port=3306;Database=MatrimonyHubDb;User=matrimony_user;Password=${DB_USER_PASSWORD:-MatrimonySecurePass2026!};TreatTinyAsBoolean=true;
      - AdminSeed__Email=admin@matrimonyhub.com
      - AdminSeed__Password=${ADMIN_SEED_PASSWORD:-Admin@Pass123!}
      - PaymentGateway__ContactUnlockFee=500.00
    volumes:
      - matrimony_uploads:/app/wwwroot/uploads
    depends_on:
      - db
    networks:
      - matrimony-network

  # 3. Cloudflare Tunnel Daemon (cloudflared)
  cloudflared:
    image: cloudflare/cloudflared:latest
    container_name: matrimony-tunnel
    restart: unless-stopped
    command: tunnel --no-autoupdate run --token ${CLOUDFLARE_TUNNEL_TOKEN}
    depends_on:
      - web
    networks:
      - matrimony-network

volumes:
  mariadb_data:
  matrimony_uploads:

networks:
  matrimony-network:
    driver: bridge
```

Create a companion `.env` file in the same directory:

```env
DB_ROOT_PASSWORD=YourComplexDbRootPasswordHere!
DB_USER_PASSWORD=YourComplexAppDbPasswordHere!
ADMIN_SEED_PASSWORD=YourAdminPortalPasswordHere!
CLOUDFLARE_TUNNEL_TOKEN=eyJhIjoiNG...YOUR_TUNNEL_TOKEN_HERE...
```

### Step 4: Configure Public Hostname in Cloudflare
Back in the Cloudflare Dashboard under your Tunnel configuration:
1. Go to the **Public Hostname** tab.
2. Click **Add a public hostname**.
3. Configure:
   - **Subdomain / Domain**: e.g., `www` / `matrimonyhub.com.bd` (or leave subdomain empty for root domain).
   - **Type**: `HTTP`
   - **URL**: `web:8080` (the container name and internal port defined in your compose network).
4. Under **Additional application settings** > **HTTP Settings**:
   - Enable **HTTP2**
   - Enable **No TLS Verify** (since origin traffic is internal HTTP inside Docker bridge network).
5. Click **Save hostname**.

### Step 5: Start the Stack
```bash
docker compose -f docker-compose.prod.yml up -d
```
Within seconds, your application will be live at `https://matrimonyhub.com.bd` with Cloudflare SSL automatically active.

---

## 3. Method 2: Cloudflare Reverse Proxy (Orange Cloud DNS)

If you already manage a VPS with a static public IP and Nginx/Kestrel directly:

### Step 1: DNS Setup
In Cloudflare DNS Management:
- Add an **A Record**: `matrimonyhub.com.bd` -> `YOUR_SERVER_PUBLIC_IP` (Proxy status: **Proxied** / Orange cloud enabled).
- Add a **CNAME Record**: `www` -> `matrimonyhub.com.bd` (Proxy status: **Proxied**).

### Step 2: SSL/TLS Mode
In Cloudflare Dashboard > **SSL/TLS**:
- Select **Full (Strict)**.  
  *(Never use "Flexible" as it causes HTTP/HTTPS infinite redirect loops with ASP.NET Core's `UseHttpsRedirection` middleware).*

### Step 3: Generate Cloudflare Origin CA Certificate
1. Go to **SSL/TLS** > **Origin Server** > **Create Certificate**.
2. Select your domains (`matrimonyhub.com.bd`, `*.matrimonyhub.com.bd`) and validity (e.g. 15 years).
3. Save the certificate file to `/etc/ssl/certs/cloudflare_origin.pem`.
4. Save the private key file to `/etc/ssl/private/cloudflare_origin.key`.

### Step 4: Nginx Reverse Proxy Configuration
If placing Nginx in front of Kestrel:

```nginx
server {
    listen 80;
    server_name matrimonyhub.com.bd www.matrimonyhub.com.bd;
    return 301 https://$host$request_uri;
}

server {
    listen 443 ssl http2;
    server_name matrimonyhub.com.bd www.matrimonyhub.com.bd;

    ssl_certificate /etc/ssl/certs/cloudflare_origin.pem;
    ssl_certificate_key /etc/ssl/private/cloudflare_origin.key;
    ssl_protocols TLSv1.2 TLSv1.3;

    # Client upload limits for photos & NID documents
    client_max_body_size 30M;

    # Forward client headers to Kestrel
    location / {
        proxy_pass http://127.0.0.1:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection $http_connection;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_set_header CF-Connecting-IP $http_cf_connecting_ip;
    }
}
```

### Step 5: Lock Down Firewall to Cloudflare IPs Only
To prevent attackers from bypassing Cloudflare by hitting your server IP directly, allow only Cloudflare's published IP ranges:

```bash
# Allow SSH
sudo ufw allow 22/tcp

# Allow Cloudflare IP blocks (IPv4)
for ip in $(curl -s https://www.cloudflare.com/ips-v4); do
    sudo ufw allow proto tcp from $ip to any port 80,443
done

# Allow Cloudflare IP blocks (IPv6)
for ip in $(curl -s https://www.cloudflare.com/ips-v6); do
    sudo ufw allow proto tcp from $ip to any port 80,443
done

sudo ufw enable
```

---

## 4. ASP.NET Core Configuration for Cloudflare

### Client Real IP & Forwarded Headers
Cloudflare passes the real visitor IP in the `CF-Connecting-IP` header and standard `X-Forwarded-For`.
Matrimony Hub has `UseForwardedHeaders` pre-configured in `src/MatrimonyHub.Web/Program.cs`:

```csharp
// Forwarded Headers for Cloudflare / Reverse Proxies
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});
...
app.UseForwardedHeaders();
app.UseHttpsRedirection();
```
This guarantees:
1. `HttpContext.Connection.RemoteIpAddress` resolves to the actual user in Bangladesh rather than Cloudflare's edge proxy IP.
2. `AdminLogs`, audit records, and security audits record authentic visitor IP addresses.
3. Cookie authentication and HTTPS redirection operate smoothly without redirect loops.

---

## 5. Cloudflare Caching & Performance Rules

Matrimony Hub features static branding and media alongside strictly private matrimonial data. Configure Cache Rules under **Caching** > **Cache Rules**:

### Rule 1: Cache Static Assets (Aggressive)
- **Condition**: 
  `URI Path starts with "/css/" or URI Path starts with "/js/" or URI Path starts with "/lib/" or URI Path starts with "/images/"`
- **Cache Eligibility**: Eligible for cache
- **Edge Cache TTL**: 1 Month
- **Browser Cache TTL**: 1 Month

### Rule 2: Cache Public Profile Photos
- **Condition**:
  `URI Path starts with "/uploads/profiles/"`
- **Cache Eligibility**: Eligible for cache
- **Edge Cache TTL**: 7 Days
- **Browser Cache TTL**: 7 Days

### Rule 3: Bypass Cache on Dynamic & Sensitive Routes
- **Condition**:
  `URI Path starts with "/Auth/" or URI Path starts with "/Payment/" or URI Path starts with "/Admin/" or URI Path starts with "/api/" or URI Path starts with "/Verification/" or URI Path starts with "/Contact/"`
- **Cache Eligibility**: Bypass cache

---

## 6. Cloudflare Security & WAF Configuration

### 1. Protect Sensitive NID Documents
Ensure NID upload documents (`/uploads/verifications/`) are protected by Cloudflare WAF or stored in a private directory accessible only to authorized administrators via controller streams rather than raw static downloads.

### 2. Rate Limiting on Authentication & Payments
Under **Security** > **WAF** > **Rate Limiting Rules**:
- **Rule 1: Login Brute Force Protection**
  - Path: `/Auth/Login`
  - Method: `POST`
  - Requests: 5 requests per 10 seconds per IP
  - Action: Managed Challenge (Cloudflare Turnstile)
- **Rule 2: Payment Gateway Callback Throttling**
  - Path: `/Payment/ProcessCallback` or `/api/payments/callback`
  - Method: `POST`
  - Requests: 30 requests per minute per IP
  - Action: Block

### 3. Enable Bot Fight Mode
Under **Security** > **Bots**:
- Enable **Bot Fight Mode** to mitigate credential-stuffing and automated scraping bots.

### 4. Enable WebSockets for Real-Time Features
Under **Network**:
- Ensure **WebSockets** is toggled **ON** (enabled by default). This allows SignalR hubs to deliver instant notifications when candidate contacts are unlocked or profiles are viewed.

---

## 7. Troubleshooting & Verification

| Issue / Error | Cause | Resolution |
|---|---|---|
| **Error 521 (Web Server Down)** | Cloudflare cannot connect to origin port 8080/443 | Verify that `docker compose ps` shows `matrimony-web-prod` running and healthy. If using Tunnel, check `docker logs matrimony-tunnel`. |
| **Error 522 (Connection Timed Out)** | Origin firewall blocking Cloudflare IPs | Ensure UFW allows Cloudflare IP ranges, or switch to Method 1 (Cloudflare Tunnel) which bypasses inbound firewalls. |
| **Too Many Redirects (`ERR_TOO_MANY_REDIRECTS`)** | Cloudflare SSL mode set to "Flexible" | Change Cloudflare SSL/TLS mode to **Full (Strict)**. |
| **Large Photo/NID Upload Fails (HTTP 413)** | Cloudflare or Kestrel body size limit exceeded | Cloudflare free tier allows up to 100MB body payloads. Configure `FormOptions.MultipartBodyLengthLimit` in ASP.NET Core if uploading > 30MB files. |
| **SignalR Disconnecting Frequently** | WebSocket proxy timeout | Ensure Cloudflare **WebSockets** is enabled in the Network tab. SignalR will automatically negotiate long polling fallback if necessary. |

---

## 8. Summary Checklist

- [ ] Domain nameservers switched to Cloudflare.
- [ ] Deployment method selected: **Cloudflare Tunnel (Zero Trust)** or **Reverse Proxy**.
- [ ] `docker-compose.prod.yml` configured with production passwords and tokens.
- [ ] SSL mode set to **Full (Strict)**.
- [ ] Static asset caching rules configured.
- [ ] Dynamic paths (`/Payment/`, `/Auth/`, `/Admin/`) bypassed from caching.
- [ ] Rate limiting enabled on `/Auth/Login`.
- [ ] Application verified live over HTTPS with valid edge certificate.
