using System;
using System.Collections.Generic;
using CrawfisSoftware.TicTacToeFramework;

namespace CrawfisSoftware
{
    class TicTacToe
    {
        private static IGameBoard<int, CellState> gameBoard;
        private static IQueryGameState<int, CellState> gameQuery;
        private static IGameScore<CellState> gameScore;
        private static IPlayer playerX;
        private static IPlayer playerO;
        private static ITurnbasedScheduler scheduler;
        private static System.Random random = new System.Random();
        private static bool gameOver = false;
        private static int playerXReplacements = 0;
        private static int playerOReplacements = 0;
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to Classic Tic Tac Toe!");
            PresentInstructions();
            CreateGame();
            CreateConsolePlayers(gameBoard);
            CreateScheduler(playerX, playerO);
            CreateGameScore(gameBoard, gameQuery);
            SubscribeToEvents(gameBoard);
            

            while (!gameOver)
            {
                IPlayer player = scheduler.SelectPlayer();
                player.TakeTurn();
                gameScore.CheckGameState();
            }
        }
        
        private static void GameScore_GameOver(object sender, CellState winner)
        {
            Console.WriteLine();
            gameOver = true;
            if(winner == CellState.Blank)
            {
                Console.WriteLine("Game is a Draw :-(");
            }
            else
            {
                Console.WriteLine("{0} wins! :-)", winner);
            }
        }

        private static void CreateGame()
        {
            var game = new TicTacToeBoard<CellState>(CheckIfBlankOrTheSame);
            gameBoard = game;
            gameQuery = game;
        }

        private static void CreateConsolePlayers(IGameBoard<int, CellState> gameBoard)
        {
            playerX = new PlayerConsoleUnsafe(CellState.X, gameBoard);
            playerO = new PlayerConsoleUnsafe(CellState.O, gameBoard);
        }

        private static void CreateScheduler(IPlayer playerX, IPlayer playerO)
        {
            scheduler = new SequentialTurnsScheduler(new List<IPlayer>() { playerX, playerO });
        }

        private static void CreateGameScore(IGameBoard<int, CellState> gameBoard, IQueryGameState<int, CellState> gameQuery)
        {
            var gameScoreComposite = new GameScoreComposite<CellState>();

            var gameScorer1 = new GameScoreThreeInARow(gameBoard, gameQuery);
            gameScoreComposite.AddGameScore(gameScorer1);

            var gameScorer2 = new GameScoreBoardFilled<CellState>(gameQuery);
            gameScoreComposite.AddGameScore(gameScorer2);

            // add the max number of turns scorer
            var gameScoreMaxTurns = new GameScoreMaxNumberOfTurns<CellState>(15, gameBoard);
            gameScoreComposite.AddGameScore(gameScoreMaxTurns);


            gameScore = gameScoreComposite;
            gameScore.GameOver += GameScore_GameOver;
        }

        private static void SubscribeToEvents(IGameBoard<int, CellState> gameBoard)
        {
            gameBoard.ChangeCellRequested += GameBoard_ChangeCellRequested;
            gameBoard.CellChanged += GameBoard_CellChanged;
        }

        private static void GameBoard_CellChanged(int cellID, CellState oldCellState, CellState newCellState)
        {
            Console.WriteLine("Cell {0} changed to {1}", cellID, newCellState);
        }

        private static void GameBoard_ChangeCellRequested(int cellID, CellState currentCellState, CellState proposedCellState)
        {
            Console.WriteLine("Attempt to change cell {0} from {1} to {2}", cellID, currentCellState, proposedCellState);
        }

        private static void PresentInstructions()
        {
            Console.WriteLine("Welcome to Classic Tic Tac Toe (Player vs. Player)!");
            Console.WriteLine("Instructions:");
            Console.WriteLine("1. The game is played on a 3x3 grid.");
            Console.WriteLine("2. Two players take turns placing their marks (X or O) on the grid.");
            Console.WriteLine("3. The first player to get 3 of their marks in a row (horizontally, vertically, or diagonally) wins.");
            Console.WriteLine("4. If all cells are filled, and no player has 3 marks in a row, the game is a draw.");
            Console.WriteLine("5. Players will enter a number between 1-9 to place their mark on the corresponding cell.");
            Console.WriteLine("6. Enjoy the game!\n");
        }

        private static bool CheckIfBlankOrTheSame(int cellID, CellState currentCellValue, CellState newCellState)
        {
            if (currentCellValue == CellState.Blank && currentCellValue != newCellState)
            {
                // If the current cell is blank and the new cell state is different, allow the move
                return true;
            }
            else if (currentCellValue != CellState.Blank && currentCellValue != newCellState)
            {
                // If the current cell is not blank and the new cell state is different (i.e., replacing an occupied cell)
                if (newCellState == CellState.X && playerXReplacements < 3)
                {
                    // If the new cell state is X and player X has replacements available
                    playerXReplacements++;
                    Console.WriteLine("Player X replaced cell {0}. Replacements remaining: {1}", cellID, 3 - playerXReplacements);
                    return true;
                }
                else if (newCellState == CellState.O && playerOReplacements < 3)
                {
                    // If the new cell state is O and player O has replacements available
                    playerOReplacements++;
                    Console.WriteLine("Player O replaced cell {0}. Replacements remaining: {1}", cellID, 3 - playerOReplacements);
                    return true;
                }
                else
                {
                    // If the player has no replacements available
                    Console.WriteLine("Player {0} cannot replace the cell. No replacements remaining.", newCellState);
                    return false;
                }
            }
            return false;
        }
    }
}