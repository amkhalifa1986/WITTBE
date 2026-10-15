# Deploying WITT to Hetzner

One server runs everything with Docker. Caddy issues HTTPS certificates automatically.

```
Internet ──► Caddy (80/443) ──► web    (WITTFE)    app.<domain>
                            ├─► admin  (WITTAdmin) admin.<domain>
                            └─► api    (WITTBE)    api.<domain> ──► MySQL (internal only)
```

Every push to `main` in WITTBE, WITTFE or WITTAdmin builds a Docker image on GitHub,
pushes it to ghcr.io, then connects to the server over SSH and restarts only that service.

## One-time setup

### 1. Server (Hetzner Cloud)
- Create a server: Ubuntu 24.04, at least **CX22 (2 vCPU / 4 GB)**. Note its IPv4 address.

### 2. Deploy SSH key (on your own computer)
```bash
ssh-keygen -t ed25519 -f witt_deploy -N "" -C "github-actions-deploy"
```
This creates `witt_deploy` (private key, goes to GitHub) and `witt_deploy.pub` (public key, goes to the server).

### 3. Prepare the server
```bash
scp deploy/server-setup.sh root@<SERVER_IP>:
ssh root@<SERVER_IP> "bash server-setup.sh '$(cat witt_deploy.pub)'"
```

### 4. DNS
Create three **A records** pointing to the server IP: `app`, `admin`, `api` (or any names you like).
No domain yet? Use `app.<SERVER_IP>.sslip.io`, `admin.<SERVER_IP>.sslip.io`, `api.<SERVER_IP>.sslip.io`. These work immediately.

### 5. Secrets file on the server
```bash
ssh root@<SERVER_IP>
sudo -u deploy nano /opt/witt/.env      # paste deploy/.env.example and fill in real values
chmod 600 /opt/witt/.env
```

### 6. GitHub settings (Settings → Secrets and variables → Actions)
In **each** of WITTBE, WITTFE, WITTAdmin, add these **Secrets**:

| Secret | Value |
|---|---|
| `SERVER_HOST` | server IP |
| `SERVER_USER` | `deploy` |
| `SERVER_SSH_KEY` | full contents of the private key file `witt_deploy` |

In **WITTFE and WITTAdmin**, add these **Variables**:

| Variable | Example |
|---|---|
| `API_URL` | `https://api.example.com` (no trailing slash) |
| `ADMIN_URL` | `https://admin.example.com/` |

### 7. First deployment (order matters)
1. **WITTBE** first: Actions → *Deploy API* → *Run workflow*. This copies the stack files and starts MySQL, the API and Caddy.
   On first start the API creates the database tables and loads the train timetable data.
2. Then **WITTFE** (*Deploy Web*) and **WITTAdmin** (*Deploy Admin*).

### 8. Right after the first deploy
- Log in to the admin console as `superadmin@whereisthetrain.com` / `SuperAdmin@123` and **change the password immediately**.

## Day-to-day
- Publishing = merging to `main`. Each repo deploys only itself.
- Logs: `ssh deploy@<SERVER_IP> "cd /opt/witt && docker compose logs -f api"`
- Status: `docker compose ps`
- Roll back: in `docker-compose.yml` on the server, replace `:latest` with `:<old commit sha>`, then `docker compose up -d <service>`.

## Backups (recommended)
Enable **Hetzner Backups** on the server (+20% of the server price). For database-only dumps:
```bash
docker compose exec -T mysql sh -c 'mysqldump -uroot -p"$MYSQL_ROOT_PASSWORD" --all-databases' | gzip > backup-$(date +%F).sql.gz
```
