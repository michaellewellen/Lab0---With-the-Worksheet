using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static char[,]maze;
    static int numCoins = 10;
    static int vaultTopRow = 7;
    static int vaultBottomRow = 13;
    static int vaultLeftColumn = 18;
    static int vaultRightColumn = 34;
    static int playerRow = 0; // Starting position of the player
    static int playerCol = 0; // Starting position of the player
    static int score = 0;
    static CancellationTokenSource cts = new CancellationTokenSource();
    static bool gameWon = false;

    static List<(int, int)> enemies = new List<(int, int)>(); // List to hold enemy positions
    static int enemyDelay; // Delay for enemy movement speed

    static void Main(string[] args)
    {
        int difficulty = GetDifficultyLevel();
        LoadMazeFromFile ("maze.txt");
        InitializeCoins(numCoins);
        InitializeEnemies(difficulty);
        SetEnemyDelay(difficulty);
        
        
        Console.Clear();
        Console.CursorVisible = false; // Hide the cursor
        DrawMaze(); // Draw the maze once at the beginning

        // Draw the player on the maze
        maze[playerRow, playerCol] = '@'; // Set player position i n maze
        Console.SetCursorPosition(playerRow, playerCol); // Move cursor to player's position
        Console.Write('@'); // Draw player at initial position

        Task.Run(() => MoveEnemies(cts.Token)); // Start enemy movement in a separate task

        while (true)
        {
            if (gameWon) break; // Exit if the game is won

            ConsoleKeyInfo key = Console.ReadKey(true);
            HandleInput(key.Key);
        }

        // Set the cursor visible on winning
        Console.CursorVisible = true; 
        Console.SetCursorPosition(0, maze.GetLength(0) + 2); // Move the cursor down for the message
        Console.WriteLine("Congratulations! You've reached the door and won the game!");
        
    }

    static int GetDifficultyLevel()
    {
        int difficulty = 0;
        do{
            Console.Clear();
            Console.WriteLine("Select Difficulty \n\n\t1 for Easy\n\t2 for Normal\n\t3 for Hard\n\t");
            string? input = Console.ReadLine();
            int.TryParse(input, out difficulty);
        } while (difficulty < 1 || difficulty > 3);
        return difficulty;
    }

    static void SetEnemyDelay(int difficulty)
    {
        switch(difficulty)
        {
            case 1: enemyDelay = 500;
            break;
            case 2: enemyDelay = 250;
            break;
            case 3: enemyDelay = 75;
            break;
        }
    }
    static void InitializeCoins(int num)
    {
        Random rand = new Random();
        for (int i = 0; i < num; i++)
        {
            int coinRow, coinCol;
            do
            {
                coinRow = rand.Next(maze.GetLength(0));
                coinCol = rand.Next(maze.GetLength(1));
            } while (maze[coinRow, coinCol] != ' ' || IsInVault(coinRow,coinCol));
            maze[coinRow,coinCol] = '^';
        }
    }

    static bool IsInVault(int row, int col)
    {
        return row >= vaultTopRow && row <= vaultBottomRow && col >=vaultLeftColumn && col <= vaultRightColumn;
    }
    static void InitializeEnemies(int difficulty)
    {
        int enemyCount = 0;
        switch (difficulty)
        {
            case 1: enemyCount = 2;
            break;
            case 2: enemyCount = 4;
            break;
            case 3: enemyCount = 6;
            break;
        }

        enemies.Clear();
        Random rand = new Random();
        for (int i = 0; i < enemyCount; i++)
        {
            int enemyRow, enemyCol;
            do
            {
                enemyRow = rand.Next(maze.GetLength(0));
                enemyCol = rand.Next(maze.GetLength(1));
            } while (maze[enemyRow, enemyCol] != ' ' || enemies.Contains((enemyRow, enemyCol)));
            enemies.Add((enemyRow,enemyCol));
        }
    }
    static void LoadMazeFromFile(string filePath)
    {
        string[] lines = File.ReadAllLines(filePath);
        int rows = lines.Length;
        int cols = lines[0].Length;

        maze = new char[rows,cols];
        for (int i = 0; i < rows; i ++)
        {
            for (int j = 0; j<cols; j++)
            {
                maze[i,j] = lines[i][j];
            }
        }
    }
    static void DrawMaze()
    {
        for (int i = 0; i < maze.GetLength(0); i++)
        {
            for (int j = 0; j < maze.GetLength(1); j++)
            {
                Console.Write(maze[i, j]);
            }
            Console.WriteLine();
        }

        // Draw enemies
        foreach (var (x, y) in enemies)
        {
            Console.SetCursorPosition(y, x);
            Console.Write('%');
        }
    }

    static void HandleInput(ConsoleKey key)
    {
        int newRow = playerRow, newCol = playerCol;

        switch (key)
        {
            case ConsoleKey.UpArrow:
                if (playerRow > 0) newRow = playerRow - 1;
                break;
            case ConsoleKey.DownArrow:
                if (playerRow < maze.GetLength(0) - 1) newRow = playerRow + 1;
                break;
            case ConsoleKey.LeftArrow:
                if (playerCol > 0) newCol = playerCol - 1;
                break;
            case ConsoleKey.RightArrow:
                if (playerCol < maze.GetLength(1) - 1) newCol = playerCol + 1;
                break;
        }

        MovePlayer(newRow, newCol);
    }

    static void MovePlayer(int newRow, int newCol)
    {
        if (maze[newRow, newCol] == '%') // Collision with enemy
        {
            GameOver();
            return;
        }
        
        if (maze[newRow, newCol] == '^')
        {
            score += 100;
            maze[newRow,newCol] = ' ';
        }
        else if (maze[newRow, newCol] == '$')
        {
            score += 1000;
            maze[newRow,newCol] = ' ';
        }

        if (score >= 1000)
        {
            OpenVault();
        }

        if (maze[newRow, newCol] != ' ' && maze[newRow, newCol] != '#') // Only move if there's no wall or the door
        {
            return; // Block movement
        }

        if (maze[newRow, newCol] == '#') // Reached the door
        {
            gameWon = true; // Set game won flag
            return; // Exit to win message
        }

        // Move the player
        maze[playerRow, playerCol] = ' '; // Clear old position
        Console.SetCursorPosition(playerCol, playerRow); // Move cursor to old position
        Console.Write(' '); // Clear old position on screen

        playerRow = newRow;
        playerCol = newCol;

        maze[playerRow, playerCol] = '@'; // Set new position
        Console.SetCursorPosition(playerCol, playerRow); // Move cursor to new position
        Console.Write('@'); // Draw player at new position

        DisplayScore();
    }

    static void OpenVault()
    {
        for (int i = 0; i< maze.GetLength(0); i++)
        {
            for (int j = 0; j< maze.GetLength(1); j++)
            {
                if (maze[i,j] == '|')
                {
                    maze[i,j] = ' ';
                    Console.SetCursorPosition(j,i);
                    Console.Write(" ");
                }
            }
        }
    }
    static void DisplayScore()
    {
        Console.SetCursorPosition(0,maze.GetLength(0) +1);
        Console.WriteLine($"Score: {score}");
    }
    static void GameOver()
    {
        Console.CursorVisible = true;
        Console.SetCursorPosition(0, maze.GetLength(0) + 2); // Move cursor down for the message
        Console.WriteLine("Game Over! You were caught by an enemy.");
        Environment.Exit(0); // Exit the game
    }

    static async Task MoveEnemies(CancellationToken token)
    {
        Random rand = new Random();
        while (!token.IsCancellationRequested)
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                MoveEnemy(i, rand);
                if(IsPlayerCaught(enemies[i]))
                {
                    GameOver();
                    return;
                }
            }

            await Task.Delay(enemyDelay);
        }
    }
     
    static void MoveEnemy (int enemyIndex, Random rand)
    {
        var (enemyX, enemyY) = enemies[enemyIndex];
        int newEnemyX = enemyX;
        int newEnemyY = enemyY;
        int distance = Math.Abs(playerRow-enemyX) + Math.Abs(playerCol-enemyY);

        // Check for horizontal line of sight
        bool hasLineOfSight = true;
        if (enemyX == playerRow)
        {
            for(int j = Math.Min(enemyY, playerCol) +1; j < Math.Max(enemyY, playerCol); j++)
            {
                if(maze[enemyX, j] == '*')
                {
                    hasLineOfSight = false;
                    break;
                }
            }
        }
        // Check for vertical line of sight
        else if (enemyY == playerCol)
        {
            for(int i = Math.Min(enemyX, playerRow) + 1; i < Math.Max(enemyX, playerCol); i++)
            {
                if(maze[i,enemyY] == '*')
                {
                    hasLineOfSight = false;
                    break;
                }
            }
        }
        
        if (distance <= 10 && hasLineOfSight)
        {
            if(playerRow < enemyX) newEnemyX --;
            else if (playerRow > enemyY) newEnemyX ++;

            if (playerCol < enemyY) newEnemyY --;
            else if (playerCol > enemyY) newEnemyY++;
        }
        else /// move randomly if not close to the player
        {
            int direction = rand.Next(4);
            switch (direction)
            {
                case 0: if (enemyX > 0) newEnemyX--; break; // Move up
                case 1: if (enemyX < maze.GetLength(0) - 1) newEnemyX++; break; // Move down
                case 2: if (enemyY > 0) newEnemyY--; break; // Move left
                case 3: if (enemyY < maze.GetLength(1) - 1) newEnemyY++; break; // Move right
            }
        }
        if (maze[newEnemyX, newEnemyY] == ' ' || (newEnemyX == playerRow && newEnemyY == playerCol))
        {
            UpdateEnemyPosition(enemyIndex, newEnemyX, newEnemyY);
            if (newEnemyX == playerRow && newEnemyY == playerCol)
            {
                GameOver();
            }
        }
    }
    
    static void UpdateEnemyPosition(int enemyIndex, int newEnemyX, int newEnemyY)
    {
        var (enemyX, enemyY) = enemies[enemyIndex];
        maze[enemyX, enemyY] = ' '; 
        Console.SetCursorPosition(enemyY, enemyX); 
        Console.Write(' '); 

        enemies[enemyIndex] = (newEnemyX, newEnemyY);
        maze[newEnemyX, newEnemyY] = '%'; // Set new enemy position
        Console.SetCursorPosition(newEnemyY, newEnemyX); // Move cursor to new enemy position
        Console.Write('%'); // Draw enemy at new position
    }

    static bool IsPlayerCaught((int x, int y) enemy)          
    { 
        return enemy.x == playerRow && enemy.y == playerCol;
    }    
}
