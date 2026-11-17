# SimpleCrudBot

[![.NET](https://github.com/Ledrunning/CrudBot/actions/workflows/dotnet.yml/badge.svg)](https://github.com/Ledrunning/CrudBot/actions/workflows/dotnet.yml)

Telegram bot that returns current weather (OpenWeather API) and demonstrates basic CRUD operations
with an MS SQL Server database over ADO.NET.

**Status:** being reworked. Planned: replace the persons CRUD with scheduled weather tracking,
statistics and charts (see Roadmap).

## Tech stack

C#, .NET 10, console application (long polling), [Telegram.Bot](https://www.nuget.org/packages/Telegram.Bot),
ADO.NET, MS SQL Server, OpenWeather API, GitHub Actions (CI)

## Configuration

Settings are in `appsettings.json` in the `CrudBot.Main` project. Do not commit real secrets;
use user secrets or environment variables.

```json
{
  "BotConfiguration": { "BotToken": "<token from @BotFather>" },
  "DatabaseConfiguration": { "ConnectionString": "<connection string>" },
  "OpenWeatherApi": {
    "BaseUrl": "https://api.openweathermap.org/data/2.5/weather?",
    "ApiKey": "<your API key>",
    "TimeOut": "-1",
    "BaseGeoUrl": "http://api.openweathermap.org/geo/1.0/direct?q=",
    "GeoCityLimit": "1"
  }
}
```

Create an empty database, set the connection string and run the project.

## Commands

| Command | Description |
|---|---|
| `/get_weather` | Get current weather |
| `/fill_data` | Load sample data from `user.json` into the database |
| `/add_person` | Add a person |
| `/get_persons` | List all persons |
| `/edit_person` | Edit a person |
| `/delete_person` | Delete a person by Id |
| `/delete_all` | Delete all persons |

## Roadmap

- [ ] Replace persons CRUD with weather measurements storage
- [ ] Scheduled data collection for tracked cities
- [ ] Statistics and charts via bot commands
- [ ] Docker Compose (bot + SQL Server) and unit tests

## Demo

![Demo](simpleCrudBot.gif)