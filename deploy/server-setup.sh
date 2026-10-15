#!/usr/bin/env bash
# One-time setup for a fresh Hetzner Ubuntu server. Run as root:
#   bash server-setup.sh "<contents of your deploy public key>"
set -euo pipefail

DEPLOY_PUBKEY="${1:?Pass the GitHub Actions deploy public key as the first argument}"

apt-get update
apt-get install -y ca-certificates curl ufw
curl -fsSL https://get.docker.com | sh

# Non-root user that GitHub Actions logs in as
id deploy >/dev/null 2>&1 || adduser --disabled-password --gecos "" deploy
usermod -aG docker deploy
install -d -m 700 -o deploy -g deploy /home/deploy/.ssh
echo "$DEPLOY_PUBKEY" >> /home/deploy/.ssh/authorized_keys
chown deploy:deploy /home/deploy/.ssh/authorized_keys
chmod 600 /home/deploy/.ssh/authorized_keys

install -d -o deploy -g deploy /opt/witt

# Firewall: SSH + web only (MySQL is not exposed)
ufw allow OpenSSH
ufw allow 80/tcp
ufw allow 443/tcp
ufw allow 443/udp
ufw --force enable

echo "Done. Now create /opt/witt/.env from .env.example (owned by deploy, chmod 600)."
