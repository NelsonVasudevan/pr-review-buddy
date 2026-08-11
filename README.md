# PR Review Buddy

A SaaS product that gives engineering teams one unified queue for pull
requests across GitHub and Azure DevOps, with AI-generated summaries and
Teams notifications.

## Project structure

- `backend/PrReviewBuddy.Api` — the .NET 8 API (talks to GitHub, Azure DevOps,
  the database, and eventually Azure OpenAI and Teams)
- `frontend` — the React dashboard the user actually sees and clicks around in

## Running it locally

Open **two** separate terminal windows.

**Terminal 1 — backend**
```
cd backend/PrReviewBuddy.Api
dotnet run
```
This starts the backend at http://localhost:5080

**Terminal 2 — frontend**
```
cd frontend
npm install
npm run dev
```
This starts the frontend at http://localhost:5173

Open http://localhost:5173 in your browser. If everything is wired up
correctly, you'll see a green box confirming the frontend successfully
reached the backend.
