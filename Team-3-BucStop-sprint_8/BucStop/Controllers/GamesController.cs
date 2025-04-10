using BucStop.Models;
using BucStop.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;

/*
 * This file handles the links to each of the game pages.
 */

namespace BucStop.Controllers
{
    [Authorize]
    public class GamesController : Controller
    {
        private readonly MicroClient _httpClient;
        private readonly PlayCountManager _playCountManager;
        private readonly GameService _gameService;

        public GamesController(MicroClient games, IWebHostEnvironment webHostEnvironment, GameService gameService)
        {
            _httpClient = games;
            _gameService = gameService;

            // Initialize the PlayCountManager with the web root path and the JSON file name
            _playCountManager = new PlayCountManager(_gameService.GetGames() ?? new List<Game>(), webHostEnvironment);
        }

        //Takes the user to the index page, passing the games list as an argument
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> IndexAsync()
        {
            List<Game> games = await GetGamesWithInfo();

            //have to update playcounts here since the we are reading it dynamically now instead of from a static list
            foreach(Game game in games)
            {
                game.PlayCount = _playCountManager.GetPlayCount(game.Id);
            }

            return View(games);
        }

        //Takes the user to the Play page, passing the game object the user wants to play
        public async Task<IActionResult> Play(int id)
        {
            List<Game> games = await GetGamesWithInfo();

            Game game = games.FirstOrDefault(x => x.Id == id);
            if (game == null)
            {
                return NotFound();
            }

            // Increment the play count for the game with the specified ID
            _playCountManager.IncrementPlayCount(id);

            int playCount = _playCountManager.GetPlayCount(id);

            // Update the game's play count
            game.PlayCount = playCount;

            return View(game);
        }

        public async Task<List<Game>> GetGamesWithInfo()
        {
            List<Game> games = _gameService.GetGames();
            GameInfo[] gameInfos = await _httpClient.GetGamesAsync();

            foreach(Game game in games)
            {
                GameInfo info = gameInfos.FirstOrDefault(x => x.Title == game.Title);
                if(info != null)
                {
                    game.Content = info.Content;
                    game.Author = info.Author;
                    game.HowTo = info.HowTo;
                    game.DateAdded = info.DateAdded;
                    game.Description = $"{info.Description} \n {info.DateAdded}";
                    game.LeaderBoard = info.LeaderBoard;
                }
            }

            return games;
        }

        //Takes the user to the deprecated snake page
        public IActionResult Snake()
        {
            return View();
        }

        //Takes the user to the deprecated tetris page
        public IActionResult Tetris()
        {
            return View();
        }

        // NEW METHOD ADDED
        //Used to get past some permissions errors, allows un-signed in users to access
        [AllowAnonymous]
        //Method for POST requests
        [HttpPost]
        //The route is "Games/UploadGame"
        [Route("Games/UploadGame")]
        public async Task<IActionResult> UploadGame([FromForm] IFormFile gameFile)
        {
            //Debug line meaning we have hit the endpoint
            Console.WriteLine("DEBUG: UploadGame endpoint hit.");

            //Make sure its not an empty file
            if (gameFile == null || gameFile.Length == 0)
            {
                Console.WriteLine("DEBUG: No file received.");
                Response.Headers.Add("Access-Control-Allow-Origin", "*");
                return BadRequest("No file uploaded.");
            }

            //Gives the storage path for files
            var uploadsPath = "/mnt/ebs-storage";
            //If the upload path doesn't exist, it creates it
            Directory.CreateDirectory(uploadsPath);
            //Making the full file path
            var filePath = Path.Combine(uploadsPath, Path.GetFileName(gameFile.FileName));

            try
            {
                //Creates a new file and filestream
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    //Copies the file to the stream
                    await gameFile.CopyToAsync(stream);
                }

                //Debug line to show where the file has been saved
                Console.WriteLine($"DEBUG: File saved to {filePath}");
                //CORS header for successful response
                Response.Headers.Add("Access-Control-Allow-Origin", "*");
                //Success message
                return Json(new { message = "File uploaded successfully!" });
            }
            catch (Exception ex)
            {
                //EXCEPTION FOR IF IT DOESN'T WORK
                Console.WriteLine("UPLOAD ERROR: " + ex.Message);
                Response.Headers.Add("Access-Control-Allow-Origin", "*");
                return StatusCode(500, $"Upload failed: {ex.Message}");
            }
        }

    }
}
