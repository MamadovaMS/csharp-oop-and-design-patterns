using System;
using System.Collections.Generic;

namespace Lab3_Refactoring
{
    class Program
    {
        static void Main(string[] args)
        {
            Team teamA = new Team { Name = "Спартак" };
            Team teamB = new Team { Name = "Зенит" };

            // ИСПРАВЛЕНИЕ: Убрали дублирование, вынесли создание игрока в отдельный метод
            teamA.AddPlayer(CreatePlayer("Иван Иванов", 28, 1.85, 80));
            teamB.AddPlayer(CreatePlayer("Алексей Петров", 30, 1.80, 75));

            Match match = new Match() { Date = DateTime.Now, HomeTeam = teamA, AwayTeam = teamB };
            match.PlayMatch();
            match.PrintScore();

            CheckTeamPerformance(teamA);
            RunDetailedReport(match);

            // ИСПРАВЛЕНИЕ: "Завистливую" функцию переместили внутрь класса Team.
            // Теперь сама команда знает, как напечатать свой результат.
            teamA.LogResult(true, match.HomeGoals, match.AwayGoals);
            teamB.LogResult(false, match.AwayGoals, match.HomeGoals);

            Console.ReadLine();
        }

        // ИСПРАВЛЕНИЕ: Выделенный метод (Extract Method) для создания игрока
        static Player CreatePlayer(string name, int age, double height, double weight)
        {
            return new Player { Name = name, Age = age, Height = height, Weight = weight };
        }

        static void CheckTeamPerformance(Team team)
        {
            if (team.MatchesPlayed == 0) { Console.WriteLine("Нет матчей"); return; }
            float winPercentage = ((float)team.Wins / team.MatchesPlayed) * 100f;

            if (winPercentage >= 70) Console.WriteLine("Высокая производительность команды.");
            else if (winPercentage >= 50) Console.WriteLine("Средняя производительность команды.");
            else Console.WriteLine("Низкая производительность команды.");
        }

        static void RunDetailedReport(Match match)
        {
            Console.WriteLine($"Отчет о матче {match.HomeTeam.Name} против {match.AwayTeam.Name} ({match.Date.ToShortDateString()}):\n" +
                              $" {match.HomeTeam.Name} забили {match.HomeGoals}, пропустили {match.AwayGoals}.\n" +
                              $" {match.AwayTeam.Name} забили {match.AwayGoals}, пропустили {match.HomeGoals}.");
        }
    }

    public class Player
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public double Height { get; set; }
        public double Weight { get; set; }

        public void DisplayInfo()
        {
            Console.WriteLine($"Имя игрока: {Name}, Возраст: {Age}, Рост: {Height}m, Вес: {Weight}kg");
        }
    }

    public class Team
    {
        private List<Player> players = new List<Player>();
        public string Name { get; set; }
        public int MatchesPlayed { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Draws { get; set; }
        public int GoalsScored { get; set; }
        public int GoalsConceded { get; set; }

        public void AddPlayer(Player player) { players.Add(player); }

        public void UpdateResults(int goalsFor, int goalsAgainst)
        {
            GoalsScored += goalsFor;
            GoalsConceded += goalsAgainst;
            if (goalsFor > goalsAgainst) Wins++;
            else if (goalsFor < goalsAgainst) Losses++;
            else Draws++;
            MatchesPlayed++;
        }

        // ИСПРАВЛЕНИЕ: Метод перемещен сюда из ProgramHelper (Move Method)
        public void LogResult(bool isWinningTeam, int goalsFor, int goalsAgainst)
        {
            if (isWinningTeam) Console.WriteLine("Команда победила!");
            else Console.WriteLine("Команда проиграла.");
            Console.WriteLine($"Голы забитые командой {this.Name}: {goalsFor}. Голы пропущены: {goalsAgainst}");
        }
    }

    public class Match
    {
        public DateTime Date { get; set; }
        public Team HomeTeam { get; set; }
        public Team AwayTeam { get; set; }
        public int HomeGoals { get; set; }
        public int AwayGoals { get; set; }

        public void PlayMatch()
        {
            Random rnd = new Random();
            HomeGoals = rnd.Next(0, 8);
            AwayGoals = rnd.Next(0, 8);
            HomeTeam.UpdateResults(HomeGoals, AwayGoals);
            AwayTeam.UpdateResults(AwayGoals, HomeGoals);
        }

        public void PrintScore()
        {
            Console.WriteLine($"{HomeTeam.Name}: {HomeGoals} vs {AwayTeam.Name}: {AwayGoals}");
        }
    }
    // Класс ProgramHelper мы удалили, так как он больше не нужен!
}