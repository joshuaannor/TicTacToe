using System;
using System.Collections.Generic;
using CrawfisSoftware.TicTacToeFramework;

namespace CrawfisSoftware
{
    class TicTacToeDumbAI
    {
        private static IGameBoard<int, CellState> gameBoard;
        private static IQueryGameState<int, CellState> gameQuery;
        private static IGameScore<CellState> gameScore;
        private static IPlayer playerX;
        private static IPlayer playerO;
        private static ITurnbasedScheduler scheduler;
        private static bool gameOver = false;

        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to Tic Tac Toe Dumb AI!");
            CreateGame();
            CreateConsolePlayers(gameBoard);
            CreateScheduler(playerX, playerO);
            CreateGameScore(gameBoard, gameQuery);
            SubscribeToEvents(gameBoard);
            PresentInstructions();

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
            if (winner == CellState.Blank)
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
            var game = new TicTacToeBoard<CellState>(CheckIfBlank);
            gameBoard = game;
            gameQuery = game;
        }

        private static bool CheckIfBlank(int cellID, CellState currentCellValue, CellState newCellState)
        {
            return currentCellValue == CellState.Blank;
        }

        private static void CreateConsolePlayers(IGameBoard<int, CellState> gameBoard)
        {
            playerX = new PlayerConsoleUnsafe(CellState.X, gameBoard);
            playerO = new PlayerDumb(CellState.O, gameBoard);
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
            Console.WriteLine("Welcome to Tic Tac Toe (Player vs. Dumb AI)!");
            Console.WriteLine("Instructions:");
            Console.WriteLine("1. The game is played on a 3x3 grid.");
            Console.WriteLine("2. You (the player) will take turns with a simple AI opponent.");
            Console.WriteLine("3. The first to get 3 marks in a row (horizontally, vertically, or diagonally) wins.");
            Console.WriteLine("4. If all cells are filled, and no one has 3 marks in a row, the game is a draw.");
            Console.WriteLine("5. You will enter a number between 1-9 to place your mark on the corresponding cell.");
            Console.WriteLine("6. The AI will randomly select a cell, even if it's already occupied (resulting in a wasted move).");
            Console.WriteLine("7. Enjoy the game!\n");
        }

       
    }
}