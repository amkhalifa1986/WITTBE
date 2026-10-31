# Local Setup – Where Is The Train (all 4 apps)

Run the full system on your own computer for testing.

| Component | Repo | Local URL |
|---|---|---|
| Database (MySQL 8) | WITTBE (Docker) | `localhost:3306` |
| Backend API (.NET 8) | WITTBE | http://localhost:5245 (Swagger: http://localhost:5245/swagger) |
| Web app (public site) | WITTFE | http://localhost:3000 |
| Admin panel | WITTAdmin | http://localhost:5174 |
| Mobile app (Expo) | mobile | Expo Go on your phone / emulator |

## 1. Install once

- **Docker Desktop**: https://www.docker.com/products/docker-desktop
- **Node.js 20+**: https://nodejs.org
- **Git**
- Optional, for the mobile app: the **Expo Go** app on your phone, or Android Studio / Xcode for emulators.
- Optional, only if you want to run the API without Docker: **.NET 8 SDK**.

Clone all four repos into the same parent folder:

```
witt/
  WITTBE/  WITTFE/  WITTAdmin/  mobile/
```

## 2. Start the database and API (one command)

```bash
cd WITTBE
docker compose -f docker-compose.local.yml up -d --build
```

The first run takes a few minutes. When it's ready, http://localhost:5245/health returns `healthy`.
On first start, the API creates the database tables and seeds the test accounts listed below.

Useful commands:

```bash
docker compose -f docker-compose.local.yml logs -f api     # watch API logs
docker compose -f docker-compose.local.yml down            # stop (data kept)
docker compose -f docker-compose.local.yml down -v         # stop and WIPE all local data
```

> **Alternative without Docker for the API:** start only MySQL
> (`docker compose -f docker-compose.local.yml up -d mysql`), then
> `cd src/WhereIsTheTrain.API && dotnet run --launch-profile http`.
> `appsettings.json` already points to `localhost:3306` with the same user and password.

## 3. Start the web app

```bash
cd WITTFE
npm install
npm run dev
```

Open http://localhost:3000. The committed `.env` already points to `http://localhost:5245`.

## 4. Start the admin panel

```bash
cd WITTAdmin
npm install
npm run dev
```

Open http://localhost:5174. It defaults to `http://localhost:5245`; no configuration needed.

## 5. Start the mobile app

```bash
cd mobile
cp .env.example .env        # Windows: copy .env.example .env
```

Edit `.env` and set `EXPO_PUBLIC_API_URL`:

- **Physical phone** (same Wi-Fi as your PC): `http://<your-PC-IP>:5245`. Find the IP with `ipconfig` on Windows or `ipconfig getifaddr en0` on Mac.
- **Android emulator**: `http://10.0.2.2:5245`
- **iOS simulator or web**: `http://localhost:5245`

```bash
npm install
npx expo start -c
```

Scan the QR code with Expo Go. On a physical phone, allow port **5245** through the Windows/Mac firewall if requests time out.

> Note: `react-native-maps` and the Google Ads integration may need a development build
> (`npm run android` / `npm run ios`) instead of Expo Go for those screens.

## Test accounts (seeded automatically)

| Where | Email | Password |
|---|---|---|
| Admin panel – Super Admin | superadmin@whereisthetrain.com | SuperAdmin@123 |
| Admin panel – Admin | admin@whereisthetrain.com | Admin@123 |
| Web / Mobile – User | test@whereisthetrain.com | Test@123 |

## Troubleshooting

- **Port 3306 already in use**: a local MySQL is already running. Stop it, or change `"3306:3306"` to `"3307:3306"` in `docker-compose.local.yml` (Docker containers are unaffected).
- **Port 5245 in use**: stop any `dotnet run` API instance that is already running.
- **Web app shows network errors**: check that http://localhost:5245/health responds.
- **Want a fresh database**: run `docker compose -f docker-compose.local.yml down -v`, then start again.
